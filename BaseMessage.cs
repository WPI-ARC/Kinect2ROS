using System;
using System.IO;

namespace ROS_CS
{
    namespace Core
    {
        public class PrettyPrinter
        {
            public static string PrettyPrint(string messageRep)
            {
                string pretty_rep = "";
                string[] lines = messageRep.Split('\n');
                int cur_spaces = 0;
                foreach (string line in lines)
                {
                    if (line.EndsWith(":"))
                    {
                        pretty_rep += new string(' ', cur_spaces) + line + "\n";
                        cur_spaces += 2;
                    }
                    else if (line.EndsWith(": "))
                    {
                        pretty_rep += new string(' ', cur_spaces) + line  + "\"\"" + "\n";
                    }
                    else if (line.Equals(""))
                    {
                        if (cur_spaces > 2)
                        {
                            cur_spaces -= 2;
                        }
                        else
                        {
                            cur_spaces = 0;
                        }
                    }
                    else
                    {
                        pretty_rep += new string(' ', cur_spaces) + line + "\n";
                    }
                }
                return pretty_rep;
            }
        }
    
        public abstract class BaseMessage
        {
            public abstract override string ToString();

            public abstract string ToStringRaw();

            public abstract void Serialize(MemoryStream stream);
    
            public abstract int Deserialize(Byte[] serialized);

            public abstract int Deserialize(Byte[] serialized, int startIndex);
        }
    }
}

