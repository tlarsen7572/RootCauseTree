using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Children;
        private HashSet<Node> _Parents;
        private Dictionary<Node, Node> _ParentChildLinks;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }

        public RemoveNodeCommand(IRootCauseDb db,Node removeNode) : this(db,removeNode, false) { }

        public RemoveNodeCommand(IRootCauseDb db, Node removeNode,bool executeImmediately)
        {
            _Children = new HashSet<Node>();
            _Parents = new HashSet<Node>();
            _ParentChildLinks = new Dictionary<Node, Node>();
            _RemoveNode = removeNode;
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.RemoveNode(_RemoveNode)) { throw new CommandFailedDbWriteException(); }

            //Make note of any links between the node's parents and the node's children
            //We need this information to rebuild these links if we undo the command
            foreach (var parent in _RemoveNode.ParentNodes)
            {
                foreach (var parentChild in parent.ChildNodes)
                {
                    foreach (var child in _RemoveNode.ChildNodes)
                    {
                        if (ReferenceEquals(parentChild, child))
                        {
                            _ParentChildLinks.Add(parent, parentChild);
                        }
                    }
                }
            }

            //Link up each of the node's children to each of the node's parents
            foreach (var node in _RemoveNode.ChildNodes)
            {
                _Children.Add(node);
                foreach (var parent in _RemoveNode.ParentNodes)
                {
                    node.AddParent(parent);
                }
                node.RemoveParent(_RemoveNode);
            }

            //Link up each of the node's parents to each of the node's children
            foreach (var parent in _RemoveNode.ParentNodes)
            {
                _Parents.Add(parent);
                foreach (var node in _RemoveNode.ChildNodes)
                {
                    parent.AddChild(node);
                }
                parent.RemoveChild(_RemoveNode);
            }

            //Remove eacho f the node's children
            foreach (var node in _Children)
            {
                _RemoveNode.RemoveChild(node);
            }

            //Remove each of the node's parents
            foreach (var parent in _Parents)
            {
                _RemoveNode.RemoveParent(parent);
            }
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.UndoRemoveNode(_RemoveNode, _Parents, _Children,_ParentChildLinks)) { throw new CommandFailedDbWriteException(); }

            //Remove the links between the node's parents and children and add the children back to the node
            foreach (var node in _Children)
            {
                _RemoveNode.AddChild(node);
                node.AddParent(_RemoveNode);
                foreach (var parent in _Parents)
                {
                    parent.RemoveChild(node);
                    node.RemoveParent(parent);
                }
            }

            //Add the parents back to the node
            foreach (var parent in _Parents)
            {
                parent.AddChild(_RemoveNode);
                _RemoveNode.AddParent(parent);
            }

            //Re-create any links from the node's parents to the node's children
            foreach (var link in _ParentChildLinks)
            {
                link.Key.AddChild(link.Value);
                link.Value.AddParent(link.Key);
            }

            _Children.Clear();
            _Parents.Clear();
            _ParentChildLinks.Clear();
            Executed = false;
        }
    }
}
