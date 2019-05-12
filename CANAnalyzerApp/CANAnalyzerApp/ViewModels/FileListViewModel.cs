using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using CANAnalyzerApp.Models;
using System.Threading.Tasks;

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

        bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set { SetProperty(ref isDownloading, value); }
        }

        public FileListViewModel(SpyFileType fileType)
        {
            Files = new List<SpyFile>();

            DownloadFilesList(fileType);
        }

        private void DownloadFilesList(SpyFileType fileType)
        {
            IsDownloading = true;

            Task.Run(async () =>
            {
                try
                {
                    Files = await AnalyzerDevice.GetSpyFiles(fileType);
                }
                catch(Exception ex)
                {
                    MessagingCenter.Send(this, "DownloadFilesListError");
                }

                IsDownloading = false;
            });
        }
    }
}
