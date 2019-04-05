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

            MenuPages.Add((int)MenuItemType.CANSpyOne, (NavigationPage)Detail);
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.CANSpyOne:
                    case (int)MenuItemType.CANSpyTwo:
                        var CANPage = new CANSpyPage();

                        if (id == (int)MenuItemType.CANSpyOne)
                            CANPage.Title = "CAN Line 1";
                        else if (id == (int)MenuItemType.CANSpyTwo)
                            CANPage.Title = "CAN Line 2";

                        var CANNavPage = new NavigationPage(CANPage);
                        CANNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, CANNavPage);
                        break;
                    // TEMPORANEO
                    case (int)MenuItemType.KLineSpy:
                        var KPage = new CANSpyPage();
                        KPage.Title = "K Line";

                        var KNavPage = new NavigationPage(KPage);
                        KNavPage.BarBackgroundColor = Color.FromHex("#282828");

                        MenuPages.Add(id, KNavPage);
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}