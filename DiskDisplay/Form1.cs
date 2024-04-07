using DiskDisplay.NewFolder1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace DiskDisplay
{
    public partial class Form1 : Form
    {
        /*private FAT32 fat32 = new FAT32("G:");
        private NTFS ntfs = new NTFS("Y:");*/
        private bool IsUserInteraction = false;
        public bool IsRecycleBin = false;
        

        public Form1()
        {
            InitializeComponent();
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;
            SystemFiles.InitializeSystemFiles();

            /*List<FileManager> files = new List<FileManager>();
            files = ntfs.ReadFileSystem();
            
            List<FileManager> fat32Files = new List<FileManager>();
            fat32Files = fat32.ReadFileSystem();

            Fat32Folder.Children = fat32Files;
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;


            NTFSFolder.Children = files;

            *//*var RecycleBin = new Directory();
            RecycleBin.Children = FileSystem.RecycleBin;*//*
            

            var SystemFolder = new Directory() ;
            SystemFolder.Children.Add(Fat32Folder);
            SystemFolder.Children.Add(NTFSFolder);
            SystemFolder.Children.Add(RecycleBin);
            SystemFolder.Populate();
            NTFSFolder.SetItemText("Y:");
            NTFSFolder.SetNodeText("Y:");
            Fat32Folder.SetItemText("G:");
            Fat32Folder.SetNodeText("G:");
            RecycleBin.SetItemText("Recycle Bin");
            RecycleBin.SetNodeText("Recycle Bin");
            RecycleBin.SetIcon("recycleBinIcon", 2);
            FileListView.History.Add(SystemFolder);
            NTFSFolder.MainName = "Y:";
            Fat32Folder.MainName = "G:";
            RecycleBin.MainName = "Recycle Bin";
            foreach (var folder in SystemFolder.Children)
            {
                folder.SetPath(folder.MainName);
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
                if(folder == SystemFolder.Children[SystemFolder.Children.Count -1 ])
                {
                    folder.GetListViewItem().Tag = folder;
                }
                if(folder.Children.Count > 0)
                foreach(var child in folder.Children)
                {
                        child.SetPath(folder.GetPath() + "/" + child.MainName);
                }
            }*/

        }


        private void Form1_RightMouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                Input input = new Input(ref folderTree, ref listView1);
                input.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            folderTree.AfterSelect += treeView_AfterSelect;
            listView1.View = View.Details;
            listView1.Columns.Add("Name", 200);
            listView1.Columns.Add("Type", 50);
            listView1.Columns.Add("Size", 100);
            listView1.Columns.Add("Created at", 100);
            listView1.SmallImageList = Image1.ImageList;
        }

        private void ShowFileContent(string content)
        {
            FileWindow f2 = new FileWindow();
            f2.ShowFileContent(content);
        }

        private void TreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var selectedItem = e.Node;
            if(selectedItem != null)
            {
                if(selectedItem.Tag is File)
                {
                    var selectedFile = selectedItem.Tag as File;
                    ShowFileContent(selectedFile.content_President);
                }
            }
        }
        

        
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog() { Description="Select your path"})
            {
                if(fbd.ShowDialog()==DialogResult.OK)
                {
                    filePathTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (!FileListView.IsFirstDirectory())
            {
                IsUserInteraction = true;
                FileListView.CurrentHistoryIndex--;
                FileListView.RenderListView(ref listView1, filePathTextBox);
                IsUserInteraction = false;

            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (!FileListView.IsLastDirectory())
            {
                IsUserInteraction = true;
                FileListView.CurrentHistoryIndex++;
                FileListView.RenderListView(ref listView1, filePathTextBox);
                IsUserInteraction = false;

            }
        }

        


        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }


        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            MessageBox.Show("ahihi");
        }
        //bool rightClicked = false;
        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewHitTestInfo hitTestInfo = listView1.HitTest(e.X, e.Y);
                if (hitTestInfo.Item != null)
                {
                    var item = listView1.SelectedItems[0].Tag as FileManager;
                    if (item.IsRecycleBin())
                    {
                        Console.WriteLine("This is recycle bin");
                        recycleBinContextMenu.Show(listView1, e.Location);
                    }
                    else
                    {
                        Console.WriteLine("This is not recycle bin");
                        contextMenuStrip1.Show(listView1, e.Location);
                    }
                }
            }
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem selecteditem = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;
            if (selecteditem != null)
            {
                if (selecteditem.Tag is File)
                {
                    var selectedFile = selecteditem.Tag as File;

                    FileWindow f2 = new FileWindow();
                    string content = "";
                    content = FileManagerDictionary.FileDictionary[selectedFile].ReadData(selectedFile);
                    f2.ShowFileContent(content);
                }
                else if (selecteditem.Tag is Directory)
                {
                    if (IsUserInteraction) return;
                    IsUserInteraction = true;
                    var selectedFolder = selecteditem.Tag as Directory;
                    bool isRecycleBinFolder = selectedFolder.MainName == "Recycle Bin";
                    if(selectedFolder.GetListViewItem().Text == "Recycle Bin" && FileListView.CurrentHistoryIndex == 0)
                    {
                        IsRecycleBin = true;
                    }
                    if (folderTree.SelectedNode != null)
                        folderTree.SelectedNode.BackColor = Color.White;
                    folderTree.SelectedNode = selectedFolder.GetNode();
                    folderTree.SelectedNode.BackColor = Color.Yellow;
                    if (FileListView.IsLastDirectory())
                    {
                        ++FileListView.CurrentHistoryIndex;
                        FileListView.History.Add(selectedFolder);
                    }
                    else
                    {
                        if (selectedFolder == FileListView.History[FileListView.CurrentHistoryIndex + 1])
                        {
                            ++FileListView.CurrentHistoryIndex;
                        }
                        else
                        {
                            int startIndex = FileListView.CurrentHistoryIndex + 1;
                            int count = FileListView.History.Count - startIndex;
                            FileListView.History.RemoveRange(startIndex, count);
                            FileListView.History.Add(selectedFolder);
                            ++FileListView.CurrentHistoryIndex;
                        }
                    }
                    FileListView.RenderListView(ref listView1, filePathTextBox, isRecycleBinFolder);
                    IsUserInteraction = false;

                }
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (IsUserInteraction)
                return;
            TreeNode selecteditem = e.Node;
            if (selecteditem != null)
            {
                if (selecteditem.Tag is File)
                {
                    var selectedFile = selecteditem.Tag as File;

                    FileWindow f2 = new FileWindow();
                    string content = "";
                    content = FileManagerDictionary.FileDictionary[selectedFile].ReadData(selectedFile);
                    f2.ShowFileContent(content);
                }
                else if (selecteditem.Tag is Directory)
                {
                    if (IsUserInteraction) return;
                    IsUserInteraction = true;
                    var selectedFolder = selecteditem.Tag as Directory;
                    bool isRecycleBinFolder = selectedFolder.MainName == "Recycle Bin";
                    if (selectedFolder.GetListViewItem().Text == "Recycle Bin" && FileListView.CurrentHistoryIndex == 0)
                    {
                        IsRecycleBin = true;
                    }
                    if (folderTree.SelectedNode != null)
                        folderTree.SelectedNode.BackColor = Color.White;
                    folderTree.SelectedNode = selectedFolder.GetNode();
                    folderTree.SelectedNode.BackColor = Color.Yellow;
                    if (FileListView.IsLastDirectory())
                    {
                        ++FileListView.CurrentHistoryIndex;
                        FileListView.History.Add(selectedFolder);
                    }
                    else
                    {
                        if (selectedFolder == FileListView.History[FileListView.CurrentHistoryIndex + 1])
                        {
                            ++FileListView.CurrentHistoryIndex;
                        }
                        else
                        {
                            int startIndex = FileListView.CurrentHistoryIndex + 1;
                            int count = FileListView.History.Count - startIndex;
                            FileListView.History.RemoveRange(startIndex, count);
                            FileListView.History.Add(selectedFolder);
                            ++FileListView.CurrentHistoryIndex;
                        }
                    }
                    FileListView.RenderListView(ref listView1, filePathTextBox, isRecycleBinFolder);
                    IsUserInteraction = false;

                }
            }
        }

        private void listView1_SelectedIndexChanged_2(object sender, EventArgs e)
        {
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0].Tag as FileManager;
            /*if(item.IsFAT32)
            {
                if(fat32.DeleteFile(item))
                {
                    listView1.Items.Remove(item.GetListViewItem());
                    item.SetRecycleBin(true);
                    item.SetVisible(false);
                    fat32.RecycleBin.Add(item);
                    Console.WriteLine(fat32.RecycleBin.Count);

                    MessageBox.Show("Delete file successfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else
                    MessageBox.Show("Delete file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                if (ntfs.DeleteFile(item))
                {
                    listView1.Items.Remove(item.GetListViewItem());
                    item.SetRecycleBin(true);
                    item.SetVisible(false);
                    ntfs.RecycleBin.Add(item);
                    MessageBox.Show("Delete file successfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Delete file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }*/
            var fileSystem = FileManagerDictionary.FileDictionary[item];
            if (fileSystem.DeleteFile(item))
            {
                listView1.Items.Remove(item.GetListViewItem());
                item.SetRecycleBin(true);
                item.SetVisible(false);
                fileSystem.RecycleBin.Add(item);
                SystemFiles.SystemFolder.Children[0].Children.Add(item);
                MessageBox.Show("Delete file successfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Delete file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0].Tag as FileManager;

            string fileContent = item.MainName;
            MessageBox.Show(fileContent, "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0].Tag as FileManager;
            /*if(item.IsFAT32)
            {
                if (fat32.RestoreFile(item))
                {
                    fat32.RecycleBin.Remove(item);
                    item.SetRecycleBin(false);
                    item.SetVisible(true);
                    listView1.Items.Remove(item.GetListViewItem());
                    
                    MessageBox.Show("Restore file succesfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show("Restore file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            else
            {
                if (ntfs.RestoreFile(item))
                {
                    ntfs.RecycleBin.Remove(item);
                    item.SetRecycleBin(false);
                    item.SetVisible(true);
                    listView1.Items.Remove(item.GetListViewItem());
                    MessageBox.Show("Restore file succesfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show("Restore file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }*/
            var fileSystem = FileManagerDictionary.FileDictionary[item];
            if(fileSystem.RestoreFile(item))
            {
                SystemFiles.SystemFolder.Children[0].Children.Remove(item);
                item.SetRecycleBin(false);
                item.SetVisible(true);
                listView1.Items.Remove(item.GetListViewItem());
                MessageBox.Show("Restore file succesfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("Restore file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
