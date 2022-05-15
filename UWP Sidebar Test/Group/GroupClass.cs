using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWP_Sidebar_Test
{
    public class GroupClass:INotifyPropertyChanged
    {
        public static ObservableCollection<GroupClass> Groups = new ObservableCollection<GroupClass>();
        public static XElement GetXElement()
        {
            var ret = new XElement("KeybindGroups");
            foreach (var group in Groups)
            {
                ret.Add(group.getXElement());
            }
            return ret;
        }
        public static byte MaxKeybindCount = 26;
        public static byte MaxGroupCount = 20;
        private string name = "Unnamed Group";
        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }
        public GroupColumnRow RowCol;
        private GroupControl control;
        public GroupControl Control
        {
            get
            {
                if (control == null)
                {
                    control = new GroupControl(this);
                    return control;
                }
                return control;
            }
            set => control = value;
        }


        internal static void AddDefault()
        {
            new GroupClass().AddKeybind(new Keybind());
        }

        public ObservableCollection<Keybind> Keybinds=new ObservableCollection<Keybind>();
        
        public static bool LoadXML(XElement xelement)
        {
            try
            {
                var kbg = xelement.Element("KeybindGroups").Elements("Group");
                Dictionary<GroupClass, List<GroupColumnRow.ColumnRow>> groups = new Dictionary<GroupClass, List<GroupColumnRow.ColumnRow>>();

                foreach (var xGroup in kbg)
                {
                    List<GroupColumnRow.ColumnRow> crs = new List<GroupColumnRow.ColumnRow>();
                    foreach (var cr in xGroup.Element("ColumnRows").Elements("ColumnRow"))
                    {
                        crs.Add(new GroupColumnRow.ColumnRow(cr));
                      //  Debug.WriteLine("heres a columnrow idk");
                    }
                    groups.Add(new GroupClass(xGroup), crs);
                }

                //group row column crap
                List<List<List<GroupClass>>> GridStuff = new List<List<List<GroupClass>>>();
                for (int columnCount = 0; columnCount < GroupGrid.ColumnCount; columnCount++)
                {
                    var cc = new List<List<GroupClass>>();
                    //Debug.WriteLine("wow");
                    var currentColumnCountCrGroups = groups.Select(e => new Tuple<GroupClass, GroupColumnRow.ColumnRow>(e.Key, e.Value.First(z => z.ColumnCount == columnCount)));
                    for (int column = 0; column < columnCount + 1; column++)
                    {
                        var cg = currentColumnCountCrGroups.Where(e => e.Item2.Column == column).OrderBy(e => e.Item2.Row).Select(e => e.Item1).ToList();
                        var columnList = new List<GroupClass>(cg);
                        cc.Add(columnList);
                        //Debug.WriteLine("Column: " + column + " Size: " + columnList.Count + " cg count: " + cg.Count);
                    }
                    GridStuff.Add(cc);
                }

                Groups.Clear();
                GroupGrid.PauseUpdate = true;
                foreach (var item in groups)
                {
                    Groups.Add(item.Key);
                }
                GroupGrid.PauseUpdate = false;
                GroupColumnRow.LoadNew(GridStuff);
                
                return true;
            }catch (Exception ex)
            {
                throw ex;
                Debug.WriteLine("unnable to load file "+ex.Message);
                Groups.Clear();
                GroupClass.AddDefault();
                return false;
            }
        }


        public GroupClass()
        {
            GroupClass.Groups.Add(this);
            ProjectInfo.ChangesSaved = false;
        }
        public GroupClass(GroupControl controller)
        {
            control = controller;
            GroupClass.Groups.Add(this);
        }
        public GroupClass(GroupColumnRow gcr)
        {
            RowCol= gcr;
            GroupClass.Groups.Add(this);
        }
        public GroupClass(XElement groupElement)
        {
            Name = groupElement.Element("Name").Value;
            foreach (var xKeybind in groupElement.Element("Keybinds").Elements("Keybind"))
            {
                AddKeybind(Keybind.FromXElement(xKeybind),false);
            }
        }
        public void AddKeybind(Keybind k,bool modifyChangesSaved=true)
        {
            if(modifyChangesSaved)
                ProjectInfo.ChangesSaved = false;
            k.Group?.Keybinds.Remove(k);
            Keybinds.Add(k);
            k.Group = this;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        internal void MoveKeybind(Keybind kb, int direction)
        {
            ProjectInfo.ChangesSaved = false;
            var newrow = Keybinds.IndexOf(kb) + direction;
            if (newrow > -1 && newrow < Keybinds.Count)
            {
                Keybinds.Remove(kb);
                Keybinds.Insert(newrow, kb);
            }
        }
      
        internal void Duplicate()
        {
            ProjectInfo.ChangesSaved = false;
            var gc = new GroupClass() { Name = name + "(Copy)" };
            foreach (var kb in Keybinds)
            {
                gc.AddKeybind(kb.Copy());
            }
            Groups.Add(gc);
        }
        internal void DisolveInto(GroupClass gc)
        {
            foreach (var kb in Keybinds.ToArray())
            {
                gc.AddKeybind(kb);
            }
            Debug.WriteLine("Groupclass.disolve: Removing self from groups");
            ProjectInfo.ChangesSaved = false;
            Groups.Remove(this);
        }
        internal int GetProjectedSize()
        {
            return 1+Math.Max(Keybinds.Count,1);
        }

        //for drag and dropping
        internal void MoveAbove(GroupControl gc)
        {
            ProjectInfo.ChangesSaved = false;
            GroupColumnRow.MoveAbove(this, gc.groupClass);
        }

        //xml saving
        public XElement getXElement()
        {
            var kbs = new XElement("Keybinds");
            foreach (var kbc in Keybinds)
            {
                kbs.Add(kbc.getXElement());
            }
            return new XElement("Group", 
                new XElement("Name",name),
                kbs,
                new XElement("ColumnRows",new GroupColumnRow(this).GetXElement()));
        }
    }
}
