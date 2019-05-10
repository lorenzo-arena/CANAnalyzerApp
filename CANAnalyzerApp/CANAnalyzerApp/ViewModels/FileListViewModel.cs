using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using CANAnalyzerApp.Models;

namespace CANAnalyzerApp.ViewModels
{
    class FileListViewModel : BaseViewModel
    {
        List<SpyFile> files;
        public List<SpyFile> Files
        {
            get { return files; }
            set { SetProperty(ref files, value); }
        }

        public FileListViewModel()
        {
            Files = new List<SpyFile>();

            Files.Add(new SpyFile { FileName = "1.txt" });
            Files.Add(new SpyFile { FileName = "2.txt" });
            Files.Add(new SpyFile { FileName = "3.txt" });
            Files.Add(new SpyFile { FileName = "4.txt" });
            Files.Add(new SpyFile { FileName = "5.txt" });
        }

    }
}
