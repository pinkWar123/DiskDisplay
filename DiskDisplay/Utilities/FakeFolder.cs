using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskDisplay
{
    class Structure
    {
        public string type;
        public string name;
        public string extension;
        public Structure[] children;

        public Structure(string type, string name, Structure[] children)
        {
            this.type = type;
            this.name = name;
            this.children = children;
        }

        public Structure(string type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public Structure(string type, string name, string extension)
        {
            this.type = type;
            this.name = name;
            this.extension = extension;
        }
    }
    internal class FakeFolder
    {
        
        public static Folder GetFakeFolder()
        {
            var structures = new Structure[]
            {
                new Structure("Folder","Root", new Structure[]
                        {
                            new Structure("Folder","SubFolder1", new Structure[]
                            {
                                new Structure("File","File2",".txt"),
                                new Structure("File","File3",".txt")
                            }),
                            new Structure("File","File1",".txt")
                        }
                ),
                new Structure("Folder1", "Root1", new Structure[]
                {
                    new Structure("File", "File4", ".txt"),
                    new Structure("File", "File5", ".txt")
                })
            };

            var folders = new Folder("Root", new List<IFileManagement>()
            {
                new File("a", ".txt", "This is content of a.txt"),
                new Folder("SubFolder1", new List<IFileManagement>()
                {
                    new File("a1", ".png", "This is content of a1.png"),
                    new Folder("SubFolder2", new List<IFileManagement>()
                    {
                        new File("a2",".txt", "This is content of a2.txt"),
                        new File("a3", ".txt", "This is content of a3.txt")
                    })
                })
            });
            return folders;
        }

        
    }
}
