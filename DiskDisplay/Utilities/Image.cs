using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DiskDisplay.NewFolder1
{
    internal class Image1
    {
        public static ImageList ImageList = new ImageList();
        

        public static void LoadImageList()
        {
            string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../Assets");

            
            // Set the current working directory to the directory containing the image file
            System.IO.Directory.SetCurrentDirectory(imageDirectory);

            // Load the image using the relative path
            ImageList.Images.Add("folderIcon", Image.FromFile("folder.png"));
            ImageList.Images.Add("fileIcon", Image.FromFile("file.jpg"));
            ImageList.Images.Add("recycleBinIcon", Image.FromFile("recycle-bin.jpg"));
        }
    }
}
