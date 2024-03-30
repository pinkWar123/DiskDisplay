using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskDisplay
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            FileSystem fat = new FAT32("E:");
            List<FileManager> files = new List<FileManager>();
            files = fat.ReadFileSystem();

            for (int i = 0; i < files.Count; i++)
            {
                files[i].PrintImfomations(0);
            }
            fat.DeleteFile(files[2]);
        }
    }
}

