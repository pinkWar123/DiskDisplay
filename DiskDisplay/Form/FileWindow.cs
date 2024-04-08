using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DiskDisplay
{
    public partial class FileWindow : Form
    {
        public FileWindow()
        {
            InitializeComponent();

            fileTextBox.Font = new Font("Arial", 12); // Set font size to 12 points
            fileTextBox.ForeColor = Color.Blue; // Set text color to blue
            fileTextBox.BackColor = Color.LightGray; // Set background color to light gray
            fileTextBox.ReadOnly = true; // Make the text read-only
        }

        public void ShowFileContent(string Content)
        {
            fileTextBox.Text = Content;
            this.ShowDialog();
        }
    }
}
