using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class SqliteDb : IRootCauseDb
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
            CurrentFile = path;
            CreateDbFile();
            return true;
        }

        public bool InsertTopLevel(Node node)
        {
            var conn = CreateConnection();
            string sql =    @"INSERT INTO toplevel (nodeid) VALUES ($newid);
                            INSERT INTO nodes (nodeid,nodetext) VALUES ($newid,$newtext);";
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddWithValue("$newid", node.NodeId);
            command.Parameters.AddWithValue("$newtext", node.Text);
            conn.Open();
            int records = command.ExecuteNonQuery();
            CleanUp(conn, command);

            return records == 1 ? true : false;
        }

        public bool AddLink(Node startNode, Node endNode)
        {
            throw new NotImplementedException();
        }

        public bool RemoveLink(Node startNode, Node endNode)
        {
            throw new NotImplementedException();
        }

        public bool ChangeNodeText(Node node, string newText)
        {
            throw new NotImplementedException();
        }

        public bool AddNode(Node startNode, Node newNode)
        {
            var conn = CreateConnection();
            string sql =    @"INSERT INTO nodes (nodeid,nodetext) VALUES ($newid,$newtext);
                            INSERT INTO hierarchy (parentid,childid) VALUES ($parentid,$newid);";
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddWithValue("$newid", newNode.NodeId);
            command.Parameters.AddWithValue("$newtext", newNode.Text);
            command.Parameters.AddWithValue("$parentid", startNode.NodeId);
            conn.Open();
            int records = command.ExecuteNonQuery();
            CleanUp(conn, command);

            return records == 1 ? true : false;
        }

        public bool RemoveNode(Node removeNode)
        {
            throw new NotImplementedException();
        }

        public bool RemoveNodeChain(Node removeNode)
        {
            throw new NotImplementedException();
        }

        public bool MoveNode(Node node, Node targetNode)
        {
            throw new NotImplementedException();
        }

        public bool UndoMoveNode(Node node, Node targetNode, IEnumerable<Node> oldParents)
        {
            throw new NotImplementedException();
        }

        public bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents)
        {
            throw new NotImplementedException();
        }

        public bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents, IEnumerable<Node> oldNodes)
        {
            throw new NotImplementedException();
        }

        public bool RemoveTopLevel(Node node)
        {
            throw new NotImplementedException();
        }

        private void CreateDbFile()
        {
            SQLiteConnection.CreateFile(CurrentFile);
            var conn = CreateConnection();
            string sql = @"DROP TABLE IF EXISTS toplevel;
                            DROP TABLE IF EXISTS nodes;
                            DROP TABLE IF EXISTS hierarchy;
                            CREATE TABLE toplevel (nodeid BIGINT, PRIMARY KEY (nodeid)) WITHOUT ROWID;
                            CREATE TABLE nodes (nodeid BIGINT, nodetext text, PRIMARY KEY (nodeid)) WITHOUT ROWID;
                            CREATE TABLE hierarchy (parentid BIGINT, childid BIGINT, PRIMARY KEY (parentid, childid)) WITHOUT ROWID;";
            var command = new SQLiteCommand(sql, conn);
            conn.Open();
            command.ExecuteNonQuery();
            CleanUp(conn, command);
        }

        private SQLiteConnection CreateConnection()
        {
            string connStr = string.Concat("Data Source=", CurrentFile, ";Version=3;");
            return new SQLiteConnection(connStr);
        }

        private void CleanUp(SQLiteConnection conn, SQLiteCommand command)
        {
            command.Dispose();
            conn.Close();
            conn.Dispose();
        }
    }
}
