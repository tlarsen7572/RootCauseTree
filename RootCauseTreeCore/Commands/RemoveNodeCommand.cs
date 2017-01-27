using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class RemoveNodeCommand : IRootCauseCommand
    {
        private Node _RemoveNode;
        private HashSet<Node> _Nodes;
        private HashSet<Node> _Parents;
        private IRootCauseDb _Db;

        public RemoveNodeCommand(IRootCauseDb db,Node removeNode) : this(db,removeNode, false) { }

        public RemoveNodeCommand(IRootCauseDb db, Node removeNode,bool executeImmediately)
        {
            _Nodes = new HashSet<Node>();
            _Parents = new HashSet<Node>();
            _RemoveNode = removeNode;
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (_Db.RemoveNode(_RemoveNode))
            {
                foreach (var node in _RemoveNode.Nodes)
                {
                    _Nodes.Add(node);
                    foreach (var parent in _RemoveNode.ParentNodes)
                    {
                        node.AddParent(parent);
                    }
                    node.RemoveParent(_RemoveNode);
                }

                foreach (var parent in _RemoveNode.ParentNodes)
                {
                    _Parents.Add(parent);
                    foreach (var node in _RemoveNode.Nodes)
                    {
                        parent.AddNode(node);
                    }
                    parent.RemoveNode(_RemoveNode);
                }

                foreach (var node in _Nodes)
                {
                    _RemoveNode.RemoveNode(node);
                }

                foreach (var parent in _Parents)
                {
                    _RemoveNode.RemoveParent(parent);
                }
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }

        public void Undo()
        {
            if (_Db.UndoRemoveNode(_RemoveNode, _Parents, _Nodes))
            {
                foreach (var node in _Nodes)
                {
                    _RemoveNode.AddNode(node);
                    node.AddParent(_RemoveNode);
                    foreach (var parent in _Parents)
                    {
                        parent.RemoveNode(node);
                        node.RemoveParent(parent);
                    }
                }
                foreach (var parent in _Parents)
                {
                    parent.AddNode(_RemoveNode);
                    _RemoveNode.AddParent(parent);
                }
                _Nodes.Clear();
                _Parents.Clear();
            }
            else
            {
                throw new CommandFailedDbWriteException();
            }
        }
    }
}
