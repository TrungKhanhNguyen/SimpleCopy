using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace SimpleCopy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public Timer timer1;
        public List<CPObj> listObj = new List<CPObj> ();

        public List<string> listLogs = new List<string>();
        

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnClear.PerformClick();
            txtTimer.Text = "5";
            timer1 = new Timer();
            timer1.Interval = Convert.ToInt32(txtTimer.Text) * 60000;
            timer1.Tick += new System.EventHandler(OnTimerEvent);

            var configfileName = "configs.txt";
            string[] lines = File.ReadAllLines(configfileName);
            foreach(var line in lines)
            {
                var tempObj = new CPObj();
                var allText = line.Split(';');
                tempObj.Channel = allText[0];
                tempObj.SourceChannel = allText[1];
                tempObj.DestinationChannel = allText[2];
                tempObj.Running = Convert.ToBoolean(allText[3]);
                listObj.Add(tempObj);
            }
            dataGridView1.DataSource = listObj;
        }

        private async void OnTimerEvent(object sender, EventArgs e)
        {
            var listRunning = listObj.Where(m => m.Running == true).ToList();
            var listTask = new List<Task>();
            foreach (var item in listRunning)
            {
                listTask.Add(Task.Run(() => PerformCopy(item.SourceChannel, item.DestinationChannel)));
            }
            await Task.WhenAll(listTask);
            WriteLog();
            UpdateLogsText("Waiting for new cycle...");
        }

        private void btnSetTimer_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("You must stop program to apply changes", "Confirmation", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes)
            {
                btnStop.PerformClick();
                timer1.Interval = Convert.ToInt32(txtTimer.Text) * 60000;
            }
            else if (result == DialogResult.No)
            {
                //...
            }
           
        }

       

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            chkRunning.Checked = false;
            txtChannelEdit.Text = "";
            txtSourceChannel.Text = "";
            txtDestinationChannel.Text = "";
            dataGridView1.ClearSelection();
        }
        

        private void PerformCopy(string source, string des)
        {
                try
                {
                    var tempDes = new DirectoryInfo(des).Name  + "-" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm");
                    Directory.CreateDirectory(tempDes);
                    var sourceInfo = new DirectoryInfo(source);
                    var desInfo = new DirectoryInfo(tempDes);

                    foreach (FileInfo fi in sourceInfo.GetFiles())
                    {
                        fi.MoveTo(Path.Combine(desInfo.FullName, fi.Name));
                    
                    }
                    var msg = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss: ") + "DONE copy from " + source + " to " + des;
                    listLogs.Add(msg);
                    UpdateLogsText(msg);
                }
                catch (Exception ex)
                {
                listLogs.Add(DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss: ERROR ") + ex.Message);
                UpdateLogsText(DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss: ERROR ") + ex.Message );
                }
            
        }

        private void UpdateLogsText(string msg)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                txtLogs.Text += Environment.NewLine+ msg;
            });
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Start();
            btnStart.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            btnStop.Enabled = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var tempitem = listObj.Where(m => m.Channel == txtChannelEdit.Text).FirstOrDefault();
            tempitem.Running = chkRunning.Checked;
            tempitem.SourceChannel = txtSourceChannel.Text.ToString();
            tempitem.DestinationChannel = txtDestinationChannel.Text.ToString();

            dataGridView1.DataSource = listObj;
            dataGridView1.Refresh();
            dataGridView1.Update();
            UpdatetoTextFile();
        }

       
        private void WriteLog()
        {
            var path = Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "logs");
            var fullPath = Path.Combine(path, "CP-" + DateTime.Now.ToString("dd-MM-yyyy-HH") + ".txt");
            using (var file = File.Open(fullPath, FileMode.OpenOrCreate))
            {
                file.Seek(0, SeekOrigin.End);
                using (var stream = new StreamWriter(file))
                {
                    foreach (var item in listLogs)
                    {
                        stream.WriteLine(item);
                    }
                }
            }

        }

        private void UpdatetoTextFile()
        {
            File.Create("configs.txt").Close();
            using (StreamWriter w = File.AppendText("configs.txt"))
            {
                foreach(var item in listObj)
                {
                    var tempText = item.Channel + ";" + item.SourceChannel + ";" + item.DestinationChannel + ";" + item.Running;
                    w.WriteLine(tempText);
                }
                
            }
        }

        private void btnStart_EnabledChanged(object sender, EventArgs e)
        {
            btnStop.Enabled = !btnStart.Enabled;
        }

        private void btnStop_EnabledChanged(object sender, EventArgs e)
        {
            btnStart.Enabled = !btnStop.Enabled;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                var channelName = row.Cells["Channel"].Value.ToString();
                var sourceChannel = row.Cells["SourceChannel"].Value.ToString();
                var destinationChannel = row.Cells["DestinationChannel"].Value.ToString();
                var running = Convert.ToBoolean(row.Cells["Running"].Value);

                txtChannelEdit.Text = channelName;
                txtSourceChannel.Text = sourceChannel;
                txtDestinationChannel.Text = destinationChannel;
                chkRunning.Checked = running;

            }
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
                this.WindowState = FormWindowState.Maximized;

        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to quit???", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
            {
                //...
            }
            
        }
    }
}
