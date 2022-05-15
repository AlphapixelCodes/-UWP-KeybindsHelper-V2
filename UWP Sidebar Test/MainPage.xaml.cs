using Keybind_Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_Sidebar_Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Frame[] Frames;
        private static event EventHandler ShowGroupPageEvent;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Settings.LoadSettings();
            GroupColumnRow.Init(true);
            Frames = new Frame[] { GroupFrame,FileFrame,SettingsFrame };

           // new MessageDialog("maybe add a rotating background animation and settings for it, themes?").ShowAsync();
            
            GroupFrame.Content = new GroupPage();
            FileFrame.Content = new FilePage();
            SettingsFrame.Content=new Keybind_Helper.SettingsPage();
            ShowFrame(GroupFrame);
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = ProjectInfo.ProjectName;
            ProjectInfo.ProjectNameChanged += (a, name) => appView.Title = name;
            ShowGroupPageEvent += (a, b) => { 
                ShowFrame(GroupFrame);
                NavigationViewControl.SelectedItem = KeybindsNavItem;
            };
            //load most recent
            
            var files = await Storage.GetFileList();
            if (files.Count > 0)
                ProjectInfo.OpenFile(files[0].ActualFile,false);
            //background animation
            new Task(async () => {
                
                while (true)
                {
                    for (double i = 0; i < 360; i+=.2)
                    {
                        Thread.Sleep(20);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { CompositTransformRotation.Rotation = i; });
                    }
                }

            }).Start();
        }
      

        private void ShowFrame(Frame frame)
        {
            foreach (var f in Frames)
            {
                f.Visibility = f.Equals(frame) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal static void ShowGroupPage()
        {
            ShowGroupPageEvent?.Invoke(null, null);
        }

        private async void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ShowFrame(SettingsFrame);
                return; 
            }
            Debug.WriteLine(args.InvokedItem);
            if(args.InvokedItem is string)
            {
                //var nvi = args.InvokedItem as NavigationViewItem;
                switch (args.InvokedItem as string)
                {
                    case "Open":
                        ShowFrame(FileFrame);
                        break;
                    case "Keybinds":
                        ShowFrame(GroupFrame);
                        break;
                }
            }
        }
        //save button
        private async void SaveChanges_NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await ProjectInfo.SaveChanges())
            {
               new ConfirmDialog("Saved", "Successfully saved as \"" + ProjectInfo.ProjectName + "\"",true).ShowAsync();
                
            }
            else
            {
                new ConfirmDialog("Error","Error Occured When Saving Changess",true).ShowAsync();
            }
        }

        private async void NewProject_NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ProjectInfo.ChangesSaved)
            {
                await ProjectInfo.NewProjectAsync();
            }
            else{
                var cd=new ConfirmDialog("Unsaved Changes", "Are you sure you want to create a new project before saving?");
                cd.ConfirmEvent += (a, b) =>
                {
                    if (b)
                        ProjectInfo.NewProjectAsync();
                };
                cd.ShowAsync();
            }
        }
    }
}
