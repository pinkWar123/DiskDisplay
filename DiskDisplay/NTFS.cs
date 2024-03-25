using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class NTFS
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

    public List<FileManager> ReadFile(string file)
    {
        // Add code here
        List<FileManager> files = new List<FileManager>();

        return files;
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
            BytePerEntry = VBRBytes[0x40];
    }
   
    public void ReadMFT(FileStream fileStream, ref List<FileManager> files)
    {
        byte[] MFTBytes = new byte[BytePerEntry];

        fileStream.Seek(OffsetWithCluster(StartingClusterOfMFT) + 0x23 * 1024 , SeekOrigin.Begin);
        int count = 0;
        //FileManager temp = new FileManager();
        List<NTFSFileManager > OrphanedFile = new List<NTFSFileManager>();
        while(count++ < 200)
        {
            fileStream.Read(MFTBytes, 0, MFTBytes.Length);

            NTFSFileManager temp = MFTEntry.MFTEntryProcess(MFTBytes);
            if(temp != null)
            {
                if(temp.RootID != 0x05)
                {
                    for (int i = 0; i < OrphanedFile.Count; i++)
                    {
                        if (OrphanedFile[i].RootID == temp.ID)
                        {
                            NTFSDirectory dir = (NTFSDirectory)temp;
                            dir.Children.Add(OrphanedFile[i]);
                            OrphanedFile.RemoveAt(i);
                            --i;
                            temp = dir;
                        }
                    }
                    bool Orphanedflag = true;
                    for(int i = 0; i < files.Count; i++)
                    {
                        NTFSFileManager tempfile = (NTFSFileManager)files[i];
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
}


static class MFTEntry
{
    private static bool IsCorrectFile(byte[] entry)
    {
        return BitConverter.ToInt32(entry, 0x00) != 0x00 && Encoding.ASCII.GetString(entry, 0, 4) != "BAAD";
    }
    public static NTFSFileManager MFTEntryProcess(byte[] entry)
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
        string filename = "";

        // Read Attribute-------------------------------
        while (AttributeOffset <= 1024)
        {
            UInt32 AttributeType = BitConverter.ToUInt32(entry, AttributeOffset);
            if (AttributeType == 0xFFFFFFFF) // End Attribute
                break;
            
            UInt32 AttributeSize = BitConverter.ToUInt32(entry, AttributeOffset + 0x04);
            byte IsNon_Resident = entry[AttributeOffset + 0x08];
            ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x14);

            if (IsNon_Resident == 0x00)
            {
                SizeOfContent = BitConverter.ToUInt32(entry, AttributeOffset + 0x10);     
            }
            else if(IsNon_Resident == 0x01)
            {
                SizeOfContent = BitConverter.ToUInt32(entry, AttributeOffset + 0x30);
            }

            if(AttributeType == 0x10)
            {
                Creationtime = DateTimeWithNanoSecond(entry, (AttributeOffset + ContentOffset + 0x00));
                Modifiedtime = DateTimeWithNanoSecond(entry, AttributeOffset + ContentOffset + 0x08);
            }
            else if(AttributeType == 0x30) {
                byte[] temp = new byte[8];
                
                for(int i = 0; i < 6; i++)
                {
                    temp[i] = entry[AttributeOffset + ContentOffset + i];
                }
                temp[6] = temp[7] =  0x00;
                RootID = BitConverter.ToUInt64(temp, 0);
                filename = Encoding.Unicode.GetString(entry,AttributeOffset + ContentOffset + 0x42, 2*entry[AttributeOffset + ContentOffset + 0x40]);
                
                if (filename[0] == '$' || filename.Length == 0)
                    return null;
            }
            else if(AttributeType == 0x80)
            {
                FileSize += SizeOfContent; // File = Size OF Content
            }
            AttributeOffset += (UInt16)AttributeSize;
        }
     

        if(status == 0x01) // File
        {
            NTFSFile result = new NTFSFile();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime);
            return result;
        }
        else
        {
            NTFSDirectory result = new NTFSDirectory();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime);
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

