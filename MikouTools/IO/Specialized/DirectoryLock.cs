using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.IO.Specialized
{
    public class DirectoryLock : IDisposable
    {
        private readonly FileStream lockStream;
        public string LockFilePath { get; }
        private readonly bool deleteOnDispose;

        public DirectoryLock(string directoryPath, string lockFileName = "lock.tmp", bool useDeleteOnClose = true)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            LockFilePath = Path.Combine(directoryPath, lockFileName);

            var fileOptions = useDeleteOnClose && OperatingSystem.IsWindows()
                ? FileOptions.DeleteOnClose
                : FileOptions.None;

            lockStream = new FileStream(
                LockFilePath,
                FileMode.CreateNew,
                FileAccess.Read,
                FileShare.None,
                4096,
                fileOptions);


            if(OperatingSystem.IsWindows())
                File.SetAttributes(LockFilePath, FileAttributes.Hidden);

            deleteOnDispose = !OperatingSystem.IsWindows() || !useDeleteOnClose;
        }
        ~DirectoryLock()
        {
            Dispose();
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                lockStream.Dispose();

                if (deleteOnDispose && File.Exists(LockFilePath))
                {
                    File.Delete(LockFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DirectoryLock] Dispose failed: {ex.Message}");
            }

            GC.SuppressFinalize(this);
        }
    }
}