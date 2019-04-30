using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Models
{
    public class CANSpyParameters
    {
        public const int SimpleFrameFormat = 11;
        public const int LongFrameFormat = 29;

        public int BitTiming { get; set; }

        public double SamplingPoint { get; set; }

        public int FrameFormat { get; set; }

        public bool ErrorReception { get; set; }

        public bool ApplyMask { get; set; }

        public UInt32 Mask { get; set; }

        public UInt32 ID { get; set; }
    }
}
