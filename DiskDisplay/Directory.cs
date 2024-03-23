using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

class FATDirectory : FATFileManager
{
    public List<FileManager> Children;
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
    public override void Populate()
    {
        CurrentItem.Tag = this;
        CurrentItem.Text = MainName;
        CurrentNode.ImageKey = "folderIcon";
        CurrentNode.SelectedImageKey = "folderIcon";
        if (Children.Count() == 0) return;
        if (CurrentNode.Text == "")
            CurrentNode.Text = MainName;
        foreach (var child in Children)
        {
            TreeNode node = new TreeNode();
            child.SetNode(node);
            child.Populate();
            CurrentNode.Nodes.Add(node);
        }
        CurrentItem.SubItems.Add("Folder");
        CurrentItem.ImageIndex = 0;
        CurrentItem.SubItems.Add(GetSize().ToString());
        CurrentItem.SubItems.Add(Creationdatetime.ToString());
    }
}