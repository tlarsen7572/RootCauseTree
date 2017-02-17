using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class SqliteDb : IRootCauseDb
    {
        private static SqliteDb _Db = new SqliteDb();
        private SQLiteConnection conn;

        public string CurrentFile { get; private set; }
        public SQLiteConnection Conn { get { return conn; } }

        private SqliteDb() { }

        public static SqliteDb GetInstance()
        {
            return _Db;
        }

        public SQLiteConnection CreateNewFile(string path)
        {
            if (conn != null) { CloseConnection(); }
            CurrentFile = path;
            CreateDbFile();
            return conn;
        }

        public IEnumerable<ProblemContainer> LoadFile(string path)
        {
            if (conn != null) { CloseConnection(); }
            ConfigureConnectionAndOpen(path);

            if (!IsSchemaCorrect())
            {
                CloseConnection();
                throw new InvalidRootCauseFileException();
            }

            List < ProblemContainer > problems = new List<ProblemContainer>();
            HashSet<long[]> hierarchy = new HashSet<long[]>();
            Dictionary<long, Node> nodes = new Dictionary<long, Node>();
            ProblemContainer problem;
            Node node;

            var command = CreateNewCommand();
            command.CommandText = "PRAGMA locking_mode=EXCLUSIVE;";
            command.ExecuteNonQuery();
            CommitAndCleanUp(command);

            command = CreateNewCommand();
            command.CommandText = 
@"SELECT a.nodeid,b.nodetext FROM toplevel a JOIN nodes b ON a.nodeid = b.nodeid;
SELECT * FROM hierarchy;
SELECT a.nodeid,a.nodetext FROM nodes a LEFT JOIN toplevel b ON a.nodeid = b.nodeid WHERE b.nodeid IS NULL;";

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                node = NodeFactory.CreateProblem(reader["nodetext"].ToString(), (long)reader["nodeid"]);
                problem = new ProblemContainer(node);
                problems.Add(problem);
                nodes.Add(node.NodeId, node);
            }

            reader.NextResult();
            while (reader.Read())
            {
                hierarchy.Add(new long[2] { (long)reader["parentid"], (long)reader["childid"] });
            }

            reader.NextResult();
            while (reader.Read())
            {
                node = NodeFactory.CreateCause(reader["nodetext"].ToString(), (long)reader["nodeid"]);
                nodes.Add(node.NodeId, node);
            }

            NullDb db = new NullDb();
            foreach (var link in hierarchy)
            {
                new AddLinkCommand(db, nodes[link[0]], nodes[link[1]],true);
            }
            CommitAndCleanUp(command);

            return problems;
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

            return records > 0 ? true : false;
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

            return records > 0 ? true : false;
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

            return records > 0 ? true : false;
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

            return records > 0 ? true : false;
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

            return records > 0 ? true : false;
        }

        public bool RemoveNode(Node removeNode)
        {
            var command = CreateNewCommand();
            command.CommandText =
@"DELETE FROM hierarchy WHERE parentid = $nodeid OR childid = $nodeid;
DELETE FROM nodes WHERE nodeid = $nodeid;";
            command.Parameters.AddWithValue("$nodeid", removeNode.NodeId);
            int records = command.ExecuteNonQuery();

            foreach (var parent in removeNode.ParentNodes)
            {
                foreach (var child in removeNode.ChildNodes)
                {
                    command.Parameters.Clear();
                    command.CommandText = @"INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parent,$child);";
                    command.Parameters.AddWithValue("$parent", parent.NodeId);
                    command.Parameters.AddWithValue("$child", child.NodeId);
                    command.ExecuteNonQuery();
                }
            }
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        public bool RemoveNodeChain(Node removeNode)
        {
            var command = CreateNewCommand();
            var sql =
@"DELETE FROM hierarchy WHERE parentid = $nodeid OR childid = $nodeid;
DELETE FROM nodes WHERE nodeid = $nodeid;";
            SQLiteParameter[] parms = new SQLiteParameter[1] { new SQLiteParameter("$nodeid", removeNode.NodeId) };
            int records = ExecuteQuery(command, sql, parms, true);
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        public bool MoveNode(Node movingNode, Node targetNode)
        {
            var command = CreateNewCommand();
            var sql =
@"DELETE FROM hierarchy WHERE childid = $movingnode;
INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($targetnode,$movingnode);";
            SQLiteParameter[] parms = new SQLiteParameter[2]
            {
                new SQLiteParameter("$movingnode", movingNode.NodeId),
                new SQLiteParameter("$targetnode",targetNode.NodeId)
            };
            int records = ExecuteQuery(command, sql, parms);
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        public bool UndoMoveNode(Node movingNode, IEnumerable<Node> oldParents)
        {
            var command = CreateNewCommand();
            var sql = @"DELETE FROM hierarchy WHERE childid = $movingnode;";
            SQLiteParameter[] parms = new SQLiteParameter[1] { new SQLiteParameter("$movingnode", movingNode.NodeId) };
            int records = ExecuteQuery(command, sql, parms);

            command.CommandText = @"INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parent,$child)";
            foreach (var oldParent in oldParents)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("$parent", oldParent.NodeId);
                command.Parameters.AddWithValue("$child", movingNode.NodeId);
                records = records + command.ExecuteNonQuery();
            }
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        public bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents)
        {
            var command = CreateNewCommand();

            //Insert the removed node
            var sql =
@"INSERT INTO nodes (nodeid,nodetext) VALUES ($removednode,$removednodetext);";
            SQLiteParameter[] parms = new SQLiteParameter[2] 
            {
                new SQLiteParameter("$removednode", removeNode.NodeId),
                new SQLiteParameter("$removednodetext",removeNode.Text)
            };
            int records = ExecuteQuery(command, sql, parms);

            //Rebuild the links between the removed node and its old parents
            command.CommandText =
@"INSERT INTO hierarchy(parentid, childid) VALUES($parentnode,$removednode);";
            foreach (var parent in oldParents)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("$parentnode", parent.NodeId);
                command.Parameters.AddWithValue("$removednode", removeNode.NodeId);
                records = records + command.ExecuteNonQuery();
            }

            //Recreate the nodes and links under the removed node
            command.CommandText =
@"INSERT OR IGNORE INTO nodes (nodeid,nodetext) VALUES ($child,$childtext);
INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parent,$child);";

            Func<SQLiteCommand, Node,int> rebuilder = null;
            rebuilder = (SQLiteCommand recurseCommand, Node parent) =>
            {
                int recurseRecords = 0;
                foreach (var child in parent.ChildNodes)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$child", child.NodeId);
                    command.Parameters.AddWithValue("$childtext", child.Text);
                    command.Parameters.AddWithValue("$parent", parent.NodeId);
                    recurseRecords = command.ExecuteNonQuery();
                    recurseRecords = recurseRecords + rebuilder(recurseCommand, child);
                }
                return recurseRecords;
            };

            records = records + rebuilder(command, removeNode);
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        public bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents, IEnumerable<Node> oldChildren,Dictionary<Node,Node> parentChildLinks)
        {
            var command = CreateNewCommand();

            //Add deleted node back to the list of nodes
            command.CommandText = @"INSERT INTO nodes (nodeid,nodetext) VALUES ($id,$text);";
            command.Parameters.AddWithValue("$id", removeNode.NodeId);
            command.Parameters.AddWithValue("$text", removeNode.Text);
            int records = command.ExecuteNonQuery();

            //Remove old links and add back links between the parents and the deleted node
            command.CommandText =
@"DELETE FROM hierarchy WHERE parentid = $parent AND childid = $child;
INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parent,$deletednode)";
            foreach (var parent in oldParents)
            {
                foreach (var child in oldChildren)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$parent", parent.NodeId);
                    command.Parameters.AddWithValue("$child", child.NodeId);
                    command.Parameters.AddWithValue("$deletednode", removeNode.NodeId);
                    records = records + command.ExecuteNonQuery();
                }
            }

            //Add back links between the children and the deleted node
            command.CommandText = $"INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($deletednode,$child);";
            foreach (var child in oldChildren)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("$child", child.NodeId);
                command.Parameters.AddWithValue("$deletednode", removeNode.NodeId);
                records = records + command.ExecuteNonQuery();
            }

            //Add back original links between the parents and children
            command.CommandText = $"INSERT OR IGNORE INTO hierarchy (parentid,childid) VALUES ($parent,$child);";
            foreach (var link in parentChildLinks)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("$child", link.Key.NodeId);
                command.Parameters.AddWithValue("$parent", link.Value.NodeId);
                records = records + command.ExecuteNonQuery();
            }

            CommitAndCleanUp(command);
            return records > 0 ? true : false;
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
            var command = CreateNewCommand();
            string sql =
@"DELETE FROM toplevel WHERE nodeid = $toplevel;
DELETE FROM nodes WHERE nodeid = $toplevel;
DELETE FROM hierarchy WHERE parentid = $toplevel OR childid = $toplevel;";
            SQLiteParameter[] parms = new SQLiteParameter[1] { new SQLiteParameter("$toplevel", node.NodeId) };
            int records = ExecuteQuery(command, sql, parms, true);
            CommitAndCleanUp(command);

            return records > 0 ? true : false;
        }

        private void CreateDbFile()
        {
            SQLiteConnection.CreateFile(CurrentFile);
            ConfigureConnectionAndOpen(CurrentFile);
            string sql = 
@"PRAGMA locking_mode=EXCLUSIVE;
DROP TABLE IF EXISTS toplevel;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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
            var command = conn.CreateCommand();
            try
            {
                command.Transaction = conn.BeginTransaction();
            }
            catch (SQLiteException)
            {
                command.Dispose();
                throw new InvalidRootCauseFileException();
            }
            catch (Exception)
            {
                command.Dispose();
                throw;
            }
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
            command.Transaction.Dispose();
            command.Dispose();
        }

        private void ConfigureConnectionAndOpen(string path)
        {
            if (IsFileLocked(path))
            {
                throw new RootCauseFileLockedException();
            }

            if (new Uri(path).IsUnc) CurrentFile = $"\\\\{path}";
            else CurrentFile = path;

            string connStr = string.Concat("Data Source=", CurrentFile, ";Version=3;");
            conn = new SQLiteConnection(connStr);
            conn.Open();
        }

        private bool IsFileLocked(string path)
        {
            System.IO.FileStream stream = null;

            try
            {
                stream = new System.IO.FileInfo(path).Open(System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
            }
            catch (System.IO.IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    { stream.Dispose(); };
            }
            return false;
        }

        public void CloseConnection()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
            GC.Collect();
        }

        private bool IsSchemaCorrect()
        {
            var columns = new List<string>();
            var command = CreateNewCommand();
            command.CommandText =
@"PRAGMA table_info('nodes');
PRAGMA table_info('hierarchy');
PRAGMA table_info('toplevel');";

            SQLiteDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (SQLiteException)
            {
                CloseConnection();
                command.Dispose();
                throw new InvalidRootCauseFileException();
            }
            catch (Exception)
            {
                CloseConnection();
                command.Dispose();
                throw;
            }

            while (reader.Read())
            {
                columns.Add($"nodes.{reader["name"].ToString()}");
            }

            reader.NextResult();
            while (reader.Read())
            {
                columns.Add($"hierarchy.{reader["name"].ToString()}");
            }

            reader.NextResult();
            while (reader.Read())
            {
                columns.Add($"toplevel.{reader["name"].ToString()}");
            }
            CommitAndCleanUp(command);

            var schema = new string[]
            {
                "nodes.nodeid",
                "nodes.nodetext",
                "hierarchy.parentid",
                "hierarchy.childid",
                "toplevel.nodeid"
            };
            IEnumerable<string> validColumns = from column in columns
                                               where schema.Contains(column)
                                               select column;
            return validColumns.Count() == schema.Length;
        }
    }
}
