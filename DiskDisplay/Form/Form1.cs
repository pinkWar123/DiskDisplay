using DiskDisplay.NewFolder1;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DiskDisplay
{
    public partial class Form1 : Form
    {
     
        private bool IsUserInteraction = false;
        public bool IsRecycleBin = false;
        
        public Form1()
        {
            InitializeComponent();
            Image1.LoadImageList();
            folderTree.ImageList = Image1.ImageList;
            SystemFiles.InitializeSystemFiles();
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
                FileListView.RenderListView(ref listView1, ref folderTree,filePathTextBox);
                IsUserInteraction = false;

            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (!FileListView.IsLastDirectory())
            {
                IsUserInteraction = true;
                FileListView.CurrentHistoryIndex++;
                FileListView.RenderListView(ref listView1, ref folderTree, filePathTextBox);
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
                        recycleBinContextMenu.Show(listView1, e.Location);
                    }
                    else
                    {
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
                    if (selectedFile.IsRecycleBin()) return;
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
                    if (selectedFolder.IsRecycleBin()) return;

                    bool isRecycleBinFolder = selectedFolder.MainName == "Recycle Bin";
                    if(selectedFolder.GetListViewItem().Text == "Recycle Bin" && FileListView.CurrentHistoryIndex == 0)
                    {
                        IsRecycleBin = true;
                    }
                    
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
                    FileListView.RenderListView(ref listView1, ref folderTree, filePathTextBox, isRecycleBinFolder);
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
                    FileListView.RenderListView(ref listView1, ref folderTree, filePathTextBox, isRecycleBinFolder);
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
            var fileSystem = FileManagerDictionary.FileDictionary[item];
            if (fileSystem.DeleteFile(item))
            {
                listView1.Items.Remove(item.GetListViewItem());
                folderTree.Nodes.Remove(item.GetNode());
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
            
            var fileSystem = FileManagerDictionary.FileDictionary[item];
            if(fileSystem.RestoreFile(item))
            {
                SystemFiles.SystemFolder.Children[0].Children.Remove(item);
                item.SetRecycleBin(false);
                item.SetVisible(true);
                listView1.Items.Remove(item.GetListViewItem());
                item.GetParent().GetNode().Nodes.Insert(item.GetTreeViewIndex(), item.GetNode());
                MessageBox.Show("Restore file succesfully", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("Restore file failed", "File Content", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
