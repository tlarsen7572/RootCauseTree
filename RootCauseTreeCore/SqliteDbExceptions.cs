using System;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    [Serializable]
    class InvalidRootCauseFileException : Exception { }

    [Serializable]
    class RootCauseFileLockedException : Exception { }
}
