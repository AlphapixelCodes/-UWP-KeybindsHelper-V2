using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWP_Sidebar_Test
{
    public class Keybind: INotifyPropertyChanged
    {
        public GroupClass Group;
        private string name = "Unnamed Keybind", kb1 = "N/A", kb2="",kb3="";
        public StackPanel KeybindStack
        {
            get {
                var s = new StackPanel();
                s.HorizontalAlignment = HorizontalAlignment.Right;
                s.Orientation = Orientation.Horizontal;
                s.Background = new SolidColorBrush(Colors.Transparent);
                s.SetValue(Grid.ColumnProperty, 1);
                foreach (var kb in new string[]{kb1,kb2,kb3 })
                {
                    if(kb.Length>0)
                        s.Children.Add(getBorderForKeybind(kb));
                }
                return s;
            }
        }
        private Border getBorderForKeybind(string kbvalue)
        {
            var ret = new Border() { BorderThickness = new Thickness(1),BorderBrush=new SolidColorBrush(Color.FromArgb(30,255,255,255)),CornerRadius=new CornerRadius(3),Background=new SolidColorBrush(Colors.Transparent), MinWidth=50,Margin=new Thickness(5,0,5,0)};
            var t = new TextBlock() { Text=kbvalue,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
            ret.Child = t;
            return ret;
        }
        public string Name { get => name;
            set{ 
                name= value;
                RaisePropertyChanged("Name");
            }
        }
        public string KB1
        {
            get { return kb1; }
            set
            {
                kb1 = value;
                RaisePropertyChanged("KB1");
                RaisePropertyChanged("KeybindStack");
                RaisePropertyChanged("VisKB1");
            }
        }
        public string KB2
        {
            get { return kb2; }
            set
            {
                kb2 = value;
                RaisePropertyChanged("KB2");
                RaisePropertyChanged("KeybindStack");
                RaisePropertyChanged("VisKB2");
            }
        }
        public string KB3
        {
            get { return kb3; }
            set
            {
                kb3 = value;
                RaisePropertyChanged("KB3");
                RaisePropertyChanged("KeybindStack");
                RaisePropertyChanged("Vi3KB1");
            }
        }
       /* public Thickness KB1Thickness => GetThickness(kb1);

        private Thickness GetThickness(string kb) => new Thickness(kb.Length > 0 ? 1 : 0);*/
         public Visibility VisKB1 => GetVisibility(kb1);
         public Visibility VisKB2 => GetVisibility(kb2);
         public Visibility VisKB3 => GetVisibility(kb3);
         private Visibility GetVisibility(string txt)=>txt.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            if (!propertyName.Contains("Vis"))
                RaisePropertyChanged("Vis" + propertyName);
        }

        internal Keybind Copy()
        {
            return new Keybind() { Name = name, KB1 = kb1, KB2 = kb2, KB3 = kb3 };
        }

        internal XElement getXElement()
        {
            var r = new XElement("Keybind");
            r.SetAttributeValue("Name", name);
            r.SetAttributeValue("Bind1", kb1);
            r.SetAttributeValue("Bind2", kb2);
            r.SetAttributeValue("Bind3", kb3);
            return r;
        }
        internal static Keybind FromXElement(XElement keybind)
        {
            var kb = new Keybind();
            kb.Name = keybind.Attribute("Name").Value;
            kb.KB1 = keybind.Attribute("Bind1").Value;
            kb.KB2 = keybind.Attribute("Bind2").Value;
            kb.KB3 = keybind.Attribute("Bind3").Value;
            return kb;
        }
    }
}
