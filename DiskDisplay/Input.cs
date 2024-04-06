using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DiskDisplay
{
    public partial class Input : Form
    {
        private System.Windows.Forms.TreeView folderTree;
        private System.Windows.Forms.ListView listView1;
        public Input(ref System.Windows.Forms.TreeView folderTree, ref System.Windows.Forms.ListView listView1)
        {
            InitializeComponent();
            this.folderTree = folderTree;
            this.listView1 = listView1;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                var result = partitionTextBox.Text;
                var fileSystem = FileSystem.Detect_FileSystem(result);
                if (fileSystem == null)
                {
                    MessageBox.Show("No disk found");
                    return;
                }
                List<FileManager> files = fileSystem.ReadFileSystem();
                SystemFiles.UpdateSystemFiles(ref fileSystem, ref files, result);
                FileManagerDictionary.UpdateFileDictionary(files, ref fileSystem);
                UpdateUI(ref folderTree, ref listView1);
                MessageBox.Show("Add disk succesfully");
                this.Close();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        public void UpdateUI(ref System.Windows.Forms.TreeView folderTree, ref System.Windows.Forms.ListView listView1) 
        {
            
            SystemFiles.UpdateUI(folderTree, listView1);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
