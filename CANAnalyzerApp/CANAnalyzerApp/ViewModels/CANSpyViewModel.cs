using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CANAnalyzerApp.Models;
using CANAnalyzerApp.Views;

namespace CANAnalyzerApp.ViewModels
{
    public class CANSpyViewModel : BaseViewModel
    {
        public CANSpyViewModel()
        {
            Title = "CAN Spy 1";
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
 