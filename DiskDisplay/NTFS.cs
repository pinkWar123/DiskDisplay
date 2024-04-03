﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class NTFS : FileSystem
{
    public UInt16 BytePerSector;
    public byte SectorPerCluster;
    public UInt16 SectorPerTrack;
    public UInt16 NumberOfHead;
    public UInt64 TotalSector;
    public UInt64 StartingClusterOfMFT;
    public UInt64 StartingClusterOfBackupMFT;
    public UInt32 BytePerEntry;

    public NTFS() { }
    public NTFS(string drive) {
        this.DriveName = drive;
    }

    public NTFS(UInt32 firstsector, string Diskname)
    {
        this.DiskName = Diskname;
        this.FirstSector = firstsector;
    }

    public override List<FileManager> ReadFileSystem()
    {
        try
        {
            string filename = @"\\.\" + DriveName;
            using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                ReadVBR(filestream);
                List<FileManager> files = new List<FileManager>();
                ReadMFT(filestream, ref files);
                return files;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        return null;
    }

    public override bool DeleteFile(FileManager file)
    {
        try
        {
            string filename = @"\\.\" + DriveName;
            using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                filestream.Seek(OffsetWithCluster(StartingClusterOfMFT) + 0x23 * 1024, SeekOrigin.Begin);
                int count = 0;
                while (count++ < 200)
                {
                    Console.WriteLine(count);
                    byte[] MFTBytes = new byte[BytePerEntry];
                    filestream.Read(MFTBytes, 0, MFTBytes.Length);

                    FileManager temp = MFTEntry.MFTEntryProcess(MFTBytes);
                    if (temp != null)
                    {
                        if (temp.ID == file.ID)
                        {
                            for (int i = 0x16; i < 0x18; i++)
                            {
                                MFTBytes[i] = 0x00;
                            }
                            filestream.Seek(-MFTBytes.Length, SeekOrigin.Current);

                            // Write the modified MFT entry back to the file
                            filestream.Write(MFTBytes, 0, MFTBytes.Length);

                            // Exit the loop since the modification is done
                            break;
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            return false;
        }
    }

    public override string ReadData(FileManager file)
    {
        if (file is File && file.IsFAT32 == false)
        {
            File tempfile = (File)file;
            if(tempfile.MainName.EndsWith(".txt"))
            {
                if (tempfile.IsNon_Resident == false)
                    return tempfile.content_President;

                string result = "";
                string filename = @"\\.\" + DriveName;
                Int32 size = (Int32)tempfile.FileSize;
                using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[BytePerSector * SectorPerCluster];
                    Int64 Offset = OffsetWithCluster((UInt64)(tempfile.StartCluster));
                    filestream.Seek(Offset, SeekOrigin.Begin);
                    while(size > 0)
                    {
                        filestream.Read(data, 0, (int)BytePerSector * SectorPerCluster);
                        result += Encoding.UTF8.GetString(data, 0, (int)((data.Length <= size) ? data.Length : (int)size));
                        size -= data.Length;
                    }
                }
                return result;
            }
            return "We dont't Support this type file\n";
            
        }
        
        return "Wrong File";
    }

    
    public override bool RestoreFile(FileManager file)
    {
        // tu tu
        return true;
    }

    public void ReadVBR(FileStream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] VBRBytes = new byte[512];
        fileStream.Read(VBRBytes, 0, VBRBytes.Length);

        BytePerSector = BitConverter.ToUInt16(VBRBytes, 0x0B);
        SectorPerCluster = VBRBytes[0x0D];
        SectorPerTrack = BitConverter.ToUInt16(VBRBytes, 0x18);
        NumberOfHead = BitConverter.ToUInt16(VBRBytes, 0x1A);
        TotalSector = BitConverter.ToUInt64(VBRBytes, 0x28);
        StartingClusterOfMFT = BitConverter.ToUInt64(VBRBytes, 0x30);
        StartingClusterOfBackupMFT = BitConverter.ToUInt64(VBRBytes, 0x38);

        if ((sbyte)(VBRBytes[0x40]) < 0)
        {
            BytePerEntry = (UInt16)Math.Pow(2, Math.Abs((sbyte)(~VBRBytes[0x40] + 1)));
        }
        else
        {
            BytePerEntry = (UInt32)VBRBytes[0x40] * BytePerSector * SectorPerCluster;
        }
    }
   
    public void ReadMFT(FileStream fileStream, ref List<FileManager> files)
    {
        byte[] MFTBytes = new byte[BytePerEntry];

        fileStream.Seek(OffsetWithCluster(StartingClusterOfMFT) + 0x23 * 1024 , SeekOrigin.Begin);
        int count = 0;
        //FileManager temp = new FileManager();
        List<FileManager > OrphanedFile = new List<FileManager>();
        while(count++ < 200)
        {
            fileStream.Read(MFTBytes, 0, MFTBytes.Length);

            FileManager temp = MFTEntry.MFTEntryProcess(MFTBytes);
            if(temp != null)
            {
                if(temp.RootID != 0x05)
                {
                    for (int i = 0; i < OrphanedFile.Count; i++)
                    {
                        if (OrphanedFile[i].RootID == temp.ID)
                        {
                            Directory dir = (Directory)temp;
                            dir.Children.Add(OrphanedFile[i]);
                            OrphanedFile.RemoveAt(i);
                            --i;
                            temp = dir;
                        }
                    }
                    bool Orphanedflag = true;
                    for(int i = 0; i < files.Count; i++)
                    {
                        FileManager tempfile = (FileManager)files[i];
                        if(tempfile.FindFather(temp) == true)
                            Orphanedflag = false;
                    }

                    if(Orphanedflag == true)
                        OrphanedFile.Add(temp);
                }
                else
                {
                    files.Add(temp);
                }
            }
        }
    }

    private Int64 OffsetWithCluster(UInt64 Cluster)
    {
        return (Int64)(Cluster * SectorPerCluster * BytePerSector);
    }

    private ulong CalculateMFTEntryOffset(string fileName, UInt64 startingClusterOfMFT, UInt32 bytePerEntry)
    {
        // Calculate the size of each cluster
        ulong clusterSize = (ulong)BytePerSector * SectorPerCluster;

        // Calculate the number of MFT entries per cluster
        uint entriesPerCluster = (uint)(clusterSize / bytePerEntry);

        // Calculate the number of clusters needed to store the MFT entry
        uint clustersNeeded = (uint)Math.Ceiling((double)fileName.Length / bytePerEntry);

        // Calculate the starting cluster index of the MFT entry
        ulong startingClusterIndex = startingClusterOfMFT + (clustersNeeded - 1);

        // Calculate the byte offset within the cluster for the MFT entry
        uint byteOffset = (uint)((fileName.Length - 1) % bytePerEntry);

        // Calculate the byte offset within the MFT for the MFT entry
        ulong mftEntryOffset = startingClusterIndex * clusterSize + byteOffset;

        return mftEntryOffset;
    }

}


static class MFTEntry
{
    
    private static bool IsCorrectFile(byte[] entry)
    {

        return BitConverter.ToInt32(entry, 0x00) != 0x00 && Encoding.ASCII.GetString(entry, 0, 4) != "BAAD";
    }
    static private UInt64 GetNumberWithKByte(byte[] entry, UInt32 Offset, int k)
    {
        byte[] temp = new byte[8];

        for (int i = 0; i < k; i++)
        {
            temp[i] = entry[Offset + i];
        }
        for(int i = k; i < 8; i++)
        {
            temp[i] = 0x00;
        }

        return BitConverter.ToUInt64(temp, 0);
    }
    public static FileManager MFTEntryProcess(byte[] entry)
    {

        if(!IsCorrectFile(entry))
            return null;
        UInt16 AttributeOffset = BitConverter.ToUInt16(entry, 0x14);
        UInt16 status = BitConverter.ToUInt16(entry, 0x16);
        UInt32 EntryID = BitConverter.ToUInt32(entry, 0x2C);

        if (status == 0x00 || status == 0x02)
            return null;


        DateTime Creationtime = DateTime.Now;
        DateTime Modifiedtime = DateTime.Now;
        UInt32 FileSize = 0;
        UInt64 RootID = 0;
        UInt32 SizeOfContent = 0;
        UInt16 ContentOffset = 0;
        byte IsNon_Resident = 0x00;
        string filename = "";
        string content = "";

        UInt32 StartingClusterOfContent = 0;
        UInt32 NumberOfContigousCluster = 0;

        // Read Attribute-------------------------------
        while (AttributeOffset <= 1024)
        {
            UInt32 AttributeType = BitConverter.ToUInt32(entry, AttributeOffset);
            if (AttributeType == 0xFFFFFFFF) // End Attribute
                break;
            
            UInt32 AttributeSize = BitConverter.ToUInt32(entry, AttributeOffset + 0x04);
            IsNon_Resident = entry[AttributeOffset + 0x08];
            ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x14);
            if (IsNon_Resident == 0x00)
            {
                SizeOfContent = BitConverter.ToUInt32(entry, AttributeOffset + 0x10);
            }
            else if(IsNon_Resident == 0x01)
            {
                SizeOfContent = BitConverter.ToUInt32(entry, AttributeOffset + 0x30);
                ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x20);
            }

            if(AttributeType == 0x10)
            {
                Creationtime = DateTimeWithNanoSecond(entry, (AttributeOffset + ContentOffset + 0x00));
                Modifiedtime = DateTimeWithNanoSecond(entry, AttributeOffset + ContentOffset + 0x08);
            }
            else if(AttributeType == 0x30) {

                RootID = GetNumberWithKByte(entry, (uint)AttributeOffset + ContentOffset, 6);
                filename = Encoding.Unicode.GetString(entry,AttributeOffset + ContentOffset + 0x42, 2*entry[AttributeOffset + ContentOffset + 0x40]);
                
                if (filename[0] == '$' || filename.Length == 0)
                    return null;
            }
            else if(AttributeType == 0x80)
            {
                FileSize += SizeOfContent;
                if(filename.EndsWith(".txt"))
                {
                    if (IsNon_Resident == 0x00)
                    {
                        content = Encoding.UTF8.GetString(entry, AttributeOffset + ContentOffset, (int)SizeOfContent);
                    }
                    else if (IsNon_Resident == 0x01)
                    {
                        // Read Runlist
                        byte firstdatarun = entry[AttributeOffset + ContentOffset];
                        byte firstFourBytes = (byte)(firstdatarun >> 4);
                        byte lastFourBytes = (byte)(firstdatarun & 0b00001111);
                        
                        StartingClusterOfContent = (UInt32)GetNumberWithKByte(entry, (uint)AttributeOffset + ContentOffset + lastFourBytes + 1, firstFourBytes);       
                        NumberOfContigousCluster = (UInt32)GetNumberWithKByte(entry, (uint)AttributeOffset + ContentOffset + 1, lastFourBytes);
                    }
                }
                
            }
            AttributeOffset += (UInt16)AttributeSize;
        }
     

        if(status == 0x01) // File
        {
            File result = new File();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, StartingClusterOfContent, NumberOfContigousCluster, IsNon_Resident, content);
            return result;
        }
        else
        {
            Directory result = new Directory();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, StartingClusterOfContent, NumberOfContigousCluster, IsNon_Resident,  content);
            return result;
        }
    }


    public static DateTime DateTimeWithNanoSecond(byte[] bytes, int index)
    {
        UInt64 temp = BitConverter.ToUInt64(bytes, index);
        DateTime result = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc); 
        ulong seconds = temp / 10000000; 
        ulong nanoseconds = (temp % 10000000) * 100; 
        result = result.AddSeconds(seconds).AddTicks((long)nanoseconds); 

        result = result.ToLocalTime(); 

        return result;
    }

}

