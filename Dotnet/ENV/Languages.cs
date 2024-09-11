using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ENV
{
    public class Languages
    {
        static Dictionary<string, Func<Dictionary<string, string>>> _languages = new Dictionary<string, Func<Dictionary<string, string>>>();
        string _currentLanguage;
        Func<Dictionary<string, string>> _currentDictionary = () => new Dictionary<string, string>();

        static Firefly.Box.ContextAndSharedValue<Languages> _current =
            new Firefly.Box.ContextAndSharedValue<Languages>(new Languages(""));
        Languages(string name)
        {
            _currentLanguage = name;
            _currentDictionary = () =>
            {

                var result = GetLanguage(name);
                _currentDictionary = () => result;
                return result;
            };
        }
        public static void SetSharedLanguage(string value)
        {
            _current.SetSharedValue(new Languages(value));
        }

        public static string ContextCurrentLanguage
        {
            get { return _current.Value._currentLanguage; }
            set
            {
                _current.SetContextValue(new Languages(value));
                ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());

            }
        }

       

        public static Dictionary<string, string> GetLanguage(string languageKey)
        {
            languageKey = languageKey.Trim();
            if (_languages.ContainsKey(languageKey))
                return _languages[languageKey]();
            else
            {
                lock (_languages)
                {
                    if (_languages.ContainsKey(languageKey))
                        return _languages[languageKey]();
                    else
                    {
                        var d = new Dictionary<string, string>();
                        _languages.Add(languageKey, () => d);
                        return _languages[languageKey]();
                    }
                }
            }
        }
        public static void LoadFromFile(string languageKey, string path)
        {
            LoadFromFile(languageKey, path, ENV.LocalizationInfo.Current.InnerEncoding);
        }

        public static void LoadFromFile(string languageKey, string path, Encoding e)
        {
            if (_languages.ContainsKey(languageKey))
                _languages.Remove(languageKey);
            _languages.Add(languageKey, () =>
            {
                var translation = new Dictionary<string, string>();
                path = ENV.PathDecoder.DecodePath(path);
                if (System.IO.File.Exists(path))
                {
                    lock (path)
                    {
                        using (StreamReader reader = new StreamReader(path, e))
                        {
                            while (reader.Peek() >= 0)
                            {
                                string key = reader.ReadLine().TrimEnd(' '),
                                       value = reader.ReadLine();
                                if (!string.IsNullOrEmpty(key)&&!string.IsNullOrEmpty(value))
                                    if (!translation.ContainsKey(key))
                                        translation.Add(key, value);
                                    else
                                        translation[key] = value;
                            }
                        }
                    }
                }
                _languages[languageKey] = () => translation;
                return translation;

            });
        }

        public static string Translate(string text)
        {
            if (text == null)
                return null;
            string result;
            var trimEnd = text.TrimEnd(' ');
            if (_current.Value._currentDictionary().TryGetValue(trimEnd, out result))
            {
                if (ENV.UserSettings.Version10Compatible)
                    return result + new string(' ', text.Length - trimEnd.Length);
                else
                    return result;
            }
            {
                if (ENV.UserSettings.Version10Compatible)
                    return text;
                else
                    return trimEnd;
            }
        }

        public static void Clear()
        {
            _languages.Clear();
            _current = new Firefly.Box.ContextAndSharedValue<Languages>(new Languages(""));
        }

    
        internal class menuItemInfo
        {
            public string Text, ToolTipText;
        }
        static Dictionary<ToolStripItem, menuItemInfo> _originalMenuName = new Dictionary<ToolStripItem, menuItemInfo>();
      
     
    }
}