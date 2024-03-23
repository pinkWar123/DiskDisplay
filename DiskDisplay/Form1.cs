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

namespace DiskDisplay
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            var fat32 = new FAT32();

            var folders = fat32.ReadFiles(@"\\.\E:");
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;
            foreach(var folder in folders)
            {
                folderTree.Nodes.Add(folder.GetNode());
                listView1.Items.Add(folder.GetListViewItem());
                folder.Populate();
                folder.PopulateListView(listView1);
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

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            if(selectedNode.Tag is File)
            {
                File selectedFile = e.Node.Tag as File;
                MessageBox.Show(selectedFile.GetContent());
                //webBrowser1.DocumentText = selectedFile.GetContent();
            }
            // Perform actions based on the selected node
            //webBrowser1.DocumentText = ("Selected Node: " + selectedNode.Text);
        }

        
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog() { Description="Select your path"})
            {
                if(fbd.ShowDialog()==DialogResult.OK)
                {
                    //webBrowser1.Url = new Uri(fbd.SelectedPath);
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            //if (webBrowser1.CanGoBack)
            //    webBrowser1.GoBack();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            //if (webBrowser1.CanGoForward)
               // webBrowser1.GoForward();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                var rectangle = listView1.GetItemRect(i);
                if (rectangle.Contains(e.Location))
                {
                    MessageBox.Show("Item " + i);
                    return;
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem selecteditem = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;
            if (selecteditem != null)
            {
                // Your logic here
                // Do something with the selected item
                MessageBox.Show(selecteditem.Text);
            }
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ListViewItem selecteditem = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;
            if (selecteditem != null)
            {
                // Your logic here
                // Do something with the selected item
                if(selecteditem.Tag is File)
                {
                    File selectedFile = selecteditem.Tag as File;
                    MessageBox.Show(selectedFile.GetContent());

                }
                else if(selecteditem.Tag is Folder)
                {
                    Folder selectedFolder = selecteditem.Tag as Folder;
                    selectedFolder.PopulateListView(listView1);
                }
                //MessageBox.Show(selecteditem.Text);
            }
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
