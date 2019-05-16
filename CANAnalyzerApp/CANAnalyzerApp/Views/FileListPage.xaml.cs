using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CANAnalyzerApp.ViewModels;
using CANAnalyzerApp.Models;
using Xamarin.Essentials;

namespace CANAnalyzerApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileListPage : ContentPage
	{
        private FileListViewModel _viewModel;
        private SpyFileType _fileType;
        private bool _isDataLoaded;

        public FileListPage()
        {
            InitializeComponent();
        }

        public FileListPage (SpyFileType fileType)
		{
            // Appena creata la pagina deve contenere la lista vuota;
            // lo spinner e' attivo mentre la pagina aspetta il messaggio dal ViewModel
            // di aggiornamento corretto della lista, che ferma lo spinner

            // In generale poi la lista e' composta da un nome del file e una pulsante "Download"
            // che quando premuto esegue un command?
			InitializeComponent ();

            MessagingCenter.Subscribe<FileListViewModel, string>(this, "DownloadFilesListError", async (sender, message) =>
            {
                await DisplayAlert("CANAnalyzer", "An error occurred during the files list download.", "Ok");
            });

            MessagingCenter.Subscribe<FileListViewModel, string>(this, "DownloadFileError", async (sender, message) =>
            {
                await DisplayAlert("CANAnalyzer", "An error occurred during the file download.", "Ok");
            });

            _viewModel = new FileListViewModel(fileType);
            BindingContext = _viewModel;

            _fileType = fileType;
            _isDataLoaded = false;
        }

        protected async override void OnAppearing()
        {
            if(!_isDataLoaded)
            {
                await _viewModel.DownloadFilesList(_fileType);

                _isDataLoaded = true;
            }
        }
    }

    public class SizeIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            int absoluteValue = (((int)value < 0) ? -((int)value) : (int)value);

            // Determino il suffisso
            string suffix;
            double readable;

            if (absoluteValue >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (int)value >> 20;
            }
            else if (absoluteValue >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (int)value >> 10;
            }
            else if (absoluteValue >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = (int)value;
            }
            else
            {
                return ((int)value).ToString("0 B"); // Byte
            }

            // Divido per 1024 per ottenere i valori decimali
            readable = (readable / 1024);

            // Restituisco la stringa formattata
            return readable.ToString("0.### ") + suffix;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int result = 0;
            return Int32.TryParse((string)value, out result) ? result : (int?)null;
        }
    }
}