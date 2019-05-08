using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Models
{
    public class KSpyParameters
    {
        public const int EvenParityType = 0;
        public const int OddParityType = 1;

        public int BitTiming { get; set; }

        public bool ParityCheck { get; set; }

        public int ParityType { get; set; }

        public bool ErrorReception { get; set; }

        public int Delay { get; set; }
    }
}
