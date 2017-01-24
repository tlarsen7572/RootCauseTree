using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    static class SequentialGuid
    {
        public static Guid NewGuid()
        {
            byte[] bytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(DateTime.UtcNow.Ticks), bytes, 8);
            Array.Copy(Guid.NewGuid().ToByteArray(),0, bytes, 8,8);
            return new Guid(bytes);
        }
    }
}
