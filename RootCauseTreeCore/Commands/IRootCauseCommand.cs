using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    interface IRootCauseCommand
    {
        void Execute();
        void Undo();
    }
}
