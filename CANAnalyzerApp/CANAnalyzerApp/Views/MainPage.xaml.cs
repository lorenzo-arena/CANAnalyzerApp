using CANAnalyzerApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CANAnalyzerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add((int)MenuItemType.DeviceSettings, (NavigationPage)Detail);
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.CANSpyOne:
                    {
                        var CANPage = new CANSpyPage(1);
                        var CANNavPage = new NavigationPage(CANPage);
                        CANNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, CANNavPage);
                        break;
                    }

                    case (int)MenuItemType.CANSpyTwo:
                    {
                        var CANPage = new CANSpyPage(2);
                        var CANNavPage = new NavigationPage(CANPage);
                        CANNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, CANNavPage);
                        break;
                    }

                    case (int)MenuItemType.KLineSpy:
                    {
                        var KPage = new KSpyPage();
                        KPage.Title = "K Line";

                        var KNavPage = new NavigationPage(KPage);
                        KNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, KNavPage);
                        break;
                    }

                    case (int)MenuItemType.DeviceSettings:
                    {
                        var SettingsPage = new DeviceSettingsPage();
                        SettingsPage.Title = "Device Settings";

                        var SettingsNavPage = new NavigationPage(SettingsPage);
                        SettingsNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, SettingsNavPage);
                        break;
                    }

                    case (int)MenuItemType.FileExplorer:
                    {
                        var ExplorerPage = new FileExplorerPage();
                        ExplorerPage.Title = "File Explorer";

                        var SettingsNavPage = new NavigationPage(ExplorerPage);
                        SettingsNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, SettingsNavPage);
                        break;
                    }

                    case (int)MenuItemType.BLETest:
                    {
                        var BLEPage = new BLETestPage();
                        BLEPage.Title = "BLE Test";

                        var BLENavPage = new NavigationPage(BLEPage);
                        BLENavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, BLENavPage);
                        break;
                    }
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);
            }

            IsPresented = false;
        }
    }
}