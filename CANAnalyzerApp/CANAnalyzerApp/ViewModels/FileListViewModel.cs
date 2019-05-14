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

        bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set { SetProperty(ref isDownloading, value); }
        }

        private ICommand downloadFileCommand;

        public FileListViewModel(SpyFileType fileType)
        {
            Files = new List<SpyFile>();

            downloadFileCommand = new Command<string>(async (fileName) => {
                try
                {
                    var fileContent = await AnalyzerDevice.GetSpyFile(fileType, fileName);

                    // Devo condividere fileContent, che sarà un byte[]
                    var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                    File.WriteAllBytes(filePath, fileContent);

                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = Path.GetFileNameWithoutExtension(fileName),
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

        private void DownloadFilesList(SpyFileType fileType)
        {
            IsDownloading = true;

            List<string> fileNames = new List<string>();

            Task.Run(async () =>
            {
                try
                {
                    fileNames = await AnalyzerDevice.GetSpyFileNames(fileType);

                    foreach(string fileName in fileNames)
                    {
                        Files.Add(new SpyFile { FileName = fileName, ItemTappedCommand = downloadFileCommand });
                    }
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
