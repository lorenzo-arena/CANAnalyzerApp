using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

using CANAnalyzerApp.Models;
using CANAnalyzerApp.Views;

using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Windows.Input;

namespace CANAnalyzerApp.ViewModels
{
    public class BLETestViewModel : BaseViewModel
    {
        public ICommand ConnectCommand { get; }
        public ICommand TestCommand { get; }

        public BLETestViewModel()
        {
            Title = "BLE Test";

            ConnectCommand = new Command(async () => {
                if (await AnalyzerDevice.ConnectToDeviceAsync())
                    MessagingCenter.Send(this, "DeviceConnectedOk");
                else
                    MessagingCenter.Send(this, "DeviceConnectedFailed");
            });

            TestCommand = new Command(async () => {
                try
                {
                    if (await AnalyzerDevice.TestCommandAsync())
                        MessagingCenter.Send(this, "DeviceTestOk");
                    else
                        MessagingCenter.Send(this, "DeviceTestFailed");
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<BLETestViewModel, string>(this, "TestError", ex.Message);
                }
            });
        }
    }
}
 