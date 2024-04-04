using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Disk
{
    string DiskName = @"PhysicalDrive1";
    Dictionary<FileSystem, List<FileManager>> fileSystem;

    private void ReadDisk()
    {
        using (FileStream filestream = new FileStream(DiskName, FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[512];
            filestream.Read(bytes, 0, bytes.Length);

            int index = 0x1BE;
            for(int i = 0; i < 2; i++)
            {
                if (bytes[index + 0x04] == 0x07) // NTFS Partition
                {
                    
                }
                else if (bytes[index + 0x04] == 0x0C) // FAT32 Partition
                {

                }
            }

        }
    }
    Disk()
    {
        
    }
    Disk(string filename)
    {
        DiskName = filename;
    }
}