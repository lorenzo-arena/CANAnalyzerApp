using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

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

        public int FileSize { get; set; }

        public ICommand ItemTappedCommand { get; set; }
    }
}
