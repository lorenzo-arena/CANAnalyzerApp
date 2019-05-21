using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Models
{
    public class CANSpyMessage
    {
        public UInt32 Time { get; set; } // utilizzare ?

        public UInt32 Id { get; set; }

        public byte DataSize { get; set; }

        public bool IsError { get; set; }

        public UInt32 ErrorCode { get; set; }

        public byte[] Data { get; set; }

        public static int StructSize = 24;
    }
}
