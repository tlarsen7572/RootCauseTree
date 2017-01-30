using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class SqliteDb
    {
        private static SqliteDb _Db = new SqliteDb();

        public string CurrentFile { get; private set; }

        private SqliteDb() { }

        public static SqliteDb GetInstance()
        {
            return _Db;
        }

        public bool CreateNewFile(string path)
        {
            CreateDbFile(path);
            CurrentFile = path;
            return true;
        }

        private void CreateDbFile(string path)
        {
            SQLiteConnection.CreateFile(path);
            string connStr = string.Concat("Data Source=", path, ";Version=3;");
            var conn = new SQLiteConnection(connStr);
            string sql =    @"DROP TABLE IF EXISTS toplevel;
                            DROP TABLE IF EXISTS nodes;
                            DROP TABLE IF EXISTS hierarchy;
                            CREATE TABLE toplevel (nodeid int);
                            CREATE TABLE nodes (nodeid int, nodetext text);
                            CREATE TABLE hierarchy (parentid int, childid int);";
            var command = new SQLiteCommand(sql, conn);
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }
    }
}
