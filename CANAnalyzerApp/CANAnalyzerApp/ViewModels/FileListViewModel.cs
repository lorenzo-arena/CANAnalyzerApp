using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using CANAnalyzerApp.Models;
using System.Threading.Tasks;
using System.IO;
using Xamarin.Essentials;

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

        string selectedFile;
        public string SelectedFile
        {
            get { return selectedFile; }
            set { SetProperty(ref selectedFile, value); }
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

            DownloadFileCommand = new Command(async () => {
                MessagingCenter.Send<FileListViewModel, string>(this, "DownloadFileError", "");
                try
                {
                    var fileContent = await AnalyzerDevice.GetSpyFile(fileType, selectedFile);

                    // Devo condividere fileContent, che sarà un byte[]
                    var filePath = Path.Combine(FileSystem.CacheDirectory, selectedFile);
                    File.WriteAllBytes(filePath, fileContent);

                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = Path.GetFileNameWithoutExtension(selectedFile),
                        File = new ShareFile(filePath)
                    });
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<FileListViewModel, string>(this, "DownloadFileError", ex.Message);
                }
            });

            DownloadFilesList(fileType);
        }

        public ICommand DownloadFileCommand { get; }

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
                    MessagingCenter.Send<FileListViewModel, string>(this, "DownloadFilesListError", ex.Message);
                }

                IsDownloading = false;
            });
        }
    }
}
