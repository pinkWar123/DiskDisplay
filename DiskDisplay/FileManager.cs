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
    public string ExtendedName = "";
    public DateTime Creationdatetime;
    public UInt16 StartCluster;

    public UInt32 ID;
    public UInt32 RootID;
    public DateTime modifieddate;
    public UInt32 NumberOfContigousClusterOfContent;
    public List<Tuple<UInt32, UInt32>> ListDataruin;
    public string content_President;
    public bool IsNon_Resident;

    public FileManager Parent;

    public List<FileManager> Children = new List<FileManager>();

    // Properties for UI
    protected TreeNode CurrentNode = new TreeNode();
    protected ListViewItem CurrentItem = new ListViewItem();
    protected bool isRecycleBin = false;
    protected bool isVisible = true;
    protected string Path = "";
    public FileManager() {
        
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
    }

    public bool GetVisible()
    {
        return isVisible;
    }

    public void SetPath(string path)
    {
        this.Path = path;
    }

    public string GetPath()
    {
        return this.Path;
    }

    public void SetNodeText(string text)
    {
        CurrentNode.Text = text;
    }

    public void SetItemText(string text)
    {
        CurrentItem.Text = text;
    }

    public void SetParent(FileManager Parent)
    {
        this.Parent = Parent;
    }

    public FileManager GetParent()
    {
        return Parent;
    }

    public bool IsRecycleBin()
    {
        return this.isRecycleBin;
    }

    public void SetRecycleBin(bool value)
    {
        isRecycleBin = value;
    }

    public void SetInvisible()
    {
        CurrentNode = null;
        CurrentItem = null;
    }

    public void SetIcon(string icon, int imgIdx)
    {
        CurrentNode.ImageKey = icon;
        CurrentNode.SelectedImageKey = icon;
        CurrentItem.ImageKey = icon;
        CurrentItem.ImageIndex = imgIdx;
    }
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
        if (data[0x00] == 0xE5) isRecycleBin = true;
    }

    virtual public void CloneData(string filename, UInt32 FileSize, UInt32 ID, UInt32 RootID, DateTime CreationDate,
        DateTime ModifiedDate,List<Tuple<UInt32, UInt32>> dataruin, byte Isnon_Resident, string content)
    {
        this.MainName = filename;
        this.FileSize = FileSize;
        this.ID = ID;
        this.RootID = RootID;
        this.Creationdatetime = CreationDate;
        this.modifieddate = ModifiedDate;
        if(dataruin.Count > 0)
            this.StartCluster = (UInt16)dataruin[0].Item2;
        this.ListDataruin = dataruin;
        this.content_President = content;
        this.IsNon_Resident = (Isnon_Resident == 0x01) ? true : false;

        IsFAT32 = false;
        IsDelete = false;
    }


    public virtual void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
            Console.Write("\t");
        Console.WriteLine("**" + ID + "--" + RootID + "--" + MainName + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
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