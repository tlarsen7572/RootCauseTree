using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    sealed class Problem: Node
    {
        public Problem(string text): base(text) { }
        public Problem(string text,long nodeId) : base(text, nodeId) { }
    }
}
