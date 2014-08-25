using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Reflection;

namespace ROS_CS
{
    namespace SocketBridge
    {
        public class SocketRX<T> where T : ROS_CS.Core.BaseMessage
        {
            private TcpClient socket_client;
            private IPEndPoint serverEndPoint;
            private NetworkStream client_stream;
            private int read_length = 0;
            private bool ok = true;
            private int message_length = -1;
            private message_callback<T> client_callback;
            private Thread listen_thread;
            private List<Byte> buffer;
    
            public SocketRX(string server_address, int server_port_number, message_callback<T> callback)
            {
                buffer = new List<byte>();
                client_callback = callback;
                read_length = 4096;
                socket_client = new TcpClient();
                IPAddress[] addresses = System.Net.Dns.GetHostAddresses(server_address);
                if (addresses.Length < 1)
                {
                    System.ArgumentException argex = new System.ArgumentException("Server address could not be resolved");
                    throw argex;
                }
                serverEndPoint = new IPEndPoint(addresses[0], server_port_number);
                socket_client.Connect(serverEndPoint);
                client_stream = socket_client.GetStream();
                listen_thread = new Thread(new ThreadStart(ListenLoop));
                listen_thread.Start();
            }
    
            public SocketRX(string server_address, int server_port_number, message_callback<T> callback, int read_len)
            {
                buffer = new List<byte>();
                client_callback = callback;
                read_length = read_len;
                socket_client = new TcpClient();
                IPAddress[] addresses = System.Net.Dns.GetHostAddresses(server_address);
                if (addresses.Length < 1)
                {
                    System.ArgumentException argex = new System.ArgumentException("Server address could not be resolved");
                    throw argex;
                }
                socket_client.Connect(serverEndPoint);
                client_stream = socket_client.GetStream();
                listen_thread = new Thread(new ThreadStart(ListenLoop));
                listen_thread.Start();
            }
    
            private void ListenLoop()
            {
                while (true)
                {
                    if (ok)
                    {
                        Byte[] new_data = null;
                        try
                        {
                            new_data = ReadFromServer(read_length);
                        }
                        catch
                        {
                            continue;
                        }
                        if (new_data == null)
                        {
                            ok = false;
                            continue;
                        }
                        else
                        {
                            buffer.AddRange(new_data);
                            buffer = AttemptDeserialization(buffer);
                        }
                    }
                    else
                    {
                        try
                        {
                            socket_client.Close();
                            socket_client.Connect(serverEndPoint);
                            client_stream = socket_client.GetStream();
                            buffer.Clear();
                            ok = true;
                        }
                        catch
                        {
                            ok = false;
                        }
                    }
                }
            }
    
            private List<Byte> AttemptDeserialization(List<Byte> cur_buffer)
            {
                while ((message_length < 0 && cur_buffer.Count >= 4) || (message_length > -1 && cur_buffer.Count >= message_length))
                {
                    if (message_length < 0 && cur_buffer.Count >= 4)
                    {
                        message_length = BitConverter.ToInt32(cur_buffer.ToArray(), 0);
                        cur_buffer.RemoveRange(0, 4);
                    }
                    if (message_length > -1 && cur_buffer.Count >= message_length)
                    {
                        CleanAndPub(cur_buffer.ToArray());
                        cur_buffer.RemoveRange(0, message_length);
                        message_length = -1;
                    }
                }
                return cur_buffer;
            }
    
            private void CleanAndPub(Byte[] serialized)
            {
                T new_msg = (T)Activator.CreateInstance(typeof(T));
                new_msg.Deserialize(serialized);
                try
                {
                    new_msg.Deserialize(serialized);
                    // This generates a delegate wrapping the callback function
                    // This spawns a new thread for the callback with the new message as a parameter
                    Thread callback_thread = new Thread(() => client_callback(new_msg));
                    callback_thread.Start();
                }
                catch
                {
                    Console.WriteLine("Unable to deserialize message");
                }
            }
    
            private byte[] ReadFromServer(int bytes_to_read)
            {
                byte[] message = new byte[bytes_to_read];
                int bytesRead = 0;
                //Try to read in data from the client
                try
                {
                    //blocks until a client sends a message
                    bytesRead = client_stream.Read(message, 0, bytes_to_read);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("There appears to have been a socket error receiving");
                    return null;
                }
                //See if the transfer was successful
                if (bytesRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine("The client appears to have disconnected");
                    return null;
                }
                else if (bytesRead < bytes_to_read)
                {
                    System.Diagnostics.Debug.WriteLine("Data received successfully, but it wasn't as many bytes as we asked for");
                }
                Byte[] received = new Byte[bytesRead];
                Array.Copy(message, received, bytesRead);
                //message has successfully been received
                return received;
            }
    
            public void Cleanup()
            {
                listen_thread.Abort();
                socket_client.Close();
            }
        }
    }
}