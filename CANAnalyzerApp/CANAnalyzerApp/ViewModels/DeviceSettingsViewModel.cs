﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

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

        public DeviceSettingsViewModel()
        {
            Title = "Device Settings";

            ConnectCommand = new Command(async () => {
                IsConnecting = true;
                IsConnected = false;

                if (await AnalyzerDevice.ConnectToDeviceAsync())
                    MessagingCenter.Send(this, "DeviceConnectedOk");
                else
                    MessagingCenter.Send(this, "DeviceConnectedFailed");

                IsConnecting = false;
                IsConnected = AnalyzerDevice.IsConnected();
            });
        }
    }
}
