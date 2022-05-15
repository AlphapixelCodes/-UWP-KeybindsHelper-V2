using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Sidebar_Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilePage : Page
    {
        public FilePage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }
        private async void RefreshList(object sender = null, RoutedEventArgs e = null)
        {

            var files=await Storage.GetFileList();
            FileListView.Items.Clear();
            var lowerSearch=SearchFileBox.Text.ToLower();
            if (SearchFileBox.Text.Length > 0)
                files = files.Where(z => z.StartNameNoExtension.ToLower().Contains(lowerSearch)).ToList();
            if (SortCombobox.SelectedItem.Equals("Name"))
            {
                files = files.OrderBy(name => name).ToList();
            }
            foreach (var file in files)
            {
                FileListView.Items.Add(file);
            }
            
        }

        private async void FileMenuFlyoutItem_Click(object sender=null, RoutedEventArgs e=null)
        {
            String tag,filename;
            if (sender is Button)
            {
                tag = (sender as Button).Tag.ToString();
                if (FileListView.SelectedIndex == -1)
                {
                    var cd=new ConfirmDialog("Select Project", "No project is selected. Please select a project");
                    cd.PrimaryButtonText = "";
                    cd.SecondaryButtonText = "Ok";
                    cd.ShowAsync();
                    return;
                }

                filename= FileListView.SelectedItem.ToString();
            }
            else
            {
                MenuFlyoutItem mfi = sender as MenuFlyoutItem;
                tag=mfi.Tag.ToString();
                filename = mfi.DataContext as string;
            }
            switch (tag)
            {
                case "Open":
                    if (!ProjectInfo.ChangesSaved)
                    {
                        var diag = new ConfirmDialog("Unsaved Changes", "Do you want to open \"" + filename + "\" without saving \"" + ProjectInfo.ProjectName + "\"?");
                        diag.ConfirmEvent +=(a,result)=> OpenFile(filename,result);
                        diag.ShowAsync();
                    }
                    else
                    {
                        OpenFile(filename);
                    }
                    break;
                case "Delete":
                    var cd= new ConfirmDialog("Delete File", "Are you sure you want to delete: " + filename);                    
                    cd.ConfirmEvent += async (a, b) =>
                    {
                        if (b)
                        {
                            await Storage.DeleteFile(filename);
                            RefreshList();
                        }
                    };
                    cd.ShowAsync();
                    break;
                case "Copy":
                    int i = 0;
                    while(await Storage.FileExists(filename+" - Copy "+((i>0)? "("+i+")":"")))
                    {
                        i++;
                    }
                    await Storage.CopyFileTo(filename, filename + " - Copy " + ((i > 0) ? "(" + i + ")" : ""));
                    RefreshList();
                    break;
            }
        }

        private async void OpenFile(string filename, bool result=true)
        {
            if (result)
                if (!await ProjectInfo.OpenFile(await Storage.GetFile(filename)))
                {
                    new MessageDialog("Failed To Open File").ShowAsync();
                }
        }

        private void SortCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

        private void SearchFileBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshList();

    }
}
