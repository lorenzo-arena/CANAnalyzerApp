using System;
using System.Collections.Generic;
using System.Text;

namespace CANAnalyzerApp.Models
{
    public enum SpyFileType
    {
        FileTypeCAN1,
        FileTypeCAN2,
        FileTypeK,
    }

    public class SpyFile
    {
        public string FileName { get; set; }
    }
}
