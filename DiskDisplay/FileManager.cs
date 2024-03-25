using System;
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

    public virtual int GetSize() { return 0; }

    // Properties for UI
    protected TreeNode CurrentNode = new TreeNode();
    protected ListViewItem CurrentItem = new ListViewItem();
    public FileManager() { }
    virtual public void PrintImfomations(int level) { }

    // Methods for UI
    public virtual void Populate() { }

    public virtual void PopulateListView(ListView ListView) { }

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
class FATFileManager : FileManager
{
    public UInt16 StartCluster;
    public FATFileManager() { }

    virtual public void CloneData(byte[] data)
    {
        MainName = Encoding.ASCII.GetString(data, 0x00, 8);
        Creationdatetime = ConvertToDateTime(data);
        StartCluster = BitConverter.ToUInt16(data, 0x1A);
        FileSize = BitConverter.ToUInt32(data, 0x1C);
    }

    public static DateTime ConvertToDateTime(byte[] data)
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
}

class NTFSFileManager : FileManager
{
    public UInt32 ID;
    public DateTime modifieddate;
    public UInt32 RootID;

    public NTFSFileManager() { }
    virtual public void CloneData(string filename, UInt32 FileSize,UInt32 ID, UInt32 RootID, DateTime CreationDate, DateTime ModifiedDate )
    {
        this.MainName = filename;
        this.FileSize = FileSize;
        this.ID = ID;
        this.RootID = RootID;
        this.Creationdatetime = CreationDate;
        this.modifieddate = ModifiedDate;

        IsFAT32 = false;
        IsDelete = false;
    }
    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
            Console.Write("\t");
        Console.WriteLine("**" + MainName + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
    }
    virtual public bool FindFather(NTFSFileManager temp)
    {
        return false;
    }
    
}