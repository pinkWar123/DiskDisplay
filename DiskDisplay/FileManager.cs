using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class FileManager
{
    public bool IsDelete;
    public bool IsFile;
    public bool IsFAT32;

    public UInt32 FileSize;
    public string MainName;
    public DateTime Creationdatetime;
    public UInt16 StartCluster;

    public UInt32 ID;
    public UInt32 RootID;
    public DateTime modifieddate;
    public UInt32 NumberOfContigousClusterOfContent;
    public string content_President;
    public bool IsNon_Resident;

    public List<FileManager> Children;

    // Properties for UI
    protected TreeNode CurrentNode = new TreeNode();
    protected ListViewItem CurrentItem = new ListViewItem();
    public FileManager() { }


    public virtual int GetSize() { return 0; }

    // 
    virtual public void CloneData(byte[] data)
    {
        MainName = Encoding.ASCII.GetString(data, 0x00, 8);
        Creationdatetime = ConvertToDateTime(data);
        StartCluster = BitConverter.ToUInt16(data, 0x1A);
        FileSize = BitConverter.ToUInt32(data, 0x1C);

        IsFAT32 = true;
        IsDelete = false;
    }

    virtual public void CloneData(string filename, UInt32 FileSize, UInt32 ID, UInt32 RootID, DateTime CreationDate,
        DateTime ModifiedDate, UInt32 StartingCluster, UInt32 ContigousCluster, byte Isnon_Resident, string content)
    {
        this.MainName = filename;
        this.FileSize = FileSize;
        this.ID = ID;
        this.RootID = RootID;
        this.Creationdatetime = CreationDate;
        this.modifieddate = ModifiedDate;
        this.StartCluster = (UInt16)StartingCluster;
        this.NumberOfContigousClusterOfContent = ContigousCluster;
        this.content_President = content;
        this.IsNon_Resident = (Isnon_Resident == 0x01) ? true : false;

        IsFAT32 = false;
        IsDelete = false;
    }


    public virtual void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
            Console.Write("\t");
        Console.WriteLine("**" + MainName + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
    }

    virtual public bool FindFather(FileManager temp)
    {
        return false;
    }

    private static DateTime ConvertToDateTime(byte[] data)
    {
        int timeOffset = 0x0D;
        int dateOffset = 0x10;
        uint time = BitConverter.ToUInt32(data, timeOffset);
        ushort date = BitConverter.ToUInt16(data, dateOffset);

        int hour = (int)((time & 0xF80000) >> 19);
        int minute = (int)((time & 0x07E000) >> 13);
        int second = (int)((time & 0x001F00) >> 8) * 2;
        int millisecond = (int)(time & 0x0000FF);

        int year = ((date & 0xFE00) >> 9) + 1980;
        int month = (date & 0x01E0) >> 5;
        int day = date & 0x001F;


        return new DateTime(year, month, day, hour, minute, second, millisecond);
    }

    //Virtual Methods for UI
    public virtual void Populate() { }
    public virtual void PopulateListView(ref ListView ListView) { }
    public void SetNode(TreeNode node)
    {
        CurrentNode = node;
    }
    public TreeNode GetNode()
    {
        return CurrentNode;
    }
    public ListViewItem GetListViewItem()
    {
        return CurrentItem;
    }

}
//class FATFileManager : FileManager
//{
//    public FATFileManager() { }

    

    
//}

//class NTFSFileManager : FileManager
//{
    

//    public NTFSFileManager() { }
    
    
    
    
//}