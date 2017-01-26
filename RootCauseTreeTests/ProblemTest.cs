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
        public void TestFactory()
        {
            Node node = NodeFactory.CreateProblem("Problem", new Guid("7f06b704-4594-08d4-5ab9-89f958d38246"));
            Assert.AreEqual("Problem", node.Text);
            Assert.AreEqual("7f06b704-4594-08d4-5ab9-89f958d38246", node.NodeId.ToString());
            node = NodeFactory.CreateCause("Cause", new Guid("7f083de2-4594-08d4-9c99-4a90b5b39046"));
            Assert.AreEqual("Cause", node.Text);
            Assert.AreEqual("7f083de2-4594-08d4-9c99-4a90b5b39046", node.NodeId.ToString());
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
            Assert.AreEqual(defaultTestTree, treeString);
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

        [TestMethod]
        public void RemoveNodeImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new RemoveNodeCommand(dict["Node 1.1"], true);
            Assert.AreEqual("Problem,Node 1,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveNodeChainAndUndo()
        {
            var dict = BuildTestTree();
            var command = new RemoveNodeChainCommand(dict["Node 1"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            command.Execute();
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveNodeChainImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new RemoveNodeChainCommand(dict["Node 1"], true);
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void MoveNodeAndUndo()
        {
            var dict = BuildTestTree();
            var command = new MoveNodeCommand(dict["Node 1"], dict["Node 2"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            command.Execute();
            Assert.AreEqual("Problem,Node 2,Node 1,Node 1.1,Node 1.2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void MoveNodeImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new MoveNodeCommand(dict["Node 1"], dict["Node 2"], true);
            Assert.AreEqual("Problem,Node 2,Node 1,Node 1.1,Node 1.2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void MoveNodeMultipleParentsAndUndo()
        {
            string startStr = "Problem,Node 1,Node 1.1,Node 1.2,I have multiple parents!,Node 2,Node 2.1,Node 2.1.1,Node 2.2,I have multiple parents!";
            var dict = BuildTestTree();
            Node node = new AddNodeCommand(dict["Node 1"], "I have multiple parents!",true).NewNode;

            new AddLinkCommand(dict["Node 2"], node,true);
            Assert.AreEqual(startStr, StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, node.CountParentNodes());
            Assert.AreEqual(3, dict["Node 1"].CountNodes());
            Assert.AreEqual(3, dict["Node 2"].CountNodes());

            IRootCauseCommand command = new MoveNodeCommand(node, dict["Problem"], true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2,I have multiple parents!", StringifyTree(dict["Problem"]));
            Assert.AreEqual(1, node.CountParentNodes());
            Assert.AreEqual(2, dict["Node 1"].CountNodes());
            Assert.AreEqual(2, dict["Node 2"].CountNodes());

            command.Undo();
            Assert.AreEqual(startStr, StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, node.CountParentNodes());
            Assert.AreEqual(3, dict["Node 1"].CountNodes());
            Assert.AreEqual(3, dict["Node 2"].CountNodes());
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

            dict.Add("Problem", NodeFactory.CreateProblem("Problem", new Guid("7ef03e2f-4594-08d4-a8f8-0914c322d848")));
            dict.Add("Node 1", NodeFactory.CreateCause("Node 1", new Guid("7ef0ee26-4594-08d4-4993-95b48dfa1440")));
            dict.Add("Node 1.1", NodeFactory.CreateCause("Node 1.1", new Guid("7ef25970-4594-08d4-1c05-c0b74305ad44")));
            dict.Add("Node 1.2", NodeFactory.CreateCause("Node 1.2", new Guid("7ef3368a-4594-08d4-cb17-73af48313444")));
            dict.Add("Node 2", NodeFactory.CreateCause("Node 2", new Guid("7ef52099-4594-08d4-1bcd-0890269be443")));
            dict.Add("Node 2.1", NodeFactory.CreateCause("Node 2.1", new Guid("7ef9b403-4594-08d4-0977-17f5ee3f5448")));
            dict.Add("Node 2.1.1", NodeFactory.CreateCause("Node 2.1.1", new Guid("7efc266a-4594-08d4-9508-5c6f7cf92141")));
            dict.Add("Node 2.2", NodeFactory.CreateCause("Node 2.2", new Guid("7f004b21-4594-08d4-9674-bce1cbedb44c")));

            new AddLinkCommand(dict["Problem"], dict["Node 1"], true);
            new AddLinkCommand(dict["Problem"], dict["Node 2"], true);
            new AddLinkCommand(dict["Node 1"], dict["Node 1.1"], true);
            new AddLinkCommand(dict["Node 1"], dict["Node 1.2"], true);
            new AddLinkCommand(dict["Node 2"], dict["Node 2.1"], true);
            new AddLinkCommand(dict["Node 2"], dict["Node 2.2"], true);
            new AddLinkCommand(dict["Node 2.1"], dict["Node 2.1.1"], true);

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
