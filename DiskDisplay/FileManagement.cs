using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiskDisplay
{
    public class IFileManagement
    {
        protected IFileManagement Parent = null;
        protected TreeNode CurrentNode = new TreeNode();
        protected ListViewItem CurrentItem = new ListViewItem();
        protected string Name { get; set; }
        public IFileManagement(string name)
        {
            Name = name;
        }
        public ListViewItem GetListViewItem()
        {
            return CurrentItem;
        }
        public void SetNode(TreeNode node)
        {
            CurrentNode = node;
        }

        public TreeNode GetNode()
        {
            return CurrentNode;
        }
        virtual public string GetFullName()
        {
            return Name;
        }
        public virtual void Populate() { }

        public virtual void PopulateListView(ListView ListView) { }
    }

    class File : IFileManagement
    {
        private string Content { get; set; }
        private string Extension { get; set; }
        public File(string name, string extension)
            : base(name)
        {
            Extension = extension;
        }
        public string GetContent() { return Content; }
        public File(string name, string extension, string content)
            : base(name)
        {
            Extension = extension;
            Content = content;
        }
        public override string GetFullName() 
        {
            return base.GetFullName() + "." + Extension;
        }
        public override void Populate()
        {
            CurrentNode.ImageKey = "fileIcon";
            CurrentNode.SelectedImageKey = "fileIcon";
            CurrentNode.Tag = this;
            CurrentItem.Tag = this;
            CurrentItem.Text = GetFullName();
        }

    }

    class Folder : IFileManagement
    {
        //private TreeNode CurrentNode = new TreeNode();
        public Folder(string name) : base(name) { }
        public Folder(string name, TreeNode node) : base(name) 
        {
            CurrentNode = node;
        }

        public Folder(string name, List<IFileManagement> NewChildren)
            : base(name)
        {
            Children = NewChildren;
        }
        private List<IFileManagement> Children = new List<IFileManagement>();
        
        public override void Populate()
        {
            CurrentItem.Tag = this;
            CurrentItem.Text = GetFullName();
            CurrentNode.ImageKey = "folderIcon";
            CurrentNode.SelectedImageKey = "folderIcon";
            if (Children.Count() == 0) return;
            foreach (var child in Children)
            {
                TreeNode node = new TreeNode(child.GetFullName());
                
                child.SetNode(node);
                child.Populate();
                CurrentNode.Nodes.Add(node);
            }
        }

        public override void PopulateListView(ListView ListView)
        {
            base.PopulateListView(ListView);
            ListView.Items.Clear();
            foreach (var child in Children)
            {
                ListViewItem item = new ListViewItem(child.GetFullName());
                ListView.Items.Add(child.GetListViewItem());
            }
        }

        public void AddNewFile(string FileName, string Extension)
        {
            var NewFile = new File(FileName, Extension);
            Children.Add(NewFile);
        }

        public void AddNewFolder(string FolderName)
        {
            var NewFolder = new Folder(FolderName);
            Children.Add(NewFolder);
        }

        public void AddNewFolder(Folder folder)
        {
            Children.Add(folder);
        }

        
    }

    class Disk
    {
        private List<Folder> Root { get; set; }

    }
}
