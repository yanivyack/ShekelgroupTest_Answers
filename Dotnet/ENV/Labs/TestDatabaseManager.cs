using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;


namespace ENV.Labs
{
    public class TestDatabaseManager
    {
        static string Server = "(local)";
        string _dbname;
        DynamicSQLSupportingDataProvider _mockDBConnection;
        public TestDatabaseManager(string dbName)
        {
            _dbname = dbName;
            ConnectionManager.Context.AddMicrosoftSQLDatabase("MOCKDB", _dbname, Server);
            InitDb();
        }

        public void SetAsActiveTestingDB()
        {
            ConnectionManager.AlternateDb.Value = (s)=>_mockDBConnection;
        }

        void InitDb()
        {
            _mockDBConnection = ConnectionManager.GetSQLDataProvider("MOCKDB");
        }

        public void Create()
        {
            CreateDB(_dbname);
        }

        public void CopyTableData<T>() where T : Entity
        {
            CopyTableData<T>(null, null);
        }

        public void CopyTableData<T>(Action<T> foreachRow) where T : Entity
        {
            CopyTableData<T>(null, foreachRow);
        }
        public void CopyTableData<T>(Func<T, FilterBase> filter) where T : Entity
        {
            CopyTableData<T>(filter, null);
        }
        public void CopyTableData<T>(Func<T, FilterBase> filter, Action<T> foreachRow) where T : Entity
        {
            ConnectionManager.AlternateDb.Value = null;
            var source = (T)System.Activator.CreateInstance(typeof(T));
            ConnectionManager.AlternateDb.Value = s=>_mockDBConnection;
            var target = (T)System.Activator.CreateInstance(typeof(T));
            ConnectionManager.AlternateDb .Value= null;
            var bp = new BusinessProcess { From = source };
            if (filter != null)
            {
                var x = filter(source);
                if (x != null)
                    bp.Where.Add(x);

            }
            bp.Relations.Add(target, RelationType.Insert);
            bp.DatabaseErrorOccurred += e => e.HandlingStrategy = DatabaseErrorHandlingStrategy.Ignore;
            bool setIdentityInsert = false;
            if (target.IdentityColumn != null && target.IdentityColumn.DbReadOnly)
            {
                target.IdentityColumn.DbReadOnly = false;
                setIdentityInsert = true;

            }
            bp.ForEachRow(() =>
            {
                if (setIdentityInsert)
                {
                    _mockDBConnection.Execute(string.Format("SET IDENTITY_INSERT {0} ON", target.EntityName));
                    setIdentityInsert = false;
                }
                for (int i = 0; i < source.Columns.Count; i++)
                {
                    target.Columns[i].Value = source.Columns[i];
                }
                if (foreachRow != null)
                    foreachRow(source);
            });


        }


        public static void CreateDB(string name)
        {
            Execute(string.Format("Create database {0}", name));

        }

        public static void DropDB(string name)
        {
            Execute("ALTER DATABASE [{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE", name);
            Execute("drop database {0}", name);
        }

        static string TestDataFolder = @"c:\TestData\";
        public static void Backup(string name, string fileName)
        {
            var path = TestDataFolder + fileName + ".bak";
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            Execute(@"BACKUP DATABASE [{0}] TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'Test1-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10"
                , name, path);
        }


        public static void Restore(string name, string fileName)
        {
            Execute(@"RESTORE DATABASE [{0}] FROM  DISK = N'{1}.bak' WITH  FILE = 1,  NOUNLOAD,  STATS = 5"
                , name, TestDataFolder + fileName);
        }

        static DynamicSQLSupportingDataProvider DB;
        static void Execute(string sql, params object[] args)
        {
            if (DB == null)
            {
                var y = ConnectionManager.AlternateDb.Value;
                ConnectionManager.AlternateDb.Value = null;
                ConnectionManager.Context.AddMicrosoftSQLDatabase("MASTERCONNECTION", "master", Server);
                DB = ConnectionManager.GetSQLDataProvider("MASTERCONNECTION");
                ConnectionManager.AlternateDb.Value = y;
            }
            DB.Execute(string.Format(sql, args));
        }

        public void DropIfExists()
        {
            try
            {
                DropDB(_dbname);
            }
            catch (Exception)
            {


            }
        }

        public void SaveBackupFile(string backupName)
        {
            Backup(_dbname, backupName);
        }

        public void Drop()
        {
            ConnectionManager.Disconnect("MOCKDB");

            _mockDBConnection.Dispose();
            System.Data.SqlClient.SqlConnection.ClearAllPools();
            DropDB(_dbname);
            InitDb();
            if (ConnectionManager.AlternateDb.Value != null)
                ConnectionManager.AlternateDb.Value = s=>_mockDBConnection;
        }

        public void LoadData(string backupFileName)
        {
            Restore(_dbname, backupFileName);
        }
    }

}
/*
 Northwind sample test class
 
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Labs;
using NUnit.Framework;
using Firefly.Box.Testing;

namespace Northwind.Testing
{
    [TestFixture]
    public class TestClass
    {

        [Test]
        public void Test1()
        {
            tdm.LoadData("TestDataForTest1");

            var o = new ShowOrders();
            var st = new ShowOrders.Totals(o);
            o.Orders.OrderID.Value = 10248;
            st.Run();
            o.v_Total.ShouldBe(437);
        }
        [Test]
        public void Test2()
        {
            tdm.LoadData("TestDataForTest2");

            var o = new ShowOrders();
            var st = new ShowOrders.Totals(o);
            o.Orders.OrderID.Value = 10249;
            st.Run();
            o.v_Total.ShouldBe(1863.4);
        }

        TestDatabaseManager tdm = new TestDatabaseManager("a123");
        [SetUp]
        public void Setup()
        {
            ENV.Common.SuppressDialogs();
            tdm.SetAsActiveTestingDB();

        }

        [TearDown]
        public void Teardown()
        {
            tdm.Drop();
        }

        void CreateTestData()
        {
            Testing.CreateTestData.CreateTestDataForOrder(10248, "TestDataForTest1");
            Testing.CreateTestData.CreateTestDataForOrder(10249, "TestDataForTest2");
        }

    }
    class CreateTestData
    {
        static string dbName = "a123";
        public static void CreateTestDataForOrder(Firefly.Box.Number orderId, string backupName)
        {
            var t = new TestDatabaseManager(dbName);
            t.DropIfExists();
            t.Create();
            t.CopyTableData<Models.Orders>(
                //orders filter
                o => o.OrderID.IsEqualTo(orderId)
                ,
                //foreach row in orders
                o => t.CopyTableData<Models.OrderDetails>(
                    //order details filter
                    od => od.OrderID.IsEqualTo(o.OrderID)
                    ,
                    //order details for each row
                    od => t.CopyTableData<Models.Products>(
                        //products filter 
                        p => p.ProductID.IsEqualTo(od.ProductID)))
                );
            t.SaveBackupFile(backupName);

        }
    }
}

 
 */