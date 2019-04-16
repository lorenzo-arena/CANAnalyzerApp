using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CANAnalyzerApp.Models;
using CANAnalyzerApp.Views;
using CANAnalyzerApp.ViewModels;

namespace CANAnalyzerApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BLETestPage : ContentPage
	{
        BLETestViewModel viewModel;

        public BLETestPage()
		{
			InitializeComponent ();

            BindingContext = viewModel = new BLETestViewModel();

            MessagingCenter.Subscribe<BLETestViewModel>(this, "DeviceConnectedOk", async (obj) =>
            {
                await DisplayAlert("CANAnalyzer", "Dispositivo connesso.", "Ok");
            });

            MessagingCenter.Subscribe<BLETestViewModel>(this, "DeviceConnectedFailed", async (obj) =>
            {
                await DisplayAlert("CANAnalyzer", "Connessioni fallita.", "Ok");
            });
        }

        
	}
}