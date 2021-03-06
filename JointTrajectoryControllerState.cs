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
    namespace pr2_controllers_msgs
    {
        public class JointTrajectoryControllerState : ROS_CS.Core.BaseMessage
        {
            public readonly string typeID = "JointTrajectoryControllerState";
            public readonly string md5sum = "10817c60c2486ef6b33e97dcd87f4474";
            public std_msgs.Header header;
            public List<string> joint_names;
            public trajectory_msgs.JointTrajectoryPoint desired;
            public trajectory_msgs.JointTrajectoryPoint actual;
            public trajectory_msgs.JointTrajectoryPoint error;

            public JointTrajectoryControllerState ()
            {
                header = new std_msgs.Header();
                joint_names = new List<string>();
                desired = new trajectory_msgs.JointTrajectoryPoint();
                actual = new trajectory_msgs.JointTrajectoryPoint();
                error = new trajectory_msgs.JointTrajectoryPoint();
            }

            public override string ToString ()
            {
                return ROS_CS.Core.PrettyPrinter.PrettyPrint(ToStringRaw());
            }

            public override string ToStringRaw ()
            {
                string string_rep = typeID + ":\n";
                string_rep += header.ToStringRaw() + "\n";
                string_rep += "joint_names:\n[";
                foreach (string element in joint_names)
                {
                    string_rep += " " + element;
                }
                string_rep += "]\n\n";
                string_rep += desired.ToStringRaw() + "\n";
                string_rep += actual.ToStringRaw() + "\n";
                string_rep += error.ToStringRaw() + "\n";
                return string_rep;
            }

            public override void Serialize(MemoryStream stream)
            {
                header.Serialize(stream);
                System.Byte[] joint_names_len_bytes = BitConverter.GetBytes((System.UInt32)joint_names.Count);
                stream.Write(joint_names_len_bytes, 0, joint_names_len_bytes.Length);
                foreach(string element in joint_names)
                {
                    System.Byte[] element_bytes = System.Text.Encoding.UTF8.GetBytes(element);
                    System.Byte[] element_len_bytes = BitConverter.GetBytes((System.UInt32)element_bytes.Length);
                    stream.Write(element_len_bytes, 0, element_len_bytes.Length);
                    stream.Write(element_bytes, 0, element_bytes.Length);
                }
                desired.Serialize(stream);
                actual.Serialize(stream);
                error.Serialize(stream);
            }

            public override int Deserialize(System.Byte[] serialized)
            {
                return Deserialize(serialized, 0);
            }

            public override int Deserialize(System.Byte[] serialized, int startIndex)
            {
                int curIndex = startIndex;
                curIndex += header.Deserialize(serialized, curIndex);
                System.UInt32 joint_names_len = BitConverter.ToUInt32(serialized, curIndex);
                curIndex += BitConverter.GetBytes(joint_names_len).Length;
                for (int i = 0; i < (int)joint_names_len; i++)
                {
                    System.UInt32 element_len = BitConverter.ToUInt32(serialized, curIndex);
                    curIndex += BitConverter.GetBytes(element_len).Length;
                    string element = System.Text.Encoding.UTF8.GetString(serialized, curIndex, (int)element_len);
                    curIndex += (int)element_len;
                    joint_names.Add(element);
                }
                curIndex += desired.Deserialize(serialized, curIndex);
                curIndex += actual.Deserialize(serialized, curIndex);
                curIndex += error.Deserialize(serialized, curIndex);
                return (curIndex - startIndex);
            }

        }
    }
}
