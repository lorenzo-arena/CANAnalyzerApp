using CANAnalyzerApp.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
                new HomeMenuItem {Id = MenuItemType.CANSpyOne, Title="CAN Line 1", Foreground = Color.FromHex("#282828") },
                new HomeMenuItem {Id = MenuItemType.CANSpyTwo, Title="CAN Line 2", Foreground = Color.FromHex("#F0F0F0") },
                new HomeMenuItem {Id = MenuItemType.KLineSpy, Title="K Line", Foreground = Color.FromHex("#F0F0F0") },
#if DEBUG
                new HomeMenuItem {Id = MenuItemType.BLETest, Title="BLE Test", Foreground = Color.FromHex("#F0F0F0") }
#endif
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                // Imposto il foreground della voce selezionata in grigio
                // e tutte le altre in bianco per ripristinare la precedente

                foreach(HomeMenuItem item in menuItems)
                {
                    if(item == (HomeMenuItem)e.SelectedItem)
                        item.Foreground = Color.FromHex("#282828");
                    else
                        item.Foreground = Color.FromHex("#F0F0F0");
                }

                ListViewMenu.ItemsSource = null;
                ListViewMenu.ItemsSource = menuItems;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            }; 
        }
    }
}