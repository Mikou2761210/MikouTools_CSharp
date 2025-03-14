namespace MikouTools.IO
{

    public static class DirectoryUtils
    {
        public static bool CreateDirectory(string Path, bool Overwrite = true, bool Hidden = false)
        {
            if (!Overwrite && System.IO.Directory.Exists(Path)) return false;

            string? dirpath = System.IO.Path.GetDirectoryName(Path);
            if (dirpath == null) { return false; }
            if (!System.IO.Directory.Exists(dirpath))
            {
                CreateDirectory(dirpath, false);
            }


            System.IO.Directory.CreateDirectory(Path);
            if (Hidden)
            {
                DirectoryInfo di = new DirectoryInfo(Path);
                di.Attributes |= FileAttributes.Hidden;
            }
            return true;
        }

        public static void DeleteDirectory(string targetDirectoryPath)
        {
            if (System.IO.Directory.Exists(targetDirectoryPath))
            {
                string[] filePaths = System.IO.Directory.GetFiles(targetDirectoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
                    System.IO.File.Delete(filePath);
                }

                string[] directoryPaths = System.IO.Directory.GetDirectories(targetDirectoryPath);
                foreach (string directoryPath in directoryPaths)
                {
                    DeleteDirectory(directoryPath);
                }
            }

            System.IO.Directory.Delete(targetDirectoryPath, false);
        }

        public static string GetUniquePath(string basePath, string delimiterStart = "[", string delimiterEnd = "]", int startCandidateNumber = 2)
        {
            return IO.PathUtils.GetUniquePath(basePath, true, delimiterStart, delimiterEnd, startCandidateNumber);
        }
    }

}
