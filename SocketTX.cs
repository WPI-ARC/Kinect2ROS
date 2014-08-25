using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace ROS_CS
{
    namespace SocketBridge
    {
        public class SocketTX<T> where T : ROS_CS.Core.BaseMessage
        {
            private TcpListener socket_server;
            private Thread listen_thread;
            private List<TcpClient> clients;

            public SocketTX(string server_IP_address, int server_port_number)
            {
                clients = new List<TcpClient>();
                socket_server = new TcpListener(IPAddress.Parse(server_IP_address), server_port_number);
                listen_thread = new Thread(new ThreadStart(ListenLoop));
                listen_thread.Start();
            }

            public SocketTX(int server_port_number)
            {
                clients = new List<TcpClient>();
                socket_server = new TcpListener(IPAddress.Any, server_port_number);
                listen_thread = new Thread(new ThreadStart(ListenLoop));
                listen_thread.Start();
            }

            private void ListenLoop()
            {
                socket_server.Start();
                while (true)
                {
                    TcpClient new_client = socket_server.AcceptTcpClient();
                    System.Diagnostics.Debug.WriteLine("Adding new client");
                    lock (clients)
                    {
                        clients.Add(new_client);
                    }
                }
            }

            public void Send(T message)
            {
                MemoryStream stream = new MemoryStream();
                message.Serialize(stream);
                stream.Capacity = (int)stream.Position;
                Byte[] serialized = stream.GetBuffer();
                Byte[] serialized_len_bytes = BitConverter.GetBytes((System.UInt32)serialized.Length);
                lock (clients)
                {
                    List<TcpClient> working_clients = new List<TcpClient>();
                    foreach (TcpClient client in clients)
                    {
                        try
                        {
                            client.GetStream().Write(serialized_len_bytes, 0, serialized_len_bytes.Length);
                            client.GetStream().Write(serialized, 0, serialized.Length);
                            working_clients.Add(client);
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine("Socket write failed, removing client");
                            client.Close();
                        }
                    }
                    clients = working_clients;
                }
            }

            public void Cleanup()
            {
                listen_thread.Abort();
                socket_server.Stop();
            }
        }
    }
}