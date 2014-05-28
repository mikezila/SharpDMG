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
        }
    }
}
