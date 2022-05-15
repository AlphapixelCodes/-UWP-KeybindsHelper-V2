using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class GroupPage : Page
    {
        public GroupPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            switch (button.Tag)
            {
                case "Add":
                    if (GroupClass.Groups.Count <= GroupClass.MaxGroupCount)
                        GroupClass.AddDefault();
                    else
                        GroupControl.ShowMaxGroupsDialog();
                    break;
                case "Delete All":

                    var cd = new ConfirmDialog("Delete all groups", "Are you sure you want to delete all groups?");
                    cd.ConfirmEvent += ((send, confirmed) =>
                      {
                          if (confirmed)
                              GroupClass.Groups.Clear();
                      });
                    cd.ShowAsync();
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GroupClass.Groups.CollectionChanged += (a, b) => AddButton.Opacity = GroupClass.Groups.Count <= GroupClass.MaxGroupCount ? 1 : .5;
            TitleBorder.Tapped += TitleBorder_Tapped;
            TitleBorder.PointerEntered += (a, b) => TitleBorder.BorderThickness = new Thickness(1);
            TitleBorder.PointerExited += (a, b) => TitleBorder.BorderThickness =new Thickness(0);
            ProjectInfo.ProjectNameChanged += (a, b) => TitleBlock.Text = ProjectInfo.ProjectName;
            TitleBlock.Text=ProjectInfo.ProjectName;
            ProjectInfo.ChangesSavedChanged += ProjectInfo_ChangesSavedChanged;
            ProjectInfo_ChangesSavedChanged(null, ProjectInfo.ChangesSaved);
        }

        private void ProjectInfo_ChangesSavedChanged(object sender, bool e)
        {
            UnsavedTextBlock.Visibility = e ? Visibility.Collapsed:Visibility.Visible;
        }

        private async void TitleBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var prompt=new PromptDialog("Set Project Title",ProjectInfo.ProjectName);
            var files = await Storage.GetFileList();
            prompt.Validation = (text) =>{
                return new Tuple<bool,string>(
                    !ProjectInfo.OriginalNameNoExtension.Equals(text) || 
                    !files.Any(file => file.StartNameNoExtension.Equals(text))
                    ,"A project already exists by that name");
            };
            prompt.ConfirmedEvent += (text, b) => ProjectInfo.ProjectName = text.ToString();
            prompt.ShowAsync();
        }
    }
}
