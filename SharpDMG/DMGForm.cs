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
            //system.GPU.DumpTiles();
        }

        private void stepThousandButton_Click(object sender, EventArgs e)
        {
            system.Step(10000);
            consoleTextBox.Text = system.DebugState;
            g.DrawImage(system.GPU.FrameBuffer, 0, 0, 160 * 2, 144 * 2);
            //system.GPU.DumpTiles();
        }

        private void dumpButton_Click(object sender, EventArgs e)
        {
            StringBuilder memDump = new StringBuilder();
            int index = 0x8000;
            int currentByte = 0;

            for (int i = 0; i < system.Memory.VRAM.Length; i += 16)
            {
                memDump.AppendLine(index.ToString("X4") + ": " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2") + " " + system.Memory.VRAM[currentByte++].ToString("X2"));
                index += 16;
            }
            vramDumpTextBox.Text = memDump.ToString();
        }

        private void breakButton_Click(object sender, EventArgs e)
        {
            system.StepUntil(ushort.Parse(breakpointTextBox.Text, System.Globalization.NumberStyles.HexNumber));
        }

    }
}
