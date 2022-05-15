using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace UWP_Sidebar_Test
{
    public class GroupColumnRow
    {
        public List<ColumnRow> columnRows;
        public static event EventHandler<ColumnRowUpdatedArgs> ColumnRowUpdated;
        public GroupColumnRow(GroupClass gc)
        {
            columnRows = new List<ColumnRow>();
            for (int i = 0; i < AllColumns.Count; i++)
            {
                List<List<GroupClass>> ColSet = AllColumns[i];
                var col = ColSet.First(e => e.Contains(gc));
                var rowIndex = col.IndexOf(gc);
                columnRows.Add(new ColumnRow(i, ColSet.IndexOf(col), rowIndex));
            }
        }
        public List<XElement> GetXElement()
        {            
            return columnRows.Select(cr => {
                var ret = new XElement("ColumnRow");
                ret.SetAttributeValue("ColumnCount", cr.ColumnCount);
                ret.SetAttributeValue("Column", cr.Column);
                ret.SetAttributeValue("Row", cr.Row);
                return ret;
            }).ToList();
        }

        public class ColumnRow
        {
            public readonly int ColumnCount, Column, Row;


            public ColumnRow(int columnCount, int column, int row)
            {
                ColumnCount = columnCount;
                Column = column;
                Row = row;
            }
            public ColumnRow(List<List<GroupClass>> columnSet,GroupClass gc)
            {
                ColumnCount = -1;
                var col = columnSet.First(e => e.Contains(gc));
                Column = columnSet.IndexOf(col);
                Row = col.IndexOf(gc);
            }
            public ColumnRow(XElement xE)
            {
                ColumnCount = int.Parse(xE.Attribute("ColumnCount").Value);
                Column = int.Parse(xE.Attribute("Column").Value);
                Row = int.Parse(xE.Attribute("Row").Value);
            }
        }


        public static List<List<List<GroupClass>>> AllColumns;

        

        public static void Init(bool firstTime)
        {
            AllColumns = new List<List<List<GroupClass>>>();
            for (int i = 0; i < 4; i++)//3 for column counts
            {
                AllColumns.Add(new List<List<GroupClass>>());
                for (int cCount = 0; cCount < i + 1; cCount++)
                {
                    AllColumns[i].Add(new List<GroupClass>());
                }
            }
            if(firstTime)
                GroupClass.Groups.CollectionChanged += Groups_CollectionChanged;
        }
        

        private static void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //var oldItems = e.OldItems?.Cast<GroupClass>().ToArray();
            Debug.WriteLine("GroupColumnRow:  action:\t"+e.Action);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<GroupClass>())
                        Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var old in e.OldItems.Cast<GroupClass>())
                        Remove(old);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var col in AllColumns)
                    {
                        foreach (var c in col)
                        {
                            c.Clear();
                        }
                    }
                    ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.Clear, null));
                   // ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.BulkAdd, null));
                    break;
                default:
                    return;
            }

        }

        internal static void LoadNew(List<List<List<GroupClass>>> gridStuff)
        {
            AllColumns = gridStuff;
            //ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.Clear, null));
            ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.BulkAdd, null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gc"></param>
        /// <param name="columnCount"></param>
        /// <returns>The column row class</returns>
        public static ColumnRow GetColumnRowClass(GroupClass gc, int columnCount)
        {
            List<List<GroupClass>> ColSet = AllColumns[columnCount];
            var col = ColSet.First(e => e.Contains(gc));
            var rowIndex = col.IndexOf(gc);
            return new ColumnRow(columnCount, ColSet.IndexOf(col), rowIndex);
        }

        internal static void MoveAbove(GroupClass movingGC, GroupClass gc)
        {
            if (movingGC == gc)
                return;
            var currentColumns = AllColumns[GroupGrid.CurrentCols];
            currentColumns.FirstOrDefault(e => e.Contains(movingGC)).Remove(movingGC);
            var newcolrow = new ColumnRow(currentColumns, gc);
            currentColumns[newcolrow.Column].Insert(newcolrow.Row, movingGC);
            ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.MoveAbove, movingGC) {ColumnRow= new ColumnRow(currentColumns,movingGC)});
        }
        internal static void MoveToColumn(GroupClass gc,int newColumn)
        {
            var currentColumns = AllColumns[GroupGrid.CurrentCols];
            currentColumns.FirstOrDefault(e => e.Contains(gc)).Remove(gc);
            currentColumns[newColumn].Add(gc);
            ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.MoveBottom, gc) { ColumnRow = new ColumnRow(GroupGrid.CurrentCols, newColumn,-1) });
        }

        public static bool Contains(GroupClass gc)
        {
            return AllColumns.Any(c => c.Any(d => d.Contains(gc)));
        }
        public static void Remove(GroupClass gc)
        {
            foreach (var RowCol in AllColumns)
            {
                foreach (var Col in RowCol)
                {
                    Col.Remove(gc);
                  
                }
            }
            ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.Remove, gc));
        }
        public static void Add(GroupClass gc, bool notify = true)
        {
            if (Contains(gc))
                return;
            if (gc.RowCol != null)
            {
                for (int i = 0; i < AllColumns.Count; i++)
                {
                    var colrow = gc.RowCol.columnRows[i];
                    InsertIntoColumn(gc, colrow.Row, AllColumns[i][colrow.Column]);
                }
            }
            else
            {
                AddToShortestColumns(gc);
            }
            if (notify)
                ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.Add, gc));
            
        }
        public static void BulkAdd(List<GroupClass> gcs)
        {
            foreach (var gc in gcs)
            {
                Add(gc,false);
            }
            ColumnRowUpdated?.Invoke(null, new ColumnRowUpdatedArgs(ColumnRowUpdatedArgs.Type.BulkAdd, null) { GroupClasses=gcs});
        }
        /// <summary>
        /// adds to the shortest column
        /// </summary>
        /// <param name="gc"></param>
        private static void AddToShortestColumns(GroupClass gc)
        {
            //Debug.WriteLine("InsertIntoBestFit called");
            foreach (var allc in AllColumns)
            {
                var order = allc.OrderBy(e => e.Sum(z => z.GetProjectedSize()));//GetColumnHeights(e).Sum());
              //  Debug.WriteLine(string.Join(",",order.Select(e => GetColumnHeights(e).Sum())));
                order.ToArray()[0].Add(gc);
            }
        }
        private static void InsertIntoColumn(GroupClass g,int desiredRow, List<GroupClass> column)
        {
            if (desiredRow == -1 || desiredRow==column.Count)
            {
                column.Add(g);
            }
            else
            {
                column.Insert(Math.Max(desiredRow,column.Count-1), g);
            }
        }
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <returns>List of actual heights of every groupclass control</returns>
        private static int[] GetColumnHeights(List<GroupClass> column)
        {
            return column.Select(gc => (int)gc.GetProjectedSize()).ToArray();
        }
        /// <summary>
        /// returns the sum of the heights of the group classes above it
        /// </summary>
        private static int GetGroupClassHeightPosition(GroupClass gc,List<GroupClass> column)
        {
            return (int)column.TakeWhile(gc2 => gc2 != gc).Sum(e => e.GetProjectedSize());
        }*/
        public class ColumnRowUpdatedArgs
        {
            public Type Action { get; }
            public GroupClass GroupClass { get; }
            public GroupClass SecondaryGroupClass { get; set; }
            public List<GroupClass> GroupClasses { get; set; }
            public ColumnRow ColumnRow { get; set; }
            
            public enum Type
            {
                Add,BulkAdd,Remove, Clear,
                MoveAbove,
                MoveBottom
            }
            public ColumnRowUpdatedArgs(Type type,GroupClass mainGc)
            {
                Action = type;
                GroupClass = mainGc;
            }
        }


    }
}
