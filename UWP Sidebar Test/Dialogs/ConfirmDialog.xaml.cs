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
    public sealed partial class ConfirmDialog : ContentDialog
    {
        public event EventHandler<bool> ConfirmEvent;
        public ConfirmDialog(string title, string content,bool useOk=false)
        {
            TitleString = title;
            ContentString = content;
            UseOk = useOk;
            this.InitializeComponent();
            
        }

        public string TitleString { get; }
        public string ContentString { get; }
        public bool UseOk { get; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => ConfirmEvent?.Invoke(this,true);

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => ConfirmEvent?.Invoke(this, false);

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ContentBlock.Text = ContentString;
            Title=TitleString;
            if (UseOk)
            {
                PrimaryButtonText = "";
                SecondaryButtonText = "Ok";
            }

        }
    }
}
