﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskDisplay
{
    internal class FileListView
    {
        public static List<FATDirectory> History = new List<FATDirectory>();
        public static int CurrentHistoryIndex = 0;
        public static bool IsCurrentlyProcessing = false;

        public static void RenderListView(ref ListView listView)
        {
            if (IsCurrentlyProcessing) return;
            
            IsCurrentlyProcessing = true;
            listView.Items.Clear();
            Console.WriteLine("Currently in " + History[CurrentHistoryIndex].MainName);
            Console.WriteLine(History.Count);
            foreach (var child in History[CurrentHistoryIndex].Children)
            {
                Console.Write("b");
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
