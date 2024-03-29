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
        private bool IsUserInteraction = false;
        public Form1()
        {
            InitializeComponent();
            NTFS ntfs = new NTFS("E:");
            List<FileManager> files = new List<FileManager>();
            files = ntfs.ReadFileSystem();

            var RootFolder = new NTFSDirectory() ;
            RootFolder.Children = files;
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;
            RootFolder.Populate();
            
            /*var RootFolder1 = new NTFSDirectory();
            RootFolder1.Children = files;*/
            /*foreach (var folder in RootFolder.Children)
            {
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
            }*/
            foreach (var folder in files)
            {
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
            }
            FileListView.History.Add(RootFolder);
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
                if (selecteditem.Tag is NTFSFile)
                {
                    var selectedFile = selecteditem.Tag as NTFSFile;
                    MessageBox.Show(selectedFile.MainName);
                }
                else if (selecteditem.Tag is NTFSDirectory )
                {
                    if (IsUserInteraction) 
                        return;
                    var selectedFolder = selecteditem.Tag as NTFSDirectory;
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
                        if (selectedFolder == FileListView.History[FileListView.CurrentHistoryIndex + 1])
                        {
                            Console.WriteLine("Here1");

                            ++FileListView.CurrentHistoryIndex;
                        }
                        else
                        {
                            Console.WriteLine("Here");
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

        private void listView1_SelectedIndexChanged_2(object sender, EventArgs e)
        {
            ListViewItem selecteditem = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;
            if (selecteditem != null)
            {
                // Your logic here
                // Do something with the selected item
                if (selecteditem.Tag is NTFSFile)
                {
                    var selectedFile = selecteditem.Tag as NTFSFile;
                    MessageBox.Show(selectedFile.MainName);

                }
                else if (selecteditem.Tag is NTFSDirectory)
                {
                    if(IsUserInteraction) return;
                    var selectedFolder = selecteditem.Tag as NTFSDirectory;
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
