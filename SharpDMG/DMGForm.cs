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
        public DMGForm()
        {
            InitializeComponent();
        }

        private void checkOpsButton_Click(object sender, EventArgs e)
        {
            Z80 cpu = new Z80();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            StringBuilder dump = new StringBuilder();
            for (int i = 0; i < 100; i++)
                dump.AppendLine(cpu.StepDebug((byte)i));
            watch.Stop();
            dump.AppendLine("Time: " + watch.ElapsedMilliseconds);
            consoleTextBox.Text = dump.ToString();
        }
    }
}
