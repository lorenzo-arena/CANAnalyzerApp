using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CANAnalyzerApp.ViewModels;
using CANAnalyzerApp.Models;

namespace CANAnalyzerApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileListPage : ContentPage
	{
        FileListViewModel viewModel;

        public FileListPage (SpyFileType fileType)
		{
            // Appena creata la pagina deve contenere la lista vuota;
            // lo spinner e' attivo mentre la pagina aspetta il messaggio dal ViewModel
            // di aggiornamento corretto della lista, che ferma lo spinner

            // In generale poi la lista e' composta da un nome del file e una pulsante "Download"
            // che quando premuto esegue un command?
			InitializeComponent ();

            BindingContext = viewModel = new FileListViewModel(fileType);

            MessagingCenter.Subscribe<FileListViewModel, string>(this, "DownloadFilesListError", async (sender, message) =>
            {
                await DisplayAlert("CANAnalyzer", "An error occurred during the files list download.", "Ok");
            });
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            // TEMPORANEO : BAD DESIGN
            DisplayAlert("Test", "testtest", "OK");
        }
    }
}