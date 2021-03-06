using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

//////////////////////////////////////////////////
/////    AUTOGENERATED MESSAGE DEFINITION    /////
//////////////////////////////////////////////////
/////         DO NOT MODIFY BY HAND!         /////
//////////////////////////////////////////////////

namespace ROS_CS
{
    namespace person_tracker
    {
        public class HandTrack : ROS_CS.Core.BaseMessage
        {
            public readonly string typeID = "HandTrack";
            public readonly string md5sum = "14ddfd868c26056eab598da2ebad8937";
            public std_msgs.Header header;
            public List<person_tracker.FingerTrack> Fingers;
            public readonly System.Byte UNKNOWN = 0;
            public readonly System.Byte CLOSED = 1;
            public readonly System.Byte OPEN = 2;
            public System.Byte HandState;
            public float HandStateConfidence;

            public HandTrack ()
            {
                header = new std_msgs.Header();
                Fingers = new List<person_tracker.FingerTrack>();
                HandState = 0;
                HandStateConfidence = 0.0f;
            }

            public override string ToString ()
            {
                return ROS_CS.Core.PrettyPrinter.PrettyPrint(ToStringRaw());
            }

            public override string ToStringRaw ()
            {
                string string_rep = typeID + ":\n";
                string_rep += header.ToStringRaw() + "\n";
                string_rep += "Fingers:\n[";
                foreach (person_tracker.FingerTrack element in Fingers)
                {
                    string_rep += " " + element.ToStringRaw();
                }
                string_rep += "]\n\n";
                string_rep += "HandState: " + Convert.ToString(HandState) + "\n";
                string_rep += "HandStateConfidence: " + Convert.ToString(HandStateConfidence) + "\n";
                return string_rep;
            }

            public override void Serialize(MemoryStream stream)
            {
                header.Serialize(stream);
                System.Byte[] Fingers_len_bytes = BitConverter.GetBytes((System.UInt32)Fingers.Count);
                stream.Write(Fingers_len_bytes, 0, Fingers_len_bytes.Length);
                foreach(person_tracker.FingerTrack element in Fingers)
                {
                    element.Serialize(stream);
                }
                System.Byte[] HandState_bytes = new System.Byte[] {HandState};
                stream.Write(HandState_bytes, 0, HandState_bytes.Length);
                System.Byte[] HandStateConfidence_bytes = BitConverter.GetBytes(HandStateConfidence);
                stream.Write(HandStateConfidence_bytes, 0, HandStateConfidence_bytes.Length);
            }

            public override int Deserialize(System.Byte[] serialized)
            {
                return Deserialize(serialized, 0);
            }

            public override int Deserialize(System.Byte[] serialized, int startIndex)
            {
                int curIndex = startIndex;
                curIndex += header.Deserialize(serialized, curIndex);
                System.UInt32 Fingers_len = BitConverter.ToUInt32(serialized, curIndex);
                curIndex += BitConverter.GetBytes(Fingers_len).Length;
                for (int i = 0; i < (int)Fingers_len; i++)
                {
                    person_tracker.FingerTrack element = new person_tracker.FingerTrack();
                    curIndex += element.Deserialize(serialized, curIndex);
                    Fingers.Add(element);
                }
                HandState = serialized[curIndex];
                curIndex++;
                HandStateConfidence = BitConverter.ToSingle(serialized, curIndex);
                curIndex += BitConverter.GetBytes(HandStateConfidence).Length;
                return (curIndex - startIndex);
            }

        }
    }
}
