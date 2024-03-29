﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CANAnalyzerApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SpyControlsPage : ContentPage
	{
		public SpyControlsPage ()
		{
			InitializeComponent ();            
        }
	}

    public class TextFromIsSpyingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return "On";
            else
                return "Off";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == "On")
                return true;
            else
                return false;
        }
    }

    public class CANMonitorStringFromBufferConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "";

            List<Models.CANSpyMessage> listBuff = (List<Models.CANSpyMessage>)value;
            string textBuff = "";

            foreach(Models.CANSpyMessage message in listBuff)
            {
                if (message.IsError)
                {
                    textBuff += "\tError Frame!\n";
                }
                else
                {
                    textBuff += message.Id.ToString("X8");
                    textBuff += " ";
                    textBuff += message.Data[0].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[1].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[2].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[3].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[4].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[5].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[6].ToString("X2");
                    textBuff += " ";
                    textBuff += message.Data[7].ToString("X2");

                    textBuff += "\n";
                }
            }

            return textBuff;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}