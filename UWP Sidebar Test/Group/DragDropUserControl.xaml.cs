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
    public sealed partial class DragDropUserControl : UserControl
    {
        public event EventHandler<GroupControl> DragWasDropped;
        private bool isDragOverHead;
        private Storyboard Darken => (Storyboard)Resources["dark"];
        private Storyboard Lighten => (Storyboard)Resources["light"];
        public DragDropUserControl()
        {
            this.InitializeComponent();
        }
        public static Type isDragDataOfType<Type>(DragEventArgs e)
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
        private void Drag_Over_Event(object sender, DragEventArgs e)
        {
            var drag = isDragDataOfType<GroupControl>(e);
            if(drag!=null && drag != Parent as GroupControl)
                e.AcceptedOperation = DataPackageOperation.Move;
            else
                e.AcceptedOperation = DataPackageOperation.None;
        }

        private void Drop_Event(object sender, DragEventArgs e)
        {
            var drag = isDragDataOfType<GroupControl>(e);
            if (drag != null && drag != Parent as GroupControl)
            {
                DragWasDropped?.Invoke(this, drag);
                isDragOverHead = false;
                ProjectInfo.ChangesSaved = false;
            }
        }

        private void BrightenAnimation_Completed(object sender, object e)
        {
            if (isDragOverHead)
                Darken.Begin();
            Debug.WriteLine("DragDropUserControl: ANIMATION COMPLETED LIGHT");
        }

        private void DarkenAnimation_Completed(object sender, object e)
        {
            if (isDragOverHead)
                Lighten.Begin();
            Debug.WriteLine("DragDropUserControl:  ANIMATION COMPLETED DARK");
        }

        private void Drag_Enter(object sender, DragEventArgs e)
        {
            isDragOverHead = true;
            Debug.WriteLine("Drag Enter DRAGDROPUSERCONTROL");
            Lighten.Begin();
        }

        private void Drag_Exit(object sender, DragEventArgs e)
        {
            Debug.WriteLine("Drag Exit DRAGDROPUSERCONTROL");
            isDragOverHead = false;
            Darken.Stop();
            Lighten.Stop();
            MainGrid.Background = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));
        }
    }
}
