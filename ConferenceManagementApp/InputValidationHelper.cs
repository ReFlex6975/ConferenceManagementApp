using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace ConferenceManagementApp
{
    public static class InputValidationHelper
    {
        public static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        public static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab)
            {
                return;
            }

            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var clipboardText = Clipboard.GetText();
                    if (!IsTextAllowed(clipboardText))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("^[a-zA-ZА-Яа-я]+$");
            return regex.IsMatch(text);
        }
    }
}
