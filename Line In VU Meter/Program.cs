using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.CoreAudioApi;

namespace Line_In_VU_Meter
{
    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

    public class Devicelist
    {
        static MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
        static MMDevice device;
        string[,] audioDevices = new string[255, 2];
        static int[] multiplierValues = new int[5] { 100, 150, 200, 250, 300 };

        public bool muted()
        {
            try
            {
                if (device.AudioEndpointVolume.Mute == true) { return true; }
                else { return false; }
            }
            catch { return false; }
        }

        public void muteToggle()
        {
            device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
        }

        public int returnVolume()
        {
            float volume = (float)device.AudioMeterInformation.MasterPeakValue * multiplierValues[Properties.Settings.Default.multiplier];

            return (int)volume;
        }

        public void updateDevice()
        {
            try
            {
                device = deviceEnum.GetDevice(audioDevices[Properties.Settings.Default.selectedDevice, 1]);
            }
            catch
            {
                Properties.Settings.Default.selectedDevice = 0;
                if (audioDevices[0, 0] == null)
                {
                    MessageBox.Show("There are no audio devices connected!");
                }
            }
        }

        public void enumerateDevices()
        {
            int i = 0;
            Array.Clear(audioDevices, 0, 255);
            if (Properties.Settings.Default.recordOnly == false)
            {
                foreach (MMDevice devices in deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                {
                    audioDevices[i, 0] = devices.FriendlyName;
                    audioDevices[i, 1] = devices.ID;
                    i++;
                }
            }
            else
            {
                foreach (MMDevice devices in deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    audioDevices[i, 0] = devices.FriendlyName;
                    audioDevices[i, 1] = devices.ID;
                    i++;
                }
            }
            updateDevice();
        }

        public string[] devices()
        {
            string[] _devices = new string[255];
            enumerateDevices();
            string[,] enumdevices = audioDevices;

            for (int i = 0; i < enumdevices.Length / 2; i++)
            {
                _devices[i] = enumdevices[i, 0];
            }
            return _devices;

        }

        static class Program
        {
            /// <summary>
            /// The main entry point for the application.
            /// </summary>
            [MTAThread]
            static void Main()
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new VU_Meter());
            }
        }
    }
}
