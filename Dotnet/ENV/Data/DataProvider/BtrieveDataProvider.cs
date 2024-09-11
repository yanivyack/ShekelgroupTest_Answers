using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    public class BtrieveDataProvider : IEntityDataProvider
    {
        [DllImport("wbtrv32.dll", EntryPoint = "BTRCALLID", CharSet = CharSet.None, SetLastError = false, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        static extern short ___BTRCALL(
            [MarshalAs(UnmanagedType.I2)] short opCode, byte[] positionBlock, byte[] dataBuffer,
           [MarshalAs(UnmanagedType.U2)] ref ushort dataBufferLength,
                                       byte[] keyBuffer, [MarshalAs(UnmanagedType.I2)] short keyBufferLength, [MarshalAs(UnmanagedType.I2)] short keyNumber, byte[] clientId);

        static short _BtrCall(short opCode, byte[] positionBlock, byte[] dataBuffer, out ushort dataBufferLength, byte[] keyBuffer, short keyNumber, short[] dontThrowOnTheseErrorCodes, bool ignoreError20, byte[] cid, byte[] errorPosition)
        {
            var dbl = (ushort)dataBuffer.Length;
            byte keyBufferLength = (byte)keyBuffer.Length;
            short result;

            var clientId = new byte[16];
            Array.Copy(new byte[] { 0x41, 0x42 }, 0, clientId, 12, 2);
            Array.Copy(cid, 0, clientId, 14, 2);
            result = ___BTRCALL((short)opCode, positionBlock, dataBuffer, ref dbl, keyBuffer, keyBufferLength, keyNumber, clientId);
            if (result != 0 && System.Array.IndexOf(dontThrowOnTheseErrorCodes, result) == -1 && !(result == 20 && ignoreError20))
            {
                var errorPos = errorPosition;
                if (errorPos == null)
                {
                    errorPos = new byte[4];
                    dbl = 4;
                    if (___BTRCALL((short)Operations.GetPosition, positionBlock, errorPosition, ref dbl, new byte[0], 0, 8, clientId) != 0)
                        errorPos = null;
                }
                var be = new BtrieveException(result, errorPos != null ? BitConverter.ToInt32(errorPos, 0) : 0);
                if (result == 5)
                    throw new DatabaseErrorException(DatabaseErrorType.DuplicateIndex, be);
                if (result == 84)
                    throw new DatabaseErrorException(DatabaseErrorType.LockedRow, be);
                if (result == 43)
                    throw new DatabaseErrorException(DatabaseErrorType.RowDoesNotExist, be, DatabaseErrorHandlingStrategy.Rollback);
                if (result == 11) // invalid file name
                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, be, DatabaseErrorHandlingStrategy.AbortAllTasks);
                if (result == 30) // not a btrieve file
                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, be, DatabaseErrorHandlingStrategy.AbortAllTasks);
                if (result == 35) // invalid directory
                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, be, DatabaseErrorHandlingStrategy.AbortAllTasks);
                if (result == 85 || result == 88) // file locked
                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, be, DatabaseErrorHandlingStrategy.Retry);
                if (result == 161) // out of licenses
                {
                    var e = new DatabaseErrorException(DatabaseErrorType.UnknownError, be, DatabaseErrorHandlingStrategy.AbortAllTasks);
                    Common.ShowExceptionDialog(e, true, "Out of Btrieve/Pervasive Licenses");
                    throw e;
                }
                throw new DatabaseErrorException(DatabaseErrorType.UnknownError, be);
            }
            dataBufferLength = dbl;
            return result;
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return false;
            }
        }
        enum Operations
        {
            Open = 0,
            Create = 14,
            Close = 1,
            Insert = 2,
            GetPreviousExtended = 37,
            GetNextExtended = 36,
            GetLast = 13,
            GetFirst = 12,
            GetLessOrEqual = 11,
            GetGreaterOrEqual = 9,
            GetPosition = 22,
            Delete = 4,
            Update = 3,
            UpdateChunk = 53,
            GetDirect = 23,
            StepFirst = 33,
            StepLast = 34,
            StepNextExtended = 38,
            StepPreviousExtended = 39,
            Unlock = 27,
            Stat = 15,
            SetOwner = 29,
            DropIndex = 32,
            CreateIndex = 31,
            GetNext = 6,
            GetPrevious = 7,
            StepNext = 24,
            StepPrevious = 35,
            BeginTransaction = 1519,
            EndTransaction = 20,
            AbortTransaction = 21,
            Reset = 28
        }

        enum OpenMode
        {
            Normal = 0,
            ReadOnly = -2,
            Exclusive = -4
        }

        const int MAX_COMM_BUFFER_SIZE = 15000;


        class BtrieveThreadInfo : IDisposable
        {
            public Dictionary<Type, FilesByTypes> _openFiles = new Dictionary<Type, FilesByTypes>();
            public readonly short CID;
            public myTrasnaction _activeTransaction;
            static bool[] _ids = new bool[] { false };

            public BtrieveThreadInfo(short cid)
            {
                CID = cid;
            }
            public BtrieveThreadInfo()
            {
                lock (_ids.SyncRoot)
                {
                    short i = 1;
                    for (; i < _ids.Length; i++)
                    {
                        if (!_ids[i])
                        {
                            _ids[i] = true;
                            CID = i;
                            return;
                        }
                    }
                    Array.Resize(ref _ids, _ids.Length + 1);
                    _ids[i] = true;
                    CID = i;
                }
            }

            public void Dispose()
            {
                _ids[CID] = false;
            }
        }

        class FilesByTypes
        {
            public OpenedBtrieveFile File = null;
        }

        internal bool ForTestingInstanceOpenFiles;
        Dictionary<Type, FilesByTypes> _openFiles = new Dictionary<Type, FilesByTypes>();

        BtrieveThreadInfo GetThreadInfoDoNotUseMeUseTheInstanceItem()
        {
            var cid = Context.Current["BtrieveClientId"] as BtrieveThreadInfo;
            if (cid == null)
            {
                cid = new BtrieveThreadInfo();
                Context.Current["BtrieveClientId"] = cid;
            }
            var ti = cid;
            if (ForTestingInstanceOpenFiles)
                return new BtrieveThreadInfo(ti.CID) { _openFiles = this._openFiles };
            return ti;
        }

        internal static short GetKeyFlags(Sort index, SortSegment segment, int segmentNumber)
        {
            var keyFlags = (short)((index.Unique ? 0 : 1) | 0x2 | 0x100);
            if (segmentNumber < index.Segments.Count) keyFlags |= 0x10;
            if (segment.Direction == SortDirection.Descending) keyFlags |= 0x40;
            return keyFlags;
        }

        class OpenedBtrieveFile
        {
            byte[] _positionBlock = new byte[128];
            Dictionary<string, ColumnDef> _columnDefsByName = new Dictionary<string, ColumnDef>();
            ColumnDef[] _columnDefsList;
            int _positionColumnIndex = -1;

            BtrieveDataProvider _parent;
            Firefly.Box.Data.Entity _entity;
            string _filePath;

            Stack<OpenClass> _openStack = new Stack<OpenClass>();

            public OpenedBtrieveFile(BtrieveDataProvider parent, Firefly.Box.Data.Entity entity, string filePath, BtrieveThreadInfo btrieveThreadInfo)
            {
                _btrieveThreadInfo = btrieveThreadInfo;
                _cid = BitConverter.GetBytes(_btrieveThreadInfo.CID);
                _parent = parent;
                _entity = entity;
                _filePath = filePath;
            }

            class OpenClass
            {
                OpenMode _openMode;
                Action<OpenMode> _open;
                Action _close;
                int _openCount = 0;

                public OpenClass(bool readOnly, BtrieveFileSharing sharing, BtrieveOpenMode openMode, Action<OpenMode> open, Action close)
                {
                    _openMode = sharing == BtrieveFileSharing.None || openMode == BtrieveOpenMode.Damaged ||
                                openMode == BtrieveOpenMode.Reindex ? OpenMode.Exclusive : (readOnly ? OpenMode.ReadOnly : OpenMode.Normal);
                    _open = open;
                    _close = close;
                }

                public void Open(OpenClass prev, Action addToOpenStack)
                {
                    Func<OpenMode, int> getOpenModePrecedence = x => x == OpenMode.Exclusive ? 3 : (x == OpenMode.Normal ? 2 : 1);
                    if (prev == null || (getOpenModePrecedence(prev._openMode) < getOpenModePrecedence(_openMode)))
                    {
                        if (prev != null) _close();
                        try
                        {
                            _open(_openMode);
                        }
                        catch (Exception)
                        {
                            if (prev != null) prev.Reopen();
                            throw;
                        }
                        _openCount++;
                        addToOpenStack();
                    }
                    else prev._openCount++;
                }

                public void Close(Action removeFromOpenStack)
                {
                    _openCount--;
                    if (_openCount > 0) return;
                    _close();
                    removeFromOpenStack();
                }

                public void Reopen()
                {
                    _open(_openMode);
                }
            }


            int _firstVariableColumnDefIndex;

            public void Open(Firefly.Box.Data.Entity e)
            {
                if (_openStack.Count == 0)
                {
                    _columnDefsByName = new Dictionary<string, ColumnDef>();
                    var columnDefsList = new List<ColumnDef>();

                    var addVariableSizeColumns = new List<Action>();

                    Action<ColumnDef> addColumn =
                        cd =>
                        {
                            _columnDefsByName.Add(cd.Name, cd);
                            columnDefsList.Add(cd);
                        };

                    var columnIndex = 0;
                    var longByteArray = new byte[24];

                    var minRowLength = 0;
                    var maxRowLength = 0;

                    foreach (var column in _entity.Columns)
                    {
                        var isVariableSizeColumn = false;
                        var isSizePrefix = false;
                        var sizePrefixType = 0;
                        var ds = new BinaryValueSaver();
                        {
                            var typedColumn = column as Firefly.Box.Data.NumberColumn;
                            if (typedColumn != null)
                            {
                                if (typedColumn is BtrievePositionColumn)
                                {
                                    _positionColumnIndex = columnIndex++;
                                    continue;
                                }
                                typedColumn.Storage.SaveTo(0, ds);
                            }
                        }

                        ColumnSaver.SaveColumn(column, ds);

                        {
                            var typedColumn = column as Firefly.Box.Data.TextColumn;
                            if (!ReferenceEquals(typedColumn, null))
                            {
                                if (ds.ValueWasByteArray)
                                {
                                    isVariableSizeColumn = true;
                                    isSizePrefix = true;
                                    sizePrefixType = GetSizeByPrefix(ds.ValueByteArray, 0, 0) == ds.ValueByteArray.Length ? 0 : 1;
                                }
                            }
                        }
                        {
                            var typedColumn = column as Firefly.Box.Data.ByteArrayColumn;
                            if (typedColumn != null)
                            {
                                isVariableSizeColumn = true;
                            }
                        }

                        var cd = new ColumnDef(column.Name, columnIndex++, maxRowLength, ds.ValueByteArray.Length, ds.DataTypeCode)
                        {
                            VariableSize = isVariableSizeColumn,
                            SizePrefix = isSizePrefix,
                            SizePrefixType = sizePrefixType
                        };

                        if (isVariableSizeColumn)
                        {
                            addVariableSizeColumns.Add(
                                () =>
                                {
                                    cd.Offset = minRowLength;
                                    addColumn(cd);
                                    maxRowLength += cd.Size;
                                });
                        }
                        else
                        {
                            addColumn(cd);
                            maxRowLength += cd.Size;
                            minRowLength += cd.Size;
                        }
                    }

                    if (minRowLength < 5)
                    {
                        var filler = 5 - minRowLength;
                        columnDefsList.Add(new ColumnDef("", -1, minRowLength, filler, 0));
                        maxRowLength = minRowLength = 5;
                    }

                    _firstVariableColumnDefIndex = columnDefsList.Count;
                    addVariableSizeColumns.ForEach(action => action());

                    _columnDefsList = columnDefsList.ToArray();
                    _maxRowLength = maxRowLength;
                    _minRowLength = minRowLength;
                }

                var sharing = BtrieveFileSharing.Write;
                var openMode = BtrieveOpenMode.Normal;
                var owner = new byte[0];
                var encrypted = false;
                var be = e as BtrieveEntity;
                if (be != null)
                {
                    sharing = be.FileSharing;
                    openMode = be.OpenMode;
                    var ownerString = Security.Entities.SecuredValues.Decode(be.Owner);
                    if (!Text.IsNullOrEmpty(ownerString))
                    {
                        owner = LocalizationInfo.Current.OuterEncoding.GetBytes(ownerString.Substring(0, Math.Min(ownerString.Length, 9)) + "\0");
                        encrypted = be.Encrypted;
                    }
                }

                Action<Sort, int, ByteWriter> prepareKeyCreationData =
                    (index, indexNumber, k) =>
                    {
                        var i = 0;
                        var bSort = index as BtrieveSort;
                        foreach (SortSegment o in index.Segments)
                        {
                            k.Add(BitConverter.GetBytes((short)(_columnDefsByName[o.Column.Name].Offset + 1)), 2);
                            k.Add(BitConverter.GetBytes((short)(bSort != null ? bSort.GetModifiedSegmentSize(o.Column, _columnDefsByName[o.Column.Name].Size) : _columnDefsByName[o.Column.Name].Size)), 2);
                            var keyFlags = GetKeyFlags(index, o, ++i);
                            k.Add(BitConverter.GetBytes(keyFlags), 2);
                            k.AddEmpty(4);
                            k.Add(new[] { (byte)_columnDefsByName[o.Column.Name].DataTypeCode }, 1);
                            k.AddEmpty(1);
                            k.AddEmpty(2);
                            k.Add(new[] { (byte)indexNumber }, 1);
                            k.AddEmpty(1);
                        }
                    };
                _keysAllowed = openMode != BtrieveOpenMode.Reindex;
                var oc = new OpenClass(e.ReadOnly, sharing, openMode,
                    om =>
                    {
                        using (ProfileOpenFile(om))
                        {
                            if (_filePath.IndexOf(' ') != -1)
                            {
                                var shortNameChars = new char[1000];
                                var length = GetShortPathName(_filePath, shortNameChars, 1000);

                                if (length > 0)
                                    _filePath = new string(shortNameChars).Substring(0, (int)length);
                            }

                            var name = LocalizationInfo.Current.OuterEncoding.GetBytes(_filePath + "\0");
                            var newFileCreated = false;
                            var openResult = BtrCall(Operations.Open, owner, name, (short)om, 12, 88, 116);

                            if (openResult == 12)
                            {
                                var k = new ByteWriter();

                                var numOfKeys = 0;
                                if (openMode != BtrieveOpenMode.Reindex)
                                {
                                    foreach (var index in _entity.Indexes)
                                    {
                                        prepareKeyCreationData(index, numOfKeys, k);
                                        numOfKeys++;
                                    }
                                }

                                var w = new ByteWriter();
                                w.Add(BitConverter.GetBytes((short)_minRowLength), 2);
                                w.Add(BitConverter.GetBytes((short)4096), 2); // Page Size
                                w.Add(new[] { (byte)numOfKeys }, 1); // Number of Keys
                                w.AddEmpty(1); // File Version
                                w.AddEmpty(4); // Reserved
                                w.Add(
                                    BitConverter.GetBytes(
                                        (short)(0x400 | (_maxRowLength != _minRowLength || LastColumnIsBlob ? 0x1 : 0))),
                                    2); // File Flags
                                w.AddEmpty(1); // Number of Extra Pointers
                                w.AddEmpty(1); //Reserved
                                w.AddEmpty(2); // Preallocated Pages

                                w.Add(k.Result, k.Result.Length);

                                var createResult = BtrCall(Operations.Create, w.Result, name, -1, 59);
                                openResult = BtrCall(Operations.Open, new byte[0], name, (short)om, 12, 88);
                                if (createResult == 0 && openResult == 0)
                                {
                                    if (owner.Length > 0)
                                        BtrCall(Operations.SetOwner, owner, owner, (short)(encrypted ? 2 : 0));

                                    newFileCreated = true;
                                }
                            }

                            if (openResult == 116)
                                throw new DatabaseErrorException(DatabaseErrorType.UnknownError, new BtrieveException(116, 0));

                            if (openResult == 88)
                            {
                                if (om == OpenMode.Exclusive && RevertToNormalIfExclusiveFileOpenFails)
                                    BtrCall(Operations.Open, owner, name, (short)OpenMode.Normal);
                                else
                                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, new BtrieveException(88, 0), DatabaseErrorHandlingStrategy.Retry);
                            }

                            if (openMode == BtrieveOpenMode.Reindex && !newFileCreated)
                            {
                                for (var i = (_entity.Indexes.Count) - 1; i >= 0; i--)
                                    BtrCall(Operations.DropIndex, new byte[0], new byte[0], (short)i);
                            }

                            if (_parent.VerifyStructure && _minRowLength == _maxRowLength && !LastColumnIsBlob)
                            {
                                var dataBuffer = new byte[1920];
                                var keyBuffer = new byte[255];
                                BtrCall(Operations.Stat, dataBuffer, keyBuffer, 0);
                                if (BitConverter.ToInt16(dataBuffer, 0) != _minRowLength || BitConverter.ToInt16(dataBuffer, 4) != (openMode == BtrieveOpenMode.Reindex ? 0 : _entity.Indexes.Count))
                                {
                                    BtrCall(Operations.Close, new byte[0], new byte[0], 0);
                                    throw new DatabaseErrorException(DatabaseErrorType.UnknownError, new InvalidTableStructureException(),
                                                                     DatabaseErrorHandlingStrategy.AbortAllTasks);
                                }
                            }

                            if (_parent.FileOpened != null)
                                _parent.FileOpened(e);
                        }
                    }, () =>
                    {
                        using (ENV.Utilities.Profiler.StartContext("Close btrieve file "))
                        {
                            if (openMode == BtrieveOpenMode.Reindex)
                            {
                                for (int i = 0; i < _entity.Indexes.Count; i++)
                                {
                                    var k = new ByteWriter();
                                    prepareKeyCreationData(_entity.Indexes[i], i, k);
                                    BtrCall(Operations.CreateIndex, k.Result, new byte[0], 0);
                                }
                            }
                            BtrCall(Operations.Close, new byte[0], new byte[0], 0);
                        }
                    });

                oc.Open(_openStack.Count == 0 ? null : _openStack.Peek(), () => _openStack.Push(oc));


                lock (_btrieveThreadInfo)
                {
                    FilesByTypes f;
                    if (!_btrieveThreadInfo._openFiles.TryGetValue(_entity.GetType(), out f))
                        _btrieveThreadInfo._openFiles.Add(_entity.GetType(), new FilesByTypes { File = this });
                    else
                        f.File = this;
                }
            }






            IDisposable ProfileOpenFile(OpenMode om)
            {
                if (ENV.Utilities.Profiler.DoNotProfile())
                    return ENV.Utilities.Profiler.DummyDisposable.Instance;
                return ENV.Utilities.Profiler.StartContext("Open btrieve file " + om + " (" + _filePath + ")");
            }

            public short BtrCall(Operations opCode, byte[] dataBuffer, byte[] keyBuffer, short keyNumber, params short[] dontThrowOnTheseErrorCodes)
            {
                ushort dataBufferLength;
                return BtrCall(opCode, false, null, dataBuffer, out dataBufferLength, keyBuffer, keyNumber, dontThrowOnTheseErrorCodes);
            }

            public short BtrCall(Operations opCode, bool addLockBias, byte[] dataBuffer, byte[] keyBuffer, short keyNumber, params short[] dontThrowOnTheseErrorCodes)
            {
                ushort dataBufferLength;
                return BtrCall(opCode, addLockBias, null, dataBuffer, out dataBufferLength, keyBuffer, keyNumber, dontThrowOnTheseErrorCodes);
            }

            public short BtrCall(Operations opCode, bool addLockBias, byte[] errorPosition, byte[] dataBuffer, out ushort dataBufferLength, byte[] keyBuffer, short keyNumber,
                params short[] dontThrowOnTheseErrorCodes)
            {
                try
                {
                    var retries = 3;
                    short r;
                    do
                    {
                        r = _BtrCall((short)(opCode + (addLockBias ? 400 : 0)), _positionBlock, dataBuffer, out dataBufferLength, keyBuffer, keyNumber, dontThrowOnTheseErrorCodes, _btrieveCalled, _cid, errorPosition);
                    }
                    while (r == 20 && --retries > 0);

                    if (r == 0)
                    {
                        switch (opCode)
                        {
                            case Operations.GetPreviousExtended:
                            case Operations.GetNextExtended:
                            case Operations.GetNext:
                            case Operations.GetPrevious:
                            case Operations.GetLast:
                            case Operations.GetFirst:
                            case Operations.GetLessOrEqual:
                            case Operations.GetGreaterOrEqual:
                            case Operations.GetDirect:
                                _activeKeyNumber = keyNumber;
                                _lastKeyBuffer = keyBuffer;
                                break;

                            case Operations.StepFirst:
                            case Operations.StepLast:
                            case Operations.StepNext:
                            case Operations.StepPrevious:
                            case Operations.StepNextExtended:
                            case Operations.StepPreviousExtended:
                                _activeKeyNumber = -1;
                                break;
                        }
                    }
                    else _activeKeyNumber = -1;
                    return r;
                }
                finally
                {
                    if (opCode != Operations.GetPosition && opCode != Operations.Update && opCode != Operations.UpdateChunk && opCode != Operations.Unlock)
                        _btrieveCalled = true;
                }
            }

            byte[] _lastKeyBuffer = new byte[0];
            public byte[] LastKeyBuffer { get { return _lastKeyBuffer; } }
            bool _btrieveCalled = false;
            public void ResetBtrieveCalled()
            {
                _btrieveCalled = false;
            }

            public bool WasBtrieveCalled { get { return _btrieveCalled; } }

            int _maxRowLength = -1;
            public int GetMaxRowLength()
            {
                return _maxRowLength;
            }

            int _minRowLength;

            public int MinRowLength
            {
                get { return _minRowLength; }
            }

            public void Close()
            {
                _openStack.Peek().Close(
                    () =>
                    {
                        _openStack.Pop();

                        if (_openStack.Count > 0)
                            _openStack.Peek().Reopen();
                        else
                        {

                            lock (_btrieveThreadInfo)
                                _btrieveThreadInfo._openFiles[_entity.GetType()].File = null;
                        }
                    });
            }

            public int GetColumnSize(ColumnBase column)
            {
                return _columnDefsByName[column.Name].Size;
            }

            public int GetColumnOffset(ColumnBase column)
            {
                return _columnDefsByName[column.Name].Offset;
            }

            public int GetColumnDataTypeCode(ColumnBase column)
            {
                return _columnDefsByName[column.Name].DataTypeCode;
            }

            static int GetColumnSize(ColumnDef c, byte[] rawData, int offset)
            {
                return c.VariableSize ? (c.SizePrefix ? GetSizeByPrefix(rawData, offset, c.SizePrefixType) : rawData.Length - offset) : c.Size;
            }

            static int GetSizeByPrefix(byte[] data, int offset, int prefixType)
            {
                if (prefixType == 1)
                    return Math.Abs(BitConverter.ToInt16(new[] { data[offset + 1], data[offset] }, 0)) + 2;
                return BitConverter.ToInt16(data, offset) + 2;
            }

            public void FillColumnsRawData(byte[] position, byte[] rawData, SelectedColumns selectedColumns, IRowStorage c1, ColumnBase[] cols)
            {
                _valueLoader.SetData(rawData);

                for (int i = 0; i < selectedColumns.FixedLengthColumns.Count; i++)
                {
                    var c = selectedColumns.FixedLengthColumns[i];
                    if (c.IndexInEntity >= 0)
                        c1.SetValue(cols[c.IndexInEntity], _valueLoader.GetFor(c.DataTypeCode, c.Offset, c.Size));
                }

                if (selectedColumns.VariableLengthColumns.Count > 0)
                {
                    var offset = _minRowLength;
                    var j = _firstVariableColumnDefIndex;
                    for (; j < _columnDefsList.Length; j++)
                    {
                        var c = _columnDefsList[j];
                        if (!c.SizePrefix && j != _columnDefsList.Length - 1) break;
                        var columnSize = GetColumnSize(c, rawData, offset);
                        if (c.IndexInEntity >= 0 && selectedColumns.VariableLengthColumns.Contains(c.Name))
                            c1.SetValue(cols[c.IndexInEntity], _valueLoader.GetFor(c.DataTypeCode, offset, columnSize));
                        offset += columnSize;
                    }

                    if (j < _columnDefsList.Length)
                    {
                        var i = j;
                        var hasBlobsHeader = true;
                        if (rawData.Length < offset + 13)
                            hasBlobsHeader = false;
                        else
                        {
                            var numOfBlobs = BitConverter.ToInt32(rawData, offset);
                            if (numOfBlobs < 1 || numOfBlobs > _columnDefsList.Length - i)
                                hasBlobsHeader = false;
                            else
                            {
                                var os1 = offset;
                                for (; j < i + numOfBlobs; j++)
                                {
                                    var c = _columnDefsList[j];
                                    var os = offset + 4 + numOfBlobs * 9 + BitConverter.ToInt32(rawData, offset + 4 + numOfBlobs + (j - i) * 8);
                                    var columnSize = BitConverter.ToInt32(rawData, offset + 4 + numOfBlobs + (j - i) * 8 + 4);
                                    if (rawData[offset + 4 + (j - i)] == 1)
                                    {
                                        os = offset;
                                        columnSize = 0;
                                    }
                                    if (c.IndexInEntity >= 0 && selectedColumns.VariableLengthColumns.Contains(c.Name))
                                        c1.SetValue(cols[c.IndexInEntity], _valueLoader.GetFor(c.DataTypeCode, os, columnSize));
                                    os1 = os + columnSize;
                                }
                                offset = os1;
                            }
                        }

                        if (!hasBlobsHeader)
                        {
                            var c = _columnDefsList[j++];
                            var columnSize = GetColumnSize(c, rawData, offset);
                            if (c.IndexInEntity >= 0 && selectedColumns.VariableLengthColumns.Contains(c.Name))
                                c1.SetValue(cols[c.IndexInEntity], _valueLoader.GetFor(c.DataTypeCode, offset, columnSize));
                            offset += columnSize;
                        }

                        for (; j < _columnDefsList.Length; j++)
                        {
                            var c = _columnDefsList[j];
                            if (!c.SizePrefix && j != _columnDefsList.Length - 1) break;
                            var columnSize = GetColumnSize(c, rawData, offset);
                            if (c.IndexInEntity >= 0 && selectedColumns.VariableLengthColumns.Contains(c.Name))
                                c1.SetValue(cols[c.IndexInEntity], _valueLoader.GetFor(c.DataTypeCode, offset, columnSize));
                            offset += columnSize;
                        }
                    }
                }
                if (selectedColumns.PositionColumnSelected && _positionColumnIndex != -1)
                {
                    c1.SetValue(cols[_positionColumnIndex], _valueLoader.GetFor(position, 14, 0, 4));
                }
            }

            public byte[] FillRawDataWithColumns(byte[] rawData, Action<int, bool, IValueSaver> saveValueOfColumnByIndex, Action<byte[]> saveFirstChunk, bool includeBlob)
            {
                var offset = 0;
                var i = 0;

                var fixedPart = new byte[_minRowLength];
                Array.Copy(rawData, 0, fixedPart, 0, Math.Min(rawData.Length, fixedPart.Length));

                var variablePart = new ByteWriter();

                for (; i < _columnDefsList.Length; i++)
                {
                    var c = _columnDefsList[i];
                    if (c.VariableSize && !c.SizePrefix) break;
                    var columnSize = GetColumnSize(c, rawData, offset);
                    var ds = new BinaryValueSaver();
                    if (c.IndexInEntity >= 0)
                        saveValueOfColumnByIndex(c.IndexInEntity, false, ds);
                    if (c.VariableSize)
                    {
                        if (ds.ValueByteArray != null)
                            variablePart.Add(ds.ValueByteArray);
                        else
                        {
                            var x = new byte[columnSize];
                            Array.Copy(rawData, offset, x, 0, columnSize);
                            variablePart.Add(x);
                        }
                    }
                    else if (ds.ValueByteArray != null)
                    {
                        Array.Clear(fixedPart, offset, c.Size);
                        Array.Copy(ds.ValueByteArray, 0, fixedPart, offset, Math.Min(ds.ValueByteArray.Length, c.Size));
                    }
                    offset += columnSize;
                }
                if (i < _columnDefsList.Length && includeBlob)
                {
                    if (i == _columnDefsList.Length - 1)
                    {
                        var c = _columnDefsList[i];
                        var ds = new BinaryValueSaver();
                        saveValueOfColumnByIndex(c.IndexInEntity, true, ds);
                        if (ds.ValueByteArray != null)
                            variablePart.Add(ds.ValueByteArray);
                    }
                    else
                    {
                        var emptyBlobsWriter = new ByteWriter();
                        var blobsPositionsWriter = new ByteWriter();
                        var blobsWriter = new ByteWriter();
                        for (var j = i; j < _columnDefsList.Length; j++)
                        {
                            var c = _columnDefsList[j];
                            var ds = new BinaryValueSaver();
                            saveValueOfColumnByIndex(c.IndexInEntity, true, ds);
                            if (ds.ValueByteArray != null && ds.ValueByteArray.Length > 0)
                            {
                                blobsPositionsWriter.AddInt(blobsWriter.Result.Length);
                                blobsPositionsWriter.AddInt(ds.ValueByteArray.Length);
                                emptyBlobsWriter.AddByte(0);
                                blobsWriter.Add(ds.ValueByteArray);
                            }
                            else
                            {
                                emptyBlobsWriter.AddByte(1);
                                blobsPositionsWriter.AddInt(0);
                                blobsPositionsWriter.AddInt(0);
                            }
                        }
                        variablePart.AddInt(_columnDefsList.Length - i);
                        variablePart.Add(emptyBlobsWriter.Result);
                        variablePart.Add(blobsPositionsWriter.Result);
                        variablePart.Add(blobsWriter.Result);
                    }
                }

                var r = fixedPart;
                if (variablePart.Result.Length > 0)
                {
                    r = new byte[fixedPart.Length + variablePart.Result.Length];
                    Array.Copy(fixedPart, r, fixedPart.Length);
                    Array.Copy(variablePart.Result, 0, r, fixedPart.Length, variablePart.Result.Length);
                }

                var maxFirstChunkSize = Math.Max(_minRowLength, MAX_COMM_BUFFER_SIZE);
                if (r.Length <= maxFirstChunkSize)
                    saveFirstChunk(r);
                else
                {
                    var x = new byte[maxFirstChunkSize];
                    Array.Copy(r, x, x.Length);
                    saveFirstChunk(x);
                    UpdateChunks(r, x.Length, new byte[0]);
                }
                return r;
            }

            public void UpdateChunks(byte[] r, int startAtOffset, byte[] keyBuffer)
            {
                var chunkOperationData = new ByteWriter();
                chunkOperationData.Add(BitConverter.GetBytes(0x80000000)); //Subfunction
                chunkOperationData.AddInt(1); // NumChunks
                chunkOperationData.AddEmpty(4); // Chunk Offset
                chunkOperationData.AddEmpty(4);
                chunkOperationData.AddEmpty(4); // User Data
                var o = startAtOffset;

                do
                {
                    var x = new byte[Math.Min(MAX_COMM_BUFFER_SIZE, r.Length - o + 20)];
                    Array.Copy(chunkOperationData.Result, x, 20);
                    Array.Copy(BitConverter.GetBytes(o), 0, x, 8, 4);
                    Array.Copy(BitConverter.GetBytes(x.Length - 20), 0, x, 12, 4);
                    Array.Copy(r, o, x, 20, x.Length - 20);
                    BtrCall(Operations.UpdateChunk, x, keyBuffer, 0xEF, 22);
                    o += x.Length - 20;
                } while (o < r.Length);
            }

            int _activeKeyNumber = -1;
            bool _keysAllowed;
            BtrieveThreadInfo _btrieveThreadInfo;
            byte[] _cid;
            public int ActiveKeyNumber { get { return _activeKeyNumber; } }
            public bool KeysAllowed { get { return _keysAllowed; } }

            public bool LastColumnIsBlob
            {
                get { return _columnDefsList[_columnDefsList.Length - 1].VariableSize && !_columnDefsList[_columnDefsList.Length - 1].SizePrefix; }
            }

            public bool PositionColumnSelected
            {
                get { return _positionColumnIndex != -1; }
            }

            public Func<BinaryValueSaver> GetColumnValue(byte[] rawRow, ColumnBase columnBase)
            {
                var cd = _columnDefsByName[columnBase.Name];
                if (cd.IndexInEntity < 0) return () => new BinaryValueSaver();
                var offset = cd.Offset;
                if (cd.VariableSize)
                {
                    offset = 0;
                    foreach (var columnDef in _columnDefsList)
                    {
                        if (columnDef.Name == cd.Name)
                            break;
                        offset += GetColumnSize(columnDef, rawRow, offset);
                    }
                }
                return () =>
                {
                    var ds = new BinaryValueSaver();
                    columnBase.LoadFrom(_valueLoader.GetFor(rawRow, cd.DataTypeCode, offset, GetColumnSize(cd, rawRow, offset))).SaveTo(ds);
                    return ds;
                };
            }

            BinaryValueLoader _valueLoader = new BinaryValueLoader();
            public void SendColumnDefinition(ColumnBase column, Action<int, int, int, bool> toMe)
            {
                var cd = _columnDefsByName[column.Name];
                toMe(cd.DataTypeCode, cd.Offset, cd.Size, cd.VariableSize);
            }

            public void PopulateSelectedColumns(SelectedColumns result, IEnumerable<ColumnBase> selectedColumns)
            {
                foreach (var c in selectedColumns)
                {
                    ColumnDef cd;
                    if (_columnDefsByName.TryGetValue(c.Name, out cd))
                    {

                        if (cd.VariableSize)
                            result.VariableLengthColumns.Add(cd.Name);
                        else
                            result.FixedLengthColumns.Add(cd);
                    }
                    else
                    {
                        if (c is BtrievePositionColumn)
                            result.PositionColumnSelected = true;
                    }
                }
            }

            public bool BlobSelected(SelectedColumns selectedColumns)
            {
                for (var i = _columnDefsList.Length - 1; i > 0; i--)
                {
                    var x = _columnDefsList[i];
                    if (!x.VariableSize || x.SizePrefix) return false;
                    if (selectedColumns.VariableLengthColumns.Contains(x.Name)) return true;
                }
                return false;
            }

            byte[] _lastAccessedRowPosition;
            byte[] _lastAccessedRowData;
            public void SetLastAccessedRowPositionAndData(byte[] position, byte[] rawData)
            {
                _lastAccessedRowPosition = position;
                _lastAccessedRowData = rawData;
            }

            public byte[] LastAccessedRowPosition { get { return _lastAccessedRowPosition; } }
            public byte[] LastAccessedRowData { get { return _lastAccessedRowData; } }
        }

        internal class ColumnSaver
        {
            static string _uncompressable32002CharsLongString;

            static ColumnSaver()
            {
                var sb = new StringBuilder(32000);
                for (int i = 0; i < 3200; i++)
                    sb.Append("1234567890");
                _uncompressable32002CharsLongString = sb + "12";
            }


            internal static void SaveColumn(ColumnBase column, BinaryValueSaver ds)
            {
                {
                    var typedColumn = column as Firefly.Box.Data.NumberColumn;
                    if (typedColumn != null)
                    {
                        typedColumn.Storage.SaveTo(0, ds);
                    }
                }
                {
                    var typedColumn = column as Firefly.Box.Data.TextColumn;
                    if (!ReferenceEquals(typedColumn, null))
                    {
                        var ml = typedColumn.FormatInfo.MaxLength;
                        typedColumn.Storage.SaveTo(
                            ml > 0 && ml <= 32002
                                ? _uncompressable32002CharsLongString.Substring(0, typedColumn.FormatInfo.MaxLength)
                                : _uncompressable32002CharsLongString, ds);
                    }
                }
                {
                    var typedColumn = column as Firefly.Box.Data.BoolColumn;
                    if (!ReferenceEquals(typedColumn, null))
                        typedColumn.Storage.SaveTo(false, ds);
                }
                {
                    var typedColumn = column as Firefly.Box.Data.DateColumn;
                    if (!ReferenceEquals(typedColumn, null))
                        typedColumn.Storage.SaveTo(Date.Empty, ds);
                }
                {
                    var typedColumn = column as Firefly.Box.Data.TimeColumn;
                    if (!ReferenceEquals(typedColumn, null))
                        typedColumn.Storage.SaveTo(Time.StartOfDay, ds);
                }
                {
                    var typedColumn = column as Firefly.Box.Data.ByteArrayColumn;
                    if (typedColumn != null)
                    {
                        ds.SaveByteArray(new byte[0]);
                    }
                }
                if (ds.ValueByteArray == null)
                {
                    column.SaveYourValueToDb(ds);
                }
            }

        }

        struct ColumnDef
        {
            public string Name;
            public int IndexInEntity;
            public int Offset;
            public int Size;
            public int DataTypeCode;
            public bool VariableSize;
            public bool SizePrefix;
            public int SizePrefixType; // 0 - normal, 1 - flipped

            public ColumnDef(string name, int indexInEntity, int offset, int size, int dataTypeCode)
            {
                Name = name;
                IndexInEntity = indexInEntity;
                Offset = offset;
                Size = size;
                DataTypeCode = dataTypeCode;
                VariableSize = false;
                SizePrefix = false;
                SizePrefixType = 0;
            }
        }

        public class ByteWriter
        {
            byte[] _bytes = new byte[0];
            public byte[] Result
            {
                get
                {
                    return _bytes;
                }
            }
            public void Add(byte[] data, int fixedLength)
            {
                var bArray = new byte[fixedLength];
                if (data != null)
                    Array.Copy(data, bArray, Math.Min(data.Length, fixedLength));
                Add(bArray);
            }
            public void AddEmpty(int fixedLendth)
            {
                Add(new byte[fixedLendth]);
            }

            public void AddShort(int i)
            {
                Add(BitConverter.GetBytes((short)i));
            }

            public void AddInt(int i)
            {
                Add(BitConverter.GetBytes(i));
            }

            public void AddByte(int i)
            {
                Add(new[] { (byte)i });
            }

            public void Add(byte[] bytes)
            {
                var i = _bytes.Length;
                var j = bytes.Length;
                Array.Resize(ref _bytes, i + j);
                Array.Copy(bytes, 0, _bytes, i, j);
            }
        }

        public ITransaction BeginTransaction()
        {
            if (ConnectionManager.EnableBtrieveTransactions)
            {
                var x = GetThreadInfoDoNotUseMeUseTheInstanceItem();
                if (x._activeTransaction == null)
                    x._activeTransaction = new myTrasnaction(this);
                return x._activeTransaction;
            }
            return dummyTransaction.Instance;
        }

        class myTrasnaction : ITransaction
        {
            BtrieveDataProvider _parent;
            public myTrasnaction(BtrieveDataProvider parent)
            {
                _parent = parent;
                _parent.BtrCall(Operations.BeginTransaction);
            }
            public void Commit()
            {
                var x = _parent.GetThreadInfoDoNotUseMeUseTheInstanceItem();
                if (x._activeTransaction == null) return;
                _parent.BtrCall(Operations.EndTransaction);
                x._activeTransaction = null;
            }

            public void Rollback()
            {
                var x = _parent.GetThreadInfoDoNotUseMeUseTheInstanceItem();
                if (x._activeTransaction == null) return;
                _parent.BtrCall(Operations.AbortTransaction);
                x._activeTransaction = null;
            }
        }

        void BtrCall(Operations op)
        {
            ushort dataBufferLength;
            _BtrCall((short)op, new byte[0], new byte[0], out dataBufferLength, new byte[0], 0, new short[0], false, BitConverter.GetBytes(GetThreadInfoDoNotUseMeUseTheInstanceItem().CID), null);
        }

        class dummyTransaction : ITransaction
        {
            private dummyTransaction() { }
            public static dummyTransaction Instance = new dummyTransaction();
            public void Commit() { }
            public void Rollback() { }
        }

        public BtrieveDataProvider()
        {
            VerifyStructure = true;
            RevertToNormalIfExclusiveFileOpenFails = true;
            CaseInsensitive = DefaultCaseInsensitive;
        }

        public bool SupportsTransactions
        {
            get { return ConnectionManager.EnableBtrieveTransactions; }
        }

        public string FilesPath { get; set; }

        public bool VerifyStructure { get; set; }

        public static bool RevertToNormalIfExclusiveFileOpenFails { get; set; }

        public static bool RetryFileOpenAndNeverRecreateExistingFile { get; set; }
        public string Name { get; internal set; }
        public bool CaseInsensitive { get; set; }
        public static bool DefaultCaseInsensitive { get; set; }


        string GetFilePath(Firefly.Box.Data.Entity entity)
        {
            var path = entity.EntityName;
            if (!string.IsNullOrEmpty(FilesPath) && !System.IO.Path.IsPathRooted(path) && !path.StartsWith(".."))
                path = System.IO.Path.Combine(ENV.PathDecoder.DecodePath(FilesPath), path);
            return path;
        }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return Common.FileExists(GetFilePath(entity));
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            long result = 0;
            DoWhileReadOnly(entity,
                () =>
                {
                    using (var r = ((myRowsSource)ProvideRowsSource(entity)))
                        result = r.CountRows();
                });
            return result;
        }

        public static bool SuppressFileExistenceCheck;

        void DoWhileReadOnly(Firefly.Box.Data.Entity e, Action action)
        {
            if (!SuppressFileExistenceCheck && !GetThreadInfoDoNotUseMeUseTheInstanceItem()._openFiles.ContainsKey(e.GetType()) && !Contains(e)) return;
            var x = e.ReadOnly;
            e.ReadOnly = true;
            try
            {
                action();
            }
            finally
            {
                e.ReadOnly = x;
            }
        }

        public long GetFileSize(Firefly.Box.Data.Entity entity)
        {
            long result = 0;
            DoWhileReadOnly(entity,
                () =>
                {
                    using (var r = ((myRowsSource)ProvideRowsSource(entity)))
                        result = r.GetFileSize();
                });
            return result;
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            try
            {
                System.IO.File.Delete(GetFilePath(entity));
            }
            catch { }
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            var bp = new BusinessProcess { From = entity, Activity = Activities.Delete };
            bp.Run();
        }
        public static Bool ForceHewbrewOem { get; set; }
        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            if (HebrewTextTools.V8HebrewOem || ForceHewbrewOem)
            {
                foreach (var col in entity.Columns)
                {
                    var c = col as Firefly.Box.Data.TextColumn;
                    if (c != null)
                    {
                        if (c.Storage is ENV.Data.Storage.AnsiStringTextStorageThatRemovesNullChars ||
                            c.Storage is Storage.AnsiStringTextStorage ||
                            c.Storage is ENV.Data.Storage.HebrewOemToAnsiTextStorage)
                            if (HebrewTextTools.V8HebrewOem)
                                c.Storage = new ENV.Data.Storage.V8HebrewOemTextStorage(c);
                            else
                                c.Storage = new ENV.Data.Storage.HebrewOemTextStorage(c);
                        else if (c.Storage is ENV.Data.Storage.ByteArrayTextStorage ||
                                 c.Storage is ENV.Data.Storage.LegacyMemoTextStorage)
                        {
                            if (HebrewTextTools.V8HebrewOem)
                                c.Storage = new ENV.Data.Storage.V8HebrewOemByteArrayStorage(c.Storage);
                            else
                                c.Storage = new ENV.Data.Storage.HebrewOemByteArrayStorage(c.Storage);


                        }
                    }
                }
            }
            if (CaseInsensitive)
            {
                foreach (var item in entity.Columns)
                {
                    var col = item as ENV.Data.TextColumn;
                    if (col != null)
                        col.DbCaseInsensitive = true;
                }
            }
            OpenedBtrieveFile file;
            var entityType = entity.GetType();
            var x = GetThreadInfoDoNotUseMeUseTheInstanceItem();
            FilesByTypes f;
            bool found = false;
            lock (x)
            {

                if ((found = x._openFiles.TryGetValue(entityType, out f)) && f.File != null)
                    file = f.File;
                else
                {
                    file = new OpenedBtrieveFile(this, entity, GetFilePath(entity), x);
                    if (!found)
                        x._openFiles.Add(entityType, f = new FilesByTypes() { File = null });
                }
            }

            file.Open(entity);
            //should be on dispose  addActionToBeCalledWhenTaskEnds(() => openFile.Close(() => _openFiles.Remove(filename)));
            return new myRowsSource(entity, f, CaseInsensitive);
        }

        class SelectedColumns
        {
            public List<ColumnDef> FixedLengthColumns;
            public HashSet<string> VariableLengthColumns = new HashSet<string>();
            public bool PositionColumnSelected;
        }

        class myRowsSource : IRowsSource
        {
            Firefly.Box.Data.Entity _table;
            FilesByTypes _file;
            ColumnBase[] _cols;
            bool _caseInsensitive;
            Dictionary<myRow, myRow> _deletedRowToNextRow = new Dictionary<myRow, myRow>();

            public myRowsSource(Firefly.Box.Data.Entity table, FilesByTypes file, bool caseInsensitive)
            {
                _table = table;
                _file = file;
                _cols = table.Columns.ToArray();
                _caseInsensitive = caseInsensitive;
                var be = _table as BtrieveEntity;
                if (be != null)
                    _reloadRowDataBeforeUpdate = be.ReloadRowDataBeforeUpdate;
            }

            bool _disposed;

            public void Dispose()
            {
                _file.File.Close();
                _disposed = true;
            }

            public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
            {
                return new myRowsProvider(this, selectedColumns, where, sort);
            }

            class myRowsProvider : IRowsProvider
            {
                myRowsSource _parent;
                IEnumerable<ColumnBase> _selectedColumns;
                IFilter _where;
                Sort _sort;

                public myRowsProvider(myRowsSource parent, IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort)
                {
                    _parent = parent;
                    _selectedColumns = selectedColumns;
                    _where = where;
                    _sort = sort;
                }

                public IRowsReader FromStart()
                {
                    return new myRowsReader(_parent, this, _selectedColumns, _where, _sort, false);
                }

                public IRowsReader From(IFilter filter, bool reverse)
                {
                    return new myRowsReader(_parent, this, _selectedColumns,
                                            new FilterThatExtendsTillTheEnd(_sort, _where, filter, reverse ? !_sort.Reversed : _sort.Reversed), _sort, reverse);
                }
                class FilterThatExtendsTillTheEnd : IFilter
                {
                    Dictionary<ColumnBase, bool> _sortedColumns = new Dictionary<ColumnBase, bool>();
                    IFilter _fixedFilter;
                    IFilter _condition;

                    public FilterThatExtendsTillTheEnd(Sort sort, IFilter fixedFilter, IFilter condition, bool reversed)
                    {
                        _fixedFilter = fixedFilter;

                        foreach (var segment in sort.Segments)
                        {
                            _sortedColumns.Add(segment.Column, segment.Direction == (!reversed ? SortDirection.Descending : SortDirection.Ascending));
                        }
                        _condition = condition;
                    }
                    class myFilter : IFilter, IFilterBuilder
                    {
                        Dictionary<ColumnBase, bool> _sortedColumns = new Dictionary<ColumnBase, bool>();

                        IFilterBuilder _filter;
                        IFilter _condition;
                        public myFilter(Dictionary<ColumnBase, bool> sortedColumns, IFilter condition)
                        {
                            _sortedColumns = sortedColumns;
                            _condition = condition;
                        }
                        public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
                        {

                            if (_sortedColumns.ContainsKey(column))
                            {
                                if (_sortedColumns[column])
                                    AddLessOrEqualTo(column, to);
                                else
                                    AddGreaterOrEqualTo(column, from);
                            }
                            else
                                _filter.AddBetween(column, from, to);
                        }

                        public void AddEqualTo(ColumnBase column, IFilterItem item)
                        {
                            if (_sortedColumns.ContainsKey(column))
                            {
                                if (!_sortedColumns[column])
                                    AddGreaterOrEqualTo(column, item);
                                else
                                    AddLessOrEqualTo(column, item);
                            }
                            else
                                _filter.AddEqualTo(column, item);
                        }

                        public void AddDifferentFrom(ColumnBase column, IFilterItem item)
                        {
                            _filter.AddDifferentFrom(column, item);
                        }

                        public void AddStartsWith(ColumnBase column, IFilterItem item)
                        {
                            _filter.AddStartsWith(column, item);
                        }

                        public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
                        {
                            if (!_sortedColumns.ContainsKey(column) || !_sortedColumns[column])
                                _filter.AddGreaterOrEqualTo(column, item);
                        }

                        public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
                        {
                            if (!_sortedColumns.ContainsKey(column) || _sortedColumns[column])
                                _filter.AddLessOrEqualTo(column, item);
                        }

                        public void AddWhere(string filterText, params IFilterItem[] formatItems)
                        {
                            _filter.AddWhere(filterText, formatItems);
                        }

                        public void AddOr(IFilter a, IFilter b)
                        {
                            _filter.AddOr(new myFilter(_sortedColumns, a), new myFilter(_sortedColumns, b));
                        }

                        public void AddLessThan(ColumnBase column, IFilterItem item)
                        {
                            if (!_sortedColumns.ContainsKey(column) || _sortedColumns[column])
                                _filter.AddLessThan(column, item);
                        }

                        public void AddGreaterThan(ColumnBase column, IFilterItem item)
                        {
                            if (!_sortedColumns.ContainsKey(column) || !_sortedColumns[column])
                                _filter.AddGreaterThan(column, item);
                        }

                        public void AddTrueCondition()
                        {
                            _filter.AddTrueCondition();
                        }

                        public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem)
                        {
                            if (!_sortedColumns.ContainsKey(column) || _sortedColumns[column])
                                _filter.AddLessOrEqualWithWildcard(column, value, filterItem);
                        }

                        public void AddTo(IFilterBuilder builder)
                        {
                            _filter = builder;
                            _condition.AddTo(this);
                        }
                    }
                    public void AddTo(IFilterBuilder builder)
                    {
                        _fixedFilter.AddTo(builder);
                        new myFilter(_sortedColumns, _condition).AddTo(builder);
                    }
                }

                public IRowsReader From(IRow row, bool reverse)
                {
                    return new myRowsReader(_parent, this, _selectedColumns, _where, _sort, false, reverse, row, false, false);
                }

                public IRowsReader FromEnd()
                {
                    return new myRowsReader(_parent, this, _selectedColumns, _where, _sort, true);
                }

                public IRowsReader After(IRow row, bool reverse)
                {
                    return new myRowsReader(_parent, this, _selectedColumns, _where, _sort, false, reverse, row, true, false);
                }

                public IRowsReader Find(IFilter filter, bool reverse)
                {
                    return new myRowsReader(_parent, this, _selectedColumns, new FilterJoiner(_where, filter), _sort, reverse);
                }

                class FilterJoiner : IFilter
                {
                    IFilter[] _filters;

                    public FilterJoiner(params IFilter[] filters)
                    {
                        _filters = filters;
                    }

                    public void AddTo(IFilterBuilder builder)
                    {
                        foreach (var f in _filters)
                        {
                            f.AddTo(builder);
                        }
                    }
                }
            }

            public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
            {
                return new myRowsReader(this, selectedColumns, false, where, sort, _table.AllowRowLocking && Context.Current.ActiveTasks[Context.Current.ActiveTasks.Count - 1].RowLocking == LockingStrategy.OnRowLoading && _table.Indexes.IndexOf(sort) >= 0);
            }

            bool[] ColumnsToBitMask(IEnumerable<ColumnBase> cols)
            {
                var tableColumns = _table.Columns.ToArray();
                var mask = new bool[tableColumns.Length];
                foreach (var c in cols)
                    mask[Array.IndexOf(tableColumns, c)] = true;
                return mask;
            }

            class myRowsReader : IRowsReader
            {
                myRowsSource _parent;
                myRowsProvider _rowsProvider;
                SelectedColumns _selectedColumns;

                byte[] _getNextExtendedData;
                bool _optimizedRejectCount;
                bool _reverse;
                bool _reversedSort;
                IRow _startOnThisRow;
                bool _advanceOnceBeyondRow;
                Sort _sort;
                bool _usingKey;
                myRow _currentMyRow;
                bool _firstRowOnly;
                OpenedBtrieveFile _file;
                IFilter _filter;
                bool _lockAllRows;

                public myRowsReader(myRowsSource parent, IEnumerable<ColumnBase> selectedColumns, bool firstRowOnly, IFilter filter, Sort sort, bool lockAllRows)
                    : this(parent, null, selectedColumns, filter, sort, firstRowOnly, false, null, false, lockAllRows)
                { }

                public myRowsReader(myRowsSource parent, myRowsProvider rowsProvider, IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool reverse)
                    : this(parent, rowsProvider, selectedColumns, filter, sort, false, reverse, null, false, false)
                { }

                public myRowsReader(myRowsSource parent, myRowsProvider rowsProvider, IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool reverse, IRow startOnThisRow, bool advanceOnceBeyondRow, bool lockAllRows)
                {
                    _parent = parent;
                    _rowsProvider = rowsProvider;
                    _file = _parent._file.File;
                    _selectedColumns = _parent.GetSelectedColumns(_file, selectedColumns);
                    _reversedSort = sort.Reversed;
                    _reverse = reverse ? !sort.Reversed : sort.Reversed;
                    _startOnThisRow = startOnThisRow;
                    _advanceOnceBeyondRow = advanceOnceBeyondRow;
                    _firstRowOnly = firstRowOnly;
                    _filter = filter;
                    _lockAllRows = lockAllRows;

                    Action<Sort, int> useKey =
                        (keySort, keyNumber) =>
                        {
                            if (!_file.KeysAllowed) return;
                            _usingKey = true;
                            _sort = keySort;
                            if (_parent._keyNumber != (short)keyNumber)
                            {
                                _parent._keyNumber = (short)keyNumber;
                                _parent._keyBuffer = new byte[255];
                            }
                        };

                    var x = _parent._table.Indexes.IndexOf(sort);
                    if (x >= 0)
                        useKey(sort, x);
                    else
                    {
                        var btrieveEntity = _parent._table as BtrieveEntity;
                        if (btrieveEntity != null)
                            x = _parent._table.Indexes.IndexOf(btrieveEntity.FetchIndex);
                        if (x >= 0)
                            useKey(btrieveEntity.FetchIndex, x);
                    }
                }

                class FilterBytes
                {
                    ColumnBase Column;
                    bool UseAsKeySearchValueForDescending;
                    bool UseAsKeySearchValueForAscending;
                    public byte[] Bytes;

                    public FilterBytes(ColumnBase column, bool useAsKeySearchValueForAscending, bool useAsKeySearchValueForDescending, byte[] bytes)
                    {
                        Column = column;
                        UseAsKeySearchValueForDescending = useAsKeySearchValueForDescending;
                        UseAsKeySearchValueForAscending = useAsKeySearchValueForAscending;
                        Bytes = bytes;
                    }

                    public int CompareTo(FilterBytes fb, List<SortSegment> segments, bool reveresedSort)
                    {
                        var x = segments.FindIndex(segment => segment.Column == Column);
                        if (x >= 0 && Column == fb.Column)
                            return GetRankForSegment(segments[x], reveresedSort).CompareTo(fb.GetRankForSegment(segments[x], reveresedSort));
                        x = x < 0 ? 99 : x;
                        var y = segments.FindIndex(segment => segment.Column == fb.Column);
                        y = y < 0 ? 99 : y;
                        return x.CompareTo(y);
                    }

                    public int GetRankForSegment(SortSegment segment, bool reversed)
                    {
                        var i = 0;
                        if (UseAsKeySearchValueForAscending)
                            i += segment.Direction == (!reversed ? SortDirection.Ascending : SortDirection.Descending) ? 1 : 2;
                        if (UseAsKeySearchValueForDescending)
                            i += segment.Direction == (!reversed ? SortDirection.Ascending : SortDirection.Descending) ? 2 : 1;
                        return i;
                    }

                    public bool IsEndFilterFor(SortSegment segment, bool reversed)
                    {
                        return Column == segment.Column &&
                            (UseAsKeySearchValueForAscending && segment.Direction == (!reversed ? SortDirection.Ascending : SortDirection.Descending) ||
                            UseAsKeySearchValueForDescending && segment.Direction == (!reversed ? SortDirection.Descending : SortDirection.Ascending));
                    }
                }

                bool _disposed;
                public void Dispose()
                {
                    _disposed = true;
                }

                const int CACHE_SIZE = 4;
                List<myRow> _cache = new List<myRow>(CACHE_SIZE);
                int _cacheActiveIndex = -1;
                bool _lastRowFetched = false;
                List<Action<myFilter1.AddFilterDelegate>> _theFilter;
                int cacheSize = CACHE_SIZE;
                private bool _onNextReadStartFromCurrent;

                bool ReadNext(bool startWithCurrent, bool reacquirePosition)
                {
                    if (_getNextExtendedData == null)
                    {
                        var w = new ByteWriter();
                        w.AddEmpty(2); // This will be set to "EG" or "UG"
                        w.Add(new byte[] { 0xff, 0xff }); // Use max reject count

                        var filters = new List<FilterBytes>();

                        var x = new myFilter1(
                            (column, filterCode, useAsKeySearchValueForAscending, useAsKeySearchValueForDescending, filterValue) =>
                            {
                                var ds = filterValue.GetValue();

                                if (!ds.ValueWasByteArray || useAsKeySearchValueForAscending && useAsKeySearchValueForDescending)
                                {
                                    _file.SendColumnDefinition(column,
                                        (dataTypeCode, offset, size, variableSize) =>
                                        {
                                            if (variableSize) return;

                                            var w1 = new ByteWriter();

                                            w1.AddByte(dataTypeCode);
                                            w1.AddShort(size);
                                            w1.AddShort(offset);
                                            w1.AddByte(filterCode + (dataTypeCode == 0 && _parent._caseInsensitive ? 128 : 0));
                                            w1.AddByte(1); // AND or last

                                            w1.Add(ds.ValueByteArray, size);

                                            filters.Add(new FilterBytes(column, useAsKeySearchValueForDescending, useAsKeySearchValueForAscending, w1.Result));
                                        });
                                }
                            });
                        _filter.AddTo(x);

                        w.AddShort(filters.Count);

                        var segments = new List<SortSegment>();
                        var highlyOptimizedRejectCount = false;

                        if (_usingKey)
                        {
                            var bSort = _sort as BtrieveSort;
                            foreach (var seg in _sort.Segments)
                            {
                                segments.Add(seg);
                                if (bSort != null && !_optimizedRejectCount)
                                {
                                    _file.SendColumnDefinition(seg.Column,
                                        (dataTypeCode, ofst, columnSize, variableSize) =>
                                        {
                                            if (bSort.GetModifiedSegmentSize(seg.Column, columnSize) != columnSize)
                                                _optimizedRejectCount = true;
                                        });
                                }
                            }
                            filters.Sort((fb1, fb2) => fb1.CompareTo(fb2, segments, _sort.Reversed));
                            if (_optimizedRejectCount)
                                highlyOptimizedRejectCount = true;
                        }
                        for (var i = 0; i < filters.Count; i++)
                        {
                            if (_optimizedRejectCount)
                            {
                                if (segments.Count != 0)
                                {
                                    if (!filters[i].IsEndFilterFor(segments[0], _sort.Reversed))
                                        highlyOptimizedRejectCount = false;
                                    segments.RemoveAt(0);
                                }
                                else
                                    highlyOptimizedRejectCount = false;
                            }
                            var f = filters[i];
                            if (i == filters.Count - 1)
                                f.Bytes[6] = 0;
                            w.Add(f.Bytes);
                        }

                        cacheSize = _lockAllRows || _firstRowOnly ? 1 : _file.GetMaxRowLength() > 0 ? Math.Min((ushort.MaxValue - 2) / (_file.GetMaxRowLength() + 6), Math.Max(1200 / _file.GetMaxRowLength(), cacheSize)) : cacheSize;

                        w.AddShort(cacheSize);

                        w.AddShort(1);
                        w.AddShort(_file.GetMaxRowLength());
                        w.AddShort(0);

                        var w2 = new ByteWriter();
                        w2.AddShort(w.Result.Length + 2);
                        w2.Add(w.Result);
                        _getNextExtendedData = w2.Result;

                        if (_optimizedRejectCount)
                            Array.Copy(highlyOptimizedRejectCount ? new byte[] { 1, 0 } : new byte[] { 1, 1 }, 0, _getNextExtendedData, 4, 2);
                    }

                    if (_onNextReadStartFromCurrent)
                    {
                        _onNextReadStartFromCurrent = false;
                        startWithCurrent = true;
                    }

                    if (!startWithCurrent)
                    {
                        if (_cacheActiveIndex + 1 < _cache.Count)
                        {
                            if (!_file.WasBtrieveCalled)
                            {
                                _cacheActiveIndex++;
                                _currentMyRow = _cache[_cacheActiveIndex];
                                return true;
                            }
                        }
                        else if (_lastRowFetched) return false;
                    }

                    _cache.Clear();
                    _cacheActiveIndex = 0;

                    if (reacquirePosition)
                    {
                        if (!_currentMyRow.GotoRow(false, DatabaseErrorHandlingStrategy.Rollback, true)) return false;
                    }

                    short result = 0;

                    ushort dataBufLength;
                    byte[] dataBuffer;
                    while (true)
                    {
                        do
                        {
                            dataBuffer = new byte[Math.Max(_getNextExtendedData.Length, 2 + cacheSize * (_file.GetMaxRowLength() + 6))];
                            Array.Copy(_getNextExtendedData, dataBuffer, _getNextExtendedData.Length);
                            Array.Copy(Encoding.ASCII.GetBytes(startWithCurrent ? "UC" : "EG"), 0, dataBuffer, 2, 2);

                            result = _file.BtrCall(
                                (_usingKey
                                    ? (_reverse ? Operations.GetPreviousExtended : Operations.GetNextExtended)
                                    : (_reverse ? Operations.StepPreviousExtended : Operations.StepNextExtended)), _lockAllRows, null,
                                dataBuffer, out dataBufLength, _parent._keyBuffer, _parent._keyNumber, 9, 22, 64, 60);
                            if (result == 9 || result == 64 || (result == 60 && _optimizedRejectCount))
                            {
                                _lastRowFetched = true;
                                if (dataBufLength <= 8)
                                    return false;
                                break;
                            }
                        } while (result == 60 && dataBufLength <= 8);

                        var rowsRetrieved = BitConverter.ToInt16(dataBuffer, 0);
                        var x = 2;
                        for (int i = 0; i < rowsRetrieved; i++)
                        {
                            var rawRow = new byte[_file.GetMaxRowLength()];
                            var size = BitConverter.ToInt16(dataBuffer, x);

                            if (size > 0)
                            {
                                var pos = new byte[4];
                                Array.Copy(dataBuffer, x + 2, pos, 0, 4);
                                Array.Copy(dataBuffer, x + 6, rawRow, 0, size);

                                var rowDoesNotMatchFilter = false;
                                if (_theFilter == null)
                                {
                                    _theFilter = new List<Action<myFilter1.AddFilterDelegate>>();
                                    var f = new myFilter1(
                                        (column, code, @ascending, @descending, filterValue) =>
                                        {

                                            _theFilter.Add(@delegate =>
                                            {
                                                @delegate(column, code, @ascending, @descending,
                                                    filterValue);
                                            });
                                            if (rowDoesNotMatchFilter) return;
                                            if (filterValue.GetValue().ValueWasByteArray &&
                                                !filterValue.CheckFilter(
                                                    _file.GetColumnValue(rawRow, column)(), _parent._caseInsensitive))
                                                rowDoesNotMatchFilter = true;
                                        });
                                    _filter.AddTo(f);
                                }
                                else
                                {
                                    for (int j = 0; j < _theFilter.Count; j++)
                                    {
                                        if (rowDoesNotMatchFilter)
                                            break;
                                        _theFilter[j]((column, code, @ascending, @descending, filterValue)
                                            =>
                                        {
                                            if (filterValue.GetValue().ValueWasByteArray &&
                                                !filterValue.CheckFilter(
                                                    _file.GetColumnValue(rawRow, column)(), _parent._caseInsensitive))
                                                rowDoesNotMatchFilter = true;
                                        });
                                    }
                                }
                                var r = new myRow(_parent, _selectedColumns, rawRow, new myStateForMyRow(this), pos,
                                    _lockAllRows);
                                if (rowDoesNotMatchFilter)
                                    r.Unlock();
                                else
                                    _cache.Add(r);
                            }
                            x += 6 + size;
                        }

                        if (!startWithCurrent)
                        {
                            while (_cache.Count > 0 && _cache[0].IsEqualTo(_currentMyRow))
                                _cache.RemoveAt(0);
                        }

                        if (_cache.Count == 0)
                        {
                            if (_lastRowFetched) return false;
                            startWithCurrent = false;
                            continue;
                        }

                        if (_lockAllRows && _currentMyRow != null)
                            _currentMyRow.Unlock();
                        _currentMyRow = _cache[0];
                        break;
                    }
                    _file.ResetBtrieveCalled();
                    return true;
                }

                class myFilter1 : IFilterBuilder
                {
                    public delegate void AddFilterDelegate(
                        ColumnBase column, int filterCode, bool useAsKeySearchValueForAscending,
                        bool useAsKeySearchValueForDescending, FilterValue filterValue);

                    public class FilterValue
                    {
                        IFilterItem _item;
                        bool _padValueWith256;
                        Func<int, bool> _testComparison;
                        BinaryValueSaver _ds = null;

                        public FilterValue(IFilterItem item, bool padValueWith256, Func<int, bool> testComparison)
                        {
                            _item = item;
                            _padValueWith256 = padValueWith256;
                            _testComparison = testComparison;
                        }

                        public BinaryValueSaver GetValue()
                        {
                            if (_ds != null) return _ds;
                            _ds = new BinaryValueSaver(_padValueWith256);
                            _item.SaveTo(_ds);
                            return _ds;
                        }

                        public bool CheckFilter(BinaryValueSaver vs, bool caseInsensitive)
                        {
                            if (caseInsensitive && vs.ComparableForCaseInsensitive != null && GetValue().ComparableForCaseInsensitive != null)
                                return _testComparison(string.Compare(vs.ComparableForCaseInsensitive, GetValue().ComparableForCaseInsensitive, true));
                            return GetValue().Comparable == null || _testComparison(Comparer.Compare(vs.Comparable, GetValue().Comparable));
                        }
                    }

                    AddFilterDelegate _addFilter;

                    public myFilter1(AddFilterDelegate addFilter)
                    {
                        _addFilter = addFilter;
                    }

                    public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
                    {
                        AddGreaterOrEqualTo(column, from);
                        AddLessOrEqualTo(column, to);
                    }

                    public void AddEqualTo(ColumnBase column, IFilterItem item)
                    {
                        AddFilter(column, item, 1, true, true, i => i == 0, false);
                    }

                    public void AddDifferentFrom(ColumnBase column, IFilterItem item)
                    {
                        throw new NotImplementedException();
                    }
                    public void AddStartsWith(ColumnBase column, IFilterItem item)
                    {
                        AddGreaterOrEqualTo(column, item);
                        AddLessOrEqualTo(column, item, true);
                    }

                    public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
                    {
                        AddFilter(column, item, 5, true, false, i => i >= 0, false);
                    }

                    public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
                    {
                        AddLessOrEqualTo(column, item, false);
                    }

                    public void AddLessThan(ColumnBase column, IFilterItem item)
                    {
                        AddFilter(column, item, 3, false, true, i => i < 0, false);
                    }

                    public void AddGreaterThan(ColumnBase column, IFilterItem item)
                    {
                        AddFilter(column, item, 2, true, false, i => i > 0, false);
                    }

                    public void AddWhere(string filterText, params IFilterItem[] formatItems)
                    {
                        throw new NotImplementedException();
                    }

                    public void AddOr(IFilter a, IFilter b)
                    {
                        throw new NotImplementedException();
                    }

                    public void AddTrueCondition()
                    {
                    }

                    public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem)
                    {
                        AddLessOrEqualTo(column, filterItem, true);
                    }

                    void AddLessOrEqualTo(ColumnBase column, IFilterItem item, bool padValueWith256)
                    {
                        AddFilter(column, item, 6, false, true, i => i <= 0, padValueWith256);
                    }

                    void AddFilter(ColumnBase column, IFilterItem item, int filterCode, bool useAsKeySearchValueForAscending,
                        bool useAsKeySearchValueForDescending, Func<int, bool> testComparison, bool padValueWith256)
                    {
                        _addFilter(column, filterCode, useAsKeySearchValueForAscending,
                                   useAsKeySearchValueForDescending, new FilterValue(item, padValueWith256, testComparison));
                    }

                }

                class KeySegmentData
                {
                    public int Offset;
                    public int Size;
                    public int Index;
                    public int DataTypeCode;
                    public bool Descending;
                    public bool HasFilterValue;
                    public object LastFilterValue;
                    public object EqualsFilterValue;
                    public ColumnBase Column;
                }

                class myFilterBuilderForFirstReadByKey
                {

                    Dictionary<string, KeySegmentData> _segmentsDictionary;
                    byte[] _keyBuffer;
                    public byte[] FilterByThisPosition;

                    public myFilterBuilderForFirstReadByKey(Dictionary<string, KeySegmentData> segmentsDictionary, byte[] keyBuffer)
                    {
                        _segmentsDictionary = segmentsDictionary;
                        _keyBuffer = keyBuffer;
                    }

                    public void AddFilter(ColumnBase column, int filterCode, bool useAsKeySearchValueForAscending,
                        bool useAsKeySearchValueForDescending, myFilter1.FilterValue filterValue)
                    {
                        KeySegmentData k;
                        if (!_segmentsDictionary.TryGetValue(column.Name, out k))
                        {
                            if (useAsKeySearchValueForAscending && useAsKeySearchValueForDescending && column is BtrievePositionColumn)
                                FilterByThisPosition = filterValue.GetValue().ValueByteArray;
                            return;
                        }

                        if (k.Descending && !useAsKeySearchValueForDescending ||
                            !k.Descending && !useAsKeySearchValueForAscending)
                            return;

                        var ds = filterValue.GetValue();

                        if ((ds.ValueWasByteArray) && !(useAsKeySearchValueForAscending && useAsKeySearchValueForDescending))
                            return;

                        if (k.LastFilterValue != null && ds.Comparable != null &&
                            ((k.Descending && Comparer.Compare(ds.Comparable, k.LastFilterValue) >= 0) ||
                            (!k.Descending && Comparer.Compare(ds.Comparable, k.LastFilterValue) <= 0)))
                            return;

                        Array.Copy(ds.ValueByteArray, 0, _keyBuffer, k.Offset, Math.Min(k.Size, ds.ValueByteArray.Length));

                        k.LastFilterValue = ds.Comparable;
                        if (useAsKeySearchValueForAscending && useAsKeySearchValueForDescending)
                        {
                            if (k.HasFilterValue)
                                k.EqualsFilterValue = null;
                            else
                                k.EqualsFilterValue = ds.Comparable;
                        }
                        k.HasFilterValue = true;
                    }
                }

                public bool Read()
                {
                    if (_currentMyRow == null)
                    {
                        if (_parent._rawRow == null)
                            _parent._rawRow = new byte[_file.GetMaxRowLength()];
                        var rawRow = _parent._rawRow;

                        var r = _startOnThisRow as myRow;
                        if (r != null)
                        {
                            myRow nextRowForDeleted = null;
                            Action runAfterReadIfSucceeded = () => { };
                            if (_parent._deletedRowToNextRow.TryGetValue(r, out nextRowForDeleted))
                            {
                                var x = r;
                                runAfterReadIfSucceeded = () => _parent._deletedRowToNextRow.Remove(x);
                                r = nextRowForDeleted;
                                _advanceOnceBeyondRow = false;
                            }
                            try
                            {
                                r.Reparent(_parent, _selectedColumns, new myStateForMyRow(this));
                                if (ReadNext(!_advanceOnceBeyondRow, false))
                                {
                                    runAfterReadIfSucceeded();
                                    return true;
                                }
                                else return false;
                            }
                            catch (DatabaseErrorException ex)
                            {
                                if (ex.ErrorType != DatabaseErrorType.RowDoesNotExist)
                                    throw;
                                return false;
                            }
                        }
                        if (_usingKey)
                        {
                            var segmentsDictionary = new Dictionary<string, KeySegmentData>(_sort.Segments.Count);
                            var segmentsList = new List<KeySegmentData>(_sort.Segments.Count);

                            var bSort = _sort as BtrieveSort;
                            var offset = 0;
                            var i = 0;
                            foreach (var s in _sort.Segments)
                            {

                                var size = 0;
                                _file.SendColumnDefinition(s.Column,
                                    (dataTypeCode, ofst, columnSize, variableSize) =>
                                    {
                                        size = columnSize;
                                        if (bSort != null)
                                            size = bSort.GetModifiedSegmentSize(s.Column, size);
                                        var data = new KeySegmentData
                                        {
                                            Offset = offset,
                                            Size = size,
                                            DataTypeCode = dataTypeCode,
                                            Descending =
                                                s.Direction == SortDirection.Ascending
                                                    ? _reverse
                                                    : !_reverse,
                                            Index = i,
                                            Column = s.Column
                                        };
                                        segmentsDictionary.Add(s.Column.Name, data);
                                        segmentsList.Add(data);
                                    });
                                i++;
                                offset += size;
                            }

                            _parent._keyBuffer = new byte[Math.Max(offset, 8)];
                            var fb = new myFilterBuilderForFirstReadByKey(segmentsDictionary, _parent._keyBuffer);
                            _filter.AddTo(new myFilter1(fb.AddFilter));
                            if (fb.FilterByThisPosition != null)
                                return ReadByPosition(fb.FilterByThisPosition);

                            var excludeColumnEqualsFilter = new HashSet<string>();
                            var numOfBytesToCompareForKeyRejectionTest = 0;
                            var firstKeySegmentWithNoEqualFilterFound = false;
                            foreach (var ks in segmentsDictionary)
                            {
                                if (!ks.Value.HasFilterValue)
                                {
                                    var valueByteArray = new byte[ks.Value.Size];

                                    for (var j = 0; j < valueByteArray.Length; j++)
                                        valueByteArray[j] = (byte)(ks.Value.Descending ? 255 : 0);

                                    switch (ks.Value.DataTypeCode)
                                    {
                                        case 1:
                                            valueByteArray[valueByteArray.Length - 1] = (byte)(ks.Value.Descending ? 127 : 128);
                                            break;
                                        case 2:
                                            if (valueByteArray.Length == 4)
                                                valueByteArray = BitConverter.GetBytes(!ks.Value.Descending ? float.MinValue : float.MaxValue);
                                            else
                                                valueByteArray = BitConverter.GetBytes(!ks.Value.Descending ? double.MinValue : double.MaxValue);
                                            break;
                                        case 9:
                                            var ds = new BinaryValueSaver();
                                            new ENV.Data.Storage.FloatMSBasicNumberStorage(valueByteArray.Length).SaveTo(
                                                !ks.Value.Descending ? decimal.MinValue : decimal.MaxValue, ds);
                                            valueByteArray = ds.ValueByteArray;
                                            break;
                                    }

                                    Array.Copy(valueByteArray, 0, _parent._keyBuffer, ks.Value.Offset, ks.Value.Size);

                                    firstKeySegmentWithNoEqualFilterFound = true;
                                }
                                else if (!firstKeySegmentWithNoEqualFilterFound)
                                {
                                    if (ks.Value.EqualsFilterValue != null)
                                    {
                                        numOfBytesToCompareForKeyRejectionTest += ks.Value.Size;
                                        excludeColumnEqualsFilter.Add(ks.Value.Column.Name);
                                    }
                                    else
                                        firstKeySegmentWithNoEqualFilterFound = true;
                                }
                            }
                            var bytesToCompare = new byte[numOfBytesToCompareForKeyRejectionTest];
                            Array.Copy(_parent._keyBuffer, bytesToCompare, numOfBytesToCompareForKeyRejectionTest);

                            var r1 = _file.BtrCall(
                                (_reverse ? Operations.GetLessOrEqual : Operations.GetGreaterOrEqual), _lockAllRows,
                                rawRow, _parent._keyBuffer, _parent._keyNumber, 9, 22, 84);
                            if (r1 == 9)
                                return false;

                            if (r1 == 84)
                            {
                                var r2 = _file.BtrCall(
                                    (_reverse ? Operations.GetLessOrEqual : Operations.GetGreaterOrEqual), false,
                                    rawRow, _parent._keyBuffer, _parent._keyNumber, 9, 22);
                                if (r2 == 9)
                                    return false;
                            }

                            var row = new myRow(_parent, _selectedColumns, rawRow, new myStateForMyRow(this), _lockAllRows && r1 != 84);

                            if (_parent._caseInsensitive)
                            {
                                foreach (var item in segmentsList)
                                {
                                    if (item.Offset + item.Size > numOfBytesToCompareForKeyRejectionTest) break;
                                    if (item.DataTypeCode != 0) continue;
                                    for (int j = item.Offset; j < item.Offset + item.Size; j++)
                                    {
                                        if (_parent._keyBuffer[j] - bytesToCompare[j] == 32 ||
                                            bytesToCompare[j] - _parent._keyBuffer[j] == 32)
                                            bytesToCompare[j] = _parent._keyBuffer[j];
                                    }
                                }
                            }

                            for (int j = 0; j < numOfBytesToCompareForKeyRejectionTest; j++)
                            {
                                if (_parent._keyBuffer[j] != bytesToCompare[j])
                                {
                                    row.Unlock();
                                    return false;
                                }
                            }

                            var rowDoesNotMatchFilter = false;
                            if (!rowDoesNotMatchFilter)
                            {
                                _filter.AddTo(new myFilter1(
                                    (column, code, ascKeyValue, descKeyValue, filterValue) =>
                                    {

                                        if (rowDoesNotMatchFilter) return;
                                        if (ascKeyValue && descKeyValue && excludeColumnEqualsFilter.Contains(column.Name)) return;

                                        rowDoesNotMatchFilter = !filterValue.CheckFilter(_file.GetColumnValue(rawRow, column)(), _parent._caseInsensitive);
                                    }));
                            }

                            if (rowDoesNotMatchFilter)
                                row.Unlock();
                            else
                            {
                                if (r1 == 84)
                                    row.ThrowLockedRowException(84);
                                _parent._rawRow = null;
                                _currentMyRow = row;
                                _file.ResetBtrieveCalled();
                                return true;
                            }
                        }
                        else
                        {
                            if (_file.PositionColumnSelected)
                            {
                                var fb = new myFilterBuilderForFirstReadByKey(new Dictionary<string, KeySegmentData>(), _parent._keyBuffer);
                                _filter.AddTo(new myFilter1(fb.AddFilter));
                                if (fb.FilterByThisPosition != null)
                                    return ReadByPosition(fb.FilterByThisPosition);
                            }
                            if (_file.BtrCall((_reverse ? Operations.StepLast : Operations.StepFirst), rawRow, _parent._keyBuffer, _parent._keyNumber, 9, 22) == 9) return false;
                        }
                        return ReadNext(true, false);
                    }
                    if (_firstRowOnly) return false;
                    return ReadNext(false, true);
                }

                bool ReadByPosition(byte[] position)
                {
                    _firstRowOnly = true;
                    var x = new myRow(_parent, _selectedColumns, null, new myStateForMyRow(this), position, false);
                    x.ReloadData();
                    _currentMyRow = x;
                    return true;
                }

                class myStateForMyRow : StateForMyRow
                {
                    myRowsReader _parent;

                    public myStateForMyRow(myRowsReader parent)
                    {
                        _parent = parent;
                    }

                    public void Deleted(myRow myRow)
                    {
                        if (_parent._disposed && _parent._rowsProvider != null)
                        {
                            var rawRow = new byte[_parent._parent._file.File.GetMaxRowLength()];
                            var r = _parent._parent._file.File.BtrCall(_parent._usingKey ? Operations.GetNext : Operations.StepNext, rawRow, _parent._parent._keyBuffer,
                                _parent._parent._keyNumber, 9, 22);
                            if (r == 9)
                            {
                                r = _parent._parent._file.File.BtrCall(_parent._usingKey ? Operations.GetPrevious : Operations.StepPrevious, rawRow, _parent._parent._keyBuffer,
                                _parent._parent._keyNumber, 9, 22);
                            }
                            if (r == 0)
                            {
                                _parent._parent._deletedRowToNextRow.Add(myRow,
                                    new myRow(_parent._parent, _parent._selectedColumns, rawRow, new myStateForMyRow(_parent), false));
                            }
                            return;
                        }
                        if (_parent._currentMyRow == myRow && !_parent._firstRowOnly)
                        {
                            _parent.ReadNext(false, false);
                            _parent._onNextReadStartFromCurrent = true;
                        }
                    }

                    public void Updated(myRow myRow, IEnumerable<ColumnBase> columns)
                    {
                        if (_parent._disposed) return;
                        if (_parent._currentMyRow == myRow && !_parent._firstRowOnly)
                        {
                            if (_parent.ReadNext(false, false))
                                _parent._onNextReadStartFromCurrent = true;
                        }
                    }
                }

                public IRow GetRow(IRowStorage c)
                {
                    return _currentMyRow.GetRow(c, _selectedColumns);
                }

                public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                {
                    throw new NotImplementedException();
                }
            }

            SelectedColumns _lastSelectedColumns;
            IEnumerable<ColumnBase> _lastSelectedColumnsEnumerable;
            SelectedColumns GetSelectedColumns(OpenedBtrieveFile file, IEnumerable<ColumnBase> selectedColumns)
            {
                if (ReferenceEquals(_lastSelectedColumnsEnumerable, selectedColumns))
                    return _lastSelectedColumns;
                _lastSelectedColumnsEnumerable = selectedColumns;
                var sc =
                    new SelectedColumns()
                    {
                        FixedLengthColumns = new List<ColumnDef>(
                            _lastSelectedColumns != null ? _lastSelectedColumns.FixedLengthColumns.Count : 10)
                    };
                _lastSelectedColumns = sc;
                file.PopulateSelectedColumns(sc, selectedColumns);
                return sc;
            }

            interface StateForMyRow
            {
                void Deleted(myRow myRow);
                void Updated(myRow myRow, IEnumerable<ColumnBase> columns);
            }

            class myRow : IRow
            {
                myRowsSource _parent;
                StateForMyRow _state;
                byte[] _position = new byte[4];
                bool _locked;
                byte[] _rawData;
                bool _blobSelected;

                public myRow(myRowsSource parent, SelectedColumns selectedColumns, byte[] rawData, StateForMyRow state, bool locked)
                    : this(parent, selectedColumns, rawData, state, null, locked)
                {
                }

                public myRow(myRowsSource parent, SelectedColumns selectedColumns, byte[] rawData, StateForMyRow state, byte[] position, bool locked)
                {
                    _parent = parent;
                    _state = state;
                    _locked = locked;
                    _blobSelected = _parent._file.File.BlobSelected(selectedColumns);
                    if (position == null)
                        _parent._file.File.BtrCall(Operations.GetPosition, _position, new byte[0], 0);
                    else
                        _position = position;
                    if (rawData != null) SetData(rawData);
                }


                void SetData(byte[] data)
                {
                    _rawData = data;
                    if (_blobSelected)
                    {
                        var ms = new MemoryStream();
                        ms.Write(_rawData, 0, _rawData.Length);
                        var chunkBuffer = new byte[MAX_COMM_BUFFER_SIZE];
                        var chunkOffset = _rawData.Length;
                        var numOfBytesRetrieved = (ushort)0;
                        var chunkOperationData = new ByteWriter();
                        chunkOperationData.Add(_position);
                        chunkOperationData.Add(BitConverter.GetBytes(0x80000000)); //Subfunction
                        chunkOperationData.AddInt(1); // NumChunks
                        chunkOperationData.AddEmpty(4); // Chunk Offset
                        chunkOperationData.AddInt(chunkBuffer.Length);
                        chunkOperationData.AddEmpty(4); // User Data
                        do
                        {
                            chunkOffset += numOfBytesRetrieved;
                            Array.Copy(chunkOperationData.Result, chunkBuffer, chunkOperationData.Result.Length);
                            Array.Copy(BitConverter.GetBytes(chunkOffset), 0, chunkBuffer, 12, 4);
                            numOfBytesRetrieved = (ushort)chunkBuffer.Length;
                            _parent._file.File.BtrCall(Operations.GetDirect, false, _position, chunkBuffer, out numOfBytesRetrieved, new byte[0], -2, 103);
                            ms.Write(chunkBuffer, 0, numOfBytesRetrieved);
                        } while (numOfBytesRetrieved > 0);
                        _rawData = ms.ToArray();
                    }
                    _parent._file.File.SetLastAccessedRowPositionAndData(_position, _rawData);
                }

                public void Delete(bool verifyRowHasNotChangedSinceLoaded)
                {
                    GotoRow(false, DatabaseErrorHandlingStrategy.Ignore, false);
                    if (_parent._file.File.BtrCall(Operations.Delete, new byte[0], new byte[0], 0, 80) == 80)
                    {
                        GotoRow(true, DatabaseErrorHandlingStrategy.Ignore, false);
                        _parent._file.File.BtrCall(Operations.Delete, new byte[0], new byte[0], 0);
                    }
                    _state.Deleted(this);
                }

                public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
                {
                    var reloadRow = _parent._reloadRowDataBeforeUpdate;
                    var s = false;
                    do
                    {
                        GotoRow(reloadRow, DatabaseErrorHandlingStrategy.Ignore, false, true);
                        var updatedColumnsBitMask = _parent.ColumnsToBitMask(columns);
                        try
                        {
                            _rawData = _parent._file.File.FillRawDataWithColumns(_rawData,
                                (i, alwaysSave, saver) =>
                                {
                                    if (alwaysSave || updatedColumnsBitMask[i])
                                        _parent._table.Columns[i].SaveYourValueToDb(saver);
                                },
                                data =>
                                {
                                    if (_parent._file.File.LastColumnIsBlob && !_blobSelected)
                                        _parent._file.File.UpdateChunks(data, 0, _parent._keyBuffer);
                                    else
                                        _parent._file.File.BtrCall(Operations.Update, data, _parent._keyBuffer, 0xFF, 22);
                                }, _blobSelected);
                            s = true;
                        }
                        catch (DatabaseErrorException ex)
                        {
                            if (!reloadRow && ex.InnerException is BtrieveException && (int)((BtrieveException)ex.InnerException).ErrorCode == 80)
                                reloadRow = true;
                            else
                                throw;
                        }
                    } while (!s && !reloadRow);
                    _parent._file.File.SetLastAccessedRowPositionAndData(_position, _rawData);
                    _state.Updated(this, columns);
                }

                public void Lock()
                {
                    if (_locked) return;

                    _locked = true;
                    try
                    {
                        GotoRow(true, DatabaseErrorHandlingStrategy.Rollback, false);
                        _loadRawDataIntoRowStorage();
                    }
                    catch (Exception)
                    {
                        _locked = false;
                        throw;
                    }
                }

                public void ReloadData()
                {
                    GotoRow(true, DatabaseErrorHandlingStrategy.Ignore, false);
                    _loadRawDataIntoRowStorage();
                }

                public bool IsEqualTo(IRow row)
                {
                    var r = row as myRow;
                    if (r != null)
                        return Comparer.Equal(r._position, _position);
                    return false;
                }

                public void Unlock()
                {
                    if (!_locked) return;
                    _parent._file.File.BtrCall(Operations.Unlock, _position, _parent._keyBuffer, -1, 81);
                    _locked = false;
                }

                static bool PositionEquals(byte[] a, byte[] b)
                {
                    for (var i = 0; i < 4; i++)
                        if (a[i] != b[i]) return false;
                    return true;
                }

                public bool GotoRow(bool forceFetch, DatabaseErrorHandlingStrategy handlingStrategyIfRowNotFound, bool ifRowDoesNotExistReturnFalseAndDoNotThrowException, bool beforeUpdate = false)
                {
                    if (!forceFetch && _parent.HasKeyNotChangedSinceLastFetch())
                    {
                        var currentPosition = new byte[4];
                        var r = _parent._file.File.BtrCall(Operations.GetPosition, currentPosition, new byte[0], 0, 8);
                        if (r == 0)
                        {
                            if (PositionEquals(currentPosition, _position))
                            {
                                if (beforeUpdate)
                                {
                                    var p = _parent._file.File.LastAccessedRowPosition;
                                    if (p != null && !object.ReferenceEquals(_rawData, _parent._file.File.LastAccessedRowData) && PositionEquals(p, _position))
                                        _rawData = _parent._file.File.LastAccessedRowData;
                                }
                                _parent._keyBuffer = _parent._file.File.LastKeyBuffer;
                                return true;
                            }
                        }
                    }

                    var dataBuffer = new byte[Math.Max(_parent._file.File.GetMaxRowLength(), _position.Length)];
                    Array.Copy(_position, dataBuffer, _position.Length);
                    ushort l;
                    var x = _parent._file.File.BtrCall(Operations.GetDirect, _locked, _position,
                        dataBuffer, out l, _parent._keyBuffer, _parent._keyNumber, 22, 43);
                    if (x == 43)
                    {
                        if (ifRowDoesNotExistReturnFalseAndDoNotThrowException) return false;
                        throw new DatabaseErrorException(DatabaseErrorType.RowDoesNotExist, new BtrieveException(43, BitConverter.ToInt32(_position, 0)), handlingStrategyIfRowNotFound);
                    }
                    SetData(dataBuffer);
                    return true;
                }

                Action _loadRawDataIntoRowStorage = () => { };
                public IRow GetRow(IRowStorage c, SelectedColumns selectedColumns)
                {
                    _loadRawDataIntoRowStorage = () => _parent._file.File.FillColumnsRawData(_position, _rawData, selectedColumns, c, _parent._cols);
                    _loadRawDataIntoRowStorage();
                    return this;
                }

                public void Reparent(myRowsSource parent, SelectedColumns selectedColumns, StateForMyRow state)
                {
                    new myRow(parent, selectedColumns, null, state, _position, _locked).GotoRow(true, DatabaseErrorHandlingStrategy.Rollback, false);
                }
                internal void ThrowLockedRowException(short btrieveErrorCode)
                {
                    throw new DatabaseErrorException(DatabaseErrorType.LockedRow, new BtrieveException(btrieveErrorCode, BitConverter.ToInt32(_position, 0)));
                }
            }

            bool HasKeyNotChangedSinceLastFetch()
            {
                return _keyNumber == _file.File.ActiveKeyNumber;
            }

            public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
            {
                return new myRowsReader(this, selectedColumns, firstRowOnly, filter, sort, lockAllRows);
            }


            public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
            {
                var sc = GetSelectedColumns(_file.File, columns);
                return new myRow(this, sc,
                    _file.File.FillRawDataWithColumns(new byte[_file.File.GetMaxRowLength()], (i, alwaysSave, saver) => _table.Columns[i].SaveYourValueToDb(saver),
                        data => _file.File.BtrCall(Operations.Insert, data, _keyBuffer, _keyNumber), true),
                        new InsertedRowStateForMyRow(), false).GetRow(storage, sc);
            }

            class InsertedRowStateForMyRow : StateForMyRow
            {
                public void Deleted(myRow myRow)
                {
                }

                public void Updated(myRow myRow, IEnumerable<ColumnBase> columns)
                {
                }
            }

            public bool IsOrderBySupported(Sort sort)
            {
                return sort.Segments.Count == 0 || _table.Indexes.IndexOf(sort) != -1;
            }

            byte[] _keyBuffer = new byte[255];
            short _keyNumber = 0;
            byte[] _rawRow;
            bool _reloadRowDataBeforeUpdate;

            public long CountRows()
            {
                var dataBuffer = new byte[1920];
                var keyBuffer = new byte[255];
                _file.File.BtrCall(Operations.Stat, dataBuffer, keyBuffer, 0);
                return BitConverter.ToInt32(dataBuffer, 6);
            }

            public long GetFileSize()
            {
                var dataBuffer = new byte[1920];
                var keyBuffer = new byte[255];
                _file.File.BtrCall(Operations.Stat, dataBuffer, keyBuffer, 0);
                return BitConverter.ToInt32(dataBuffer, 6) * BitConverter.ToInt16(dataBuffer, 0);
            }
        }


        public void Dispose()
        {
        }

        public event FileOpenedEvent FileOpened;

        public delegate void FileOpenedEvent(Firefly.Box.Data.Entity e);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);
    }

    public interface IBtrieveValueLoader : IValueLoader
    {
        Time GetTime();
        Date GetDate();
    }

    public interface IBtrieveValueSaver : IValueSaver
    {
        void SaveInteger(Number value, byte[] valueBytes, int dataTypeCode);
        void SaveSingleDecimal(Number value);
        void SaveByteArray(byte[] value, IComparable comparable);
        void SaveTime(Time value);
        void SaveDate(Date value);
        void SaveByteArray(byte[] value, IComparable comparable, short btrieveExtendedDataType);
    }

    class BinaryValueLoader : IBtrieveValueLoader
    {
        byte[] _data;
        int _dataTypeCode;
        int _startIndex;
        int _length;

        public void SetData(byte[] rawData)
        {
            _data = rawData;
        }

        public BinaryValueLoader()
        {
        }
        public BinaryValueLoader GetFor(byte[] data, int dataTypeCode, int startIndex, int length)
        {
            SetData(data);
            _dataTypeCode = dataTypeCode;
            _startIndex = startIndex;
            _length = length;
            return this;
        }
        public BinaryValueLoader GetFor(int dataTypeCode, int startIndex, int length)
        {
            _dataTypeCode = dataTypeCode;
            _startIndex = startIndex;
            _length = length;
            return this;
        }


        public bool IsNull()
        {
            return _data == null || _length == 0;
        }

        public Number GetNumber()
        {
            if (_dataTypeCode == 1)
            {
                if (_length == 4)
                    return BitConverter.ToInt32(_data, _startIndex);
                if (_length == 8)
                    return BitConverter.ToInt64(_data, _startIndex);
            }
            if (_dataTypeCode == 14)
            {
                if (_length == 4)
                    return BitConverter.ToUInt32(_data, _startIndex);
                if (_length == 8)
                    throw new NotImplementedException();
            }
            if (_dataTypeCode == 2)
            {
                if (_length == 4)
                    return BitConverter.ToSingle(_data, _startIndex);
                if (_length == 8)
                {
                    try
                    {
                        return BitConverter.ToDouble(_data, _startIndex);
                    }
                    catch (OverflowException)
                    {
                        return 0;
                    }
                }
            }
            throw new NotImplementedException();
        }

        public string GetString()
        {
            if (_dataTypeCode == 25)
                return Encoding.Unicode.GetString(_data, _startIndex, _length);
            return LocalizationInfo.Current.OuterEncoding.GetString(_data, _startIndex, _length);
        }

        public DateTime GetDateTime()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetTimeSpan()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean()
        {
            return _data[_startIndex] != 0;
        }

        public byte[] GetByteArray()
        {
            var result = new byte[_length];
            Array.Copy(_data, _startIndex, result, 0, _length);
            return result;
        }

        public Time GetTime()
        {
            return new Time(_data[_startIndex + 3], _data[_startIndex + 2], _data[_startIndex + 1]);
        }

        public Date GetDate()
        {
            return new Date(_data[_startIndex + 3] * 256 + _data[_startIndex + 2], _data[_startIndex + 1], _data[_startIndex]);
        }


    }

    class BinaryValueSaver : IBtrieveValueSaver, IFilterItemSaver
    {
        public byte[] ValueByteArray;
        public int DataTypeCode = 0;
        public object Comparable;
        public bool ValueWasByteArray = false;
        public string ComparableForCaseInsensitive;

        bool _padValueWith256;

        public BinaryValueSaver() { }
        public BinaryValueSaver(bool padValueWith256)
        {
            _padValueWith256 = padValueWith256;
        }

        void SetBytes(byte[] bytes, int size)
        {
            var bytesLength = bytes.Length;
            if (bytesLength != size)
            {
                Array.Resize(ref bytes, size);
                if (_padValueWith256)
                    for (var i = bytesLength; i < size; i++)
                        bytes[i] = (byte)255;
            }
            ValueByteArray = bytes;
        }

        public void SaveInt(int value)
        {
            DataTypeCode = 1;
            SetBytes(BitConverter.GetBytes(value), 4);
            Comparable = value;
        }

        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
            DataTypeCode = 2;

            if (value == 0)
            {
                SetBytes(new byte[8], 8);
                Comparable = 0d;
                return;
            }

            var d = decimal.ToDouble(value);
            var bytes = BitConverter.GetBytes(d);

            if (BitConverter.GetBytes(decimal.GetBits(value)[3])[2] > 0)
                value = value / 1.000000000000000000000000000000000m;
            if (BitConverter.GetBytes(decimal.GetBits(value)[3])[2] > 0 && (bytes[0] != 0 || bytes[1] != 0 || bytes[2] != 0))
            {
                var bits = BitConverter.DoubleToInt64Bits(d);
                var dec1 = decimal.Parse(BitConverter.Int64BitsToDouble(bits + 1).ToString("R"), System.Globalization.NumberStyles.Float);
                var dec2 = decimal.Parse(BitConverter.Int64BitsToDouble(bits - 1).ToString("R"), System.Globalization.NumberStyles.Float);

                short i = 0;
                if (d > 0 && dec1 - value >= value - dec2)
                    i = -1;
                if (d < 0 && dec1 - value <= value - dec2)
                    i = 1;
                if (i != 0)
                {
                    bits += i;
                    d = BitConverter.Int64BitsToDouble(bits);
                    bytes = BitConverter.GetBytes(d);
                }
            }

            SetBytes(bytes, 8);
            Comparable = d;
        }

        public void SaveString(string value, int length, bool fixedWidth)
        {
            DataTypeCode = 25;
            SetBytes(Encoding.Unicode.GetBytes(value), length * 2);
            Comparable = Encoding.Unicode.GetBytes(value);
            ComparableForCaseInsensitive = value;
        }

        public void SaveAnsiString(string value, int length, bool fixedWidth)
        {
            DataTypeCode = 0;
            var bytes = LocalizationInfo.Current.OuterEncoding.GetBytes(value);
            SetBytes(bytes, length);
            Comparable = bytes;
            ComparableForCaseInsensitive = value;
        }

        public void SaveNull()
        {
            ValueByteArray = new byte[] { };
        }

        public void SaveDateTime(DateTime value)
        {
            throw new NotImplementedException();
        }

        public void SaveTimeSpan(TimeSpan value)
        {
            throw new NotImplementedException();
        }

        public void SaveBoolean(bool value)
        {
            DataTypeCode = 0;
            SetBytes(new[] { (byte)(value ? 1 : 0) }, 1);
            Comparable = value;
        }

        public void SaveByteArray(byte[] value)
        {
            if (value == null)
                value = new byte[0];

            DataTypeCode = 0;
            SetBytes(value, value.Length);
            ValueWasByteArray = true;
        }

        public void SaveColumn(ColumnBase column)
        {
            throw new NotImplementedException();
        }

        public void SaveInteger(Number value, byte[] valueBytes, int dataTypeCode)
        {
            DataTypeCode = dataTypeCode;
            SetBytes(valueBytes, valueBytes.Length);
            Comparable = value;
        }
        static byte[] _zeroFloatBytes = new byte[] { 0, 0, 0, 0 };
        static float _zeroFloat = 0;
        public void SaveSingleDecimal(Number value)
        {
            DataTypeCode = 2;
            if (value == Number.Zero)
            {
                SetBytes(_zeroFloatBytes, 4);
                Comparable = _zeroFloat;
            }
            else
            {
                var f = decimal.ToSingle(value);
                var bts = BitConverter.GetBytes(f);

                if (Math.Abs((double)f) > Math.Abs(decimal.ToDouble(value)))
                    bts = BitConverter.GetBytes(BitConverter.ToUInt32(bts, 0) - 1);

                SetBytes(bts, 4);
                Comparable = f;
            }
        }

        public void SaveByteArray(byte[] value, IComparable comparable)
        {
            SaveByteArray(value);
            Comparable = comparable;
        }

        public void SaveTime(Time value)
        {
            DataTypeCode = 4;
            SetBytes(new byte[] { 0, (byte)value.Second, (byte)value.Minute, (byte)value.Hour }, 4);
            Comparable = value;
        }

        public void SaveDate(Date value)
        {
            DataTypeCode = 3;
            SetBytes(new[] { (byte)(value.Day), (byte)(value.Month), (byte)(value.Year % 256), (byte)(value.Year / 256) }, 4);
            Comparable = value;
        }

        public void SaveByteArray(byte[] value, IComparable comparable, short btrieveExtendedDataType)
        {
            SaveByteArray(value, comparable);
            DataTypeCode = btrieveExtendedDataType;
            ValueWasByteArray = false;
        }

        public void SaveEmptyDateTime()
        {
            throw new NotImplementedException();
        }
    }

    class BtrieveException : System.Exception
    {
        short _btrieveErrorCode;
        int _btrievePosition;

        public BtrieveException(short btrieveErrorCode, int btrievePosition)
        {
            _btrieveErrorCode = btrieveErrorCode;
            _btrievePosition = btrievePosition;
        }

        public override string Message
        {
            get
            {
                var result = string.Format("Btrieve Error {0}", _btrieveErrorCode);
                switch (_btrieveErrorCode)
                {
                    case 11:
                        result += " Invalid File Name";
                        break;
                    case 88:
                        result += " File Locked";
                        break;
                }
                return result;
            }
        }

        public BtrieveError ErrorCode
        {
            get { return (BtrieveError)_btrieveErrorCode; }
        }
        public int ErrorPosition { get { return _btrievePosition; } }
    }

    enum BtrieveError
    {
        InvalidFileName = 11
    }

    public class BtrieveSort : Index
    {
        public BtrieveSort(params ColumnBase[] columns)
            : base(columns)
        {
        }

        Dictionary<ColumnBase, int> _sizes = new Dictionary<ColumnBase, int>();
        public void ModifySegmentLength(ColumnBase column, int length)
        {
            if (_sizes.ContainsKey(column))
                _sizes[column] = length;
            else
                _sizes.Add(column, length);
        }

        internal int GetModifiedSegmentSize(ColumnBase column, int originalSize)
        {
            int size;
            return _sizes.TryGetValue(column, out size) ? size : originalSize;
        }

        internal int GetSize(SortSegment seg)
        {
            int size;
            return _sizes.TryGetValue(seg.Column, out size) ? size : 0;
        }
    }

    public class BtrievePositionColumn : NumberColumn
    {
        public BtrievePositionColumn()
        {
            Name = "s__sequence";
            Format = "18";
        }
    }
    public class InvalidTableStructureException : Exception
    {
        public InvalidTableStructureException() : base(LocalizationInfo.Current.InvalidTableStructure)
        {
        }
    }
}
