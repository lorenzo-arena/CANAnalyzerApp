using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CANAnalyzerApp.Models;

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
            // Quando clicco una cella apro la pagina con la lista dei file del tipo scelto
            if((sender as ViewCell) == CAN1LineViewCell)
            {
                // Faccio il push della pagina contenente la lista
                Navigation.PushAsync(new FileListPage(SpyFileType.FileTypeCAN1));
            }
            else if ((sender as ViewCell) == CAN2LineViewCell)
            {
                Navigation.PushAsync(new FileListPage(SpyFileType.FileTypeCAN2));
            }
            else if ((sender as ViewCell) == KLineViewCell)
            {
                Navigation.PushAsync(new FileListPage(SpyFileType.FileTypeK));
            }
        }
    }
}