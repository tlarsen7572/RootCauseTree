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
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$newid", node.NodeId),
                new SQLiteParameter("$newtext", node.Text),
            };
            string sql =
@"INSERT INTO toplevel (nodeid) VALUES ($newid);
INSERT INTO nodes (nodeid,nodetext) VALUES ($newid,$newtext);";

            int records = ExecuteQuery(sql ,parms);

            return records == 1 ? true : false;
        }

        public bool AddLink(Node startNode, Node endNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parent", startNode.NodeId),
                new SQLiteParameter("$child", endNode.NodeId),
            };
            int records = ExecuteQuery(@"INSERT INTO hierarchy (parentid,childid) VALUES ($parent,$child);", parms);

            return records == 1 ? true : false;
        }

        public bool RemoveLink(Node startNode, Node endNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parentnode", startNode.NodeId),
                new SQLiteParameter("$childnode", endNode.NodeId),
            };
            int records = ExecuteQuery(@"DELETE FROM hierarchy WHERE parentid = $parentnode AND childid = $childnode;", parms,true);

            return records == 1 ? true : false;
        }

        public bool ChangeNodeText(Node node, string newText)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$newtext", newText),
                new SQLiteParameter("$nodeid", node.NodeId),
            };
            int records = ExecuteQuery(@"UPDATE nodes SET nodetext = $newtext WHERE nodeid = $nodeid;", parms);

            return records == 1 ? true : false;
        }

        public bool AddNode(Node startNode, Node newNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$newid", newNode.NodeId),
                new SQLiteParameter("$newtext", newNode.Text),
                new SQLiteParameter("$parentid", startNode.NodeId),
            };
            string sql =
@"INSERT INTO nodes (nodeid,nodetext) VALUES ($newid,$newtext);
INSERT INTO hierarchy (parentid,childid) VALUES ($parentid,$newid);";

            int records = ExecuteQuery(sql, parms);

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

        public bool UndoRemoveLink(Node startNode,Node endNode)
        {
            var conn = CreateConnection();
            conn.Open();
            int records = RecurseUndoRemoveLinks(conn, startNode, endNode);
            conn.Close();
            conn.Dispose();

            return records > 0 ? true : false;
        }

        private int RecurseUndoRemoveLinks(SQLiteConnection conn, Node startNode, Node endNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parentid", startNode.NodeId),
                new SQLiteParameter("$childid", endNode.NodeId),
                new SQLiteParameter("$childtext", endNode.Text),
            };
            string sql =
@"INSERT OR IGNORE INTO nodes (nodeid,nodetext) VALUES ($childid,$childtext);
INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parentid,$childid);";
            int records = ExecuteQuery(conn, sql, parms);

            foreach (var child in endNode.Nodes)
            {
                records = records + RecurseUndoRemoveLinks(conn, endNode, child);
            }
            return records;
        }

        public bool RemoveTopLevel(Node node)
        {
            throw new NotImplementedException();
        }

        private void CreateDbFile()
        {
            SQLiteConnection.CreateFile(CurrentFile);
            var conn = CreateConnection();
            string sql = 
@"DROP TABLE IF EXISTS toplevel;
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

        private int ExecuteQuery(SQLiteConnection conn, string sql, SQLiteParameter[] parms, bool doOrphanCleanup = false)
        {
            var command = new SQLiteCommand(sql, conn);
            command.Parameters.AddRange(parms);
            int records = command.ExecuteNonQuery();
            if (doOrphanCleanup) { CleanUpOrphans(conn); }
            command.Dispose();
            return records;
        }

        private int ExecuteQuery(string sql,SQLiteParameter[] parms,bool doOrphanCleanup = false)
        {
            var conn = CreateConnection();
            conn.Open();
            int records = ExecuteQuery(conn,sql, parms, doOrphanCleanup);
            conn.Close();
            conn.Dispose();
            return records;
        }

        private SQLiteConnection CreateConnection()
        {
            string connStr = string.Concat("Data Source=", CurrentFile, ";Version=3;");
            return new SQLiteConnection(connStr);
        }

        private void CleanUpOrphans(SQLiteConnection conn)
        {
            string sql = 
@"DROP TABLE IF EXISTS t_orphans;
CREATE TEMPORARY TABLE t_orphans AS
SELECT a.nodeid FROM nodes a
LEFT JOIN toplevel b ON a.nodeid = b.nodeid
LEFT JOIN hierarchy c ON a.nodeid = c.childid
WHERE b.nodeid IS NULL AND c.childid IS NULL;
DELETE FROM nodes WHERE nodeid IN t_orphans;
DELETE FROM hierarchy WHERE parentid IN t_orphans;";

            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int deletedRecords;

            do
            {
                deletedRecords = command.ExecuteNonQuery();
            } while (deletedRecords > 0);
            command.Dispose();
        }

        private void CleanUp(SQLiteConnection conn, SQLiteCommand command)
        {
            command.Dispose();
            conn.Close();
            conn.Dispose();
        }
    }
}
