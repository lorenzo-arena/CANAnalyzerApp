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

            MessagingCenter.Subscribe<DeviceSettingsViewModel>(this, "DeviceConnectedFailed", async (sender) =>
            {
                await OnDeviceConnectionFailed();
            });

            MessagingCenter.Subscribe<DeviceSettingsViewModel, string>(this, "DeviceConnectedError", async (sender, message) =>
            {
                await OnDeviceConnectionError(message);
            });
        }

        private async Task OnDeviceConnectionFailed()
        {
            await DisplayAlert("CANAnalyzer", "Connection failed.", "Ok");
        }

        private async Task OnDeviceConnectionError(string message)
        {
            await DisplayAlert("CANAnalyzer", "Error: " + message, "Ok");
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

    public class StatusStringFromBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return "Connected";
            else
                return "Disconnected";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "Connected")
                return true;
            else
                return false;
        }
    }

    public class ColorFromBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Color.FromHex("#00DE00");
            else
                return Color.FromHex("#DE0000");
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Color)value == Color.FromHex("#00DE00"))
                return true;
            else
                return false;
        }
    }
}