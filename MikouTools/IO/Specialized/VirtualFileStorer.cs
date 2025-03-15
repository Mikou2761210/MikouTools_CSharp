using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.IO.Specialized
{
    public class VirtualFileStorer
    {
        readonly string _targetStorerDir;
        public string TargetStorerDir => _targetStorerDir;
        protected readonly Dictionary<string, string> _actualNameToVirtualName = [];

        public IReadOnlyDictionary<string, string> ActualNameToVirtualName => _actualNameToVirtualName;

        public VirtualFileStorer(string targetStorerDir, Dictionary<string, string>? initalData)
        {
            if(!Directory.Exists(targetStorerDir)) throw new ArgumentException("The specified directory does not exist.");
            _targetStorerDir = targetStorerDir;
            foreach (var itemPath in Directory.GetFiles(targetStorerDir))
            {
                string fileName = Path.GetFileName(itemPath);   
                if (initalData?.ContainsKey(fileName) ?? false)
                {
                    _actualNameToVirtualName.Add(fileName, initalData[fileName]);
                }
            }
        }

        public bool TryAddFile(string baseFilePath, string actualName, string virtualName, bool isCopy = false)
        {
            // actualName が既に存在しているかどうかで重複チェックする
            if (!_actualNameToVirtualName.ContainsKey(actualName) && File.Exists(baseFilePath) && Path.Combine(_targetStorerDir, actualName) is string destFilePath && !File.Exists(destFilePath))
            {
                try
                {
                    if (isCopy)
                        File.Copy(baseFilePath, destFilePath);
                    else
                        File.Move(baseFilePath, destFilePath);

                    _actualNameToVirtualName.Add(actualName, virtualName);
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
        public bool TryRemoveFile(string actualName)
        {
            if (_actualNameToVirtualName.ContainsKey(actualName) && Path.Combine(_targetStorerDir, actualName) is string removeFile && File.Exists(removeFile))
            {
                try
                {
                    File.Delete(removeFile);
                    _actualNameToVirtualName.Remove(actualName);
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
        public bool TryMoveFile(string actualName, string destFilePath, bool isCopy = false)
        {
            if (_actualNameToVirtualName.ContainsKey(actualName) && !File.Exists(destFilePath) && Path.Combine(_targetStorerDir, actualName) is string moveFile && File.Exists(moveFile))
            {
                try
                {
                    if (isCopy) File.Copy(moveFile, destFilePath);
                    else
                    {
                        File.Move(moveFile, destFilePath);
                        _actualNameToVirtualName.Remove(actualName);
                    }
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
        public bool TryActualToVirtual(string actualName, [MaybeNullWhen(false)] out string virtualName)
        {
            return _actualNameToVirtualName.TryGetValue(actualName,out virtualName);
        }

    }
}
