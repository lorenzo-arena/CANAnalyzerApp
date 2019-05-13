using CANAnalyzerApp.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CANAnalyzerApp.ViewModels;

namespace CANAnalyzerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;

        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.DeviceSettings, Title="Device Settings", Foreground = Color.FromHex("#F0F0F0"), Enabled = true },
                new HomeMenuItem {Id = MenuItemType.CANSpyOne, Title="CAN Line 1", Foreground = Color.FromHex("#8F8F8F"), Enabled = false },
                new HomeMenuItem {Id = MenuItemType.CANSpyTwo, Title="CAN Line 2", Foreground = Color.FromHex("#8F8F8F"), Enabled = false },
                //new HomeMenuItem {Id = MenuItemType.KLineSpy, Title="K Line", Foreground = Color.FromHex("#8F8F8F"), Enabled = false },
                new HomeMenuItem {Id = MenuItemType.FileExplorer, Title="File Explorer", Foreground = Color.FromHex("#8F8F8F"), Enabled = false },
                //new HomeMenuItem {Id = MenuItemType.BLETest, Title="BLE Test", Foreground = Color.FromHex("#8F8F8F"), Enabled = false }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.ItemTapped += async (sender, e) =>
            {
                if (e.Item == null)
                    return;

                ListViewMenu.SelectedItem = null;

                var id = (int)((HomeMenuItem)e.Item).Id;
                await RootPage.NavigateFromMenu(id);
            };

            MessagingCenter.Subscribe<DeviceSettingsViewModel>(this, "DeviceConnectedOk", (obj) =>
            {
                EnableMenuItems();
            });
        }

        public void EnableMenuItems()
        {
            foreach (HomeMenuItem item in menuItems)
            {
                if(item.Title != "Device Settings")
                {
                    item.Enabled = true;
                    item.Foreground = Color.FromHex("#F0F0F0");
                }
            }

            ListViewMenu.ItemsSource = null;
            ListViewMenu.ItemsSource = menuItems;
        }
    }
}