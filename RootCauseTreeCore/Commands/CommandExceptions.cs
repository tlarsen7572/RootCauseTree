using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    [Serializable]
    class CommandFailedDbWriteException : Exception { }

    [Serializable]
    class CommandAlreadyExecutedException : Exception { }

    [Serializable]
    class CommandNotExecutedException:Exception { }
}
