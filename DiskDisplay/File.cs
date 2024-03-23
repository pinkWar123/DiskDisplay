using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class FATFile : FATFileManager
{
    public string ExtendedName;
    public FATFile() { }

    public override void CloneData(byte[] data)
    {
        base.CloneData(data);
        ExtendedName = Encoding.ASCII.GetString(data, 0x08, 3);
        IsDelete = false;
        IsFile = true;
        IsFAT32 = true;
    }
    public override void PrintImfomations(int level)
    {
        for (int i = 0; i < level; i++)
            Console.Write("\t");
        Console.WriteLine("**" + MainName + "." + ExtendedName + "--" + FileSize + "--" + Creationdatetime.Day + "/" + Creationdatetime.Month + "/" + Creationdatetime.Year + "-" + Creationdatetime.Hour + ":" + Creationdatetime.Minute + ":" + Creationdatetime.Second);
    }

    public override int GetSize() { return (int)FileSize; }

    ~FATFile() { }


    // Methods for UI
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
}