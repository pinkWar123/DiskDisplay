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

        public static void RenderListView(ref ListView listView)
        {
            if (IsCurrentlyProcessing) return;

            IsCurrentlyProcessing = true;
            listView.Items.Clear();
            Console.WriteLine("Current index: " + FileListView.CurrentHistoryIndex);
            Console.WriteLine("History length: " + FileListView.History.Count);
            if (History[CurrentHistoryIndex].Children != null)
            foreach (var child in History[CurrentHistoryIndex].Children)
            {
                listView.Items.Add(child.GetListViewItem());
            }

            listView.SelectedItems.Clear();
            IsCurrentlyProcessing = false;
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
