using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.IO.Specialized
{
    public class DirectoryResilienceWatcher : IDisposable
    {
        private readonly FileSystemWatcher fileSystemWatcher;
        private readonly string monitoredDirectoryPath;
        private readonly bool recreateOnRename;

        /// <summary>
        /// ディレクトリが再作成またはリネーム復元されたときに発火されます。
        /// true: 再作成された / false: 元に戻された
        /// </summary>
        public event Action<bool>? DirectoryRestored;

        public DirectoryResilienceWatcher(string directoryPath, bool recreateIfRenamed = true)
        {
            monitoredDirectoryPath = Path.GetFullPath(directoryPath);
            recreateOnRename = recreateIfRenamed;

            if (!Directory.Exists(monitoredDirectoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {monitoredDirectoryPath}");

            var parentDirectory = Path.GetDirectoryName(monitoredDirectoryPath);
            if (string.IsNullOrEmpty(parentDirectory))
                throw new InvalidOperationException("Cannot watch the root directory.");

            fileSystemWatcher = new FileSystemWatcher(parentDirectory)
            {
                IncludeSubdirectories = false,
                Filter = Path.GetFileName(monitoredDirectoryPath),
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName
            };

            fileSystemWatcher.Deleted += HandleDirectoryDeleted;
            fileSystemWatcher.Renamed += HandleDirectoryRenamed;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
        ~DirectoryResilienceWatcher()
        {
            Dispose();
        }

        private void HandleDirectoryDeleted(object? sender, FileSystemEventArgs e)
        {
            if (string.Equals(e.FullPath, monitoredDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Directory deleted. Recreating: {monitoredDirectoryPath}");
                Directory.CreateDirectory(monitoredDirectoryPath);
                DirectoryRestored?.Invoke(true);
            }
        }

        private void HandleDirectoryRenamed(object? sender, RenamedEventArgs e)
        {
            if (string.Equals(e.OldFullPath, monitoredDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Directory renamed. Handling recovery: {monitoredDirectoryPath}");

                if (recreateOnRename)
                {
                    Directory.CreateDirectory(monitoredDirectoryPath);
                    DirectoryRestored?.Invoke(true);
                }
                else
                {
                    Directory.Move(e.FullPath, e.OldFullPath);
                    DirectoryRestored?.Invoke(false);
                }
            }
        }

        bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            fileSystemWatcher.Deleted -= HandleDirectoryDeleted;
            fileSystemWatcher.Renamed -= HandleDirectoryRenamed;
            fileSystemWatcher.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
