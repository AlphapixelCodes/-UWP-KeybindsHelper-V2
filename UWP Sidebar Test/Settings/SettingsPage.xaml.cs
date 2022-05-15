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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Keybind_Helper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        

        public SettingsPage()
        {
            this.InitializeComponent();
        }
        private void settingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            GridPreferedColumnCountComboBox.SelectedIndex = Settings.Grid_PreferedColumnCount+1;
            MaxColumnWidthSlider.Value = Settings.Grid_MinColumnWidth;
            GridPreferedColumnCountComboBox.SelectionChanged += (a, b) => { 
                Settings.Grid_PreferedColumnCount = GridPreferedColumnCountComboBox.SelectedIndex - 1;
                MaxColumnWidthSlider.IsEnabled = Settings.Grid_PreferedColumnCount == -1;
            };
            MaxColumnWidthSlider.PointerExited += (a, b) =>
            {
                if (Settings.Grid_MinColumnWidth != (int)MaxColumnWidthSlider.Value)
                    Settings.Grid_MinColumnWidth = (int)MaxColumnWidthSlider.Value;
            };
            MaxColumnWidthSlider.IsEnabled = Settings.Grid_PreferedColumnCount == -1;
        }


        
        



        private void Default_Grid_Slider_Click(object sender, RoutedEventArgs e) => MaxColumnWidthSlider.Value = 375;

        private void Default_Grid_PreferedColumnCount_Click(object sender, RoutedEventArgs e) => GridPreferedColumnCountComboBox.SelectedIndex = 0;
    }
}
