using System;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    [Serializable]
    class CommandFailedDbWriteException : Exception { }

    [Serializable]
    class CommandAlreadyExecutedException : Exception { }

    [Serializable]
    class CommandNotExecutedException:Exception { }
}
