using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace UWP_Sidebar_Test
{
    public static class Storage
    {

        private static string FileExtension = ".xml";
        private static StorageFolder appInstalledFolder = ApplicationData.Current.LocalFolder;
        private static StorageFolder ProjectsFolder;
       
       
        public async static Task<List<File>> GetFileList()
        {
            if (ProjectsFolder == null)
                await CreateProjectsFolder();
            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new string[] { FileExtension });
            // clear all existing sorts
            queryOptions.SortOrder.Clear();
            // add descending sort by date modified
            SortEntry se = new SortEntry();
            se.PropertyName = "System.DateModified";
            se.AscendingOrder = false;
            queryOptions.SortOrder.Add(se);

            
            StorageFileQueryResult queryResult = ProjectsFolder.CreateFileQueryWithOptions(queryOptions);
            IReadOnlyList<StorageFile> files = await queryResult.GetFilesAsync();
            List<File> ret = new List<File>();
            foreach (StorageFile file in files)
                ret.Add(await File.Get(file));
            return ret;
            
        }

        internal async static Task SaveSettingsFile(string fileNameNoExtension, string data)
        {
            try
            {
                var file = await appInstalledFolder.CreateFileAsync(fileNameNoExtension + FileExtension, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, data);
            }catch (Exception ex)
            {
                Debug.WriteLine("Storage.SaveSettingsFile: Failed to save file: " + ex.Message);
            }
        }

        private async static Task CreateProjectsFolder()
        {
            if (ProjectsFolder == null)
            {
                if((await appInstalledFolder.GetFoldersAsync()).Any(e=>e.Name.Equals("Projects")))
                {
                    ProjectsFolder = await appInstalledFolder.GetFolderAsync("Projects");
                    
                }
                else
                {
                    ProjectsFolder = await appInstalledFolder.CreateFolderAsync("Projects");
                }
                
            }
                
        }

        public async static Task<bool> DeleteFile(string name,bool isProject=true)
        {   
            await (await GetFile(name,isProject)).DeleteAsync();
            return true;
        }
        public async static Task<StorageFile> GetFile(string nameNoExtension, bool isProject = true)
        {
            if (isProject)
            {
                await CreateProjectsFolder();
                return await ProjectsFolder.GetFileAsync(nameNoExtension + FileExtension);
            }
            return await appInstalledFolder.GetFileAsync(nameNoExtension + FileExtension);
        }
        public async static Task<bool> FileExists(string nameNoExtension, bool isProject=true)
        {
            try
            {
                await GetFile(nameNoExtension,isProject);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        internal async static Task<StorageFile> SaveFile(string projectName, string originalNameNoExtension, string data)
        {
            await CreateProjectsFolder();
            if(projectName!=originalNameNoExtension && await FileExists(originalNameNoExtension))
            {
                await DeleteFile(originalNameNoExtension);
            }
            var file=await ProjectsFolder.CreateFileAsync(projectName+FileExtension, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, data);
            return file;
        }

        internal static async Task CopyFileTo(string oldName,string newName)
        {
            var olddata=await GetFile(oldName);
            await SaveFile(newName,"", await FileIO.ReadTextAsync(await GetFile(oldName)));
        }
    }
    
    public class File
    {
        public string StartNameNoExtension {get; set;}
        public string DateModified { get; set; }
        public StorageFile ActualFile { get; set; }
        public static async Task<File> Get(StorageFile sf)
        {
            var r = new File();
            r.ActualFile = sf;
            r.StartNameNoExtension = Path.GetFileNameWithoutExtension(sf.Name);
            r.DateModified = (await sf.GetBasicPropertiesAsync()).DateModified.ToString().Split(" ")[0].ToString();
            return r;
        }
        public override string ToString()
        {
            return StartNameNoExtension;
        }
    }
}
