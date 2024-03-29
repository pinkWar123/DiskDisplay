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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            /*NTFS ntfs = new NTFS("E:");
            List<FileManager> files = new List<FileManager>();
            files = ntfs.ReadFileSystem();

            for (int i = 0; i < files.Count; i++)
            {
                for(int j = 0; j < files[i].Children.Count; j++)
                    Console.WriteLine(ntfs.ReadData(files[i].Children[j]));

            }*/
        }
    }
}
