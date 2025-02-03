using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools
{

    public static class DirectoryTools
    {
        public static bool CreateDirectory(string Path, bool Overwrite = true, bool Hidden = false)
        {
            if (!Overwrite && Directory.Exists(Path)) return false;

            string? dirpath = System.IO.Path.GetDirectoryName(Path);
            if (dirpath == null) { return false; }
            if (!Directory.Exists(dirpath))
            {
                CreateDirectory(dirpath, false);
            }


            Directory.CreateDirectory(Path);
            if (Hidden)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Path);
                di.Attributes |= System.IO.FileAttributes.Hidden;
            }
            return true;
        }

        public static void DeleteDirectory(string targetDirectoryPath)
        {
            if (Directory.Exists(targetDirectoryPath))
            {
                string[] filePaths = Directory.GetFiles(targetDirectoryPath);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }

                string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
                foreach (string directoryPath in directoryPaths)
                {
                    DeleteDirectory(directoryPath);
                }
            }

            Directory.Delete(targetDirectoryPath, false);
        }

        public static string AvoidPathDuplication(string Path, string Bracket_1 = " [", string Bracket_2 = "]")
        {
            return PathTools.AvoidPathDuplication(Path, true, Bracket_1, Bracket_2);
        }
    }

}
