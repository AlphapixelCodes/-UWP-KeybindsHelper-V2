using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace UWP_Sidebar_Test
{
    public class Theme
    {
        public static List<Theme> Themes = new List<Theme>()
        {
            new Theme("Black And White",Colors.Black,Colors.White),
            new Theme("Default", Color.FromArgb(127, 0, 0, 0), Colors.White)
    };
        public static Theme Default { get; internal set; } = new Theme("Default", Color.FromArgb(127, 0, 0, 0), Colors.White);
        public Theme(string name,Brush background,Brush foreground)
        {
            Background = background;
            Foreground = foreground;
        }
        public Theme(string name,Color background, Color foreground)
        {
            Background = new SolidColorBrush(background);
            Foreground = new SolidColorBrush(foreground);
        }

        public Brush Background { get; }
        public Brush Foreground { get; }
       
    }
}
