using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Models
{
    public class CANSpyMessage
    {
        public UInt32 Time { get; set; } // utilizzare ?

        public UInt32 id { get; set; }

        public byte dataSize { get; set; }

        public bool isError { get; set; }

        public UInt32 errorCode { get; set; }

        public byte[] data { get; set; }
    }
}
