using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    static class NodeFactory
    {
        public static Node CreateCause(string text,Guid nodeId)
        {
            return new Cause(text, nodeId);
        }

        public static Node CreateProblem(string text,Guid nodeId)
        {
            return new Problem(text, nodeId);
        }
    }
}
