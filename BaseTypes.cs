using System;
using System.IO;

namespace ROS_CS
{
    public delegate void message_callback<T>(T message) where T : ROS_CS.Core.BaseMessage;

    namespace Core
    {
        public class Header : BaseMessage
        {
            public UInt32 seq;
            public Time stamp;
            public string frame_id;
    
            public Header ()
            {
                seq = 0;
                stamp = new Time(0, 0);
                frame_id = "";
            }
    
            public Header (Time _stamp)
            {
                seq = 0;
                stamp = _stamp;
                frame_id = "";
            }
    
            public Header (string _frame_id)
            {
                seq = 0;
                stamp = new Time(0, 0);
                frame_id = _frame_id;
            }
    
            public Header (Time _stamp, string _frame_id)
            {
                seq = 0;
                stamp = _stamp;
                frame_id = _frame_id;
            }

            public override string ToString ()
            {
                return ROS_CS.Core.PrettyPrinter.PrettyPrint(ToStringRaw());
            }

            public override string ToStringRaw ()
            {
                string string_rep = "";
                string_rep += "seq: " + Convert.ToString(seq) + "\n";
                string_rep += stamp.ToStringRaw() + "\n";
                string_rep += "frame_id: " + frame_id + "\n";
                return string_rep;
            }
    
            public override void Serialize(MemoryStream stream)
            {
                Byte[] seq_bytes = BitConverter.GetBytes(seq);
                Byte[] frame_id_bytes = System.Text.Encoding.UTF8.GetBytes(frame_id);
                Byte[] frame_id_len_bytes = BitConverter.GetBytes((UInt32)frame_id_bytes.Length);
                stream.Write(seq_bytes, 0, seq_bytes.Length);
                stamp.Serialize(stream);
                stream.Write(frame_id_len_bytes, 0, frame_id_len_bytes.Length);
                stream.Write(frame_id_bytes, 0, frame_id_bytes.Length);
            }
    
            public override int Deserialize(Byte[] serialized)
            {
                return Deserialize(serialized, 0);
            }

            public override int Deserialize(Byte[] serialized, int startIndex)
            {
                if ((serialized.Length - startIndex) < 16)
                {
                    throw new ArgumentException("Failed to deserialize message to type: header - serialized data is too short for the message type");
                }
                try
                {
                    int curIndex = startIndex;
                    seq = BitConverter.ToUInt32(serialized, curIndex);
                    curIndex += BitConverter.GetBytes(seq).Length;
                    curIndex += stamp.Deserialize(serialized, curIndex);
                    UInt32 frame_id_len = BitConverter.ToUInt32(serialized, curIndex);
                    curIndex += BitConverter.GetBytes(frame_id_len).Length;
                    frame_id = System.Text.Encoding.UTF8.GetString(serialized, curIndex, (int)frame_id_len);
                    curIndex += (int)frame_id_len;
                    return (curIndex - startIndex);
                }
                catch(Exception innerException)
                {
                    throw new ArgumentException("Failed to deserialize message to type: header", innerException);
                }
            }
        }
    
        public class Time : BaseMessage
        {
            public Int32 secs;
            public Int32 nsecs;
    
            public Time ()
            {
                secs = 0;
                nsecs = 0;
            }
    
            public Time (Int32 _secs, Int32 _nsecs)
            {
                secs = _secs;
                nsecs = _nsecs;
            }
    
            public Time (double seconds)
            {
                secs = Convert.ToInt32(seconds);
                nsecs = Convert.ToInt32((seconds - secs) * 1000000000.0);
            }
    
            public double InSeconds()
            {
                double seconds = Convert.ToDouble(secs) + (Convert.ToDouble(nsecs) / 1000000000.0);
                return seconds;
            }
    
            public override string ToString()
            {
                return ROS_CS.Core.PrettyPrinter.PrettyPrint(ToStringRaw());
            }

            public override string ToStringRaw()
            {
                string string_rep = "Time:\nsecs: " + Convert.ToString(secs) + "\nnsecs: " + Convert.ToString(nsecs) + "\n";
                return string_rep;
            }
    
            public override void Serialize(MemoryStream stream)
            {
                Byte[] secs_bytes = BitConverter.GetBytes(secs);
                stream.Write(secs_bytes, 0, secs_bytes.Length);
                Byte[] nsecs_bytes = BitConverter.GetBytes(nsecs);
                stream.Write(nsecs_bytes, 0, nsecs_bytes.Length);
            }
    
            public override int Deserialize(Byte[] serialized)
            {
                return Deserialize(serialized, 0);
            }
    
            public override int Deserialize(Byte[] serialized, int startIndex)
            {
                if ((serialized.Length - startIndex) < 8)
                {
                    throw new ArgumentException("Failed to deserialize message to type: time - serialized data is too short for the message type");
                }
                try
                {
                    secs = BitConverter.ToInt32(serialized, startIndex);
                    nsecs = BitConverter.ToInt32(serialized, startIndex + 4);
                    return 8;
                }
                catch(Exception innerException)
                {
                    throw new ArgumentException("Failed to deserialize message to type: time", innerException);
                }
            }
        }
    
        public class Duration : BaseMessage
        {
            public Int32 secs;
            public Int32 nsecs;
    
            public Duration ()
            {
                secs = 0;
                nsecs = 0;
            }
    
            public Duration (Int32 _secs, Int32 _nsecs)
            {
                secs = _secs;
                nsecs = _nsecs;
            }
    
            public Duration (double seconds)
            {
                secs = Convert.ToInt32(seconds);
                nsecs = Convert.ToInt32((seconds - secs) * 1000000000.0);
            }
    
            public double InSeconds()
            {
                double seconds = Convert.ToDouble(secs) + (Convert.ToDouble(nsecs) / 1000000000.0);
                return seconds;
            }

            public override string ToString()
            {
                return ROS_CS.Core.PrettyPrinter.PrettyPrint(ToStringRaw());
            }

            public override string ToStringRaw()
            {
                string string_rep = "Duration:\nsecs: " + Convert.ToString(secs) + "\nnsecs: " + Convert.ToString(nsecs) + "\n";
                return string_rep;
            }
    
            public override void Serialize(MemoryStream stream)
            {
                Byte[] secs_bytes = BitConverter.GetBytes(secs);
                stream.Write(secs_bytes, 0, secs_bytes.Length);
                Byte[] nsecs_bytes = BitConverter.GetBytes(nsecs);
                stream.Write(nsecs_bytes, 0, nsecs_bytes.Length);
            }

            public override int Deserialize(Byte[] serialized)
            {
                return Deserialize(serialized, 0);
            }
    
            public override int Deserialize(Byte[] serialized, int startIndex)
            {
                if ((serialized.Length - startIndex) < 8)
                {
                    throw new ArgumentException("Failed to deserialize message to type: duration - serialized data is too short for the message type");
                }
                try
                {
                    secs = BitConverter.ToInt32(serialized, startIndex);
                    nsecs = BitConverter.ToInt32(serialized, startIndex + 4);
                    return 8;
                }
                catch(Exception innerException)
                {
                    throw new ArgumentException("Failed to deserialize message to type: duration", innerException);
                }
            }
        }
    }
}

