namespace MikouTools.IO
{
    public static class FileUtils
    {
        public static void StringFileSave(string path, string fileSaveData, bool overwrite = true)
        {
            if (!overwrite && System.IO.File.Exists(path)) return;

            string? dirpath = System.IO.Path.GetDirectoryName(path);
            if (dirpath == null) return;
            DirectoryUtils.CreateDirectory(dirpath, false);

            using (StreamWriter writer = new(path, false, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(fileSaveData);
                writer.Close();
                writer.Dispose();
            }
        }
        
        
        /// <summary>
        /// Reads the entire content of a file and returns it as a string.
        /// </summary>
        /// <param name="path">The file path to read the content from.</param>
        /// <returns>
        /// The content of the file if it exists; otherwise, null if the file does not exist.
        /// </returns>
        public static string? StringFileLoad(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    using (StreamReader streamReader = new StreamReader(path, System.Text.Encoding.UTF8))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Reads the content of a file into a string.
        /// </summary>
        /// <param name="path">The file path to read from.</param>
        /// <param name="nullResult">The value to return if the file does not exist or an error occurs. Default is an empty string.</param>
        /// <param name="encoding">The encoding used to read the file. Default is UTF-8.</param>
        /// <returns>The content of the file or the specified nullResult.</returns>
        public static string StringFileLoad(string path, string nullResult = "", System.Text.Encoding? encoding = null)
        {
            encoding ??= System.Text.Encoding.UTF8;

            try
            {
                if (System.IO.File.Exists(path))
                {
                    using (StreamReader streamReader = new StreamReader(path, encoding))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
                else
                {
                    return nullResult;
                }
            }
            catch (Exception)
            {
                return nullResult;
            }
        }
        public static string GetUniquePath(string basePath, string delimiterStart = "[", string delimiterEnd = "]", int startCandidateNumber = 2)
        {
            return IO.PathUtils.GetUniquePath(basePath, false, delimiterStart, delimiterEnd, startCandidateNumber);
        }

        public static bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return !fileInfo.Exists || fileInfo.Length == 0;
        }


        #region FileLock
        /// <summary>
        /// Waits for the file to be unlocked within a given timeout period.
        /// </summary>
        /// <param name="path">The path of the file to check for lock status.</param>
        /// <param name="interval">The time interval (in milliseconds) between each check for the file lock status. Default is 500 milliseconds.</param>
        /// <param name="timeout">The maximum time to wait (in milliseconds) for the file to be unlocked. Default is 5000 milliseconds.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="TimeoutException">Thrown if the file is not unlocked within the specified timeout period.</exception>
        public async static Task WaitForFileUnlock(string path,int interval = 500, int timeout = 5000)
        {
            // 秒をミリ秒に変換して、最大試行回数を計算
            int maxAttempts = timeout / interval;

            for (int count = 0; count < maxAttempts; count++)
            {
                if (!IsFileLocked(path))
                {
                    return;
                }
                await Task.Delay(interval);
            }

            throw new TimeoutException("File unlock timed out.");
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
        #endregion

        #region SaveLoadBytes
        public static void SaveBytes(string path, byte[] data , FileOptions fileOptions = FileOptions.None)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, fileOptions);
            fs.Write(data, 0, data.Length);
        }

        public static byte[] LoadBytes(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static bool TrySaveBytes(string path, byte[] data, FileOptions fileOptions = FileOptions.None)
        {
            try
            {
                SaveBytes(path, data, fileOptions);
                return true;
            }
            catch (Exception) {}
            return false;
        }
        public static bool TryLoadBytes(string path, out byte[]? value)
        {
            try
            {
                value = LoadBytes(path);
                return true;
            }
            catch (Exception) { }
            value = null;
            return false;
        }
        #endregion
    }



}
