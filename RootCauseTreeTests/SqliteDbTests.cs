﻿using System;
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

            string sql = $"SELECT * FROM hierarchy;";
            var command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            var tests = new Dictionary<string, bool>()
            {
                { "Problem links to Node 1",false },
                { "Problem links to Node 2",false },
                { "Node 1 links to Node 2", false }
            };

            int links = 0;
            while (reader.Read())
            {
                if (reader["parentid"].ToString().Equals(problem.NodeId.ToString()) && reader["childid"].ToString().Equals(node1.NodeId.ToString())) { tests["Problem links to Node 1"] = true; }
                if (reader["parentid"].ToString().Equals(problem.NodeId.ToString()) && reader["childid"].ToString().Equals(node2.NodeId.ToString())) { tests["Problem links to Node 2"] = true; }
                if (reader["parentid"].ToString().Equals(node1.NodeId.ToString()) && reader["childid"].ToString().Equals(node2.NodeId.ToString())) { tests["Node 1 links to Node 2"] = true; }
                links++;
            }

            if (tests.ContainsValue(false))
            {
                Assert.Fail("One or more expected records were not returned from the database.");
                foreach (var item in tests)
                {
                    System.Diagnostics.Debug.WriteLine($"{item.Key}: {item.Value.ToString()}");
                }
            }
            Assert.AreEqual(3, links);
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

        private void CleanUpDbObjects(SQLiteConnection conn, SQLiteCommand command,SQLiteDataReader reader)
        {
            reader.Close();
            command.Dispose();
            conn.Close();
            conn.Dispose();
        }
    }
}
