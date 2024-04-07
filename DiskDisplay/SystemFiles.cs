using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace DiskDisplay
{
    internal static class SystemFiles
    {
        public static Directory SystemFolder = new Directory();

        public static void InitializeSystemFiles()
        {
            Directory SystemRecycleBin = new Directory();
            SystemRecycleBin.MainName = "Recycle Bin";
            SystemRecycleBin.SetItemText("Recycle Bin");
            SystemRecycleBin.SetNodeText("Recycle Bin");
            //SystemRecycleBin.SetIcon("recycleBinIcon", 2);
            SystemFolder.Children.Add(SystemRecycleBin);
            FileListView.History.Add(SystemFolder);
        }

        public static int GetSystemFolderSize()
        {
            return SystemFolder.Children.Count;
        }

        public static void UpdateSystemFiles(ref FileSystem fileSystem, ref List<FileManager> fileManager, string name)
        {
            if(SystemFolder.Children.Count > 0)
            {
                foreach(var folder in SystemFolder.Children)
                {
                    if(folder.MainName == name)
                    {
                        MessageBox.Show("Partition has been created");
                        return;
                    }
                }
            }

            Directory dir = new Directory();
            dir.MainName = name;
            dir.Children = fileManager;
            dir.IsFAT32 = (fileSystem is FAT32);
            dir.SetItemText(name);
            dir.SetNodeText(name);
            //dir.SetIcon()
            SystemFolder.Children.Add(dir);
            SystemFolder.Populate();
            UpdateRecycleBin(ref fileSystem.RecycleBin);
        }

        public static void UpdateUI(System.Windows.Forms.TreeView folderTree, System.Windows.Forms.ListView listView1)
        {
            listView1.Items.Clear();
            folderTree.Nodes.Clear();
            foreach (var folder in SystemFolder.Children)
            {
                folder.SetPath(folder.MainName);
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
                if (folder == SystemFolder.Children[SystemFolder.Children.Count - 1])
                {
                    folder.GetListViewItem().Tag = folder;
                }
                if (folder.Children.Count > 0)
                    foreach (var child in folder.Children)
                    {
                        child.SetPath(folder.GetPath() + "/" + child.MainName);
                    }
            }
        }

        public static void UpdateRecycleBin(ref List<FileManager> trash)
        {
            if(trash.Count > 0)
                SystemFolder.Children[0].Children.AddRange(trash);
        }
    }
}
