using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CursorPositionMacro
{
    public partial class Form2 : Form
    {
        // Form variables
        private int m_nCurrentX = -1000;
        private int m_nCurrentY = -1000;
        private int m_nDimension = 2;
        private int m_nIndex = -1;

        // List of open forms
        List<Form> openForms = new List<Form>();

        // Form status
        private bool showForms;
        private bool hideForms;
        private bool closeForms;

        public Form2(int nPosX, int nPosY, int coordNum, bool showForm)
        {
            Console.WriteLine("---START---");
            // Set form status
            bool allForms = coordNum == -1;
            bool singleForm = !allForms;
            showForms = showForm;
            hideForms = !showForm;
            closeForms = !showForm && nPosX == -1001;

            // Set form variables
            m_nCurrentX = nPosX - m_nDimension;
            m_nCurrentY = nPosY - m_nDimension;
            m_nIndex = coordNum;

            // Initialize components
            InitializeComponent();

            // Set label with coordinate identification
            this.label1.Text = coordNum.ToString();
            this.label2.Text += coordNum.ToString();
            this.Name = "CoordinateDebug";
            this.Location = new Point(m_nCurrentX, m_nCurrentY);

            //
            if (allForms || hideForms)
            {
                Console.WriteLine("All Forms:");
                foreach (Form f in Application.OpenForms)
                {
                    if (f.Name == "CoordinateDebug")
                        openForms.Add(f);
                }
                //
                if (openForms.Count > 0)
                {
                    HandleOpenForms(showForm, allForms);
                }
            }

            if (singleForm)
            {
                Console.WriteLine("Single Forms:");
                if (nPosX != -1000 && coordNum != -1)
                {
                    this.Show();
                    if (hideForms) this.Hide();
                    //
                    Console.WriteLine(this.label2.Text + (showForms ? " Show" : closeForms ? " Close" : " Hide"));
                }
                if (closeForms) this.Close();
            }
            Console.WriteLine("---END---");
        }

        public void HandleOpenForms(bool showForms, bool allForms)
        {
            bool singleForm = !allForms;
            string consoleComp = allForms ? " (ALL)" : "";

            foreach (Form2 f in openForms)
            {
                Console.WriteLine(f.label1.Text);
                if (singleForm && f.label1.Text != m_nIndex.ToString()) continue;
                // Show forms change
                string consoleText = showForms && f.label1.Text != "-1" ? " Show" : closeForms ? " Close" : " Hide";
                //Console.WriteLine(f.label2.Text + f.label1.Text + consoleText + consoleComp);
                //
                if (closeForms)
                {
                    f.Close();
                    continue;
                }
                if (showForms && f.label1.Text != "-1") f.Show();
                else f.Hide();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            // this.pictureBox1.Image = this.imageList1.Images[1];
        }
    }
}