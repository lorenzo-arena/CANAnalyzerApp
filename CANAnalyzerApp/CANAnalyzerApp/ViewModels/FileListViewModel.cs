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
using CANAnalyzerApp.Services;

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

        bool isDownloadingList;
        public bool IsDownloadingList
        {
            get { return isDownloadingList; }
            set { SetProperty(ref isDownloadingList, value); }
        }

        bool isDownloadingFile;
        public bool IsDownloadingFile
        {
            get { return isDownloadingFile; }
            set { SetProperty(ref isDownloadingFile, value); }
        }

        bool isDownloadedList;
        public bool IsDownloadedList
        {
            get { return isDownloadedList; }
            set { SetProperty(ref isDownloadedList, value); }
        }

        double downloadFileProgress;
        public double DownloadFileProgress
        {
            get { return downloadFileProgress; }
            set { SetProperty(ref downloadFileProgress, value); }
        }

        private ICommand downloadFileCommand;

        public FileListViewModel(SpyFileType fileType)
        {
            files = new List<SpyFile>();
            isDownloadingList = false;
            isDownloadedList = false;
            isDownloadingFile = false;
            downloadFileProgress = 0;

            downloadFileCommand = new Command<string>(async (fileName) => {
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (SpyFile file in Files)
                            file.IsDownloadingFile = true;

                        IsDownloadingFile = true;
                    });

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

                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (SpyFile file in Files)
                        file.IsDownloadingFile = false;

                    IsDownloadingFile = true;

                    DownloadFileProgress = 0;
                });
            });

            MessagingCenter.Subscribe<IAnalyzerDevice, double>(this, "DownloadFileProgress", (sender, progress) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DownloadFileProgress = progress;
                });
            });
        }

        public async Task DownloadFilesList(SpyFileType fileType)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsDownloadingList = true;
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
                            Files.Add(new SpyFile { FileName = fileNames[fileIndex],
                                                    FileSize = fileSizes[fileIndex],
                                                    ItemTappedCommand = downloadFileCommand,
                                                    IsDownloadingFile = false });
                        }
                    });
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsDownloadedList = true;
                });
            }
            catch (Exception ex)
            {
                MessagingCenter.Send<FileListViewModel, string>(this, "DownloadFilesListError", ex.Message);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                IsDownloadingList = false;
            });
        }
    }
}
