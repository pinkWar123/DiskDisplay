using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

//class FATFile : FATFileManager
//{
//    public string ExtendedName;
//    public FATFile() { }

//    ~FATFile() { }
//}

//class NTFSFile : NTFSFileManager
//{
//    public NTFSFile() { }
//}

class File : FileManager
{
    public string ExtendedName;
    public File() {
        ExtendedName = "";
    }
    public override int GetSize() { return (int)FileSize; }

    public override void CloneData(byte[] data)
    {
        base.CloneData(data);
        ExtendedName = Encoding.ASCII.GetString(data, 0x08, 3);
        IsFile = true;
    }

    public override void CloneData(string filename, uint FileSize, uint ID, uint RootID, DateTime CreationDate, DateTime ModifiedDate, UInt32 StartingCluster, UInt32 ContigousCluster, byte Isnon_Resident, string content)
    {
        base.CloneData(filename, FileSize, ID, RootID, CreationDate, ModifiedDate, StartingCluster, ContigousCluster, Isnon_Resident, content);
        IsFile = true;
    }
    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
            Console.Write("\t");
        Console.WriteLine("**" + MainName  + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
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
        CurrentItem.SubItems.Add(Creationdatetime.ToString());

    }

    public override bool FindFather(FileManager temp)
    {
        return base.FindFather(temp);
    }

    ~File() { }
}

