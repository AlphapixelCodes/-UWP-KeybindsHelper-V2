using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class EditKeybindDialog : ContentDialog
    {
        private Keybind Keybind { get; }

        public EditKeybindDialog(Keybind keybind)
        {
            Keybind = keybind;
            this.InitializeComponent();
            
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Keybind.Name = NameBox.Text;
            Keybind.KB1 = KB1.Text;
            Keybind.KB2 = KB2.Text;
            Keybind.KB3 = KB3.Text;
            ProjectInfo.ChangesSaved = false;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var tb in StackControl.Children.OfType<TextBox>())
            {
                tb.KeyDown += Tb_KeyDown;
                tb.TextChanged += TextChanged_Event;
            }
            
            NameBox.Text = Keybind.Name;
            KB1.Text = Keybind.KB1;
            KB2.Text = Keybind.KB2;
            KB3.Text = Keybind.KB3;
        }

        private void Tb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
           
            if (e.Key == Windows.System.VirtualKey.Enter && PrimaryButtonText.Length>0)
            {
                ContentDialog_PrimaryButtonClick(null, null);
                Hide();
            }

        }
        private bool Validate()
        {
            var kb1Blank = KB1.Text.Length == 0;
            var kb2Blank = KB2.Text.Length == 0;
            var kb3Blank = KB3.Text.Length == 0;
            return !((kb1Blank && (!kb2Blank || !kb3Blank)) || (kb2Blank && !kb3Blank));
        }


        private void TextChanged_Event(object sender, TextChangedEventArgs e)
        {
            var kb1Blank = KB1.Text.Length == 0;
            var kb2Blank = KB2.Text.Length == 0;
            var kb3Blank = KB3.Text.Length == 0;
            Debug.WriteLine(kb1Blank+" "+kb2Blank +" "+kb3Blank);
            if(kb1Blank && (!kb2Blank || !kb3Blank))
            {
                Error.Text = "Second/Third Keybind cannot be set if the First Keybind is Empty";
                PrimaryButtonText = "";
            }else if(kb2Blank && !kb3Blank)
            {
                PrimaryButtonText = "";
                Error.Text = "Third Keybind cannot be set if the Second Keybind is Empty";
            }else
            {
                PrimaryButtonText = "Save";
                Error.Text = "";
            }
        }
    }
}

