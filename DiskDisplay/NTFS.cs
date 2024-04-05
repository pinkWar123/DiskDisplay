using DiskDisplay;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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
    public Dictionary<UInt32, UInt32> DeletedIdInfo = new Dictionary<UInt32,UInt32>(); // This dictionary contains a map between ID of the deleted file and its RootID
    public NTFS() { }
    public NTFS(string drive) {
        this.DriveName = drive;
    }

    public NTFS(UInt32 firstsector, string Diskname)
    {
        this.DiskName = Diskname;
        this.FirstSector = firstsector;
    }
    static public bool IsNTFS(string name)
    {
        try
        {
            string filename = @"\\.\" + name;
            using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[10];
                filestream.Read(bytes, 0, bytes.Length);

                string type = Encoding.ASCII.GetString(bytes, 0x03, 4);

                if(type == "NTFS")
                    return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        return false;
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

    /*public override bool DeleteFile(FileManager file)
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
                            MFTBytes[0x16] = 0;
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
    }*/

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
                    for(int i = 0; i < file.ListDataruin.Count; i++)
                    {
                        filestream.Seek(OffsetWithCluster(file.ListDataruin[i].Item2), SeekOrigin.Begin);
                        for(int j = 0; j <  file.ListDataruin[i].Item1; j++)
                        {
                            filestream.Read(data, 0, (int)BytePerSector * SectorPerCluster);
                            result += Encoding.UTF8.GetString(data, 0, (int)((data.Length <= size) ? data.Length : (int)size));
                            size -= data.Length;
                            if (size <= 0)
                                break;
                        }
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
        try
        {
            if(DeletedIdInfo.ContainsKey(file.ID))
            {
                UInt32 rootFolderID = DeletedIdInfo[file.ID];
                var isRestoreSucceful = ChangeRootID(file, rootFolderID);
                if(isRestoreSucceful)
                {
                    DeletedIdInfo.Remove(file.ID);
                }
                return isRestoreSucceful;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring file: {ex.Message}");
            return false;
        }
    }

    private bool ChangeRootID(FileManager file, UInt32 parentId)
    {
        string filename = @"\\.\" + DriveName;
        using (FileStream filestream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
        {
            filestream.Seek(OffsetWithCluster(StartingClusterOfMFT) + 0x23 * 1024, SeekOrigin.Begin);
            int count = 0;
            while (count++ < 200)
            {
                byte[] entry = new byte[BytePerEntry];
                filestream.Read(entry, 0, entry.Length);

                FileManager temp = MFTEntry.MFTEntryProcess(entry);
                if (temp != null)
                {
                    if (temp.ID == file.ID)
                    {
                        UInt16 AttributeOffset = BitConverter.ToUInt16(entry, 0x14);
                        UInt16 status = BitConverter.ToUInt16(entry, 0x16);

                        if (status == 0x00 || status == 0x02)
                            return false;

                        // Read Attribute-------------------------------
                        while (AttributeOffset <= 1024)
                        {
                            UInt32 AttributeType = BitConverter.ToUInt32(entry, AttributeOffset);
                            if (AttributeType == 0xFFFFFFFF) // End Attribute
                                break;
                            UInt32 AttributeSize = BitConverter.ToUInt32(entry, AttributeOffset + 0x04);
                            UInt32 ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x14);
                            if (AttributeType == 0x30)
                            {
                                byte[] rootIdBytes = BitConverter.GetBytes(parentId);

                                entry[AttributeOffset + ContentOffset + 0x06] = 0x05;
                                Array.Copy(rootIdBytes, 0, entry, AttributeOffset + ContentOffset, Math.Min(6, rootIdBytes.Length));
                                filestream.Seek(-entry.Length, SeekOrigin.Current);

                                // Write the modified entry back to the file
                                filestream.Write(entry, 0, entry.Length);
                            }

                            AttributeOffset += (UInt16)AttributeSize;
                        }
                    }
                }
            }
        }
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

    /*public override bool DeleteFile(FileManager file)
    {
        try
        {
            var isDeleteSucessful = ChangeRootID(file, MFTEntry.RecyclerId);
            return isDeleteSucessful;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            return false;
        }
    }*/

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
                    byte[] entry = new byte[BytePerEntry];
                    filestream.Read(entry, 0, entry.Length);

                    FileManager temp = MFTEntry.MFTEntryProcess(entry);
                    if (temp != null)
                    {
                        if (temp.ID == file.ID)
                        {
                            UInt16 AttributeOffset = BitConverter.ToUInt16(entry, 0x14);
                            UInt16 status = BitConverter.ToUInt16(entry, 0x16);

                            if (status == 0x00 || status == 0x02)
                                return false;
                            // Read Attribute-------------------------------
                            while (AttributeOffset <= 1024)
                            {
                                UInt32 AttributeType = BitConverter.ToUInt32(entry, AttributeOffset);
                                if (AttributeType == 0xFFFFFFFF) // End Attribute
                                    break;
                                UInt32 AttributeSize = BitConverter.ToUInt32(entry, AttributeOffset + 0x04);
                                UInt32 ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x14);
                                if (AttributeType == 0x30)
                                {
                                    byte[] rootIdBytes = BitConverter.GetBytes(MFTEntry.RecyclerId);
                                    Console.WriteLine(MFTEntry.RecyclerId);
                                    Array.Copy(rootIdBytes, 0, entry, AttributeOffset + ContentOffset, Math.Min(6, rootIdBytes.Length));

                                    entry[AttributeOffset + ContentOffset + 0x38 + 0] = 0x01;
                                    entry[AttributeOffset + ContentOffset + 0x38 + 1] = 0x00;
                                    entry[AttributeOffset + ContentOffset + 0x38 + 2] = 0x00;
                                    entry[AttributeOffset + ContentOffset + 0x38 + 3] = 0x00;
                                    entry[AttributeOffset + ContentOffset + 0x16] = 0x00;
                                    entry[AttributeOffset + ContentOffset + 0x06] = 0x01;
                                    if (!DeletedIdInfo.ContainsKey(file.ID))
                                    {
                                        DeletedIdInfo.Add(file.ID, file.RootID);
                                    }
                                    /*
                                    Random random = new Random();
                                    string randomChars = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 6)
                                                                          .Select(s => s[random.Next(s.Length)]).ToArray());

                                    // Convert the random characters to Unicode (UTF-16) bytes

                                    byte[] randomBytes = Encoding.Unicode.GetBytes(randomChars);

                                    // Create a byte array for the new filename, including "$R" prefix and 5 random characters
                                    byte[] newFilename = new byte[30]; // 2 bytes for each character (12 characters)
                                    newFilename[0] = (byte)'$'; // First character: $
                                    newFilename[1] = 0;         // Second character: null terminator for Unicode (UTF-16)
                                    newFilename[2] = (byte)'R'; // Third character: R
                                    newFilename[3] = 0;         // Fourth character: null terminator for Unicode (UTF-16)
                                    Array.Copy(randomBytes, 0, newFilename, 4, randomBytes.Length); // Copy random characters starting from index 4
                                    Array.Copy(Encoding.Unicode.GetBytes(".txt"), 0, newFilename, 16, 8); // Add ".txt" extension
                                                                                                          // Padding with zeros
                                    for (int i = 24; i < 30; i++)
                                    {
                                        newFilename[i] = 0;
                                    }*/

                                    // Update the filename in the entry array at offset 42
                                    //Array.Copy(newFilename, 0, entry, AttributeOffset + ContentOffset + 0x42, newFilename.Length);
                                    filestream.Seek(-entry.Length, SeekOrigin.Current);

                                    // Write the modified entry back to the file
                                    filestream.Write(entry, 0, entry.Length);

                                }

                                AttributeOffset += (UInt16)AttributeSize;
                            }
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

    private Int64 OffsetWithCluster(UInt64 Cluster)
    {
        return (Int64)(Cluster * SectorPerCluster * BytePerSector);
    }


}


static class MFTEntry
{
    public static UInt32 RecycleBinId = 0;
    public static UInt32 RecyclerId = 0;
    private static bool IsCorrectFile(byte[] entry)
    {

        return BitConverter.ToInt32(entry, 0x00) != 0x00 && Encoding.ASCII.GetString(entry, 0, 4) != "BAAD";
    }
    static private Int64 GetNumberWithKByte(byte[] entry, UInt32 Offset, int k)
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

        return BitConverter.ToInt64(temp, 0);
    }
    public static FileManager MFTEntryProcess(byte[] entry, bool updateRecycleBin = false)
    {

        if (!IsCorrectFile(entry))
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

        Int32 StartingClusterOfContent = 0;
        Int32 NumberOfContigousCluster = 0;
        List<Tuple<UInt32, UInt32>> dataRuin = new List<Tuple<UInt32, UInt32>>();

        // Read Attribute-------------------------------
        while (AttributeOffset <= 1024)
        {
            UInt32 AttributeType = BitConverter.ToUInt32(entry, AttributeOffset);
            if (AttributeType == 0xFFFFFFFF) // End Attribute
                break;
            
            UInt32 AttributeSize = BitConverter.ToUInt32(entry, AttributeOffset + 0x04);
            byte IsNonResidentByte = entry[AttributeOffset + 0x08];
            ContentOffset = BitConverter.ToUInt16(entry, AttributeOffset + 0x14);

            if (IsNonResidentByte == 0x00)
            {
                SizeOfContent = BitConverter.ToUInt32(entry, AttributeOffset + 0x10);
            }
            else if(IsNonResidentByte == 0x01)
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
                RootID = (UInt32)GetNumberWithKByte(entry, (uint)AttributeOffset + ContentOffset, 6);
                filename = Encoding.Unicode.GetString(entry,AttributeOffset + ContentOffset + 0x42, 2*entry[AttributeOffset + ContentOffset + 0x40]);
                
                if (filename[0] == '$' || filename.Length == 0)
                {
                    Console.WriteLine("----------");
                    Console.WriteLine("File name: " + filename);
                    Console.WriteLine("ID: " + EntryID);
                    Console.WriteLine("RootID: " + RootID);
                    Console.WriteLine("----------");
                    if(filename == "$RECYCLE.BIN")
                    {
                        RecycleBinId = EntryID;
                    }
                    return null;
                }
                /*if(RecyclerId != 0 && RootID == RecyclerId)
                {
                    if (status == 0x01) // File
                    {
                        File result = new File();
                        result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, StartingClusterOfContent, NumberOfContigousCluster, IsNon_Resident, content);
                        FileSystem.RecycleBin.Add(result);
                        result.SetRecycleBin(true);
                    }
                    else
                    {
                        Directory result = new Directory();
                        result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, StartingClusterOfContent, NumberOfContigousCluster, IsNon_Resident, content);
                        FileSystem.RecycleBin.Add(result);
                        result.SetRecycleBin(true);
                    }

                }

                if(RecycleBinId != 0 && RootID == RecycleBinId)
                {
                    RecyclerId = EntryID;
                }*/
            }
            else if(AttributeType == 0x80)
            {
                FileSize += SizeOfContent;
                if(filename.EndsWith(".txt"))
                {
                    if (IsNonResidentByte == 0x00)
                    {
                        content = Encoding.UTF8.GetString(entry, AttributeOffset + ContentOffset, (int)SizeOfContent);
                    }
                    else if (IsNonResidentByte == 0x01)
                    {
                        // Read Runlist
                        IsNon_Resident = IsNonResidentByte;
                        int index = 0;
                        while(true)
                        {
                            byte firstdatarun = entry[AttributeOffset + ContentOffset + index];
                            if (firstdatarun == 0x00)
                                break;
                            byte firstFourBytes = (byte)(firstdatarun >> 4);
                            byte lastFourBytes = (byte)(firstdatarun & 0b00001111);
                        
                            StartingClusterOfContent = (Int32)GetNumberWithKByte(entry, (uint)(AttributeOffset + ContentOffset + lastFourBytes + index + 1), firstFourBytes);       
                            NumberOfContigousCluster = (Int32)GetNumberWithKByte(entry, (uint)(AttributeOffset + ContentOffset + index + 1), lastFourBytes);
                            if(index != 0 || dataRuin.Count != 0)
                            {
                                StartingClusterOfContent = (Int32)dataRuin[dataRuin.Count - 1].Item2 + StartingClusterOfContent;
                            }
                            Tuple<UInt32, UInt32> t = Tuple.Create((UInt32)NumberOfContigousCluster, (UInt32)StartingClusterOfContent);

                            dataRuin.Add(t);
                            
                            index += (firstFourBytes + lastFourBytes + 1);
                        }
                    }
                }
                
            }
            AttributeOffset += (UInt16)AttributeSize;
        }
     

        if(status == 0x01)
        {
            File result = new File();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, dataRuin, IsNon_Resident, content);
            return result;
        }
        else
        {
            Directory result = new Directory();
            result.CloneData(filename, FileSize, EntryID, (UInt32)RootID, Creationtime, Modifiedtime, dataRuin, IsNon_Resident,  content);
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

