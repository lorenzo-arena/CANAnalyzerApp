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
	public partial class CANSettingsPage : ContentPage
	{
		public CANSettingsPage ()
		{
			InitializeComponent ();
		}
	}

    public class HexValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private static bool IsHex(char c)
        {
            if((c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F'))
            {
                return true;
            }

            return false;
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                int previusCursorPosition = ((Entry)sender).CursorPosition;
                bool isValid = args.NewTextValue.ToCharArray().All(x => IsHex(x)); // Controllo se il carattere e' esadecimale

                // Se il carattere e' esadecimale, controllo anche di avere massimo 8 caratteri
                if(isValid)
                {
                    if (args.NewTextValue.Length >= 9)
                        isValid = false;
                }

                ((Entry)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(((Entry)sender).CursorPosition, 1);

                ((Entry)sender).CursorPosition = previusCursorPosition;
            }
        }
    }
}