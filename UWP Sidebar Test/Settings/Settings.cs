using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UWP_Sidebar_Test;
using Windows.Storage;

namespace Keybind_Helper
{
    public static class Settings
    {
        public static event EventHandler<SettingsType> SettingsUpdated;
        public enum SettingsType { Grid, All }
        //Grid Settings
        private static int minColumnWidth = 375;
        public static int Grid_MinColumnWidth { 
            get => minColumnWidth;
            set
            {
                minColumnWidth = value;
                SettingsUpdated?.Invoke(null, SettingsType.Grid);
                Save();
            }
        }
        private static int preferedColumnCount = -1;
        public static int Grid_PreferedColumnCount
        {
            get => preferedColumnCount;
            set
            {
                preferedColumnCount = value;
                SettingsUpdated?.Invoke(null, SettingsType.Grid);
                Save();
            }
        }
        private static XElement Grid_GetSaveXElement()
        {
            var ret = new XElement("Grid");
            ret.SetAttributeValue("PreferedColumnCount",preferedColumnCount.ToString());
            ret.SetAttributeValue("MinColumnWidth",minColumnWidth.ToString());
            return ret;
        }
        private static void Grid_LoadXElement(XElement xelm)
        {
            if (xelm.Element("Grid") != null)
            {
                var g = xelm.Element("Grid");
                if (g.Attribute("PreferedColumnCount") != null)
                {
                    int.TryParse(g.Attribute("PreferedColumnCount").Value, out preferedColumnCount);
                }
                if (g.Attribute("MinColumnWidth") != null)
                {
                    int.TryParse(g.Attribute("MinColumnWidth").Value, out minColumnWidth);
                }
            }
        }


        public async static void Save()
        {
            XElement main = new XElement("Settings");
            main.Add(Grid_GetSaveXElement());
           await Storage.SaveSettingsFile("Settings", main.ToString());
        }
        public async static Task LoadSettings()
        {
            if (await Storage.FileExists("Settings", false))
            {
                try
                {
                    var xelm= XElement.Parse(await FileIO.ReadTextAsync(await Storage.GetFile("Settings", false)));
                    Grid_LoadXElement(xelm);
                    SettingsUpdated?.Invoke(null, SettingsType.All);
                }
                catch
                {
                    throw new Exception();
                }
            }
            else
            {
                Debug.WriteLine("Settings file not found");
                //throw new Exception();
            }
        }
    }
}
