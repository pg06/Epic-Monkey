using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;

namespace CursorPositionMacro
{
    public partial class frmMain : Form
    {
        // We need to use unmanaged code
        [DllImport("user32.dll")]
        // GetCursorPos() makes everything possible
        static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int xPos, int yPos);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        // Drawing DEBUG
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        // Change cursor icon
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32
        uiParam, String pvParam, UInt32 fWinIni);
        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr pcur);

        private static uint CROSS = 32515;
        private static uint NORMAL = 32512;
        private static uint IBEAM = 32513;

        // Variable we will need to count the traveled pixels
        static protected long totalPixels = 0;
        static protected int currX;
        static protected int currY;
        static protected int diffX;
        static protected int diffY;
        // Avoid multiple click saves
        static protected bool ToggleSaveCoords;
        static protected bool CheckLeftClick;
        static protected bool CheckMiddleClick;
        static protected bool DebugCoods;
        static protected bool EnableMacroCoods;
        static protected int currentX;
        static protected int currentY;
        static protected string DebugLabelText;
        static protected bool EnableDebugViewF;
        ArrayList CoordList = new ArrayList();
        static protected string StatusLabelDefaultText = "Ready";

        public frmMain()
        {
            SystemParametersInfo(0x0057, 0, null, 0); // Set Default Cursor
            CheckLeftClick = true;
            CheckMiddleClick = true;
            ToggleSaveCoords = false;
            DebugCoods = false;
            EnableMacroCoods = true;
            InitializeComponent();
        }

        // Simulate mouse drag and click
        public static void FakeMouseClick(string MouseButtonChosen, int xpos, int ypos)
        {
            int MouseButtonChosenUp = 0;
            int MouseButtonChosenDown = 0;
            CheckMiddleClick = false;
            switch (MouseButtonChosen)
            {
                case "LEFT":
                default:
                    MouseButtonChosenUp = MOUSEEVENTF_LEFTUP;
                    MouseButtonChosenDown = MOUSEEVENTF_LEFTDOWN;
                    break;
                case "RIGHT":
                    MouseButtonChosenUp = MOUSEEVENTF_RIGHTUP;
                    MouseButtonChosenDown = MOUSEEVENTF_RIGHTDOWN;
                    break;
            }
            mouse_event(MouseButtonChosenDown, xpos, ypos, 0, 0);
            SetCursorPos(xpos, ypos);
            mouse_event(MouseButtonChosenUp, xpos, ypos, 0, 0);
            //
            // System.Threading.Thread.Sleep(100);
            // SetCursorPos(currentX, currentY);
            //
            // Back to initial cursor position
            Task.Delay(100).ContinueWith(t =>
            {
                SetCursorPos(currentX, currentY);
                CheckMiddleClick = true;
            });
        }

        private void tmrDef_Tick(object sender, EventArgs e)
        {
            // New point that will be updated by the function with the current coordinates
            Point defPnt = new Point();
            // Call the function and pass the Point, defPnt
            GetCursorPos(ref defPnt);
            // Now after calling the function, defPnt contains the coordinates which we can read
            lblCoords.Text = "X:" + (defPnt.X + 1).ToString() + " ; Y:" + (defPnt.Y + 1).ToString();
            // Check if isnt inside macro window
            switch (RectangleToScreen(Bounds).Contains(PointToScreen(Cursor.Position)))
            {
                case true:
                    if (Opacity != .9999D)
                        Opacity = .9999D;
                    break;
                case false:
                    CheckCursorCoord();
                    break;
            }
        }

        // Paint cursor saved coordinates
        private void DrawDebugCoord(int x_, int y_, int num_, bool visible_)
        {
            Form2 fd = new Form2(x_, y_, num_, visible_);
        }

        private void SaveCoord_Click()
        {
            // New point that will be updated by the function with the current coordinates
            Point defPnt = new Point();
            // Call the function and pass the Point, defPnt
            GetCursorPos(ref defPnt);
            // Add Coordinates to ArrayList (CoordList)
            int CoordIndex = CoordList.Count == 0 ? 1 : ((Coordinates)CoordList[CoordList.Count - 1]).NUMBER + 1;
            int CoordX = defPnt.X;
            int CoordY = defPnt.Y;
            foreach (Coordinates cl in CoordList)
            {
                if (cl.X == CoordX && cl.Y == CoordY) return;
            }
            Coordinates Coordinate = new Coordinates("Coord #" + CoordIndex.ToString(), defPnt.X, defPnt.Y, CoordIndex);
            CoordList.Add(Coordinate);
            listBox1.DataSource = null; // refresh list box
            listBox1.DataSource = CoordList; // refresh list box
            listBox1.DisplayMember = "Label"; // set display member to list box
            statusStripLabel1_Change("Saved", null); // change first status label
            DrawDebugCoord(CoordX, CoordY, CoordIndex, checkBox1.Checked); // Add Single Coordinates DEBUG
        }

        private void listBox1_RemoveSelectedItem(object sender, MouseEventArgs e)
        {
            // Click in List Box
            if (e.Button == MouseButtons.Right)
            {
                int index = this.listBox1.IndexFromPoint(e.Location);
                if (index != System.Windows.Forms.ListBox.NoMatches)
                {
                    // Refresh list box
                    int number = ((Coordinates)CoordList[index]).NUMBER;
                    CoordList.RemoveAt(index);
                    this.listBox1.DataSource = null;
                    this.listBox1.DataSource = CoordList;
                    this.listBox1.DisplayMember = "Label";
                    DrawDebugCoord(-1001, -1001, number, false); // Remove Single Coordinates DEBUG
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            totalPixels = 0;
            CoordList.Clear();
            listBox1.DataSource = null;
            listBox1.DataSource = CoordList;
            statusStripLabel1_Change("Reset", null);
            DrawDebugCoord(-1001, -1001, -1, false); // Remove All Coordinates DEBUG
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/Disable Debug Mouse Coordinates (CheckBox)
            // Change status label 3 DEBUG
            this.toolStripStatusLabel3.Text = this.checkBox1.Checked ? "Debug" : "";
            DrawDebugCoord(-1000, -1000, -1, this.checkBox1.Checked); // Toggle All Coordinates DEBUG
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/Disable Mouse Macro (CheckBox)
            this.toolStripStatusLabel2.Text = this.checkBox2.Checked ? "Enabled" : "Disabled";
        }

        private void CheckCursorCoord()
        {
            if (MouseButtons != MouseButtons.Left && !CheckLeftClick)
            {
                // Detect mouse left click up
                CheckLeftClick = true;
                // MessageBox.Show("Left Unclick");
            }
            if (MouseButtons != MouseButtons.Middle && !CheckMiddleClick)
            {
                // Detect mouse left click up
                // CheckMiddleClick = true;
                // MessageBox.Show("Left Unclick");
            }
            if (MouseButtons == MouseButtons.Left && CheckLeftClick && ToggleSaveCoords)
            {
                // Detect mouse left click down
                CheckLeftClick = false;
                // MessageBox.Show("Left Click");
                SaveCoord_Click();
            }
            if (MouseButtons == MouseButtons.Middle && CheckMiddleClick)
            {
                // Detect mouse middle click down
                // CheckMiddleClick = false;
                // MessageBox.Show("Middle Click");
                DragCursor_MiddleClick();
            }
        }

        private void EnableSaveCoord(object sender, EventArgs e)
        {
            if (ToggleSaveCoords)
            {
                SystemParametersInfo(0x0057, 0, null, 0);
                this.btnSaveCoord.Text = "Save";
                statusStripLabel1_Change("Ready", "Ready");
            }
            else
            {
                uint[] Cursors = { NORMAL, IBEAM };
                for (int i = 0; i < Cursors.Length; i++)
                    SetSystemCursor(CopyIcon(LoadCursor(IntPtr.Zero, (int)CROSS)), Cursors[i]);
                //
                this.btnSaveCoord.Text = "Saving...";
                statusStripLabel1_Change("Saving...", "Saving...");
            }
            ToggleSaveCoords = !ToggleSaveCoords;
        }

        private void DragCursor_MiddleClick()
        {
            if (!checkBox2.Checked) return; // If debug checkbox is unmarked disable macro
            if (listBox1.SelectedIndices.Count > 0)
            {
                // New point that will be updated by the function with the current coordinates
                // Save current cursor position in screen
                Point defPnt = new Point();
                GetCursorPos(ref defPnt);
                currentX = defPnt.X;
                currentY = defPnt.Y;
                // Simulate cursor drag and click
                Coordinates selectedCoordinates = (Coordinates)listBox1.SelectedItem;
                int selectedX = selectedCoordinates.X;
                int selectedY = selectedCoordinates.Y;
                FakeMouseClick("LEFT", selectedX, selectedY);
            }
        }

        private void statusStripLabel1_Change(string StatusLabelText, string ChangeDefault)
        {
            // Change Status Label Position 1 (Left)
            if (ChangeDefault != null) StatusLabelDefaultText = ChangeDefault;
            toolStripStatusLabel1.Text = StatusLabelText;
            //
            // Restore status label after 1s
            Task.Delay(1000).ContinueWith(t =>
            {
                toolStripStatusLabel1.Text = StatusLabelDefaultText;
            });
        }

        private void OnLoad(object sender, EventArgs e)
        {
        }

        private void OnTrayIcon(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
            {
                //AboutForm formAbout = new AboutForm();
                //formAbout.ShowDialog();

                // or
                //this.Close();
            }
        }
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.ShowInTaskbar = false;
                this.Hide();
                // Minimized app set to default
                if (ToggleSaveCoords) EnableSaveCoord(this, new EventArgs());
            }
        }

        private void notifyIconTray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            
        }
    }
}