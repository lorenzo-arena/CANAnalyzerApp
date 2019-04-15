using CANAnalyzerApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CANAnalyzerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KSpyPage : TabbedPage
    {
        KSpyViewModel viewModel;
   
        public KSpyPage ()
        {
            InitializeComponent();

            BindingContext = viewModel = new KSpyViewModel();

            foreach (var page in Children)
            {
                page.BindingContext = viewModel;
            }
        }
    }
}