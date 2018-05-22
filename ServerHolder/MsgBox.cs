#region using

using System.Windows.Forms;

#endregion

namespace ServerHolder
{
    public partial class MsgBox : Form
    {
        public MsgBox()
        {
            InitializeComponent();
        }

        public static void Show(string content, string title)
        {
            var box = new MsgBox();
            box.textBox.Text = content;
            box.Text = title;
            box.Show();
        }
    }
}