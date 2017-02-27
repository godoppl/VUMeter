using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Line_In_VU_Meter
{
    public partial class Settings_Form : Form
    {
        Devicelist devicelist = new Devicelist();
        bool cb2fix = false;
        bool cb3fix = false;
        public event EventHandler ForceUpdate;


        public Settings_Form()
        {
            InitializeComponent();
            loadSettings(); // Load the settings from Settings.settings
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.savePos = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.recordOnly = checkBox2.Checked;
            Properties.Settings.Default.Save();
            comboBox1.DataSource = devicelist.devices();
            comboBox1.SelectedIndex = Properties.Settings.Default.selectedDevice;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.clickThrough = checkBox3.Checked;
            updateTransp(EventArgs.Empty);
            Properties.Settings.Default.Save();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.selectedDevice = comboBox1.SelectedIndex;
            Properties.Settings.Default.recordOnly = checkBox2.Checked;
            Properties.Settings.Default.Save();
            devicelist.updateDevice();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb2fix == true)
            {
                Properties.Settings.Default.multiplier = comboBox2.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            else {cb2fix = true;}
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb3fix == true)
            {
                Properties.Settings.Default.transparancy = comboBox3.SelectedIndex;
                updateTransp(EventArgs.Empty);
                Properties.Settings.Default.Save();
            }
            else { cb3fix = true; }
        }
        
        private void loadSettings()
        {
            //System.Threading.Thread.Sleep(1000);

            try
            {
                comboBox1.DataSource = devicelist.devices();
                this.comboBox1.SelectedIndex = Properties.Settings.Default.selectedDevice;
                this.comboBox2.SelectedIndex = Properties.Settings.Default.multiplier;
                this.comboBox3.SelectedIndex = Properties.Settings.Default.transparancy;
            }

            catch
            {
                MessageBox.Show("Something happened");
                this.Close();
            }
        }

        protected virtual void updateTransp(EventArgs e)
        {
            //make sure we have someone subscribed to our event before we try to raise it
            if (this.ForceUpdate != null)
            {
                this.ForceUpdate(this, e);
            }
        }

        private void focusEvent(object sender, EventArgs e)
        {
            this.checkBox3.Checked = global::Line_In_VU_Meter.Properties.Settings.Default.clickThrough;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Location = Properties.Settings.Default.tempPos;
            Properties.Settings.Default.Save();
        }

        private void Settings_Form_Load(object sender, EventArgs e)
        {

        }
    }
}
