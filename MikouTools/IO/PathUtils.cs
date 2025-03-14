using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MikouTools.IO
{
    public static class PathUtils
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


        public static string GetUniquePath(string basePath, bool isDirectory = false, string delimiterStart = "[", string delimiterEnd = "]", int startCandidateNumber = 2)
        {
            // 対象パスが未使用なら、そのまま返す
            if ((!isDirectory && !File.Exists(basePath) && !Directory.Exists(basePath)) || (isDirectory && !Directory.Exists(basePath) && !File.Exists(basePath)))
            {
                return basePath;
            }

            if (!isDirectory)
            {
                // ファイルの場合
                string dir = Path.GetDirectoryName(basePath) ?? "";
                string nameWithoutExt = Path.GetFileNameWithoutExtension(basePath);
                string extension = Path.GetExtension(basePath);

                for (int candidateNum = startCandidateNumber; ; candidateNum++)
                {
                    string candidate = Path.Combine(dir, $"{nameWithoutExt}{delimiterStart}{candidateNum}{delimiterEnd}{extension}");
                    if (!File.Exists(candidate) && !Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }
            else
            {
                // ディレクトリの場合
                for (int candidateNum = startCandidateNumber; ; candidateNum++)
                {
                    string candidate = $"{basePath}{delimiterStart}{candidateNum}{delimiterEnd}";
                    if (!File.Exists(candidate) && !Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }
    }
}
