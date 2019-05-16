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

        bool isDownloaded;
        public bool IsDownloaded
        {
            get { return isDownloaded; }
            set { SetProperty(ref isDownloaded, value); }
        }

        private ICommand downloadFileCommand;

        public FileListViewModel(SpyFileType fileType)
        {
            files = new List<SpyFile>();
            isDownloading = false;
            isDownloaded = false;

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
        }

        public async Task DownloadFilesList(SpyFileType fileType)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsDownloading = true;
            });

            try
            {
                var fileNum = await AnalyzerDevice.GetSpyFileNumber(fileType);

                if(fileNum > 0)
                {
                    var fileNames = await AnalyzerDevice.GetSpyFileNames(fileType);
                    var fileSizes = await AnalyzerDevice.GetSpyFileSizes(fileType);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        for (int fileIndex = 0; fileIndex < fileNum; fileIndex++)
                        {
                            Files.Add(new SpyFile { FileName = fileNames[fileIndex], FileSize = fileSizes[fileIndex], ItemTappedCommand = downloadFileCommand });
                        }
                    });
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsDownloaded = true;
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<FileListViewModel, string>(this, "DownloadFilesListError", ex.Message);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                IsDownloading = false;
            });
        }
    }
}
