using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWP_Sidebar_Test
{
    public sealed partial class GroupControl : UserControl
    {

        public GroupClass groupClass { get; set; }
        public event EventHandler<bool> DragEvent;
        public GroupControl()
        {
          //  Debug.WriteLine("GroupControl Constructor Called");
            groupClass = new GroupClass(this);
            InitializeComponent();
        }

        public GroupControl(GroupClass gc)
        {
            groupClass = gc;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GroupClass.Groups.CollectionChanged += Groups_CollectionChanged;
            Groups_CollectionChanged(null, null);
            groupClass.RaisePropertyChanged("Name");
            KeybindListView.ItemsSource = groupClass.Keybinds;
            BorderHoverHighlight(TitleBorder);
            DragDropControl.DragWasDropped += DragDropControl_DragWasDropped;
            DragDropControl.Visibility = Visibility.Collapsed;
            
        }

       

        private void BorderHoverHighlight(Border border)
        {
            if ((Thickness)border.GetValue(Border.BorderThicknessProperty) == null)
                border.BorderThickness = new Thickness(5);

            var borderColor=new RevealBorderBrush() { Color= Colors.Gray };
            var transparent = new SolidColorBrush(Colors.Transparent);
            border.PointerEntered += (a, b) => border.BorderBrush = borderColor;
            border.PointerExited+=(a, b) => border.BorderBrush = transparent;
            
        }
        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CombineWithGroupFlyout.Items.Clear();
            var white =new SolidColorBrush(Colors.White);
            if (GroupClass.Groups.Count == 1)
            {
                CombineWithGroupFlyout.Items.Add(new MenuFlyoutItem() {Text="None" });
            }
            else
            {
                foreach (var group in GroupClass.Groups)
                {
                    if (group != groupClass)
                    {

                        var fly = new MenuFlyoutItem();
                        fly.PointerEntered += (a, b) => group.Control.HighlightControl(true);
                        fly.PointerExited += (a, b) => group.Control.HighlightControl(false);
                        fly.DataContext = group;
                        fly.SetBinding(MenuFlyoutItem.TextProperty, new Binding() { Path = new PropertyPath("Name") });
                        CombineWithGroupFlyout.Items.Add(fly);
                        fly.Click += (a, b) => {
                            if (group.Keybinds.Count + groupClass.Keybinds.Count < GroupClass.MaxKeybindCount)
                                groupClass.DisolveInto(group);
                            else
                                new ConfirmDialog("Max Keybinds Per Group", $"Cannot merge \"{groupClass.Name}\" into \"{group.Name}\", combined keybinds excedes group limit ({GroupClass.MaxKeybindCount}).", true).ShowAsync();
                        };
                    }
                }
            }
        }
        private bool isHighlighting;
        public void HighlightControl(bool highlight)
        {
            //THIS MIGHT HELP
            //https://stackoverflow.com/questions/40065861/how-to-animated-grid-background-like-instagram-uwp-app
            isHighlighting = highlight;
            if(highlight)
            {
                var bright = (Storyboard)Resources["Brighten"];
                bright.Stop();
                bright.Begin();
            }
            //var dark = ((Storyboard)Resources["Darken"]);        
            Debug.WriteLine("Highlight!!!!!!!!!!");

        }
        private void BrightenAnimation_Completed(object sender, object e)
        {
            if (isHighlighting)
                ((Storyboard)Resources["Darken"]).Begin();
            else
            {
                BorderGrid.Background=Application.Current.Resources["GroupBackground"] as SolidColorBrush;
            }

        }
        private void DarkenAnimation_Completed(object sender, object e)
        {
            if (isHighlighting)
                ((Storyboard)Resources["Brighten"]).Begin();
            else
                BorderGrid.Background = Application.Current.Resources["GroupBackground"] as SolidColorBrush;
        }

        /// <summary>
        /// Shows the Group Flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopGrid_RightTapped(object sender, RightTappedRoutedEventArgs e) => GroupFlyout.ShowAt((FrameworkElement)sender);
        private void GroupFlyout_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton appbarbutton=null;
            var text = "";
            
            if (sender is AppBarButton)
            {
                appbarbutton = sender as AppBarButton;
                if(appbarbutton.Tag==null)
                    text = appbarbutton.Label;
                else
                    text=appbarbutton.Tag as string;
                
            }
            else if (sender is MenuBarItem)
                text = (sender as MenuBarItem).Title;
            else
                return;
            switch (text)
            {
                case "Delete":
                    GroupClass.Groups.Remove(groupClass);
                    break;
                case "Copy":
                    if (GroupClass.Groups.Count < GroupClass.MaxGroupCount)
                        groupClass.Duplicate();
                    else
                        ShowMaxGroupsDialog();
                    break;
                case "Clear":
                    groupClass.Keybinds.Clear();
                    break;
                case "Rename Group":
                    RenameGroupPrompt();
                    break;
                case "CollapseButton":
                    if(KeybindListView.Visibility == Visibility.Visible)
                    {
                        KeybindListView.Visibility = Visibility.Collapsed;
                        appbarbutton.Label = "Show Keybinds";
                    }
                    else
                    {
                        appbarbutton.Label = "Hide Keybinds";
                        KeybindListView.Visibility = Visibility.Visible;
                    }
                    break;
            }
            
        }

        public static void ShowMaxGroupsDialog() => new ConfirmDialog("Max Groups", "Max Group Count Reached.", true).ShowAsync();

        private void AddKeybind_Click(object sender, RoutedEventArgs e)
        {
            if (groupClass.Keybinds.Count < GroupClass.MaxKeybindCount)
                groupClass.AddKeybind(new Keybind());
            else
                ShowMaxKeybindsDialog();
        }

        private void TittleBox_Tapped(object sender, TappedRoutedEventArgs e) => RenameGroupPrompt();
        
        private void RenameGroupPrompt()
        {
            var pd = new PromptDialog("Rename Group", groupClass.Name);
            pd.ConfirmedEvent += (Text, args) => { groupClass.Name = (string)Text; ProjectInfo.ChangesSaved = false; };
            pd.ShowAsync();
        }
    
        /// 
        /// 
        /// 
        ///     GroupDrag
        /// 
        /// 
        private static Type isDragDataOfType<Type>(DragEventArgs e)
        {
            object obj;
            if (e.DataView.Properties.TryGetValue("ItemViewModel", out obj))
            {
                if (obj is Type)
                {
                    return (Type)obj;
                }
            }
            return default(Type);
        }
        private void Title_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.AllowedOperations = DataPackageOperation.Move;
            args.Data.Properties.Add("ItemViewModel", this);
            DragEvent?.Invoke(this, true);
            Debug.WriteLine("Drag Group Starting");
        }

        private void GroupGrid_DragEnter(object sender, DragEventArgs e)
        {

            var drag = isDragDataOfType<GroupControl>(e);
            Debug.WriteLine("Drag enter");
            if (drag != null && drag != this)
            {
                DragDropControl.Visibility = Visibility.Visible;
            }
        }
        private void GroupGrid_DragLeave(object sender, DragEventArgs e)
        {
            Debug.WriteLine("Drag leaving");
            var drag = isDragDataOfType<GroupControl>(e);
            if (drag != null && drag != this)
            {
                DragDropControl.Visibility = Visibility.Collapsed;
            }
        }
        private void DragDropControl_DragWasDropped(object sender, GroupControl e)
        {
            e.groupClass.MoveAbove(this);
            DragDropControl.Visibility = Visibility.Collapsed;
        }
        private void GridGroup_DragComplete(UIElement sender, DropCompletedEventArgs args)
        {
            DragEvent?.Invoke(this, false);
            
        }
        
        

        //listbox blue selection fix
        private void StupidListBoxFix_Enter(object sender, PointerRoutedEventArgs e) => ((ContentPresenter)sender).Background = new SolidColorBrush(Color.FromArgb(50, 125, 125, 125));

        private void StupidListBoxFix_Exit(object sender, PointerRoutedEventArgs e) => ((ContentPresenter)sender).Background = new SolidColorBrush(Colors.Transparent);




        ///
        /// Keybind stuff
        ///


        private void Keybind_DragOver_Control(object sender, DragEventArgs e)
        {
            var kb = isDragDataOfType<Keybind>(e);
            
            if (kb!=null && groupClass.Keybinds.Contains(kb) || groupClass.Keybinds.Count < GroupClass.MaxKeybindCount)
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void Keybind_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => new EditKeybindDialog((Keybind)((Grid)sender).DataContext).ShowAsync();      
        ///
        /// Drag Drop
        ///
        
        private void Keybind_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.AllowedOperations = DataPackageOperation.Move;
            object package = (Keybind)((Grid)sender).DataContext; ;
            args.Data.Properties.Add("ItemViewModel", package);
        }

        private void Keybind_Drop(object sender, DragEventArgs e)
        {
            var kb = isDragDataOfType<Keybind>(e);
            if (kb != null)
                if (groupClass.Keybinds.Contains(kb) || groupClass.Keybinds.Count < GroupClass.MaxKeybindCount)
                    groupClass.AddKeybind(kb);
                else
                    ShowMaxKeybindsDialog();
        }

        /// <summary>
        /// enter/ space key on keybinds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Keybind_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space))
            {
                var s = KeybindListView.SelectedItem;
                if (s != null)
                {
                    new EditKeybindDialog((Keybind)s).ShowAsync();
                }
            }
        }

        private void KeybindFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var keybind = (Keybind)button.DataContext;
            switch (button.Label)
            {
                case "Edit":
                    new EditKeybindDialog(keybind).ShowAsync();
                    break;
                case "Copy":
                    if (groupClass.Keybinds.Count < GroupClass.MaxKeybindCount)
                    {
                        groupClass.AddKeybind(keybind.Copy());
                    }
                    else
                        ShowMaxKeybindsDialog();
                    break;
                case "Delete":
                    groupClass.Keybinds.Remove(keybind);
                    break;
                case "Up":
                    groupClass.MoveKeybind(keybind, -1);
                    break;
                case "Down":
                    groupClass.MoveKeybind(keybind, 1);
                    break;
            }
        }

        private void ShowMaxKeybindsDialog()=> new ConfirmDialog("Max Keybinds", "Keybind Limit for group has been reached.", true).ShowAsync();
    }
}
