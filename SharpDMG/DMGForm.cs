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

namespace SharpDMG
{
    public partial class DMGForm : Form
    {
        public DMGForm()
        {
            InitializeComponent();
            Z80 cpu = new Z80();

            for (int i = 0; i < 100; i++)
                consoleTextBox.Text += cpu.StepDebug();
        }
    }
}
