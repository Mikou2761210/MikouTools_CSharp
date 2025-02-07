using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.AppTools
{
    public class LanguageManager : INotifyPropertyChanged
    {
        public string LangDirectoryPath { get; private set; }

        public string MasterLanguageCode { get; private set; }
        private Dictionary<string, int> _keytoIndex = new Dictionary<string, int>();
        private string[] _masterLanguageResources;

        public string CurrentLanguageCode { get; private set; }
        private string?[] _currentLanguageResources;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LanguageManager(string _LangDirectoryPath, string _MasterLanguageCode, string[] MasterKeys, string[] MasterLanguageResources)
        {
            if (!Directory.Exists(_LangDirectoryPath)) throw new DirectoryNotFoundException($"DirectoryNotFound {_LangDirectoryPath}");
            LangDirectoryPath = _LangDirectoryPath;
            MasterLanguageCode = _MasterLanguageCode;
            _masterLanguageResources = MasterLanguageResources;
            for (int i = 0; i < MasterKeys.Length; i++)
            {
                _keytoIndex.Add(MasterKeys[i], i);
            }
            _currentLanguageResources = new string[MasterKeys.Length];
            ChangeLanguage(_MasterLanguageCode);
            if (CurrentLanguageCode == null) throw new NullReferenceException();
        }


        public string this[string Key]
        {
            get
            {
                if (_keytoIndex.TryGetValue(Key, out int index))
                {
                    return _currentLanguageResources[index] ?? _masterLanguageResources[index];
                }
                return string.Empty;
            }
        }
        public string GetMasterLanguageResource(string Key)
        {
            if (_keytoIndex.TryGetValue(Key, out int index))
            {
                return _masterLanguageResources[index];
            }
            return string.Empty;
        }

        public string? GetCurrentLanguageResource(string Key)
        {
            if (_keytoIndex.TryGetValue(Key, out int index))
            {
                return _currentLanguageResources[index];
            }
            return string.Empty;
        }

        public bool ChangeLanguage(string LangCode)
        {
            string filePath = Path.Combine(LangDirectoryPath, $"{LangCode}.lang");


            if (File.Exists(filePath))
            {
                CurrentLanguageCode = LangCode;
                string[] entries = File.ReadAllLines(filePath);
                foreach (string entry in entries)
                {
                    string[] key_value = entry.Split('=', 2);

                    if (key_value.Length >= 2 && _keytoIndex.TryGetValue(key_value[0], out int index))
                    {
                        _currentLanguageResources[index] = key_value[1];
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key_value[0]}]"));
                    }
                }

                return true;
            }
            else
            {
                CurrentLanguageCode = MasterLanguageCode;
                _currentLanguageResources = _masterLanguageResources.ToArray();
                foreach (string key in _keytoIndex.Keys)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
                }
                return false;
            }
        }

    }
}
