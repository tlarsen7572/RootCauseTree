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
            var command = CreateNewCommand();
            int records = ExecuteQuery(command,sql ,parms);
            CommitAndCleanUp(command);

            return records == 1 ? true : false;
        }

        public bool AddLink(Node parentNode, Node childNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parent", parentNode.NodeId),
                new SQLiteParameter("$child", childNode.NodeId),
            };
            var command = CreateNewCommand();
            int records = ExecuteQuery(command,@"INSERT INTO hierarchy (parentid,childid) VALUES ($parent,$child);", parms);
            CommitAndCleanUp(command);

            return records == 1 ? true : false;
        }

        public bool RemoveLink(Node parentNode, Node childNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parentnode", parentNode.NodeId),
                new SQLiteParameter("$childnode", childNode.NodeId),
            };
            var command = CreateNewCommand();
            int records = ExecuteQuery(command,@"DELETE FROM hierarchy WHERE parentid = $parentnode AND childid = $childnode;", parms,true);
            CommitAndCleanUp(command);

            return records == 1 ? true : false;
        }

        public bool ChangeNodeText(Node node, string newText)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$newtext", newText),
                new SQLiteParameter("$nodeid", node.NodeId),
            };
            var command = CreateNewCommand();
            int records = ExecuteQuery(command,@"UPDATE nodes SET nodetext = $newtext WHERE nodeid = $nodeid;", parms);
            CommitAndCleanUp(command);

            return records == 1 ? true : false;
        }

        public bool AddNode(Node parentNode, Node newNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$newid", newNode.NodeId),
                new SQLiteParameter("$newtext", newNode.Text),
                new SQLiteParameter("$parentid", parentNode.NodeId),
            };
            string sql =
@"INSERT INTO nodes (nodeid,nodetext) VALUES ($newid,$newtext);
INSERT INTO hierarchy (parentid,childid) VALUES ($parentid,$newid);";

            var command = CreateNewCommand();
            int records = ExecuteQuery(command,sql, parms);
            CommitAndCleanUp(command);

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

        public bool MoveNode(Node movingNode, Node targetNode)
        {
            throw new NotImplementedException();
        }

        public bool UndoMoveNode(Node movingNode, Node targetNode, IEnumerable<Node> oldParents)
        {
            throw new NotImplementedException();
        }

        public bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents)
        {
            throw new NotImplementedException();
        }

        public bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents, IEnumerable<Node> oldNodes,Dictionary<Node,Node> parentChildLinks)
        {
            throw new NotImplementedException();
        }

        public bool UndoRemoveLink(Node parentNode,Node childNode)
        {
            var command = CreateNewCommand();
            int records = RecurseUndoRemoveLinks(command, parentNode, childNode);
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        private int RecurseUndoRemoveLinks(SQLiteCommand command, Node parentNode, Node childNode)
        {
            SQLiteParameter[] parms = new SQLiteParameter[]
            {
                new SQLiteParameter("$parentid", parentNode.NodeId),
                new SQLiteParameter("$childid", childNode.NodeId),
                new SQLiteParameter("$childtext", childNode.Text),
            };
            string sql =
@"INSERT OR IGNORE INTO nodes (nodeid,nodetext) VALUES ($childid,$childtext);
INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parentid,$childid);";

            int records = ExecuteQuery(command, sql, parms);

            foreach (var child in childNode.ChildNodes)
            {
                records = records + RecurseUndoRemoveLinks(command, childNode, child);
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
            string sql = 
@"DROP TABLE IF EXISTS toplevel;
DROP TABLE IF EXISTS nodes;
DROP TABLE IF EXISTS hierarchy;
CREATE TABLE toplevel (nodeid BIGINT, PRIMARY KEY (nodeid)) WITHOUT ROWID;
CREATE TABLE nodes (nodeid BIGINT, nodetext text, PRIMARY KEY (nodeid)) WITHOUT ROWID;
CREATE TABLE hierarchy (parentid BIGINT, childid BIGINT, PRIMARY KEY (parentid, childid)) WITHOUT ROWID;";

            var command = CreateNewCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
            CommitAndCleanUp(command);
        }

        private int ExecuteQuery(SQLiteCommand command, string sql, SQLiteParameter[] parms, bool doOrphanCleanup = false)
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(parms);
            int records = command.ExecuteNonQuery();
            if (doOrphanCleanup) { CleanUpOrphans(command); }
            return records;
        }

        private SQLiteCommand CreateNewCommand()
        {
            string connStr = string.Concat("Data Source=", CurrentFile, ";Version=3;");
            var conn = new SQLiteConnection(connStr);
            conn.Open();
            var command = conn.CreateCommand();
            command.Transaction = conn.BeginTransaction();
            return command;
        }

        private void CleanUpOrphans(SQLiteCommand command)
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

            command.CommandText = sql;
            command.Parameters.Clear();
            int deletedRecords;

            do
            {
                deletedRecords = command.ExecuteNonQuery();
            } while (deletedRecords > 0);
        }

        private void CommitAndCleanUp(SQLiteCommand command)
        {
            command.Transaction.Commit();
            command.Connection.Close();
            command.Transaction.Dispose();
            command.Connection.Dispose();
            command.Dispose();
        }
    }
}
