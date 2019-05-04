using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CANAnalyzerApp.Models;
using CANAnalyzerApp.Views;
using System.Collections.Generic;
using System.Windows.Input;

namespace CANAnalyzerApp.ViewModels
{
    public class CANSpyViewModel : BaseViewModel
    {
        List<int> bitTimings;
        public List<int> BitTimings
        {
            get { return bitTimings; }
        }

        int selectedBitTiming;
        public int SelectedBitTiming
        {
            get { return selectedBitTiming; }
            set { SetProperty(ref selectedBitTiming, value); }
        }

        List<double> samplingPoints;
        public List<double> SamplingPoints
        {
            get { return samplingPoints; }
        }

        double selectedSamplingPoint;
        public double SelectedSamplingPoint
        {
            get { return selectedSamplingPoint; }
            set { SetProperty(ref selectedSamplingPoint, value); }
        }

        private const string frameFormat11Bit = "11 bit";
        private const string frameFormat29Bit = "29 bit";

        List<string> frameFormats;
        public List<string> FrameFormats
        {
            get { return frameFormats; }
        }

        string selectedFrameFormat;
        public string SelectedFrameFormat
        {
            get { return selectedFrameFormat; }
            set { SetProperty(ref selectedFrameFormat, value); }
        }

        bool enableErrorReception;
        public bool EnableErrorReception
        {
            get { return enableErrorReception; }
            set { SetProperty(ref enableErrorReception, value); }
        }

        bool applyMask;
        public bool ApplyMask
        {
            get { return applyMask; }
            set { SetProperty(ref applyMask, value); }
        }

        string mask;
        public string Mask
        {
            get { return mask; }
            set { SetProperty(ref mask, value); }
        }

        string id;
        public string ID
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        int lineNumber;
        public int LineNumber
        {
            get { return lineNumber; }
            set { SetProperty(ref lineNumber, value); }
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public CANSpyViewModel(int line)
        {
            if(line == 1)
                Title = "CAN Line 1";
            else
                Title = "CAN Line 2";

            bitTimings = new List<int>();
            bitTimings.Add(50000);
            bitTimings.Add(100000);
            bitTimings.Add(125000);
            bitTimings.Add(200000);
            bitTimings.Add(250000);
            bitTimings.Add(300000);
            bitTimings.Add(400000);
            bitTimings.Add(500000);
            bitTimings.Add(1000000);

            selectedBitTiming = 50000;

            samplingPoints = new List<double>();
            samplingPoints.Add(75.00);
            samplingPoints.Add(87.50);

            selectedSamplingPoint = 75.00;

            frameFormats = new List<string>();
            FrameFormats.Add(frameFormat11Bit);
            FrameFormats.Add(frameFormat29Bit);

            selectedFrameFormat = frameFormat11Bit;

            enableErrorReception = true;
            applyMask = false;

            lineNumber = line;

            StartCommand = new Command(async () => {
                try
                {
                    var param = new CANSpyParameters();
                    param.BitTiming = SelectedBitTiming;
                    //param.SamplingPoint = SelectedSamplingPoint;

                    if (selectedFrameFormat == frameFormat11Bit)
                        param.FrameFormat = CANSpyParameters.SimpleFrameFormat;
                    else
                        param.FrameFormat = CANSpyParameters.LongFrameFormat;

                    param.ErrorReception = EnableErrorReception;
                    param.ApplyMask = ApplyMask;
                    param.Mask = Convert.ToUInt32(Mask, 16);
                    param.ID = Convert.ToUInt32(ID, 16);



                    if (lineNumber == 1)
                    {
                        await AnalyzerDevice.SetCANParametersAsync(Services.SpyType.CANSpyOne, param);
                        await AnalyzerDevice.StartSpyAsync(Services.SpyType.CANSpyOne);
                    }
                    else if (lineNumber == 2)
                    {
                        await AnalyzerDevice.SetCANParametersAsync(Services.SpyType.CANSpyTwo, param);
                        await AnalyzerDevice.StartSpyAsync(Services.SpyType.CANSpyTwo);
                    }
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<CANSpyViewModel, string>(this, "StartError", ex.Message);
                }
            });

            StopCommand = new Command(async () => {
                try
                {
                    if (lineNumber == 1)
                        await AnalyzerDevice.StopSpyAsync(Services.SpyType.CANSpyOne);
                    else if (lineNumber == 2)
                        await AnalyzerDevice.StopSpyAsync(Services.SpyType.CANSpyTwo);
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<CANSpyViewModel, string>(this, "StopError", ex.Message);
                }
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

    // Poi nel costruttore del viewModel che vuole questo evento
    MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) =>
    {
        var newItem = item as Item;
        Items.Add(newItem);
        await DataStore.AddItemAsync(newItem);
    });
    */
}
