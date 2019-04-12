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
                if (await AnalyzerDevice.TestCommandAsync())
                    MessagingCenter.Send(this, "DeviceTestOk");
                else
                    MessagingCenter.Send(this, "DeviceTestFailed");
            });
        }
    }

    // Questo da usare come esempio per gli ICommand in MVVM
    /*
     * 
     * public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));
        }

        public ICommand OpenWebCommand { get; }
    }
    */

    // Questo invece come esempio per restituire qualcosa da una modal, usando il messagging center
    /*
     * 
     * 
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();

            Item = new Item
            {
                Text = "Item name",
                Description = "This is an item description."
            };

            BindingContext = this;
        }

        async void Save_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send(this, "AddItem", Item);
            await Navigation.PopModalAsync();
        }

        async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
    */
}
 