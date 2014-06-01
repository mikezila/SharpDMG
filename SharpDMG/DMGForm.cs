using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDMG.Emulation;
using System.Diagnostics;

namespace SharpDMG
{
    public partial class DMGForm : Form
    {
        Graphics g;

        public DMGForm()
        {
            InitializeComponent();
            g = CreateGraphics();
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        }

        private DMGSystem system = new DMGSystem();

        private void button1_Click(object sender, EventArgs e)
        {
            system.Step();
            consoleTextBox.Text = system.DebugState;
            g.DrawImage(system.GPU.FrameBuffer, 0, 0, 160 * 2, 144 * 2);
            //system.GPU.DumpTiles();
        }

        private void stepTenButton_Click(object sender, EventArgs e)
        {
            system.Step(10);
            consoleTextBox.Text = system.DebugState;
            g.DrawImage(system.GPU.FrameBuffer, 0, 0, 160 * 2, 144 * 2);
            //system.GPU.DumpTiles();
        }

        private void stepHundredButton_Click(object sender, EventArgs e)
        {
            system.Step(100);
            consoleTextBox.Text = system.DebugState;
            g.DrawImage(system.GPU.FrameBuffer, 0, 0, 160 * 2, 144 * 2);
            system.GPU.DumpTiles();
        }

        private void stepThousandButton_Click(object sender, EventArgs e)
        {
            system.Step(10000);
            consoleTextBox.Text = system.DebugState;
            g.DrawImage(system.GPU.FrameBuffer, 0, 0, 160 * 2, 144 * 2);
            //system.GPU.DumpTiles();
        }

    }
}
