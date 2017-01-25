using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeTests
{
    [TestClass]
    public class ProblemTests
    {
        private Problem problem = new Problem("This is my problem");
        string defaultTestTree = "Problem,Node 1,Node 1.1,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2";

        [TestMethod]
        public void NewProblem()
        {
            Assert.AreEqual("This is my problem", problem.Text);
            Assert.AreEqual(0, problem.CountNodes());
            Assert.AreEqual(0, problem.CountParentNodes());
        }

        [TestMethod]
        public void AddNodeAndUndo()
        {
            IRootCauseCommand command = new AddNodeCommand(problem, "Child 1");
            command.Execute();
            Assert.AreEqual(1, problem.CountNodes());
            foreach (var node in problem.Nodes)
            {
                Assert.AreEqual("Child 1", node.Text);
                Assert.AreEqual(1, node.CountParentNodes());
            }
            command.Undo();
            Assert.AreEqual(0, problem.CountNodes());
        }

        [TestMethod]
        public void AddNodeTwice()
        {
            IRootCauseCommand command = new AddNodeCommand(problem, "Child 1", true);
            command.Execute();
            Assert.AreEqual(1, problem.CountNodes());
        }

        [TestMethod]
        public void AddNodeImmediately()
        {
            Assert.AreEqual(0, problem.CountNodes());
            Node node = new AddNodeCommand(problem, "Child 1", true).NewNode;
            Assert.AreEqual("Child 1", node.Text);
            Assert.AreEqual(1, node.CountParentNodes());
            Assert.AreEqual(1, problem.CountNodes());
        }

        [TestMethod]
        public void ChangeNodeTextAndUndo()
        {
            AddNodeCommand addCommand = new AddNodeCommand(problem, "Child 1",true);
            addCommand = new AddNodeCommand(problem, "Child 2");
            Node node = new AddNodeCommand(problem, "Child 2",true).NewNode;

            ChangeNodeTextCommand txtCommand = new ChangeNodeTextCommand(node, "Child 2");
            Assert.AreEqual("Child 2", node.Text);
            txtCommand.Execute();
            Assert.AreEqual("Child 2", node.Text);
            txtCommand = new ChangeNodeTextCommand(node, "Child 3");
            Assert.AreEqual("Child 2", node.Text);
            txtCommand.Execute();
            Assert.AreEqual("Child 3", node.Text);
            txtCommand.Undo();
            Assert.AreEqual("Child 2", node.Text);
        }

        [TestMethod]
        public void ChangeNodeTextImmediately()
        {
            Assert.AreEqual("This is my problem", problem.Text);
            new ChangeNodeTextCommand(problem, "New problem", true);
            Assert.AreEqual("New problem", problem.Text);
        }

        [TestMethod]
        public void TestBuildTestTree()
        {
            var dict = BuildTestTree();
            string treeString = StringifyTree(dict["Problem"]);
            Assert.AreEqual(
                defaultTestTree,
                treeString);
            foreach (var node in dict.Values)
            {
                if (node.Text.Equals("Problem"))
                {
                    Assert.AreEqual(0, node.CountParentNodes());
                }
                else
                {
                    Assert.AreEqual(1, node.CountParentNodes());

                }
            } 
        }

        [TestMethod]
        public void AddLinkAndUndo()
        {
            var dict = BuildTestTree();
            AddLinkCommand command = new AddLinkCommand(dict["Node 1.1"], dict["Node 2.2"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            command.Execute();
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
            command.Undo();
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void AddLinkImmediately()
        {
            var dict = BuildTestTree();
            AddLinkCommand command = new AddLinkCommand(dict["Node 1.1"], dict["Node 2.2"],true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLinkAndUndo()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(dict["Node 1.1"], dict["Node 2.2"],true);
            RemoveLinkCommand command = new RemoveLinkCommand(dict["Node 1.1"], dict["Node 2.2"]);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
            command.Execute();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            command.Undo();
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
        }

        [TestMethod]
        public void RemoveLinkImmediately()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(dict["Node 1.1"], dict["Node 2.2"], true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            new RemoveLinkCommand(dict["Node 1.1"], dict["Node 2.2"], true);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLastLinkOfLastNode()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveLinkCommand(dict["Node 1"], dict["Node 1.2"],true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLastLinkOfMiddleNode()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveLinkCommand(dict["Problem"], dict["Node 1"], true);
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveNodeAndUndo()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveNodeCommand(dict["Node 1"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            command.Execute();
            foreach (var node in new Node[]{ dict["Node 1.1"],dict["Node 1.2"]})
            {
                Assert.AreEqual(1, node.CountParentNodes());
                foreach (var parent in node.ParentNodes)
                {
                    Assert.AreEqual("Problem", parent.Text);
                }
            }

            Assert.AreEqual(3, dict["Problem"].CountNodes());
            foreach (var node in dict["Problem"].Nodes)
            {
                Assert.IsTrue("Node 1.1,Node 1.2,Node 2".Contains(node.Text));
            }
            
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        /* This method builds a complex tree for some of the link and removal command testing.
         * The structure of the tree is as follows:
         * Problem
         *      -> Node 1
         *          -> Node 1.1
         *          -> Node 1.2
         *      -> Node 2
         *          -> Node 2.1
         *              -> Node 2.1.1
         *          -> Node 2.2
         */
        private Dictionary<string,Node> BuildTestTree()
        {
            Dictionary<string, Node> dict = new Dictionary<string, Node>();
            Node node;
            Problem testTree = new Problem("Problem");
            dict.Add(testTree.Text, testTree);

            node = new AddNodeCommand(testTree, "Node 1",true).NewNode;
            dict.Add(node.Text, node);

            dict.Add("Node 1.1", new AddNodeCommand(node, "Node 1.1",true).NewNode);
            dict.Add("Node 1.2", new AddNodeCommand(node, "Node 1.2", true).NewNode);

            Node node2 = new AddNodeCommand(testTree, "Node 2", true).NewNode;
            dict.Add(node2.Text, node2);

            node = new AddNodeCommand(node2, "Node 2.1", true).NewNode;
            dict.Add(node.Text, node);

            dict.Add("Node 2.1.1", new AddNodeCommand(node, "Node 2.1.1", true).NewNode);
            dict.Add("Node 2.2", new AddNodeCommand(node2, "Node 2.2", true).NewNode);

            return dict;
        }

        private string StringifyTree(Node node)
        {
            string str = node.Text;
            foreach (Node child in node.Nodes)
            {
                str = string.Concat(str, ",", StringifyTree(child));
            }
            return str;
        }
    }
}
