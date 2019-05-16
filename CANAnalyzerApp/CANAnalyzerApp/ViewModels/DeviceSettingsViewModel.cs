using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;

namespace CANAnalyzerApp.ViewModels
{
    class DeviceSettingsViewModel : BaseViewModel
    {
        public ICommand ConnectCommand { get; }

        bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }

        bool isConnecting = false;
        public bool IsConnecting
        {
            get { return isConnecting; }
            set { SetProperty(ref isConnecting, value); }
        }

        string serialNumber = "";
        public string SerialNumber
        {
            get { return serialNumber; }
            set { SetProperty(ref serialNumber, value); }
        }

        string firmwareVersion = "";
        public string FirmwareVersion
        {
            get { return firmwareVersion; }
            set { SetProperty(ref firmwareVersion, value); }
        }

        string appVersion = "";
        public string AppVersion
        {
            get { return appVersion; }
            set { SetProperty(ref appVersion, value); }
        }

        public DeviceSettingsViewModel()
        {
            Title = "Device Settings";

            AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

            ConnectCommand = new Command(async () => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsConnecting = true;
                    IsConnected = false;
                });

                try
                {
                    if (await AnalyzerDevice.ConnectToDeviceAsync())
                        MessagingCenter.Send<DeviceSettingsViewModel>(this, "DeviceConnectedOk");
                    else
                        MessagingCenter.Send<DeviceSettingsViewModel>(this, "DeviceConnectedFailed");
                }
                catch(Exception ex)
                {
                    MessagingCenter.Send<DeviceSettingsViewModel, string>(this, "DeviceConnectedError", ex.Message);
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsConnecting = false;
                    IsConnected = AnalyzerDevice.IsConnected();

                    if (IsConnected)
                    {
                        SerialNumber = AnalyzerDevice.GetSerialNumber();
                        FirmwareVersion = AnalyzerDevice.GetFirmwareVersion();
                    }
                });
            });
        }
    }
}
