#region using

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

#endregion

namespace ServerHolder
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Color HeadColor
        {
            set { groupBox1.BackColor = value; }
        }

        public string Info
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var row = dataGridView1.SelectedRows[0];
            var info = (ServerInfo) row.DataBoundItem;
            var status = info.Status;
            var start = status.IndexOf('\n') + 1;
            var count = status.LastIndexOf('\n') - start - 1;
            status = status.Substring(start, count);
            MsgBox.Show(status, "Status");
        }

        private void dumpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            foreach (var info in Program.ServerInfos)
            {
                sb.AppendLine(info.GetStackTrace());
            }

            File.WriteAllText("../status/stack.txt", sb.ToString());
            File.Delete("stack");
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Stop();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Program.Start();
        }

        public void SetDataBinding(object data)
        {
            dataGridView1.DataSource = data;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
            {
                var data = selectedRow.DataBoundItem as ServerInfo;
                data.Start();
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows)
            {
                var data = selectedRow.DataBoundItem as ServerInfo;
                data.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataGridView1.Refresh();
        }
    }
}