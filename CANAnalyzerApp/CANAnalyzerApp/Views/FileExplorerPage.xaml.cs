using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CANAnalyzerApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FileExplorerPage : ContentPage
	{
		public FileExplorerPage ()
		{
			InitializeComponent ();
		}

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if((sender as ViewCell) == CAN1LineViewCell)
            {
                // Faccio il push della pagina contenente la lista
                DisplayAlert("Titolo", "CAN 1", "OK");
                Navigation.PushAsync(new FileListPage());
            }
            else if ((sender as ViewCell) == CAN2LineViewCell)
            {
                DisplayAlert("Titolo", "CAN 2", "OK");
            }
            else if ((sender as ViewCell) == KLineViewCell)
            {
                DisplayAlert("Titolo", "K", "OK");
            }
        }
    }
}