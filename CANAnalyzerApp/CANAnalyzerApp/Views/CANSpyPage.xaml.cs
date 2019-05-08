using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CANAnalyzerApp.Models;
using CANAnalyzerApp.Views;
using CANAnalyzerApp.ViewModels;

namespace CANAnalyzerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CANSpyPage : TabbedPage
    {
        CANSpyViewModel viewModel;

        public CANSpyPage(int line)
        {
            InitializeComponent();

            BindingContext = viewModel = new CANSpyViewModel(line);

            foreach(var page in Children)
            {
                page.BindingContext = viewModel;
            }

            MessagingCenter.Subscribe<CANSpyViewModel, string>(this, "StartError", async (sender, message) =>
            {
                await OnError(message);
            });

            MessagingCenter.Subscribe<CANSpyViewModel, string>(this, "StopError", async (sender, message) =>
            {
                await OnError(message);
            });
        }

        private async Task OnError(string message)
        {
            await DisplayAlert("CANAnalyzer", message, "Ok");
        }
    }
}