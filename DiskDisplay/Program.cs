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

            NTFS ntfs = new NTFS();
            List<FileManager> files = new List<FileManager> ();
            using (FileStream fileStream = new FileStream(@"\\.\E:", FileMode.Open, FileAccess.Read))
            {
                ntfs.ReadVBR(fileStream);
                Console.WriteLine("Byte Per Sector: " + ntfs.BytePerSector);
                Console.WriteLine("Sector Per Cluster: " + ntfs.SectorPerCluster);
                Console.WriteLine("Sector per track: " + ntfs.SectorPerTrack);
                Console.WriteLine("Number Of Head: " + ntfs.NumberOfHead);
                Console.WriteLine("Total Sector: " + ntfs.TotalSector);
                Console.WriteLine("Starting Cluster of MFT: " + ntfs.StartingClusterOfMFT);
                Console.WriteLine("Starting Cluster of Back up MFT: " + ntfs.StartingClusterOfBackupMFT);
                Console.WriteLine("Byte per entry " + ntfs.BytePerEntry);
                ntfs.ReadMFT(fileStream, ref files);

            }
            for(int i = 0; i < files.Count; i++)
            {
                files[i].PrintImfomations(0);
            }
        }
    }
}
