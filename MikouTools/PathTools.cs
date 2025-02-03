using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools
{
    public static class PathTools
    {
        public static string ChangeUnusableCharacters(string name)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (invalidChars.Contains(name[i])) { stringBuilder.Append("_"); } else { stringBuilder.Append(name[i]); }

            }
            return stringBuilder.ToString().TrimStart().TrimEnd().TrimEnd('.');//name.Replace(@"\", "￥").Replace("/", "／").Replace(":", "：").Replace("*", "＊").Replace("\"", "”").Replace("?", "？").Replace("<", "＜").Replace(">", "＞").Replace("|", "｜");
        }

        public static string PathCharacterLengthCheck(string FullPath)
        {
            if (FullPath.Length > 259)
            {
                return FullPath.Remove(258);
            }
            return FullPath;
        }

        public static string AvoidPathDuplication(string Path, bool Directory = false, string Bracket_1 = " [", string Bracket_2 = "]")
        {
            if (!Directory)
            {

                if (File.Exists(Path) || System.IO.Directory.Exists(Path))
                {
                    //拡張子なしのファイルパッチ
                    string DirectoryPath = $@"{System.IO.Path.GetDirectoryName(Path)}\{System.IO.Path.GetFileNameWithoutExtension(Path)}";
                    //拡張子
                    string Extension = System.IO.Path.GetExtension(Path);
                    int index = 2;
                    while (true)
                    {
                        string newpath = $@"{DirectoryPath}{Bracket_1}{index}{Bracket_2}{Extension}";
                        if (!File.Exists(newpath) && !System.IO.Directory.Exists(Path))
                        {
                            return newpath;
                        }
                        index++;
                    }
                }
                else
                {
                    return Path;
                }
            }
            else
            {
                if (File.Exists(Path) || System.IO.Directory.Exists(Path))
                {
                    int index = 2;
                    while (true)
                    {
                        string newpath = $@"{Path}{Bracket_1}{index}{Bracket_2}";
                        if (!File.Exists(Path) && !System.IO.Directory.Exists(newpath))
                        {
                            return newpath;
                        }
                        index++;
                    }
                }
                else
                {
                    return Path;
                }
            }
        }
    }
}
