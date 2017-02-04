using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class NullDb : IRootCauseDb
    {
        private bool _ReturnValue;
        public NullDb():this(true) { }
        public NullDb(bool returnValue) { _ReturnValue = returnValue; }
        public bool InsertTopLevel(Node node) { return _ReturnValue; }
        public bool AddLink(Node startNode, Node endNode) { return _ReturnValue; }
        public bool RemoveLink(Node startNode, Node endNode) { return _ReturnValue; }
        public bool ChangeNodeText(Node node, string newText) { return _ReturnValue; }
        public bool AddNode(Node startNode, Node newNode) { return _ReturnValue; }
        public bool RemoveNode(Node removeNode) { return _ReturnValue; }
        public bool RemoveNodeChain(Node removeNode) { return _ReturnValue; }
        public bool MoveNode(Node node, Node targetNode) { return _ReturnValue; }
        public bool UndoMoveNode(Node node, Node targetNode, IEnumerable<Node> oldParents) { return _ReturnValue; }
        public bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents) { return _ReturnValue; }
        public bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents, IEnumerable<Node> oldNodes) { return _ReturnValue; }
        public bool UndoRemoveLink(Node startNode,Node endNode) { return true; }
        public bool RemoveTopLevel(Node node) { return true; }
    }
}
