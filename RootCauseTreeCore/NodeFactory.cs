using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    static class NodeFactory
    {
        public static Node CreateCause(string text,long nodeId)
        {
            return new Cause(text, nodeId);
        }

        public static Node CreateProblem(string text,long nodeId)
        {
            return new Problem(text, nodeId);
        }
    }
}
