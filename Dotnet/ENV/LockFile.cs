using System;
using System.Collections.Generic;
using System.IO;

namespace ENV
{
    public static class LockFile
    {
        static string _path;
        public static string Path { set { _path = value; } get { return PathDecoder.DecodePath(_path); } }

        public static bool IsByteLocked(long position)
        {
            return _lockedBytes.Contains(position);
        }
        static HashSet<long> _lockedBytes = new HashSet<long>();
        static FileStream _stream;
        static FileStream Stream
        {
            get
            {
                if (!File.Exists(Path))
                    File.Create(Path).Dispose();
                if (_stream == null)
                    _stream = new FileStream(Path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                return _stream;
            }
        }
        public static bool LockByte(long position)
        {
            try
            {
                Stream.Lock(position, 1);
                _lockedBytes.Add(position);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void UnlockByte(long position)
        {
            try
            {
                Stream.Unlock(position, 1);
                _lockedBytes.Remove(position);
            }
            catch (Exception)
            {
            }
        }
    }
}
