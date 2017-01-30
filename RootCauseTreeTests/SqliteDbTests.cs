using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;
using System.Data.SQLite;

namespace com.PorcupineSupernova.RootCauseTreeTests
{
    [TestClass]
    public class SqliteDbTests
    {
        [TestMethod]
        public void TestSingletonPattern()
        {
            SqliteDb db1 = SqliteDb.GetInstance();
            SqliteDb db2 = SqliteDb.GetInstance();
            Assert.ReferenceEquals(db1, db2);
        }

        [TestMethod]
        public void CreateNewFile()
        {
            SqliteDb db = SqliteDb.GetInstance();
            string file = string.Concat(System.IO.Directory.GetCurrentDirectory(), "test.rootcause");
            bool result = db.CreateNewFile(file);
            Assert.AreEqual(true, result);
            Assert.IsTrue(System.IO.File.Exists(file));
            string connStr = string.Concat("Data Source=", file, ";Version=3;");
            var conn = new SQLiteConnection(connStr);
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('toplevel','nodes','hierarchy') ORDER BY name;";
            var command = new SQLiteCommand(sql, conn);
            conn.Open();
            SQLiteDataReader reader = command.ExecuteReader();
            int records = 0;
            string[] names = new string[3];
            while (reader.Read())
            {
                names[records] = reader["name"] as string;
                records++;
            }
            conn.Close();
            Assert.AreEqual(3, records);
            Assert.AreEqual("hierarchy,nodes,toplevel", string.Join(",", names));
        }
    }
}
