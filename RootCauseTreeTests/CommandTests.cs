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
            IRootCauseCommand command = new AddNodeCommand(new NullDb(), problem, "Child 1");
            command.Execute();
            Assert.AreEqual(1, problem.CountNodes());
            Assert.AreEqual(true, command.Executed);
            foreach (var node in problem.Nodes)
            {
                Assert.AreEqual("Child 1", node.Text);
                Assert.AreEqual(1, node.CountParentNodes());
            }
            command.Undo();
            Assert.AreEqual(0, problem.CountNodes());
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void AddNodeUndoBeforeExecute()
        {
            IRootCauseCommand command = new AddNodeCommand(new NullDb(), problem, "Child 1");
            command.Undo();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void AddNodeTwice()
        {
            IRootCauseCommand command = new AddNodeCommand(new NullDb(), problem, "Child 1", true);
            command.Execute();
        }

        [TestMethod]
        public void AddNodeImmediately()
        {
            Assert.AreEqual(0, problem.CountNodes());
            Node node = new AddNodeCommand(new NullDb(), problem, "Child 1", true).NewNode;
            Assert.AreEqual("Child 1", node.Text);
            Assert.AreEqual(1, node.CountParentNodes());
            Assert.AreEqual(1, problem.CountNodes());
        }

        [TestMethod]
        public void TestFactory()
        {
            Node node = NodeFactory.CreateProblem("Problem", 1);
            Assert.AreEqual("Problem", node.Text);
            Assert.AreEqual(1, node.NodeId);
            node = NodeFactory.CreateCause("Cause", 2);
            Assert.AreEqual("Cause", node.Text);
            Assert.AreEqual(2, node.NodeId);
        }

        [TestMethod]
        public void ChangeNodeTextAndUndo()
        {
            AddNodeCommand addCommand = new AddNodeCommand(new NullDb(), problem, "Child 1",true);
            addCommand = new AddNodeCommand(new NullDb(), problem, "Child 2");
            Node node = new AddNodeCommand(new NullDb(), problem, "Child 2",true).NewNode;

            ChangeNodeTextCommand txtCommand = new ChangeNodeTextCommand(new NullDb(), node, "Child 2");
            Assert.AreEqual("Child 2", node.Text);
            txtCommand.Execute();
            Assert.AreEqual("Child 2", node.Text);
            txtCommand = new ChangeNodeTextCommand(new NullDb(), node, "Child 3");
            Assert.AreEqual("Child 2", node.Text);
            txtCommand.Execute();
            Assert.AreEqual("Child 3", node.Text);
            Assert.AreEqual(true, txtCommand.Executed);
            txtCommand.Undo();
            Assert.AreEqual("Child 2", node.Text);
            Assert.AreEqual(false, txtCommand.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void ChangeNodeTextTwice()
        {
            IRootCauseCommand command =new ChangeNodeTextCommand(new NullDb(), problem, "New Text", true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void ChangeNodeTextUndoBeforeExecute()
        {
            IRootCauseCommand command = new ChangeNodeTextCommand(new NullDb(), problem, "New Text");
            command.Undo();
        }

        [TestMethod]
        public void ChangeNodeTextImmediately()
        {
            Assert.AreEqual("This is my problem", problem.Text);
            new ChangeNodeTextCommand(new NullDb(), problem, "New problem", true);
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
            AddLinkCommand command = new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            command.Execute();
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
            Assert.AreEqual(true, command.Executed);
            command.Undo();
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void AddLinkTwice()
        {
            var dict = BuildTestTree();
            AddLinkCommand command = new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"],true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void AddLinkUndoBeforeExecute()
        {
            var dict = BuildTestTree();
            AddLinkCommand command = new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"]);
            command.Undo();
        }

        [TestMethod]
        public void AddLinkImmediately()
        {
            var dict = BuildTestTree();
            AddLinkCommand command = new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"],true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLinkAndUndo()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"],true);
            RemoveLinkCommand command = new RemoveLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"]);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
            command.Execute();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(1, dict["Node 2.2"].CountParentNodes());
            Assert.AreEqual(true, command.Executed);
            command.Undo();
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, dict["Node 2.2"].CountParentNodes());
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void RemoveLinkTwice()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"], true);
            RemoveLinkCommand command = new RemoveLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"],true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void RemoveLinkUndoBeforeExecute()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"], true);
            RemoveLinkCommand command = new RemoveLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"]);
            command.Undo();
        }

        [TestMethod]
        public void RemoveLinkImmediately()
        {
            var dict = BuildTestTree();
            new AddLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"], true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2.2,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            new RemoveLinkCommand(new NullDb(), dict["Node 1.1"], dict["Node 2.2"], true);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLastLinkOfLastNode()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveLinkCommand(new NullDb(), dict["Node 1"], dict["Node 1.2"],true);
            Assert.AreEqual("Problem,Node 1,Node 1.1,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveLastLinkOfMiddleNode()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveLinkCommand(new NullDb(), dict["Problem"], dict["Node 1"], true);
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveNodeAndUndo()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveNodeCommand(new NullDb(), dict["Node 1"]);
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
            Assert.AreEqual(true, command.Executed);
            
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void RemoveNodeTwice()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveNodeCommand(new NullDb(), dict["Node 1"],true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void RemoveNodeUndoBeforeExecute()
        {
            var dict = BuildTestTree();
            IRootCauseCommand command = new RemoveNodeCommand(new NullDb(), dict["Node 1"]);
            command.Undo();
        }

        [TestMethod]
        public void RemoveNodeImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new RemoveNodeCommand(new NullDb(), dict["Node 1.1"], true);
            Assert.AreEqual("Problem,Node 1,Node 1.2,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void RemoveNodeChainAndUndo()
        {
            var dict = BuildTestTree();
            var command = new RemoveNodeChainCommand(new NullDb(), dict["Node 1"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            command.Execute();
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(true, command.Executed);
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void RemoveNodeChainTwice()
        {
            var dict = BuildTestTree();
            var command = new RemoveNodeChainCommand(new NullDb(), dict["Node 1"],true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void RemoveNodeChainUndoBeforeExecute()
        {
            var dict = BuildTestTree();
            var command = new RemoveNodeChainCommand(new NullDb(), dict["Node 1"]);
            command.Undo();
        }

        [TestMethod]
        public void RemoveNodeChainImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new RemoveNodeChainCommand(new NullDb(), dict["Node 1"], true);
            Assert.AreEqual("Problem,Node 2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void MoveNodeAndUndo()
        {
            var dict = BuildTestTree();
            var command = new MoveNodeCommand(new NullDb(), dict["Node 1"], dict["Node 2"]);
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            command.Execute();
            Assert.AreEqual("Problem,Node 2,Node 1,Node 1.1,Node 1.2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
            Assert.AreEqual(true, command.Executed);
            command.Undo();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            Assert.AreEqual(false, command.Executed);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandAlreadyExecutedException))]
        public void MoveNodeChainTwice()
        {
            var dict = BuildTestTree();
            var command = new MoveNodeCommand(new NullDb(), dict["Node 1"], dict["Node 2"],true);
            command.Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(CommandNotExecutedException))]
        public void MoveNodeChainUndoBeforeExecute()
        {
            var dict = BuildTestTree();
            var command = new MoveNodeCommand(new NullDb(), dict["Node 1"], dict["Node 2"]);
            command.Undo();
        }

        [TestMethod]
        public void MoveNodeImmediately()
        {
            var dict = BuildTestTree();
            Assert.AreEqual(defaultTestTree, StringifyTree(dict["Problem"]));
            new MoveNodeCommand(new NullDb(), dict["Node 1"], dict["Node 2"], true);
            Assert.AreEqual("Problem,Node 2,Node 1,Node 1.1,Node 1.2,Node 2.1,Node 2.1.1,Node 2.2", StringifyTree(dict["Problem"]));
        }

        [TestMethod]
        public void MoveNodeMultipleParentsAndUndo()
        {
            string startStr = "Problem,Node 1,Node 1.1,Node 1.2,I have multiple parents!,Node 2,Node 2.1,Node 2.1.1,Node 2.2,I have multiple parents!";
            var dict = BuildTestTree();
            Node node = new AddNodeCommand(new NullDb(), dict["Node 1"], "I have multiple parents!",true).NewNode;

            new AddLinkCommand(new NullDb(), dict["Node 2"], node,true);
            Assert.AreEqual(startStr, StringifyTree(dict["Problem"]));
            Assert.AreEqual(2, node.CountParentNodes());
            Assert.AreEqual(3, dict["Node 1"].CountNodes());
            Assert.AreEqual(3, dict["Node 2"].CountNodes());

            IRootCauseCommand command = new MoveNodeCommand(new NullDb(), node, dict["Problem"], true);
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

        [TestMethod]
        public void NewProblemContainer()
        {
            IRootCauseDb db = new NullDb();
            var command = new CreateProblemContainer(db, "This is a problem");
            command.Execute();
            ProblemContainer container = command.Container;
            Node problem = container.InitialProblem;
            Assert.AreEqual("This is a problem", problem.Text);
            Assert.AreEqual(0, container.CountUndoActions());
            Assert.AreEqual(0, container.CountRedoActions());

            IRootCauseCommand add = new AddNodeCommand(db, problem, "Cause 1");
            container.AddAction(add);
            Assert.AreEqual(true, add.Executed);
            Assert.AreEqual(1, container.CountUndoActions());
            Assert.AreEqual(0, container.CountRedoActions());
            Assert.AreEqual(1,problem.CountNodes());

            add = new AddNodeCommand(db, problem, "Cause 2",true);
            container.AddAction(add);
            Assert.AreEqual(true, add.Executed);
            Assert.AreEqual(2, container.CountUndoActions());
            Assert.AreEqual(0, container.CountRedoActions());
            Assert.AreEqual(2, problem.CountNodes());

            container.Undo();
            Assert.AreEqual(1, container.CountUndoActions());
            Assert.AreEqual(1, container.CountRedoActions());
            Assert.AreEqual(1, problem.CountNodes());

            container.Redo();
            Assert.AreEqual(2, container.CountUndoActions());
            Assert.AreEqual(0, container.CountRedoActions());
            Assert.AreEqual(2, problem.CountNodes());

            container.Undo();
            add = new AddNodeCommand(db, problem, "Cause 3", true);
            container.AddAction(add);
            Assert.AreEqual(2, container.CountUndoActions());
            Assert.AreEqual(0, container.CountRedoActions());
            Assert.AreEqual(2, problem.CountNodes());
        }

        [TestMethod]
        public void CreateContainerImmediately()
        {
            var command = new CreateProblemContainer(new NullDb(), "This is a problem",true);
            Assert.AreEqual(true, command.Executed);
        }

        [TestMethod]
        public void ContainerUndoWithNoActions()
        {
            var container = new CreateProblemContainer(new NullDb(), "This is a problem", true).Container;
            container.Undo();
        }

        [TestMethod]
        public void ContainerRedoWithNoActions()
        {
            var container = new CreateProblemContainer(new NullDb(), "This is a problem", true).Container;
            container.Redo();
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
            IRootCauseDb db = new NullDb();
            dict.Add("Problem", NodeFactory.CreateProblem("Problem", SequentialId.NewId()));
            dict.Add("Node 1", NodeFactory.CreateCause("Node 1", SequentialId.NewId()));
            dict.Add("Node 1.1", NodeFactory.CreateCause("Node 1.1", SequentialId.NewId()));
            dict.Add("Node 1.2", NodeFactory.CreateCause("Node 1.2", SequentialId.NewId()));
            dict.Add("Node 2", NodeFactory.CreateCause("Node 2", SequentialId.NewId()));
            dict.Add("Node 2.1", NodeFactory.CreateCause("Node 2.1", SequentialId.NewId()));
            dict.Add("Node 2.1.1", NodeFactory.CreateCause("Node 2.1.1", SequentialId.NewId()));
            dict.Add("Node 2.2", NodeFactory.CreateCause("Node 2.2", SequentialId.NewId()));

            new AddLinkCommand(db, dict["Problem"], dict["Node 1"], true);
            new AddLinkCommand(db, dict["Problem"], dict["Node 2"], true);
            new AddLinkCommand(db, dict["Node 1"], dict["Node 1.1"], true);
            new AddLinkCommand(db, dict["Node 1"], dict["Node 1.2"], true);
            new AddLinkCommand(db, dict["Node 2"], dict["Node 2.1"], true);
            new AddLinkCommand(db, dict["Node 2"], dict["Node 2.2"], true);
            new AddLinkCommand(db, dict["Node 2.1"], dict["Node 2.1.1"], true);

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
