using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class CommandFailedDbWriteException : Exception { }

    class CommandAlreadyExecutedException : Exception { }

    class CommandNotExecutedException:Exception { }
}
