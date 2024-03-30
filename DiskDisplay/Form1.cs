using DiskDisplay.NewFolder1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace DiskDisplay
{
    public partial class Form1 : Form
    {
        private FAT32 fat32 = new FAT32("E:");
        private NTFS ntfs = new NTFS("Y:");
        private bool IsUserInteraction = false;
        public Form1()
        {
            InitializeComponent();
            List<FileManager> files = new List<FileManager>();
            files = ntfs.ReadFileSystem();
            
            List<FileManager> fat32Files = new List<FileManager>();
            fat32Files = fat32.ReadFileSystem();

            var RootFolder = new Directory() ;
            RootFolder.Children = fat32Files;
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;
            RootFolder.SetItemText("F:");
            RootFolder.SetNodeText("F:");

            var RootFolder1 = new Directory();
            RootFolder1.Children = files;
            RootFolder1.SetItemText("G:");
            RootFolder1.SetNodeText("G:");



            var SystemFolder = new Directory() ;
            SystemFolder.Children.Add(RootFolder);
            SystemFolder.Children.Add(RootFolder1);
            SystemFolder.Populate();
            foreach (var folder in SystemFolder.Children)
            {
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
            }
            FileListView.History.Add(SystemFolder);
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
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selecteditem = e.Node;
            if (selecteditem != null)
            {
                // Your logic here
                // Do something with the selected item
                if (selecteditem.Tag is File)
                {
                    var selectedFile = selecteditem.Tag as File;
                    MessageBox.Show(selectedFile.content_President);
                }
                else if (selecteditem.Tag is Directory )
                {
                    if (IsUserInteraction) 
                        return;
                    var selectedFolder = selecteditem.Tag as Directory;
                    if (folderTree.SelectedNode != null && folderTree.SelectedNode != selecteditem)
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
                        if (selectedFolder != FileListView.History[FileListView.CurrentHistoryIndex + 1])
                        {
                            int startIndex = FileListView.CurrentHistoryIndex + 1;
                            int count = FileListView.History.Count - startIndex;
                            FileListView.History.RemoveRange(startIndex, count);
                            FileListView.History.Add(selectedFolder);
                        }
                            ++FileListView.CurrentHistoryIndex;
                    }
                    FileListView.RenderListView(ref listView1);

                }

            }
        }

        
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog() { Description="Select your path"})
            {
                if(fbd.ShowDialog()==DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (!FileListView.IsFirstDirectory())
            {
                IsUserInteraction = true;
                FileListView.CurrentHistoryIndex--;
                FileListView.RenderListView(ref listView1);
                IsUserInteraction = false;

            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (!FileListView.IsLastDirectory())
            {
                IsUserInteraction = true;
                FileListView.CurrentHistoryIndex++;
                FileListView.RenderListView(ref listView1);
                IsUserInteraction = false;

            }
        }

        


        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                rightClicked = true;
            }
            else
            {
                rightClicked = false;
            }
        }
        bool rightClicked = false;
        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            rightClicked = false;
            if (e.Button == MouseButtons.Right)
            {
                ListViewHitTestInfo hitTestInfo = listView1.HitTest(e.X, e.Y);
                if (hitTestInfo.Item != null)
                {
                    contextMenuStrip1.Show(listView1, e.Location);
                }
            }
        }
        private void listView1_SelectedIndexChanged_2(object sender, EventArgs e)
        {
            if(rightClicked)
            {
                listView1.SelectedItems.Clear();
                return;
            }
            if(e is MouseEventArgs)
            {
                var _e = e as MouseEventArgs;
                if (_e.Button == MouseButtons.Right) return;
            }
            ListViewItem selecteditem = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;
            if (selecteditem != null)
            {
                if (selecteditem.Tag is File)
                {
                    var selectedFile = selecteditem.Tag as File;
                    
                    FileWindow f2 = new FileWindow();
                    f2.ShowFileContent(ntfs.ReadData(selectedFile));
                }
                else if (selecteditem.Tag is Directory)
                {
                    if(IsUserInteraction) return;
                    var selectedFolder = selecteditem.Tag as Directory;
                    if(folderTree.SelectedNode != null)
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
                            Console.WriteLine("Here1");

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
                    FileListView.RenderListView(ref listView1);
                    
                }

            }
        }

        
    }
}
