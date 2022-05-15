using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace UWP_Sidebar_Test
{
    public static class ProjectInfo
    {
        public static event EventHandler<string> ProjectNameChanged;
        public static event EventHandler<bool> ChangesSavedChanged;
        private static string projectName="Unnamed Project";
        public static string OriginalNameNoExtension = "Unnamed Project";
        private static bool changesSaved = false;
        public static bool ChangesSaved
        {
            get => changesSaved;
            set { 
                changesSaved = value;
                Debug.WriteLine("changes saved: " + value);
                ChangesSavedChanged?.Invoke(null,value);
            }
        }
        public static string ProjectName
        {
            get => projectName;
            set
            {
                projectName = value;
                ProjectNameChanged?.Invoke(null, ProjectName);
            }
        }

        private static XElement GetXElement()
        {
            var ret= new XElement("Project", GroupClass.GetXElement());
            ret.SetAttributeValue("Name", ProjectName);
            return ret;
        }
        public static string getSaveString()
        {
            return GetXElement().ToString();
        }

        public async static Task<bool> SaveChanges()
        {
            try
            {
                await Storage.SaveFile(ProjectName, OriginalNameNoExtension, "<?xml version=\"1.0\" encoding=\"utf - 8\" ?>\n" + getSaveString());
                ChangesSaved = true;
                return true;
            }catch (Exception ex)
            {
                return false;
            }
        }
        internal static async Task<bool> OpenFile(StorageFile storageFile,bool showFailedAlert=true)
        {
            try
            {
                var txt = await FileIO.ReadTextAsync(storageFile);
                var xelm = XElement.Parse(txt);
                if (GroupClass.LoadXML(xelm))
                {
                    ProjectName = xelm.Attribute("Name").Value;
                    MainPage.ShowGroupPage();
                    ChangesSaved = true;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        internal static async Task NewProjectAsync()
        {
            GroupClass.Groups.Clear();
            int i = 0;
            while ((await Storage.GetFileList()).Any(e => e.StartNameNoExtension.Equals("Unnamed Project" + ((i > 0) ? " " + i : ""))))
                i++;
            ProjectName = "Unnamed Project" + ((i > 0) ? " " + i : "");
            GroupColumnRow.Init(false);
            GroupClass.AddDefault();
        }
    }
}
