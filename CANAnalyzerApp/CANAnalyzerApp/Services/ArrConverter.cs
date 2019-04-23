using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Services
{
    public static class ArrConverter
    {
        public static UInt16 GetUInt16FromBuffer(byte[] buff, int index)
        {
            return (UInt16)((buff[index] << 8) |
                            (buff[index + 1]));
        }

        public static UInt32 GetUInt32FromBuffer(byte[] buff, int index)
        {
            return (UInt32)((buff[index] << 24) |
                            (buff[index + 1] << 16) |
                            (buff[index + 2] << 8) |
                            (buff[index + 3]));
        }

        public static void SetBufferFromUInt16(UInt16 toSet, byte[] buff, int index)
        {
            buff[index] = (byte)((toSet & 0xFF00) >> 8);
            buff[index + 1] = (byte)(toSet & 0x00FF);

            return;
        }

        public static void SetBufferFromUInt32(UInt32 toSet, byte[] buff, int index)
        {
            buff[index] = (byte)((toSet & 0xFF000000) >> 24);
            buff[index + 1] = (byte)((toSet & 0x00FF0000) >> 16);
            buff[index + 2] = (byte)((toSet & 0x0000FF00) >> 8);
            buff[index + 3] = (byte)((toSet & 0x000000FF));

            return;
        }
    }
}
