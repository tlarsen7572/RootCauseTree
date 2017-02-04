using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    interface IRootCauseDb
    {
        bool InsertTopLevel(Node node);
        bool AddLink(Node startNode,Node endNode);
        bool RemoveLink(Node startNode,Node endNode);
        bool ChangeNodeText(Node node, string newText);
        bool AddNode(Node startNode, Node newNode);
        bool RemoveNode(Node removeNode);
        bool RemoveNodeChain(Node removeNode);
        bool MoveNode(Node node, Node targetNode);
        bool UndoMoveNode(Node node, Node targetNode, IEnumerable<Node> oldParents);
        bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents);
        bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents,IEnumerable<Node> oldNodes);
        bool UndoRemoveLink(Node startNode, Node endNode);
        bool RemoveTopLevel(Node node);
    }
}
