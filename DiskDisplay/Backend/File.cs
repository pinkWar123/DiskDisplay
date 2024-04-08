using System;
using System.Collections.Generic;
using System.Text;

class File : FileManager
{
    public File() {
        //Parent = new FileManager();
    }
    public override int GetSize() { return (int)FileSize; }

    public override void CloneData(byte[] data)
    {
        base.CloneData(data);
        ExtendedName = Encoding.ASCII.GetString(data, 0x08, 3);
        IsFile = true;
    }

    public override void CloneData(string filename, uint FileSize, uint ID, uint RootID, DateTime CreationDate, DateTime ModifiedDate, List<Tuple<UInt32, UInt32>> dataruin, byte Isnon_Resident, string content)
    {
        base.CloneData(filename, FileSize, ID, RootID, CreationDate, ModifiedDate, dataruin, Isnon_Resident, content);
        IsFile = true;
    }
    public override void PrintImfomations(int level)
    {
        base.PrintImfomations(level);
    }
    public override void Populate()
    {
        CurrentNode.ImageKey = "fileIcon";
        CurrentNode.SelectedImageKey = "fileIcon";
        CurrentNode.Tag = this;
        if (CurrentNode.Text == "")
            CurrentNode.Text = MainName;

        CurrentItem.Tag = this;
        CurrentItem.ImageIndex = 1;
        CurrentItem.Text = MainName;
        CurrentItem.SubItems.Add("File");
        CurrentItem.SubItems.Add(GetSize().ToString());
        CurrentItem.SubItems.Add(Creationdatetime.ToString() == "1/1/2001 12:00:00 AM" ? "" : Creationdatetime.ToString());
    }

    public override bool FindFather(FileManager temp)
    {
        return base.FindFather(temp);
    }

    ~File() { }
}

