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
        FileListViewModel _viewModel;
        SpyFileType _fileType;

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
        }

        protected async override void OnAppearing()
        {
            await _viewModel.DownloadFilesList(_fileType);
 
            // Faccio un taccone per adesso,
            // dissocio e riassocio il BindingContext
            BindingContext = null;
            BindingContext = _viewModel;
        }
    }
}