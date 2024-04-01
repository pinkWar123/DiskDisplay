using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO.Pipes;
using System.Linq;
using System.Xml.Linq;

class FAT32 : FileSystem
{
    // This area is used for Boost_Sector's Variables
    public UInt16 BytesPerSector;
    public byte SectorPerCluster;
    public UInt16 ReversedSector;
    public byte NumberOfFAT;
    public UInt32 VolumeSize;
    public UInt32 SectorPerFAT;
    public UInt32 StartingClusterOfRDET;
    public string FATType;
    private UInt32 SeedId = 0x05;


    //--------------------------------------------------------------

    // This area is used for FAT's Variables

    UInt32[] FATTable;
    //--------------------------------------------------------------


    //--------------------------------------------------------------

    public FAT32()
    {
        FATType = "";
    }
    public FAT32(string file)
    {
        DriveName = file;
    }
    ~FAT32() { }

    public override List<FileManager> ReadFileSystem()
    {
        string filename = @"\\.\" + DriveName;
        using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            filestream.Seek(0, SeekOrigin.Begin);

        }
        using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            ReadBoostSector(filestream);
            ReadFAT1(filestream);
            List<FileManager> files = new List<FileManager>();
            ReadDET(filestream, StartingClusterOfRDET, ref files,SeedId);
            return files;
        }
    }

    public override string ReadData(FileManager file)
    {
        if (file is File && file.IsFAT32)
        {
            string result = "";
            File temp = (File)file;
            Console.WriteLine("-" + temp.ExtendedName + "-");
            if (temp.ExtendedName != "TXT")
                return "We don't support this file";
            string filename = @"\\.\" + DriveName;
            int filesize = (int)temp.FileSize;
            using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                List<UInt32> dataoffile = FindListOfClusters(temp.StartCluster);
                byte[] bytes = new byte[BytesPerSector * SectorPerCluster];
                for (int i = 0; i < dataoffile.Count; i++)
                {
                    filestream.Seek(OffSetWithCluster(dataoffile[i]), SeekOrigin.Begin);
                    filestream.Read(bytes, 0, bytes.Length);
                    int length = (filesize <= bytes.Length) ? filesize : bytes.Length;
                    result += Encoding.UTF8.GetString(bytes, 0, length);
                    filesize -= length;

                }
            }
            return result;
        }

        return "";
    }
    private bool ChangeFat(FileStream filestream, UInt32 Offset, List<UInt32> ListofCLuster, Int32 value)
    {
        byte[] bytes;
        for (int i = 0; i < ListofCLuster.Count; i++)
        {
            if (value != 0x00)
                bytes = BitConverter.GetBytes(value);
            else
            {
                if (i == ListofCLuster.Count)
                    bytes = BitConverter.GetBytes(0xFFFFFFFF);
                else
                    bytes = BitConverter.GetBytes(ListofCLuster[i]);
            }

            filestream.Write(bytes, (int)(ListofCLuster[i] - 2) * 4 + (int)Offset, bytes.Length);
        }
        return true;
    }
    private bool ChangeEntry(FileStream filestream, FileManager file)
    {
        return true;
    }
    public override bool DeleteFile(FileManager file)
    {
        if (file.IsFAT32 == false)
            return false;
        string filename = @"\\.\" + DriveName;
        using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
        {
            byte[] value = { 0xE5 };
            if (ChangeEntryWithValue(filestream, file, StartingClusterOfRDET, value))
            {
                return true;
            }
        }

        return false;
    }
    private bool ChangeEntryWithValue(FileStream fileStream, FileManager file, UInt32 StartingCluter, byte[] value)
    {
        List<UInt32> ListOfCluster = FindListOfClusters(StartingCluter);
        byte[] ClusterByte = new byte[0];
        // Merge Cluster------
        for (int i = 0; i < ListOfCluster.Count; i++)
        {
            byte[] k = new byte[SectorPerCluster * BytesPerSector];
            fileStream.Seek(OffSetWithCluster(ListOfCluster[i]), SeekOrigin.Begin);
            fileStream.Read(k, 0, k.Length);
            ClusterByte = ClusterByte.Concat(k).ToArray();
        }

        //--------------
        byte[] temp = new byte[32];

        int total = 0;
        while (total < SectorPerCluster * BytesPerSector * ListOfCluster.Count)
        {
            Array.Copy(ClusterByte, total, temp, 0, 32);
            total += 32;
            if (temp[0x00] == 0x00)
                continue;

            string name = "";
            bool longNameFile = false;
            int count = 1;
            if (temp[0x0B] == 0x0F)
            {
                while (temp[0x0B] == 0x0F)
                {
                    string filename = "";
                    string name1 = Encoding.Unicode.GetString(temp, 0x01, FindLengthOfName(temp, 0x01, 10));
                    string name2 = Encoding.Unicode.GetString(temp, 0x0E, FindLengthOfName(temp, 0x0E, 12));
                    string name3 = Encoding.Unicode.GetString(temp, 0x1C, FindLengthOfName(temp, 0x1C, 4));

                    filename += name1;
                    filename += name2;
                    filename += name3;
                    name = filename + name;
                    Array.Copy(ClusterByte, total, temp, 0, 32);
                    total += 32;
                    count++;
                }
                longNameFile = true;
            }
            if (!longNameFile)
                name = Encoding.ASCII.GetString(temp, 0, 8);
            bool flag = false;
            if (file == null)
                flag = true;
            else if (name == file.MainName && (file is Directory || file.ExtendedName == Encoding.ASCII.GetString(temp, 0x08, 3))
                                           && BitConverter.ToUInt16(temp, 0x1A) == file.StartCluster)
                flag = true;

            if (flag)
            {
                int backuptotal = total;
                while (count > 0)
                {
                    total -= 32;
                    ClusterByte[total] = value[0];
                    count--;
                }
                total = backuptotal;
            }
            //Check if it is a Directory --> Change in its children
            if (!CheckBitAt(temp[0x0B], 5) && CheckBitAt(temp[0x0B], 4) && !CheckBitAt(temp[0x0B], 1) 
                && !name.Contains(".       ") && !name.Contains("..      ")
                && BitConverter.ToUInt16(temp, 0x1A) != StartingCluter)
            {
                UInt16 s = BitConverter.ToUInt16(temp, 0x1A);
                bool HasDelete = false;
                if (flag)
                    HasDelete = ChangeEntryWithValue(fileStream, null, s, value);
                else // find in it's children
                    HasDelete = ChangeEntryWithValue(fileStream, file, s, value);
                if (HasDelete)
                    flag = true;

            }
            if (flag && file != null)
            {
                for (int i = 0; i < ListOfCluster.Count; i++)
                {
                    fileStream.Seek(OffSetWithCluster(ListOfCluster[i]), SeekOrigin.Begin);
                    fileStream.Write(ClusterByte, i * SectorPerCluster * BytesPerSector, SectorPerCluster * BytesPerSector);
                }
                return true;
            }
        }
        if (file == null)
        {
            for (int i = 0; i < ListOfCluster.Count; i++)
            {
                fileStream.Seek(OffSetWithCluster(ListOfCluster[i]), SeekOrigin.Begin);
                fileStream.Write(ClusterByte, i * SectorPerCluster * BytesPerSector, SectorPerCluster * BytesPerSector);
            }
            return true;
        }


        return false;
    }
    public override bool RestoreFile(FileManager file)
    {
        if (file.IsFAT32 == false)
            return false;
        string filename = @"\\.\" + DriveName;
        using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
        {
            byte[] value = { 0x48 };
            if (ChangeEntryWithValue(filestream, file, StartingClusterOfRDET, value))
            {
                return true;
            }
        }

        return false;
    }


    // This Function is used to read Boost Sector in partition
    public void ReadBoostSector(FileStream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] bootSectorBytes = new byte[512];
        fileStream.Read(bootSectorBytes, 0, bootSectorBytes.Length);

        BytesPerSector = BitConverter.ToUInt16(bootSectorBytes, 0x0B);
        SectorPerCluster = bootSectorBytes[0x0D];
        ReversedSector = BitConverter.ToUInt16(bootSectorBytes, 0x0E);
        NumberOfFAT = bootSectorBytes[0x10];
        VolumeSize = BitConverter.ToUInt32(bootSectorBytes, 0x20);
        SectorPerFAT = BitConverter.ToUInt32(bootSectorBytes, 0x24);
        StartingClusterOfRDET = BitConverter.ToUInt32(bootSectorBytes, 0x2C);
        FATType = Encoding.ASCII.GetString(bootSectorBytes, 0x52, 8);
    }

    // This Function is used to read File Allocation table (FAT1)
    public void ReadFAT1(FileStream fileStream)
    {
        fileStream.Seek(BytesPerSector * ReversedSector, SeekOrigin.Begin);
        int totalBytes = Convert.ToInt32(SectorPerFAT * BytesPerSector);
        byte[] FATBytes = new byte[totalBytes];
        FATTable = new UInt32[totalBytes / 4];
        fileStream.Read(FATBytes, 0, totalBytes);

        int index = 0;
        for (int i = 0; i < totalBytes; i += 4)
        {
            FATTable[index++] = BitConverter.ToUInt32(FATBytes, i);
        }
    }

    // This function will determie if This File is Archive
    //private bool IsArchive(byte data)
    //{
    //    byte mask = (byte)(1 << 5);
    //    // check if Archive bit(bit 5) is 1
    //    if ((data & mask) != 0)
    //    {
    //        return true;
    //    }
    //    return false;
    //}
    private bool CheckBitAt(byte data, int k)
    {
        byte mask = (byte)(1 << k);
        // check if Archive bit(bit 5) is 1
        if ((data & mask) != 0)
        {
            return true;
        }
        return false;
    }
    //This function find Length of Name with Long name in Secondery Entry
    private int FindLengthOfName(byte[] data, int index, int maxlength)
    {
        for (int i = 0; i < maxlength; i += 2)
        {
            UInt16 temp = BitConverter.ToUInt16(data, i + index);
            if (temp == 0x0000 || temp == 0xFFFF)
                return i;
        }
        return maxlength;
    }
    private bool CheckInValidEntry(byte[] buffer, UInt32 StartingCluster)
    {
        byte[] deleteddotdotFolder = { 0xE5, 0x2E, 0, 0, 0, 0, 0, 0 };
        return buffer[0] == 0x00 || buffer[0] == 0x05 || buffer[0x0B] == 0x08
                    || (CheckBitAt(buffer[0x0B], 1) && buffer[0x0B] != 0x0F)
                    || BitConverter.ToUInt16(buffer, 0x1A) == StartingCluster
                    || Encoding.ASCII.GetString(buffer, 0, 8) == Encoding.ASCII.GetString(deleteddotdotFolder)
                    || Encoding.ASCII.GetString(buffer, 0, 8) == "..      ";
    }

    // This Function will Read all FIle in RDET and SDET and return list of FileManager
    private void ReadDET(FileStream fileStream, UInt32 StartingCluster, ref List<FileManager> FileRoot, UInt32 RootID)
    {
        List<UInt32> ListofCluster = FindListOfClusters(StartingCluster);
        List<byte[]> EntryQueue = new List<byte[]>();

        for (int i = 0; i < ListofCluster.Count; i++)
        {
            fileStream.Seek(OffSetWithCluster(ListofCluster[i]), SeekOrigin.Begin);
            int Count = 0;
            while (Count < SectorPerCluster * BytesPerSector)
            {
                byte[] buffer = new byte[32];
                Count += 32;
                fileStream.Read(buffer, 0, 32);
                if (CheckInValidEntry(buffer, StartingCluster))
                {
                    while ((EntryQueue.Count > 0))
                    {
                        if (EntryQueue[EntryQueue.Count - 1][0x0B] != 0x0F)
                            break;
                        EntryQueue.RemoveAt(EntryQueue.Count - 1);
                    }
                    continue;
                }
                EntryQueue.Add(buffer);
            }
        }
        while (EntryQueue.Count > 0)
        {
            byte[] temp = EntryQueue[0];
            EntryQueue.RemoveAt(0);
            if (temp[0x0B] == 0x0F)
            {
                FileManager tempfile = ProcessLongName(fileStream, ref EntryQueue, temp, RootID);
                if (temp[0x00] == 0xE5 && tempfile.RootID == 5) RecycleBin.Add(tempfile);
                else FileRoot.Add(tempfile);
            }
            else
            {
                FileManager tempfile = ProcessShortName(fileStream, temp, RootID);
                if (temp[0x00] == 0xE5 && tempfile.RootID == 5) RecycleBin.Add(tempfile);
                else FileRoot.Add(tempfile);
            }

        }

    }

    private FileManager ProcessLongName(FileStream fileStream, ref List<byte[]> EntryQueue, byte[] temp, UInt32 RootID)
    {
        List<string> filenamefragment = new List<string>();
        while (true)
        {
            if (temp[0x0B] != 0x0F)
                break;
            string filename = "";
            string name1 = Encoding.Unicode.GetString(temp, 0x01, FindLengthOfName(temp, 0x01, 10));
            string name2 = Encoding.Unicode.GetString(temp, 0x0E, FindLengthOfName(temp, 0x0E, 12));
            string name3 = Encoding.Unicode.GetString(temp, 0x1C, FindLengthOfName(temp, 0x1C, 4));

            filename += name1;
            filename += name2;
            filename += name3;
            filenamefragment.Add(filename);
            temp = EntryQueue[0];
            EntryQueue.RemoveAt(0);
        }
        
        FileManager tempfile = ProcessShortName(fileStream, temp, RootID);
        tempfile.MainName = "";
        
        for (int i = filenamefragment.Count - 1; i >= 0; i--)
        {
            tempfile.MainName += filenamefragment[i];
        }

        return tempfile;
    }
    private FileManager ProcessShortName(FileStream fileStream, byte[] temp, UInt32 RootID)
    {
        if (CheckBitAt(temp[0x0B], 5))
        {
            File tempfile = new File();
            tempfile.ID = SeedId++;
            tempfile.RootID = RootID;
            tempfile.CloneData(temp);

            return tempfile;
        }

        Directory tempdirectory = new Directory();
        tempdirectory.CloneData(temp);
        tempdirectory.ID = SeedId++;
        tempdirectory.RootID = RootID;
        if (tempdirectory.MainName != ".       " && tempdirectory.MainName != "..      ")
        {
            ReadDET(fileStream, tempdirectory.StartCluster, ref tempdirectory.Children, tempdirectory.ID);
        }
        return tempdirectory;

    }

    public UInt32 OffSetWithCluster(UInt32 Cluster)
    {
        return ((Cluster - 2) * SectorPerCluster + ReversedSector + NumberOfFAT * SectorPerFAT) * BytesPerSector;
    }

    private List<UInt32> FindListOfClusters(UInt32 StartCluster)
    {
        List<UInt32> result = new List<UInt32>();
        if (StartCluster == 0 || StartCluster == 1)
            return result;
        UInt32 CurrentCluster = StartCluster;
        while (true)
        {
            result.Add(CurrentCluster);

            if (
                FATTable[(int)CurrentCluster] == 0xFFFFFFFF
                || FATTable[(int)CurrentCluster] == 0x0FFFFFFF
                || FATTable[(int)CurrentCluster] == 0xF7FFFFFF
                || FATTable[(int)CurrentCluster] == 0x0FFFFFF8)
            {
                break;
            }
            CurrentCluster = FATTable[CurrentCluster];
        }
        return result;
    }

}