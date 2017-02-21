using System;
using IdGen;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    static class SequentialId
    {
        private static IdGenerator generator = new IdGenerator(new Random().Next(1024));

        public static long NewId()
        {
            return generator.CreateId();
        }
    }
}
