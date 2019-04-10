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

        public CANSpyPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new CANSpyViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}