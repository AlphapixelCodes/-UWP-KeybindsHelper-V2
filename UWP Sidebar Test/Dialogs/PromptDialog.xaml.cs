using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Sidebar_Test
{
    public sealed partial class PromptDialog : ContentDialog
    {
        public event EventHandler ConfirmedEvent;
        private string t, c;
        public delegate Tuple<bool, String> ValidationDelegate(string text);
        public ValidationDelegate Validation;
        public PromptDialog(string title, string Current = "")
        {
            t = title;
            c = Current;

            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Box.Text.Length == 0)
            {
                args.Cancel = true;
                ErrorBox.Text = "Input Cannot Be Empty";
                return;
            }
            if (Validation != null)
            {
                var res = Validation(Box.Text);
                if (!res.Item1)
                {
                    args.Cancel = true;
                    ErrorBox.Text = res.Item2;
                    return;
                }
            }
            ConfirmedEvent?.Invoke(Box.Text, EventArgs.Empty);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
                Hide();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Box.KeyDown += (a, b) =>
            {
                if (b.Key.Equals(Windows.System.VirtualKey.Enter))
                {
                    if (Box.Text.Length == 0)
                        ErrorBox.Text = "Input Cannot Be Empty";
                    else if (Validation != null)
                    {
                        var res = Validation(Box.Text);
                        if (!res.Item1)
                        {
                            ErrorBox.Text = res.Item2;
                            return;
                        }
                    }
                    else
                    {
                        ConfirmedEvent?.Invoke(Box.Text, EventArgs.Empty);
                        Hide();
                    }
                }
            };
            Title = t;
            Box.Text = c;
        }
    }
}
