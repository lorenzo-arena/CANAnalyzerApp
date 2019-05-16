using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace CANAnalyzerApp.ViewModels
{
    class KSpyViewModel : BaseViewModel
    {
        List<int> baudRates;
        public List<int> BaudRates
        {
            get { return baudRates; }
        }

        int selectedBaudRate;
        public int SelectedBaudRate
        {
            get { return selectedBaudRate; }
            set { SetProperty(ref selectedBaudRate, value); }
        }

        List<string> parityTypes;
        public List<string> ParityTypes
        {
            get { return parityTypes; }
        }

        string selectedParityType;
        public string SelectedParityType
        {
            get { return selectedParityType; }
            set { SetProperty(ref selectedParityType, value); }
        }

        private const string parityTypeEven = "Even";
        private const string parityTypeOdd = "Odd";

        bool enableParityCheck;
        public bool EnableParityCheck
        {
            get { return enableParityCheck; }
            set { SetProperty(ref enableParityCheck, value); }
        }

        bool errorReception;
        public bool ErrorReception
        {
            get { return errorReception; }
            set { SetProperty(ref errorReception, value); }
        }

        int delay;
        public int Delay
        {
            get { return delay; }
            set { SetProperty(ref delay, value); }
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public KSpyViewModel()
        {
            Title = "K Line 1";

            baudRates = new List<int>();
            baudRates.Add(9600);
            baudRates.Add(10400);
            baudRates.Add(19200);
            baudRates.Add(38400);
            baudRates.Add(57600);
            baudRates.Add(115200);
            baudRates.Add(125000);
            baudRates.Add(250000);

            selectedBaudRate = 9600;

            parityTypes = new List<string>();
            parityTypes.Add(parityTypeEven);
            parityTypes.Add(parityTypeOdd);

            selectedParityType = parityTypeEven;

            enableParityCheck = false;
            errorReception = false;

            delay = 1;            

            StartCommand = new Command(async () => {
                // TODO : Implementare
            });

            StopCommand = new Command(async () => {
                // TODO : Implementare
            });
        }
    }
}
