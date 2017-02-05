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
        public bool AddLink(Node parentNode, Node childNode) { return _ReturnValue; }
        public bool RemoveLink(Node parentNode, Node childNode) { return _ReturnValue; }
        public bool ChangeNodeText(Node node, string newText) { return _ReturnValue; }
        public bool AddNode(Node parentNode, Node newNode) { return _ReturnValue; }
        public bool RemoveNode(Node removeNode) { return _ReturnValue; }
        public bool RemoveNodeChain(Node removeNode) { return _ReturnValue; }
        public bool MoveNode(Node movingNode, Node targetNode) { return _ReturnValue; }
        public bool UndoMoveNode(Node movingNode, Node targetNode, IEnumerable<Node> oldParents) { return _ReturnValue; }
        public bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents) { return _ReturnValue; }
        public bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents, IEnumerable<Node> oldNodes,Dictionary<Node,Node> parentChildLinks) { return _ReturnValue; }
        public bool UndoRemoveLink(Node parentNode,Node movingNode) { return _ReturnValue; }
        public bool RemoveTopLevel(Node node) { return _ReturnValue; }
    }
}
