using System;
using System.Collections.Generic;

namespace ENV.Data.DataProvider
{
    class IsamFileManager<IsamFileType, IndentifierType>
    {
        public interface OpenedFile<T>
        {
            void Close();
            T File { get; }
        }

        Action<IsamFileType> _closeFile;

        public IsamFileManager(Action<IsamFileType> closeFile)
        {
            _closeFile = closeFile;
        }

        class InternalFile
        {
            IsamFileManager<IsamFileType, IndentifierType> _parent;
            IsamFileType _file;
            Stack<Node> _nodes = new Stack<Node>();
            IndentifierType _entityType;

            class Node
            {
                public int Priority;
                public Func<IsamFileType> Open;
            }

            public InternalFile(IsamFileManager<IsamFileType, IndentifierType> parent, IndentifierType entityType)
            {
                _parent = parent;
                _entityType = entityType;
            }

            public IsamFileType File
            {
                get { return _file; }
            }

            public OpenedFile<IsamFileType> Open(int priority, Func<IsamFileType> open)
            {
                if (_nodes.Count == 0 || _nodes.Peek().Priority < priority)
                {
                    if (_nodes.Count > 0)
                        _parent._closeFile(_file);
                    _file = open();
                    _nodes.Push(new Node { Priority = priority, Open = open });
                    return new myOpenedFile(this,
                        () =>
                        {
                            _parent._closeFile(_file);
                            _nodes.Pop();
                            if (_nodes.Count > 0)
                                _file = _nodes.Peek().Open();
                            else
                                _parent._files.Remove(_entityType);
                        });
                }
                return new myOpenedFile(this, () => { });
            }

            public class myOpenedFile : OpenedFile<IsamFileType>
            {
                InternalFile _parent;
                Action _close;

                public myOpenedFile(InternalFile parent, Action close)
                {
                    _parent = parent;
                    _close = close;
                }

                public void Close()
                {
                    _close();
                }
                public IsamFileType File { get { return _parent.File; } }
            }


        }

        Dictionary<IndentifierType, InternalFile> _files = new Dictionary<IndentifierType, InternalFile>();

        public OpenedFile<IsamFileType> OpenFile(IndentifierType entityType, int priority, Func<IsamFileType> open)
        {
            InternalFile f;
            if (!_files.TryGetValue(entityType, out f))
            {
                f = new InternalFile(this, entityType);
                _files.Add(entityType, f);
            }
            return f.Open(priority, open);
        }
    }
}
