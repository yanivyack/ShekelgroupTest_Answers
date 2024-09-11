using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENV.Utilities
{
    public class DatabaseUpgradeBase
    {
        class CurrentDbVersion : Entity
        {
            [PrimaryKey]
            public NumberColumn RowIndex = new NumberColumn("RowIndex", "1");
            public NumberColumn Verision = new NumberColumn("DbVerison", "9");
            public CurrentDbVersion(IEntityDataProvider source, string name) : base(name, source)
            {
            }
        }
        CurrentDbVersion _ver;
        DynamicSQLSupportingDataProvider _dataSource;
        int _currentDbVersion;

        public DatabaseUpgradeBase(DynamicSQLSupportingDataProvider dataSource, string name = "CurrentDbVersion")
        {
            _ver = new CurrentDbVersion(dataSource, name);
            _dataSource = dataSource;
            Init();
        }
        internal void DropDatabaseVersionTable()
        {
            _ver.Drop();
            Init();
        }
        void Init()
        {
            if (!_ver.Exists())
                _dataSource.CreateTable(_ver);
            _currentDbVersion = GetDatabaseVersion();
        }

        internal int GetDatabaseVersion()
        {
            var r = 0;
            _ver.ForEachRow(_ver.RowIndex.IsEqualTo(1), () => r = _ver.Verision.Value);
            return r;
        }

        protected internal void Step(int stepNumber, Action whatToDo, string description = null)
        {
            if (stepNumber > _currentDbVersion + 1)
            {
                throw new InvalidStepNumberException((_currentDbVersion + 1), stepNumber);
            }

            if (stepNumber <= _currentDbVersion)
            {
                return;
            }
            try
            {
                whatToDo();
                SetDatabaseVersion(stepNumber);
            }
            catch(Exception ex)
            {
                throw new UpgradeStepFailed(stepNumber, ex, description);
            }
        }
        protected virtual void BuildSteps()
        {
        }
        public void Run()
        {
            BuildSteps();
        }

        internal void SetDatabaseVersion(int databaseVersion)
        {
            _ver.InsertIfNotFound(_ver.RowIndex.BindEqualTo(1), () =>
            {
                _ver.Verision.Value = databaseVersion;
            });
            _currentDbVersion = databaseVersion;
        }
        internal class InvalidStepNumberException : Exception
        {
            public InvalidStepNumberException(int expectedStep, int gotStep) : base("Expected step number " + (expectedStep) + ", but got " + gotStep)
            {
            }
        }

       

        internal class UpgradeStepFailed : Exception
        {
            public UpgradeStepFailed(int stepNumber, Exception exception, string description):base("Failed to perform database step "+stepNumber+(!string.IsNullOrWhiteSpace(description)?" "+description:"")+":"+exception.Message,exception)
            {
            }
        }
        

    }

}
