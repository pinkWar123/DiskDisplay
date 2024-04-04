using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileSystem
{
    public UInt32 FirstSector;
    public string DiskName;
    public string DriveName;
    public bool IsFAT32Type;
    public static List<FileManager> RecycleBin = new List<FileManager>();
    public FileSystem() { }
    public FileSystem(string name)
    {
        this.DriveName = name;
    }
    static public FileSystem Detect_FileSystem(string name)
    {

        return null;
    }
    // read drive and return list of file
    virtual public List<FileManager> ReadFileSystem()
    {
        return null;
    }

    // read text file and return it
    virtual public string ReadData(FileManager file)
    {
        return null;
    }

    //Delete file 
    virtual public bool DeleteFile(FileManager file)
    {
        return true;
    }

    //Restore file
    virtual public bool RestoreFile(FileManager file)
    {
        return true;
    }
}
