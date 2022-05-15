using Keybind_Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWP_Sidebar_Test
{
    public sealed partial class GroupGrid : UserControl
    {
        public static int CurrentCols = 3, ColumnCount = 4;
       
        private StackPanel[] Stacks=new StackPanel[ColumnCount];
        private ColumnDefinition[] Columns=new ColumnDefinition[ColumnCount];
        private DragDropUserControl[] DragDrops=new DragDropUserControl[ColumnCount];
        private Dictionary<StackPanel,bool> isMouseOverStack=new Dictionary<StackPanel,bool>();

        public static bool PauseUpdate { get; internal set; }

        public GroupGrid()
        {
            InitializeComponent();
            var g=new GroupClass();
            g.AddKeybind(new Keybind());
            GroupClass.Groups.Add(g);
        }
        public void UpdateColumns()
        {
            for (int i = 0; i < Columns.Length; i++)
            {
                var coldef= Columns[i];
                if (i <= CurrentCols)
                {
                    coldef.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                }
                else
                {
                    coldef.SetValue(ColumnDefinition.WidthProperty, new GridLength(0));
                }
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
            var Transparent = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < ColumnCount; i++)
            {
                ///column definitions
                var coldef = new ColumnDefinition();
                StackGrid.ColumnDefinitions.Add(coldef);
                Columns[i] = coldef;

                //drag drop
                var dragdrop = new DragDropUserControl();
                DragDrops[i] = dragdrop;
                var col = i;
                dragdrop.DragWasDropped += (a, b) => GroupColumnRow.MoveToColumn(b.groupClass, col);

                //stackpanel
                var stack = new StackPanel();
                /*stack.BorderBrush = new SolidColorBrush(Colors.Red);
                stack.BorderThickness = new Thickness(1);*/
                Stacks[i] = stack;
                stack.SetValue(Grid.ColumnProperty, i);
                StackGrid.Children.Add(stack);
                stack.AllowDrop= true;
                stack.Background = Transparent;
                stack.DragEnter += (s, args) => ShowStackDragDrop(stack,dragdrop,args,true);
                stack.DragLeave += (s, args) => ShowStackDragDrop(stack, dragdrop, args, false);
                isMouseOverStack.Add(stack, false);
                stack.PointerEntered += (s, args) => isMouseOverStack[stack] = true;
                stack.PointerExited += (s, args) => isMouseOverStack[stack] = false;
            }
            GroupColumnRow.ColumnRowUpdated += GroupColumnRow_ColumnRowUpdated;
            UpdateColumnCount();
            UpdateColumns();
            rootPage.SizeChanged+=(a,b)=>UpdateColumnCount();
            Settings.SettingsUpdated+= (a,b)=>UpdateColumnCount();
        }

        private void UpdateColumnCount()
        {
            if (Settings.Grid_PreferedColumnCount == -1)
            {
                var newCols = (byte)(Math.Max(Math.Min(Math.Floor(rootPage.ActualWidth / Settings.Grid_MinColumnWidth), 4), 1) - 1);
                //Debug.WriteLine("Now Col: " + (Math.Max(Math.Min(Math.Floor(rootPage.ActualWidth / Settings.Grid_MinColumnWidth), 4), 1) - 1));
                if (newCols != CurrentCols)
                {
                    CurrentCols = newCols;
                    UpdateColumns();
                    RefreshGroups();
                }
             /*   Debug.WriteLine("Sugguested cols count: " + newCols);
                Debug.WriteLine("actual width: " + rootPage.ActualWidth);
                Debug.WriteLine("awidth/maxwidth: " + (rootPage.ActualWidth / Settings.Grid_MinColumnWidth));*/
            }
            else if(CurrentCols!=Settings.Grid_PreferedColumnCount)
            {
                CurrentCols = Settings.Grid_PreferedColumnCount;
                UpdateColumns();
                RefreshGroups();
            }
        }

        private void RefreshGroups()
        {
            ClearAllStacks();
            var grid = GroupColumnRow.AllColumns[CurrentCols];
            for (int i = 0; i < grid.Count; i++)
            {
                foreach (var gc in grid[i])
                {
                    Stacks[i].Children.Add(gc.Control);
                }
                
            }
        }

        private void GroupClass_DragEvent(object sender, bool dragging)
        {
            if(!dragging)
              for (int i = 0; i < Stacks.Length; i++)
               {
                   Stacks[i].Children.Remove(DragDrops[i]);
               }
        }
        private void ShowStackDragDrop(StackPanel stack, DragDropUserControl dragdrop, DragEventArgs args,bool isOver)
        {
            if (isOver || isMouseOverStack[stack])
            {
                GroupControl gc = DragDropUserControl.isDragDataOfType<GroupControl>(args);
                if (gc != null && !stack.Children.Contains(dragdrop))
                {
                    stack.Children.Add(dragdrop);
                }
            }
            else
            {
                stack.Children.Remove(dragdrop);
            }
        }

        private void GroupColumnRow_ColumnRowUpdated(object sender, GroupColumnRow.ColumnRowUpdatedArgs e)
        {
            if (!PauseUpdate)
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
                        AddGroupClass(e.GroupClass, e.ColumnRow);
                        break;
                    case GroupColumnRow.ColumnRowUpdatedArgs.Type.MoveBottom:
                        RemoveGroupClass(e.GroupClass);
                        AddGroupClass(e.GroupClass, e.ColumnRow);
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
                    default:
                        //throw new Exception();//shouldnt reach here, if you did, you forgot you added another enum to ColumnRowUpdatedArgs
                        break;
                }
            }
        }
        private void AddGroupClass(GroupClass gc, GroupColumnRow.ColumnRow cr = null)
        {
            gc.Control.DragEvent += GroupClass_DragEvent;
            if (cr == null)
            {
                cr = new GroupColumnRow(gc).columnRows[CurrentCols];
            }
            else
            {
                Debug.WriteLine(gc.Name + " | GroupViewr Col: " + cr.Column + "\tRow: " + cr.Row);
            }
            var ch = Stacks[cr.Column].Children;
            if (ch.Count >= cr.Row && cr.Row > -1)
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
                if (Stacks.Any(e => e.Children.Contains(gc.Control)))
                    Debug.WriteLine(gc.Control);
                stack.Children.Remove(gc.Control);
            }
        }


    }
}
