using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;
using System.Data.SQLite;
using System.Collections.Generic;

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
        public void TestSqliteDbInterface()
        {
            //Run tests sequentially
            //These tests are run sequentially because they will be using the same disk file
            //The tests are not interdependent on each other; the disk file is overwritten with each new test
            TestCreateNewFile();
            TestCreateProblemStatement();
            TestCreateNewNode();
            TestChangeNodeText();
            TestAddLink();
            TestRemoveLink();
            TestRemoveFinalLink();
            TestUndoRemoveFinalLink();
        }

        //TESTS
        private void TestCreateNewFile()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('toplevel','nodes','hierarchy') ORDER BY name;";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            int records = 0;
            string[] names = new string[3];
            while (reader.Read())
            {
                names[records] = reader["name"] as string;
                records++;
            }
            Assert.AreEqual(3, records);
            Assert.AreEqual("hierarchy,nodes,toplevel", string.Join(",", names));

            CleanUpDbObjects(conn, command, reader);
        }

        private void TestCreateProblemStatement()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            var node = NodeFactory.CreateProblem("This is my problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(node);
            string nodeId = node.NodeId.ToString();
            string sql = $"SELECT count(*) AS result FROM toplevel WHERE nodeid = '{nodeId}'";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            Assert.AreEqual("1", reader["result"].ToString());
            reader.Close();
            command.Dispose();

            sql = $"SELECT * FROM nodes WHERE nodeid = '{nodeId}'";
            command = new SQLiteCommand(sql, conn);
            reader = command.ExecuteReader();
            reader.Read();
            Assert.AreEqual(nodeId, reader["nodeid"].ToString());
            Assert.AreEqual("This is my problem", reader["nodetext"]);

            CleanUpDbObjects(conn,command, reader);
        }

        private void TestCreateNewNode()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);

            var node = NodeFactory.CreateCause("Node 1", SequentialId.NewId());
            SqliteDb.GetInstance().AddNode(problem,node);

            string sql = $"SELECT * FROM nodes WHERE nodeid = '{node.NodeId}'";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            Assert.AreEqual("Node 1", reader["nodetext"]);
            reader.Close();
            command.Dispose();

            sql = $"SELECT count(*) AS result FROM hierarchy WHERE parentid = '{problem.NodeId}' AND childid = '{node.NodeId}';";
            command = new SQLiteCommand(sql, conn);
            reader = command.ExecuteReader();
            reader.Read();
            Assert.AreEqual("1", reader["result"].ToString());

            CleanUpDbObjects(conn, command, reader);
        }

        private void TestChangeNodeText()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);
            SqliteDb.GetInstance().ChangeNodeText(problem, "This is my problem");

            string sql = $"SELECT * FROM nodes WHERE nodeid = '{problem.NodeId}';";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            Assert.AreEqual("This is my problem", reader["nodetext"]);

            CleanUpDbObjects(conn, command, reader);
        }

        private void TestAddLink()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            var node1 = NodeFactory.CreateCause("Node 1", SequentialId.NewId());
            var node2 = NodeFactory.CreateCause("Node 2", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);
            SqliteDb.GetInstance().AddNode(problem, node1);
            SqliteDb.GetInstance().AddNode(node1, node2);
            SqliteDb.GetInstance().AddLink(problem, node2);

            var expectedLinks = new Node[3,2]
            {
                {problem,node1 },
                {problem,node2 },
                {node1,node2 }
            };
            TestHierarchy(conn, expectedLinks);
            CleanUpConnection(conn);
        }

        private void TestRemoveLink()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            var node1 = NodeFactory.CreateCause("Node 1", SequentialId.NewId());
            var node2 = NodeFactory.CreateCause("Node 2", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);
            SqliteDb.GetInstance().AddNode(problem, node1);
            SqliteDb.GetInstance().AddNode(node1, node2);
            SqliteDb.GetInstance().AddLink(problem, node2);
            SqliteDb.GetInstance().RemoveLink(problem, node2);

            var expectedLinks = new Node[2, 2]
            {
                {problem,node1 },
                {node1,node2 }
            };
            TestHierarchy(conn, expectedLinks);
            CleanUpConnection(conn);
        }

        private void TestRemoveFinalLink()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");
            Dictionary<string,Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().RemoveLink(nodes["Problem"], nodes["Node 1.2"]);

            var expectedLinks = new Node[3, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 2.1"], nodes["Node 3.1"] }
            };
            TestHierarchy(conn, expectedLinks);

            var expectedNodes = new Node[4]
            {
                nodes["Problem"],
                nodes["Node 1.1"],
                nodes["Node 2.1"],
                nodes["Node 3.1"]
            };
            TestNodes(conn,expectedNodes);
            CleanUpConnection(conn);
        }

        private void TestUndoRemoveFinalLink()
        {
            SQLiteConnection conn = CreateNewDbAndOpen("Tester.rootcause");
            Dictionary<string, Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().RemoveLink(nodes["Problem"], nodes["Node 1.2"]);
            SqliteDb.GetInstance().UndoRemoveLink(nodes["Problem"], nodes["Node 1.2"]);

            var expectedLinks = new Node[10, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Problem"],nodes["Node 1.2"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 1.2"],nodes["Node 2.1"] },
                {nodes["Node 1.2"],nodes["Node 2.2"] },
                {nodes["Node 1.2"],nodes["Node 2.3"] },
                {nodes["Node 2.1"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.2"] },
                {nodes["Node 2.3"], nodes["Node 3.2"] }
            };
            TestHierarchy(conn, expectedLinks);

            var expectedNodes = new Node[8]
            {
                nodes["Problem"],
                nodes["Node 1.1"],
                nodes["Node 1.2"],
                nodes["Node 2.1"],
                nodes["Node 2.2"],
                nodes["Node 2.3"],
                nodes["Node 3.1"],
                nodes["Node 3.2"]
            };
            TestNodes(conn, expectedNodes);
            CleanUpConnection(conn);
        }


        //UTILITIES
        private SQLiteConnection CreateNewDbAndOpen(string fileName)
        {
            string filePath = GetPath(fileName);
            SqliteDb.GetInstance().CreateNewFile(filePath);
            string connStr = string.Concat("Data Source=", filePath, ";Version=3;");
            SQLiteConnection conn = new SQLiteConnection(connStr);
            conn.Open();
            return conn;
        }

        private SQLiteConnection CreateConnection(string fileName)
        {
            return new SQLiteConnection(string.Concat("Data Source=", GetPath(fileName), ";Version=3;"));
        }

        private string GetPath(string fileName)
        {
            return string.Concat(System.IO.Directory.GetCurrentDirectory(), @"\", fileName);
        }

        private void CleanUpConnection(SQLiteConnection conn)
        {
            conn.Close();
            conn.Dispose();
        }

        private void CleanUpDbObjects(SQLiteConnection conn, SQLiteCommand command,SQLiteDataReader reader)
        {
            reader.Close();
            command.Dispose();
            CleanUpConnection(conn);
        }

        private void CheckTests(Dictionary<string,bool> tests)
        {
            if (tests.ContainsValue(false))
            {
                foreach (var item in tests)
                {
                    System.Diagnostics.Debug.WriteLine($"{item.Key}: {item.Value.ToString()}");
                }
                Assert.Fail("One or more expected records were not returned from the database.");
            }
        }

        private void TestNodes(SQLiteConnection conn, Node[] expectedNodes)
        {
            int expectedRows = expectedNodes.Length;
            var tests = new Dictionary<string, bool>();
            for (int i = 0; i < expectedRows; i++)
            {
                tests.Add(expectedNodes[i].Text, false);
            }

            string sql = $"SELECT * FROM nodes;";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();

            int rows = 0;
            while (reader.Read())
            {
                for (int i = 0; i < expectedRows; i++)
                {
                    if (reader["nodetext"].Equals(expectedNodes[i].Text)) { tests[expectedNodes[i].Text] = true; }
                }
                rows++;
            }
            reader.Close();
            command.Dispose();

            CheckTests(tests);
            Assert.AreEqual(expectedRows, rows);
        }

        private void TestHierarchy(SQLiteConnection conn, Node[,] expectedLinks)
        {
            int expectedRows = expectedLinks.GetLength(0);
            var tests = new Dictionary<string, bool>();
            for (int i = 0; i < expectedRows; i++)
            {
                tests.Add($"{expectedLinks[i,0].Text} links to {expectedLinks[i,1].Text}", false);
            }

            string sql = $"SELECT * FROM hierarchy;";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();

            int links = 0;
            while (reader.Read())
            {
                for (int i = 0; i < expectedRows; i++)
                {
                    CheckHierarchyExists(reader, expectedLinks[i,0], expectedLinks[i,1],tests);
                }
                links++;
            }
            reader.Close();
            command.Dispose();

            CheckTests(tests);
            Assert.AreEqual(expectedRows, links);
        }

        private void CheckHierarchyExists(SQLiteDataReader reader,Node parent,Node child,Dictionary<string,bool> tests)
        {
            if (reader["parentid"].ToString().Equals(parent.NodeId.ToString()) && reader["childid"].ToString().Equals(child.NodeId.ToString()))
            {
                tests[$"{parent.Text} links to {child.Text}"] = true;
            }
        }

        /*This is the model that is created by CreateComplexModelForText
         * 
         *                  Problem
         *                     |
         *               -------------
         *               |           |
         *            Node 1.1    Node 1.2
         *               |           |
         *               |      -------------------------
         *               |      |           |           |
         *               |---Node 2.1    Node 2.2    Node 2.3
         *                      |           |           |
         *                      |      -------------    |
         *                      |      |           |    |
         *                      |---Node 3.1    Node 3.2-
        */
        private Dictionary<string,Node> CreateComplexModelForTest()
        {
            Node problem = NodeFactory.CreateProblem("Problem",SequentialId.NewId());
            Node node1_1 = NodeFactory.CreateCause("Node 1.1", SequentialId.NewId());
            Node node1_2 = NodeFactory.CreateCause("Node 1.2", SequentialId.NewId());
            Node node2_1 = NodeFactory.CreateCause("Node 2.1", SequentialId.NewId());
            Node node2_2 = NodeFactory.CreateCause("Node 2.2", SequentialId.NewId());
            Node node2_3 = NodeFactory.CreateCause("Node 2.3", SequentialId.NewId());
            Node node3_1 = NodeFactory.CreateCause("Node 3.1", SequentialId.NewId());
            Node node3_2 = NodeFactory.CreateCause("Node 3.2", SequentialId.NewId());

            problem.AddNode(node1_1);
            problem.AddNode(node1_2);
            node1_1.AddNode(node2_1);
            node1_2.AddNode(node2_1);
            node1_2.AddNode(node2_2);
            node1_2.AddNode(node2_3);
            node2_1.AddNode(node3_1);
            node2_2.AddNode(node3_1);
            node2_2.AddNode(node3_2);
            node2_3.AddNode(node3_2);

            SqliteDb.GetInstance().InsertTopLevel(problem);
            SqliteDb.GetInstance().AddNode(problem, node1_1);
            SqliteDb.GetInstance().AddNode(problem, node1_2);
            SqliteDb.GetInstance().AddNode(node1_2, node2_1);
            SqliteDb.GetInstance().AddNode(node1_2, node2_2);
            SqliteDb.GetInstance().AddNode(node1_2, node2_3);
            SqliteDb.GetInstance().AddNode(node2_2, node3_1);
            SqliteDb.GetInstance().AddNode(node2_2, node3_2);
            SqliteDb.GetInstance().AddLink(node1_1, node2_1);
            SqliteDb.GetInstance().AddLink(node2_1, node3_1);
            SqliteDb.GetInstance().AddLink(node2_3, node3_2);
            return new Dictionary<string, Node>()
            {
                {problem.Text,problem },
                {node1_1.Text,node1_1 },
                {node1_2.Text,node1_2 },
                {node2_1.Text,node2_1 },
                {node2_2.Text,node2_2 },
                {node2_3.Text,node2_3 },
                {node3_1.Text,node3_1 },
                {node3_2.Text,node3_2 }
            };
        }
    }
}
