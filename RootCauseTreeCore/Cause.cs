using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    sealed class Cause : Node
    {
        public Cause(string text) : base(text) { }
        public Cause(string text,Guid nodeId) : base(text, nodeId) { }
    }
}
