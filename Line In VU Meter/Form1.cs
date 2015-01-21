using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.CoreAudioApi;

namespace Line_In_VU_Meter
{
    public partial class Form1 : Form
    {
        static MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
        MMDevice device;
        string[,] audioDevices = new string[255,2];
        int i;
        

        public Form1()
        {
            InitializeComponent();
            enumerateDevices(); //Enumerate the Audio Devices on the system
            updateDevice(); //Refresh the comboBox list

            try
            {
                checkBox1.Checked = Properties.Settings.Default.displayConfig;
                checkBox2.Checked = Properties.Settings.Default.recordOnly;
                comboBox1.SelectedIndex = Properties.Settings.Default.selectedDevice;
            }
            catch
            {

            }

            if (Properties.Settings.Default.displayConfig == true)
            {
                this.Height = this.MaximumSize.Height;
            }

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (device.AudioEndpointVolume.Mute.Equals(true))
            {
                panel1.Visible = true;
            }
            else
            {
                panel1.Visible = false;
                try
                {
                    float volume = (float)device.AudioMeterInformation.MasterPeakValue * 200;
                    progressBar1.Value = (int)volume;
                }
                catch 
                {
                    progressBar1.Value = 100;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true) {
                Properties.Settings.Default.displayConfig = true;
                Properties.Settings.Default.Save();
                this.Height = this.MaximumSize.Height;
            }
            else
            {
                Properties.Settings.Default.displayConfig = false;
                Properties.Settings.Default.Save();
                this.Height = this.MinimumSize.Height;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.selectedDevice = comboBox1.SelectedIndex;
            Properties.Settings.Default.recordOnly = checkBox2.Checked;
            Properties.Settings.Default.Save();
            updateDevice();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.recordOnly = checkBox2.Checked;
            enumerateDevices();
        }

        public void enumerateDevices()
        {
            comboBox1.Items.Clear();
            i = 0;

            if (Properties.Settings.Default.recordOnly == false)
            {

                foreach (MMDevice devices in deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                {
                    audioDevices[i, 0] = devices.FriendlyName;
                    audioDevices[i, 1] = devices.ID;
                    comboBox1.Items.Add(audioDevices[i, 0].ToString());
                    i++;
                }
            }
            else
            {
                foreach (MMDevice devices in deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    audioDevices[i, 0] = devices.FriendlyName;
                    audioDevices[i, 1] = devices.ID;
                    comboBox1.Items.Add(audioDevices[i, 0].ToString());
                    i++;
                }
            }
            
        }
        public void updateDevice()
        {
            try
            {
                device = deviceEnum.GetDevice(audioDevices[Properties.Settings.Default.selectedDevice, 1]);
                this.Text = device.FriendlyName;
            }
            catch
            {
                Properties.Settings.Default.selectedDevice = 0;
                if (audioDevices[0, 0] == null)
                {
                    MessageBox.Show("There are no devices to list!");
                    this.Close();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

    }
}
