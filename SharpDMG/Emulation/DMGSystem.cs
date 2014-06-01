using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDMG.Cartridge;

namespace SharpDMG.Emulation
{
    class DMGSystem
    {
        public Z80 CPU { get; private set; }
        public GameBoyGPU GPU { get; private set; }
        public string DebugState
        {
            get
            {
                StringBuilder debugger = new StringBuilder();
                debugger.Clear();
                debugger.AppendLine("z80 Debugger" + Environment.NewLine + "------------------------" + Environment.NewLine + "Registers:");
                debugger.AppendLine("PC: " + CPU.PC.ToString("X4") + " SP: " + CPU.SP.ToString("X4"));
                debugger.AppendLine("HL: " + CPU.HL.ToString("X4") + " BC: " + CPU.BC.ToString("X4"));
                debugger.AppendLine("DE: " + CPU.DE.ToString("X4") + " AF: " + CPU.AF.ToString("X4"));
                debugger.AppendLine("Flags: Z-" + CPU.ZeroFlag + " N-" + CPU.SubtractionFlag + " H-" + CPU.HalfCaryFlag + " C-" + CPU.CarryFlag);
                debugger.AppendLine("Next OP: " + CPU.NextThreeBytes());
                if (CPU.Crashed)
                    debugger.AppendLine("Crashed!");
                return debugger.ToString();
            }
        }

        public DMGSystem()
        {
            ICartridge memory = new EmulatedCartridge("tetris.gb");
            CPU = new Z80(memory);
            GPU = new GameBoyGPU(memory);
        }

        public void Step(int steps)
        {
            for (int i = 0; i < steps; i++)
                Step();
        }

        public void Step()
        {
            CPU.Step();
            CPU.mClock += CPU.m;
            CPU.tClock += CPU.t;

            GPU.Step(CPU.t);
        }


    }
}
