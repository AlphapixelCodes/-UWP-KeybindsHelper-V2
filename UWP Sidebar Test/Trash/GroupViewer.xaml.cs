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
using System.Collections.Specialized;
using System.Diagnostics;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Sidebar_Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GroupViewer : Page
    {
        public event EventHandler PageLoaded;
        public static byte CurrentCols = 3;
        private StackPanel[] Stacks;
        private ColumnDefinition[] Columns;
        public GroupViewer()
        {
            this.InitializeComponent();
        }

    

        public void UpdateColumns()
        {
            if (CurrentCols == 4)
            {
                foreach (var colDef in Columns)
                {
                    colDef.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                }
            }
            else//2 cols
            {
                CD1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                CD2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                CD3.SetValue(ColumnDefinition.WidthProperty, new GridLength(0, GridUnitType.Star));
                CD4.SetValue(ColumnDefinition.WidthProperty, new GridLength(0, GridUnitType.Star));
            }
        }
        public void ClearAllStacks()
        {
            foreach (var Stacks in Stacks)
            {
                Stacks.Children.Clear();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Stacks= new StackPanel[] { S1, S2, S3, S4 };
            Columns = new ColumnDefinition[] { CD1, CD2, CD3, CD4 };

            GroupColumnRow.ColumnRowUpdated += GroupColumnRow_ColumnRowUpdated;
            PageLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void GroupColumnRow_ColumnRowUpdated(object sender, GroupColumnRow.ColumnRowUpdatedArgs e)
        {           
            switch (e.Action)
            {
                case GroupColumnRow.ColumnRowUpdatedArgs.Type.Add:
                    {
                        AddGroupClass(e.GroupClass);
                    }
                    break;
                case GroupColumnRow.ColumnRowUpdatedArgs.Type.MoveAbove:
                    RemoveGroupClass(e.GroupClass);
                    AddGroupClass(e.GroupClass,e.ColumnRow);
                    break;
                case GroupColumnRow.ColumnRowUpdatedArgs.Type.Remove:
                    RemoveGroupClass(e.GroupClass);
                    break;
                case GroupColumnRow.ColumnRowUpdatedArgs.Type.BulkAdd:
                    {
                        ClearAllStacks();
                        var currentcols = GroupColumnRow.AllColumns[CurrentCols];
                        for (int i = 0; i < currentcols.Count; i++)
                        {
                            foreach (var item in currentcols[i])
                            {
                                Stacks[i].Children.Add(item.Control);
                            }
                            
                        }
                    }
                    break;
                case GroupColumnRow.ColumnRowUpdatedArgs.Type.Clear:
                    ClearAllStacks();
                    break;
            }
        }
        private void AddGroupClass(GroupClass gc,GroupColumnRow.ColumnRow cr=null)
        {
            //gc.Control.DragEvent += GroupClass_DragEvent;
            if (cr == null)
            {
                cr=new GroupColumnRow(gc).columnRows[CurrentCols];
            }
            else
            {
                Debug.WriteLine(gc.Name + " | GroupViewr Col: " + cr.Column + "\tRow: " + cr.Row);
            }
            var ch = Stacks[cr.Column].Children;
            if (ch.Count >= cr.Row && cr.Row>-1)
            {
                ch.Insert(cr.Row, gc.Control);
            }
            else
            {
                ch.Add(gc.Control);
            }
            
        }
        private void RemoveGroupClass(GroupClass gc)
        {
            foreach (var stack in Stacks)
            {
                if(Stacks.Any(e=>e.Children.Contains(gc.Control)))
                    Debug.WriteLine(gc.Control);
                stack.Children.Remove(gc.Control);
            }
        }
      
    }
}
