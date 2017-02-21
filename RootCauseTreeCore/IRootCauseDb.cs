using System.Collections.Generic;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    interface IRootCauseDb
    {
        bool InsertTopLevel(Node node);
        bool AddLink(Node parentNode,Node childNode);
        bool RemoveLink(Node parentNode,Node childNode);
        bool ChangeNodeText(Node node, string newText);
        bool AddNode(Node parentNode, Node newNode);
        bool RemoveNode(Node removeNode);
        bool RemoveNodeChain(Node removeNode);
        bool MoveNode(Node movingNode, Node targetNode);
        bool UndoMoveNode(Node moveingNode, IEnumerable<Node> oldParents);
        bool UndoRemoveNodeChain(Node removeNode, IEnumerable<Node> oldParents);
        bool UndoRemoveNode(Node removeNode, IEnumerable<Node> oldParents,IEnumerable<Node> oldNodes,Dictionary<Node,Node> parentChildLinks);
        bool UndoRemoveLink(Node parentNode, Node childNode);
        bool RemoveTopLevel(Node node);
    }
}
