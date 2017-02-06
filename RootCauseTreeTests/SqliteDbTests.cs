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
            TestRemoveNode();
            TestRemoveNodeChain();
            TestMoveNodeCommand();
            TestUndoMoveNodeCommand();
        }

        //TESTS
        private void TestCreateNewFile()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");

            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('toplevel','nodes','hierarchy') ORDER BY name;";
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            int records = 0;
            string[] names = new string[3];
            while (reader.Read())
            {
                names[records] = reader["name"] as string;
                records++;
            }
            reader.Close();
            CleanUpCommand(command);

            Assert.AreEqual(3, records);
            Assert.AreEqual("hierarchy,nodes,toplevel", string.Join(",", names));

        }

        private void TestCreateProblemStatement()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            var node = NodeFactory.CreateProblem("This is my problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(node);
            string nodeId = node.NodeId.ToString();

            string sql = $"SELECT count(*) AS result FROM toplevel WHERE nodeid = '{nodeId}'";
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            string toplevels = reader["result"].ToString();
            reader.Close();

            sql = $"SELECT * FROM nodes WHERE nodeid = '{nodeId}'";
            command.CommandText = sql;
            reader = command.ExecuteReader();
            reader.Read();
            string topLevelId = reader["nodeid"].ToString();
            string topLevelText = reader["nodetext"].ToString();
            reader.Close();
            CleanUpCommand(command);

            Assert.AreEqual("1", toplevels);
            Assert.AreEqual(nodeId, topLevelId);
            Assert.AreEqual("This is my problem", topLevelText);
        }

        private void TestCreateNewNode()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);

            var node = NodeFactory.CreateCause("Node 1", SequentialId.NewId());
            SqliteDb.GetInstance().AddNode(problem,node);

            string sql = $"SELECT * FROM nodes WHERE nodeid = '{node.NodeId}'";
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            string newNodeText = reader["nodetext"].ToString();
            reader.Close();

            sql = $"SELECT count(*) AS result FROM hierarchy WHERE parentid = '{problem.NodeId}' AND childid = '{node.NodeId}';";
            command.CommandText = sql;
            reader = command.ExecuteReader();
            reader.Read();
            string hierarchies = reader["result"].ToString();
            reader.Close();
            CleanUpCommand(command);

            Assert.AreEqual("Node 1", newNodeText);
            Assert.AreEqual("1", hierarchies);
        }

        private void TestChangeNodeText()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");

            var problem = NodeFactory.CreateProblem("Problem", SequentialId.NewId());
            SqliteDb.GetInstance().InsertTopLevel(problem);
            SqliteDb.GetInstance().ChangeNodeText(problem, "This is my problem");

            string sql = $"SELECT * FROM nodes WHERE nodeid = '{problem.NodeId}';";
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            string newText = reader["nodetext"].ToString();
            reader.Close();
            CleanUpCommand(command);

            Assert.AreEqual("This is my problem", newText);
        }

        private void TestAddLink()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");

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
            TestHierarchy(command, expectedLinks);
            CleanUpCommand(command);
        }

        private void TestRemoveLink()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");

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
            TestHierarchy(command, expectedLinks);
            CleanUpCommand(command);
        }

        private void TestRemoveFinalLink()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            Dictionary<string,Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().RemoveLink(nodes["Problem"], nodes["Node 1.2"]);

            var expectedLinks = new Node[3, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 2.1"], nodes["Node 3.1"] }
            };
            TestHierarchy(command, expectedLinks);

            var expectedNodes = new Node[4]
            {
                nodes["Problem"],
                nodes["Node 1.1"],
                nodes["Node 2.1"],
                nodes["Node 3.1"]
            };
            TestNodes(command,expectedNodes);
            CleanUpCommand(command);
        }

        private void TestUndoRemoveFinalLink()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
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
            TestHierarchy(command, expectedLinks);

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
            TestNodes(command, expectedNodes);
            CleanUpCommand(command);
        }

        private void TestRemoveNode()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            Dictionary<string, Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().RemoveNode(nodes["Node 1.2"]);

            var expectedLinks = new Node[9, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Problem"],nodes["Node 2.1"] },
                {nodes["Problem"],nodes["Node 2.2"] },
                {nodes["Problem"],nodes["Node 2.3"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 2.1"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.2"] },
                {nodes["Node 2.3"], nodes["Node 3.2"] }
            };
            TestHierarchy(command, expectedLinks);

            var expectedNodes = new Node[7]
            {
                nodes["Problem"],
                nodes["Node 1.1"],
                nodes["Node 2.1"],
                nodes["Node 2.2"],
                nodes["Node 2.3"],
                nodes["Node 3.1"],
                nodes["Node 3.2"]
            };
            TestNodes(command, expectedNodes);
            CleanUpCommand(command);
        }

        private void TestRemoveNodeChain()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            Dictionary<string, Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().RemoveNodeChain(nodes["Node 1.2"]);

            var expectedLinks = new Node[3, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 2.1"], nodes["Node 3.1"] }
            };
            TestHierarchy(command, expectedLinks);

            var expectedNodes = new Node[4]
            {
                nodes["Problem"],
                nodes["Node 1.1"],
                nodes["Node 2.1"],
                nodes["Node 3.1"]
            };
            TestNodes(command, expectedNodes);
            CleanUpCommand(command);
        }

        private void TestMoveNodeCommand()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            Dictionary<string, Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().MoveNode(nodes["Node 2.1"],nodes["Node 3.2"]);

            var expectedLinks = new Node[9, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Problem"],nodes["Node 1.2"] },
                {nodes["Node 1.2"],nodes["Node 2.2"] },
                {nodes["Node 1.2"],nodes["Node 2.3"] },
                {nodes["Node 2.2"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.2"] },
                {nodes["Node 2.3"],nodes["Node 3.2"] },
                {nodes["Node 3.2"],nodes["Node 2.1"] },
                {nodes["Node 2.1"], nodes["Node 3.1"] }
            };
            TestHierarchy(command, expectedLinks);

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
            TestNodes(command, expectedNodes);
            CleanUpCommand(command);
        }

        private void TestUndoMoveNodeCommand()
        {
            var command = CreateCommandForNewDb("Tester.rootcause");
            Dictionary<string, Node> nodes = CreateComplexModelForTest();
            SqliteDb.GetInstance().MoveNode(nodes["Node 2.1"], nodes["Node 3.2"]);
            SqliteDb.GetInstance().UndoMoveNode(nodes["Node 2.1"], new Node[2] { nodes["Node 1.1"], nodes["Node 1.2"] });

            var expectedLinks = new Node[10, 2]
            {
                {nodes["Problem"],nodes["Node 1.1"] },
                {nodes["Problem"],nodes["Node 1.2"] },
                {nodes["Node 1.1"],nodes["Node 2.1"] },
                {nodes["Node 1.2"],nodes["Node 2.1"] },
                {nodes["Node 1.2"],nodes["Node 2.2"] },
                {nodes["Node 1.2"],nodes["Node 2.3"] },
                {nodes["Node 2.2"],nodes["Node 3.1"] },
                {nodes["Node 2.2"],nodes["Node 3.2"] },
                {nodes["Node 2.3"],nodes["Node 3.2"] },
                {nodes["Node 2.1"], nodes["Node 3.1"] }
            };
            TestHierarchy(command, expectedLinks);

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
            TestNodes(command, expectedNodes);
            CleanUpCommand(command);
        }

        //UTILITIES
        private SQLiteCommand CreateCommandForNewDb(string fileName)
        {
            string filePath = GetPath(fileName);
            SqliteDb.GetInstance().CreateNewFile(filePath);
            string connStr = string.Concat("Data Source=", filePath, ";Version=3;");
            SQLiteConnection conn = new SQLiteConnection(connStr);
            conn.Open();
            return conn.CreateCommand();
        }

        private SQLiteConnection CreateConnection(string fileName)
        {
            return new SQLiteConnection(string.Concat("Data Source=", GetPath(fileName), ";Version=3;"));
        }

        private string GetPath(string fileName)
        {
            return string.Concat(System.IO.Directory.GetCurrentDirectory(), @"\", fileName);
        }

        private void CleanUpCommand(SQLiteCommand command)
        {
            command.Connection.Close();
            command.Connection.Dispose();
            command.Dispose();
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

        private void TestNodes(SQLiteCommand command, Node[] expectedNodes)
        {
            int expectedRows = expectedNodes.Length;
            var tests = new Dictionary<string, bool>();
            for (int i = 0; i < expectedRows; i++)
            {
                tests.Add(expectedNodes[i].Text, false);
            }

            string sql = $"SELECT * FROM nodes;";
            command.CommandText = sql;
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

            CheckTests(tests);
            Assert.AreEqual(expectedRows, rows);
        }

        private void TestHierarchy(SQLiteCommand command, Node[,] expectedLinks)
        {
            int expectedRows = expectedLinks.GetLength(0);
            var tests = new Dictionary<string, bool>();
            for (int i = 0; i < expectedRows; i++)
            {
                tests.Add($"{expectedLinks[i,0].Text} links to {expectedLinks[i,1].Text}", false);
            }

            string sql = $"SELECT * FROM hierarchy;";
            command.CommandText = sql;
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
            var db = SqliteDb.GetInstance();
            Node problem = NodeFactory.CreateProblem("Problem",SequentialId.NewId());
            db.InsertTopLevel(problem);

            Node node1_1 = new AddNodeCommand(db, problem, "Node 1.1", true).NewNode;
            Node node1_2 = new AddNodeCommand(db, problem, "Node 1.2", true).NewNode;
            Node node2_1 = new AddNodeCommand(db, node1_2, "Node 2.1", true).NewNode;
            Node node2_2 = new AddNodeCommand(db, node1_2, "Node 2.2", true).NewNode;
            Node node2_3 = new AddNodeCommand(db, node1_2, "Node 2.3", true).NewNode;
            Node node3_1 = new AddNodeCommand(db, node2_2, "Node 3.1", true).NewNode;
            Node node3_2 = new AddNodeCommand(db, node2_2, "Node 3.2", true).NewNode;

            new AddLinkCommand(db, node1_1, node2_1, true);
            new AddLinkCommand(db, node2_1, node3_1, true);
            new AddLinkCommand(db, node2_3, node3_2, true);

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
