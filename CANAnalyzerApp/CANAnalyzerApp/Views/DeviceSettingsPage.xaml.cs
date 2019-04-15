using CANAnalyzerApp.ViewModels;
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
	public partial class DeviceSettingsPage : ContentPage
	{
        DeviceSettingsViewModel viewModel;

        public DeviceSettingsPage ()
		{
			InitializeComponent ();

            BindingContext = viewModel = new DeviceSettingsViewModel();

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

    public class SettingsFromStringConverter : IValueConverter
    {
        private const char dash = '\u2014';

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "")
                return dash.ToString();
            else
                return (string)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == dash.ToString())
                return "";
            else
                return (string)value;
        }
    }
}