using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools
{
    public static class StringTools
    {
        public static string NormalizeNewlines(string input, string newLine = "\r\n")
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            /*            if (string.IsNullOrEmpty(input)) return input;

                        int length = input.Length;
                        var sb = new StringBuilder(length);

                        for (int i = 0; i < length; i++)
                        {
                            char c = input[i];

                            if (c == '\r')
                            {
                                if (i + 1 < length && input[i + 1] == '\n')
                                {
                                    i++;
                                }
                                sb.Append(newLine);
                            }
                            else if (c == '\n')
                            {
                                sb.Append(newLine);
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }

                        return sb.ToString();*/
        }
    }
}
