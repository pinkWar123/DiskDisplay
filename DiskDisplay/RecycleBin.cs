using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskDisplay
{
    internal static class RecycleBin
    {
        /*protected static TreeNode CurrentNode = new TreeNode();
        protected static ListViewItem CurrentItem = new ListViewItem();*/
        public static List<FileManager> Children;
        public static void Populate()
        {
            /*CurrentNode.ImageKey = IsFile ? "fileIcon" : "folderIcon";
            CurrentNode.SelectedImageKey = IsFile ? "fileIcon" : "folderIcon";
            CurrentNode.Tag = this;

            CurrentNode.Text = MainName;
            if (Children != null)
                foreach (var child in Children)
                {
                    TreeNode node = new TreeNode();
                    child.SetNode(node);
                    child.Populate();
                    CurrentNode.Nodes.Add(node);
                }
            CurrentItem.Text = MainName;
            CurrentItem.Tag = this;
            CurrentItem.SubItems.Add(IsFile ? "fileIcon" : "folderIcon");
            CurrentItem.ImageIndex = 0;
            CurrentItem.SubItems.Add(GetSize().ToString());
            CurrentItem.SubItems.Add(Creationdatetime.ToString());*/

        }
    }
}
