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
        public ICartridge Memory { get; private set; }

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
                debugger.AppendLine("A: " + CPU.A.ToString("X2") + " F: " + CPU.F.ToString("X2"));
                debugger.AppendLine("B: " + CPU.B.ToString("X2") + " C: " + CPU.C.ToString("X2"));
                debugger.AppendLine("D: " + CPU.D.ToString("X2") + " E: " + CPU.E.ToString("X2"));
                debugger.AppendLine("H: " + CPU.H.ToString("X2") + " L: " + CPU.L.ToString("X2"));
                debugger.AppendLine("Flags: Z-" + CPU.ZeroFlag + " N-" + CPU.SubtractionFlag + " H-" + CPU.HalfCaryFlag + " C-" + CPU.CarryFlag);
                debugger.AppendLine("Next OP: " + CPU.NextThreeBytes());
                if (CPU.Crashed)
                    debugger.AppendLine("Crashed!");
                return debugger.ToString();
            }
        }

        public DMGSystem()
        {
            Memory = new EmulatedCartridge("tetris.gb");
            //Memory = new RealCartridge("COM3");
            CPU = new Z80(Memory);
            GPU = new GameBoyGPU(Memory);
        }

        public void StepUntil(ushort PC)
        {
            while (CPU.PC != PC)
                Step(1);
        }

        public void Step(int steps)
        {
            if (GPU.VBlankFired)
            {
                GPU.VBlankFired = false;
                CPU.VBlankWaiting = true;
            }

            if (GPU.HBlankFired)
            {
                GPU.HBlankFired = false;
                CPU.HBlankWaiting = true;
            }

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
