using DiskDisplay.NewFolder1;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


class Directory : FileManager
{

    public Directory()
    {
        Parent = new FileManager();
    }

    // Overloading Clone function for FAT32 file
    public override void CloneData(byte[] data)
    {
        base.CloneData(data);
        IsFile = false;
    }

    //Overloading Clone funct
    public override void CloneData(string filename, uint FileSize, uint ID, uint RootID, DateTime CreationDate, DateTime ModifiedDate, List<Tuple<UInt32, UInt32>> dataruin, byte Isnon_Resident, string content)
    {
        base.CloneData(filename, FileSize, ID, RootID, CreationDate, ModifiedDate,dataruin, Isnon_Resident, content);
        IsFile = false;
    }
    public override int GetSize()
    {
        var totalSize = 0;
        if(Children != null && Children.Count > 0)
        foreach (var child in Children)
        {
            totalSize += child.GetSize();
        }
        return totalSize;
    }

    public override bool FindFather(FileManager temp)
    {
        if (this.ID == temp.RootID)
        {
            this.Children.Add(temp);
            return true;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            FileManager tempfile = (FileManager)Children[i];
            if (tempfile.FindFather(temp) == true)
                return true;
        }
        return false;
    }

    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
        {
            Console.Write("\t");
        }
        Console.WriteLine(ID + "--" + RootID + "--" + MainName + "--" + GetSize() + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].PrintImfomations(level + 1);
        }
    }

    public override void Populate()
    {
        /*CurrentNode.Nodes.Clear();
        CurrentItem.SubItems.Clear();*/
        string imageKey;
        bool isRecycleBin = this.MainName == "Recycle Bin";
        if(isRecycleBin)
        {
            imageKey = Image1.recycleBinIconKey;
        }
        else
        {
            imageKey = Image1.folderIconKey;
        }
        CurrentNode.ImageKey = imageKey;
        CurrentNode.SelectedImageKey = imageKey;
        CurrentNode.Tag = this;
        CurrentNode.Text = MainName;
        if (Children != null)
        {
            foreach (var child in Children)
            {
                TreeNode node = new TreeNode();
                child.SetNode(node);
                child.Populate();
                child.SetPath(this.Path + "/" + child.MainName);
                child.SetParent(this);
                CurrentNode.Nodes.Add(node);
            }
        }
        CurrentItem.Text = MainName;
        CurrentItem.Tag = this;
        CurrentItem.SubItems.Add("folder");
        CurrentItem.ImageIndex = isRecycleBin ? 2 : 0;
        CurrentItem.SubItems.Add(GetSize().ToString());
        CurrentItem.SubItems.Add(Creationdatetime.ToString() == "1/1/0001 12:00:00 AM" ? "" : Creationdatetime.ToString());

    }
}

