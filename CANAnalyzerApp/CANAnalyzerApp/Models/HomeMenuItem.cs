using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CANAnalyzerApp.Models
{
    public enum MenuItemType
    {
        CANSpyOne,
        CANSpyTwo,
        KLineSpy
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
        public Color Foreground { get; set; }
    }
}
