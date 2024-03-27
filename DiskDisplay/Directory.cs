using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DiskDisplay;
using System.Collections;


class FATDirectory : FATFileManager
{
    
    public FATDirectory() { }
    
    public override void CloneData(byte[] data)
    {
        base.CloneData(data);
        IsDelete = false;
        IsFile = false;
        IsFAT32 = true;
        Children = new List<FileManager>();
    }

    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
        {
            Console.Write("\t");
        }
        Console.WriteLine(MainName + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].PrintImfomations(level + 1);
        }
    }

    public override int GetSize() 
    {
        var totalSize = 0;
        foreach (var child in Children)
        {
            totalSize += child.GetSize();
        }
        return totalSize;
    }

    // Methods for UI
    
    public override void PopulateListView(ref ListView ListView)
    {
        base.PopulateListView(ref ListView);
        if (FileListView.IsLastDirectory())
        {
            ++FileListView.CurrentHistoryIndex;
            Console.Write("a");
            FileListView.History.Add(this);
        }
        else
        {
            if (this == FileListView.History[FileListView.CurrentHistoryIndex + 1])
            {
                ++FileListView.CurrentHistoryIndex;
            }
            else
            {
                int startIndex = FileListView.CurrentHistoryIndex + 1;
                int count = FileListView.History.Count - startIndex;
                FileListView.History.RemoveRange(startIndex, count);
                FileListView.History.Add(this);
            }
        }
        //FileListView.RenderListView(ref ListView);
    }
}

class NTFSDirectory : NTFSFileManager
{
    
    public NTFSDirectory() {
        Children = new List<FileManager>();
    }


    public override void CloneData(string filename, uint FileSize, uint ID, uint RootID, DateTime CreationDate, DateTime ModifiedDate, UInt32 StartingCluster, UInt32 ContigousCluster, byte Isnon_Resident, string content)
    {
        base.CloneData(filename, FileSize, ID, RootID, CreationDate, ModifiedDate, StartingCluster, ContigousCluster, Isnon_Resident, content);
        IsFile = false;
    }
    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
        {
            Console.Write("\t");
        }
        Console.WriteLine(MainName + "--" + GetSize() + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].PrintImfomations(level + 1);
        }
    }

    public override bool FindFather(NTFSFileManager temp)
    {
        if(this.ID == temp.RootID)
        {
            this.Children.Add(temp);
            return true;
        }
        
        for(int i = 0;i < Children.Count; i++)
        {
            NTFSFileManager tempfile = (NTFSFileManager)Children[i];
            if (tempfile.FindFather(temp) == true)
                return true;
        }
        return false;

    }

    public override int GetSize()
    {
        var totalSize = 0;
        foreach (var child in Children)
        {
            totalSize += child.GetSize();
        }
        return totalSize;
    }
    

}

    
