namespace MikouTools
{
    public static class FileTools
    {
        public static void StringFileSave(string Path, string FileSaveData, bool Overwrite = true)
        {
            if (!Overwrite && System.IO.File.Exists(Path)) return;

            string? dirpath = System.IO.Path.GetDirectoryName(Path);
            if (dirpath == null) return;
            DirectoryTools.CreateDirectory(dirpath, false);

            using (StreamWriter writer = new StreamWriter(Path, false, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(FileSaveData);
                writer.Close();
                writer.Dispose();
            }
        }
        public static string? StringFileLoad(string Path)
        {
            if (System.IO.File.Exists(Path))
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(Path, System.Text.Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
            else
            {
                return null;
            }
        }
        public static string StringFileLoad(string Path, string NullReturnValue = "")
        {
            if (System.IO.File.Exists(Path))
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(Path, System.Text.Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
            else
            {
                return NullReturnValue;
            }
        }
        public static string AvoidPathDuplication(string Path, string Bracket_1 = " [", string Bracket_2 = "]")
        {
            return PathTools.AvoidPathDuplication(Path, false, Bracket_1, Bracket_2);
        }

        public static bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return (!fileInfo.Exists || fileInfo.Length == 0);
        }
        public async static Task FileUnlockWait(string Path, int timeout = 10)
        {
            int count = 0;
            while (count < timeout * 2)
            {
                if (!IsFileLocked(Path))
                {
                    return;
                }
                await Task.Delay(500);
                //Thread.Sleep(500);
                count++;
            }
            throw new Exception("FileLock");
        }
        public static bool IsFileLocked(string path)
        {
            FileStream? stream = null;

            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            return false;
        }
    }



}
