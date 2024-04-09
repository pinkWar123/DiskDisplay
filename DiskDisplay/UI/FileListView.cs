using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskDisplay
{
    internal class FileListView
    {
        public static List<FileManager> History = new List<FileManager>();
        public static int CurrentHistoryIndex = 0;
        public static bool IsCurrentlyProcessing = false;

        public static void RenderListView(ref ListView listView, ref TreeView treeView,TextBox textBox, bool isRecycleBin = false)
        {
            if (IsCurrentlyProcessing) return;

            IsCurrentlyProcessing = true;
            listView.Items.Clear();
            if (History[CurrentHistoryIndex].Children != null)
            {
                var file = History[CurrentHistoryIndex];
                textBox.Text = file.GetPath();
                foreach (var child in file.Children)
                {
                    if ((file.MainName == "Recycle Bin") || child.GetVisible())
                    {
                        listView.Items.Add(child.GetListViewItem());
                    }
                }
            }

            listView.SelectedItems.Clear();
            IsCurrentlyProcessing = false;
            Console.WriteLine(History[CurrentHistoryIndex].GetPath());
        }

        public static bool IsLastDirectory()
        {
            return CurrentHistoryIndex == History.Count - 1;
        }

        public static bool IsFirstDirectory()
        {
            return CurrentHistoryIndex == 0;
        }
    }
}
