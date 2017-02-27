using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace Line_In_VU_Meter
{
    public partial class VU_Meter : Form
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);
        
        [DllImport("user32.dll", EntryPoint = "RedrawWindow")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lpRectUpdate, IntPtr hrgnUpdate, UInt32 flags);

        public static IntPtr NULL = (IntPtr)0;
        private NotifyIcon trayIcon;
        private bool _dragging = false;
        private bool _resizing = false;
        private Point _start_point = new Point(0, 0);
        static double[] transparancyValues = new double[10] { 0.1D, 0.2D, 0.3D, 0.4D, 0.5D, 0.6D, 0.7D, 0.8D, 0.9D, 1.0D };
        Devicelist devicelist = new Devicelist();
        
        
        
        public enum GWL
        {
            ExStyle = -20
        }

        public enum WS_EX
        {
            Layered = 0x80000
        }
               

        public VU_Meter()
        {
            InitializeComponent();

            trayIcon = new NotifyIcon();
            trayIcon.Text = "VU Meter";
            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = contextMenuStrip1;
            trayIcon.MouseDoubleClick += new MouseEventHandler(trayIcon_DoubleClick);
            trayIcon.MouseClick += new MouseEventHandler(trayIcon_Click);
            trayIcon.Visible = true;
            devicelist.enumerateDevices();
            devicelist.updateDevice();
            timer1.Start();
            Height = Properties.Settings.Default.Height;
        }

        private void VU_Meter_Load(object sender, EventArgs e)
        {
            this.Opacity = transparancyValues[Properties.Settings.Default.transparancy];
            click_Trough(Properties.Settings.Default.clickThrough);
            this.Opacity = 0.1D;
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
        }

        private void VU_Meter_FormClosing(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.savePos == true)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Save();
            }
        }

        public void updateTransp(object sender, EventArgs e)
        {
            this.Opacity = transparancyValues[Properties.Settings.Default.transparancy];
            click_Trough(Properties.Settings.Default.clickThrough);
        }

        public void click_Trough(bool set)
        {
            if (set)
            {
                label1.Hide();
                label2.Hide();
                int wl = GetWindowLong(this.Handle, GWL.ExStyle);
                SetWindowLong(this.Handle, GWL.ExStyle, wl | 0x80000 | 0x20);
            }
            else
            {
                label1.Show();
                label2.Show();
                int wl = GetWindowLong(this.Handle, GWL.ExStyle);
                SetWindowLong(this.Handle, GWL.ExStyle, wl & ~0x20);
                RedrawWindow(this.Handle, NULL, NULL, 0x0004 | 0x0001 | 0x0400 | 0x0080);
            }
        }

        private void fade_Form(bool type)
        {
            double stepping = (transparancyValues[Properties.Settings.Default.transparancy] / 5);
            if (type)
            {
                System.Threading.Thread.Sleep(100);
                while (this.Opacity > 0.15D) { this.Opacity -= stepping; System.Threading.Thread.Sleep(100); }
                this.Opacity = 0.05D;
            }
            else
            {
                while (this.Opacity < transparancyValues[Properties.Settings.Default.transparancy]) { this.Opacity += stepping; System.Threading.Thread.Sleep(100); }
                this.Opacity = transparancyValues[Properties.Settings.Default.transparancy];
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.clickThrough == true && this.Bounds.Contains(Cursor.Position) == true) 
            {
                fade_Form(true); 
            }                
            else
            {
                fade_Form(false);
            }

            if (devicelist.muted() == true)
            {
                progressBar1.Visible = false;
            }
            else
            {
                progressBar1.Visible = true;
                try 
                {
                    int volume = devicelist.returnVolume(); 
                    progressBar1.Value = volume;
                }
                catch { progressBar1.Value = 100; }
            }
        }


        //
        // Context Menu
        //

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            clickThroughToolStripMenuItem.Checked = Properties.Settings.Default.clickThrough;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void savePositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Height = Height;
            Properties.Settings.Default.Save();
        }

        private void resetPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Location = _start_point;
        }
        
        private void clickThroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.clickThrough = !clickThroughToolStripMenuItem.Checked;
            updateTransp(sender, e);
            Properties.Settings.Default.Save();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label1_DoubleClick(sender, e);
        }





        private void label1_DoubleClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.tempPos = Location;
            Properties.Settings.Default.Save();
            if (Application.OpenForms["Settings_Form"] == null)
            {
                Settings_Form settings = new Settings_Form();
                settings.ForceUpdate += new EventHandler(updateTransp);
                settings.Show();
            }
            else { Application.OpenForms["Settings_Form"].Focus(); }
        }

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    contextMenuStrip1.Show(this, new Point(e.X, e.Y));
                    break;
                case MouseButtons.Left:
                    {
                        _dragging = true;  // _dragging is your variable flag
                        _start_point = new Point(e.X, e.Y);
                    }
                    break;
            }
        }

        private void label2_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void label2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this._start_point.X, p.Y - this._start_point.Y);
            }
        }

        private void trayIcon_Click(object sender, MouseEventArgs e)
        {
        if (e.Button == MouseButtons.Left) { 
            devicelist.muteToggle(); 
            }
        }


        private void trayIcon_DoubleClick(object sender, MouseEventArgs e)
        {
            this.Focus();
        }


        private void sizeBar_MouseDown(object sender, MouseEventArgs e)
        {
            _resizing = true;  // _resizing is your variable flag
            _start_point = new Point(Location.X, Location.Y);
        }

        private void sizeBar_MouseUp(object sender, MouseEventArgs e)
        {
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Save();
            _resizing = false;
        }

        private void sizeBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                Point p = PointToScreen(e.Location);
                this.Height = p.Y - _start_point.Y + this.Height;
            }
        }

        private void sizeBar_MouseHover(object sender, System.EventArgs e)
        {
            Cursor = Cursors.SizeNS;
        }

        private void sizeBar_MouseLeave(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Location;
            Height = Properties.Settings.Default.Height;
        }
    }
}
