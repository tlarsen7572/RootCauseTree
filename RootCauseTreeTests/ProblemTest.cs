using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeTests
{
    [TestClass]
    public class ProblemTests
    {
        private Problem problem;

        [TestInitialize]
        public void Startup()
        {
            problem = new Problem("This is my problem");
            System.Diagnostics.Debug.WriteLine(problem.NodeId.ToString());
        }
        
        [TestMethod]
        public void NewProblem()
        {
            Assert.AreEqual("This is my problem", problem.Text);
            Assert.AreEqual(0, problem.CountNodes());
        }

        [TestMethod]
        public void AddNodeAndUndo()
        {
            IRootCauseCommand command = new AddCauseCommand(problem, "Child 1");
            command.Execute();
            Assert.AreEqual(1, problem.CountNodes());
            foreach (var node in problem.Nodes)
            {
                Assert.AreEqual("Child 1", node.Text);
            }
            command.Undo();
            Assert.AreEqual(0, problem.CountNodes());
        }

        [TestMethod]
        public void AddNodeImmediately()
        {
            Node node = new AddCauseCommand(problem, "Child 1", true).NewNode;
            Assert.AreEqual("Child 1", node.Text);
            Assert.AreEqual(1, problem.CountNodes());
        }

        [TestMethod]
        public void ChangeNodeTextAndUndo()
        {
            AddCauseCommand addCommand = new AddCauseCommand(problem, "Child 1");
            addCommand.Execute();
            addCommand = new AddCauseCommand(problem, "Child 2");
            Node node = addCommand.NewNode;
            addCommand.Execute();
            ChangeNodeTextCommand txtCommand = new ChangeNodeTextCommand(node, "Child 2");
            txtCommand.Execute();
            Assert.AreEqual("Child 2", node.Text);
            txtCommand = new ChangeNodeTextCommand(node, "Child 3");
            txtCommand.Execute();
            Assert.AreEqual("Child 3", node.Text);
            txtCommand.Undo();
            Assert.AreEqual("Child 2", node.Text);
        }

        [TestMethod]
        public void ChangeNodeTextImmediately()
        {
            new ChangeNodeTextCommand(problem, "New problem", true);
            Assert.AreEqual("New problem", problem.Text);
        }

        [TestMethod]
        public void TestBuildTestTree()
        {
            Node node = BuildTestTree();
            string treeString = StringifyTree(node);
            Assert.AreEqual(
                "Problem,Node 1,Node 1.1,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2",
                treeString);
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
        private Node BuildTestTree()
        {
            Node node;
            Problem testTree = new Problem("Problem");
            node = new AddCauseCommand(testTree, "Node 1",true).NewNode;
            new AddCauseCommand(node, "Node 1.1").Execute();
            new AddCauseCommand(node, "Node 1.2").Execute();
            Node node2 = new AddCauseCommand(testTree, "Node 2", true).NewNode;
            node = new AddCauseCommand(node2, "Node 2.1", true).NewNode;
            new AddCauseCommand(node, "Node 2.1.1").Execute();
            new AddCauseCommand(node2, "Node 2.2").Execute();
            return testTree;
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
