using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskDisplay
{
    internal static class FileManagerDictionary
    {
        public static Dictionary<FileManager, FileSystem> FileDictionary = new Dictionary<FileManager, FileSystem>();

        public static void UpdateFileDictionary(List<FileManager> FileManagers, ref FileSystem fileSystem)
        {
            if (FileManagers.Count > 0)
            {
                foreach (var fileManager in FileManagers)
                {
                    FileDictionary.Add(fileManager, fileSystem);

                    if (fileManager is Directory)
                    {
                        UpdateFileDictionary(fileManager.Children, ref fileSystem);
                    }
                }
            }
        }
    }
}
