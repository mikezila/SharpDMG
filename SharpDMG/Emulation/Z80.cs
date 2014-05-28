using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDMG.Cartridge;

namespace SharpDMG.Emulation
{
    class Z80
    {
        //Gameboy Cartridge
        //Can be either a real or emulated cartridge.
        ICartridge cartridge;

        //Registers
        byte a, b, c, d, e, h, l;

        //Flags
        //This can be used directly or queried via the bool properites
        byte f;

        //Stack and program pointers
        public short PC { get; private set; }
        short sp;

        //Cycle timers
        public int m { get; private set; }
        public int t { get { return m * 4; } }

        bool ZeroFlag
        {
            get { return TestBit(f, 7); }
            set { if (value) SetBit(ref f, 7); else ResetBit(ref f, 7); }
        }

        bool SubtractionFlag
        {
            get { return TestBit(f, 6); }
            set { if (value) SetBit(ref f, 6); else ResetBit(ref f, 6); }
        }

        bool HalfCaryFlag
        {
            get { return TestBit(f, 5); }
            set { if (value) SetBit(ref f, 5); else ResetBit(ref f, 5); }
        }

        bool CarryFlag
        {
            get { return TestBit(f, 4); }
            set { if (value) SetBit(ref f, 4); else ResetBit(ref f, 4); }
        }

        short BC
        {
            get { return (short)(b << 8 | c); }
            set { b = (byte)((value >> 8) & 0xFF); c = (byte)(value & 0xFF); }
        }

        short DE
        {
            get { return (short)(d << 8 | e); }
            set { d = (byte)((value >> 8) & 0xFF); e = (byte)(value & 0xFF); }
        }

        short HL
        {
            get { return (short)(h << 8 | l); }
            set { h = (byte)((value >> 8) & 0xFF); l = (byte)(value & 0xFF); }
        }

        public Z80()
        {
            cartridge = new RealCartridge("COM3");
        }

        private void Reset()
        {
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
            h = 0;
            l = 0;
            sp = 0;
            PC = 0;
            m = 0;
            f = 0;
            Console.WriteLine("Z80 core reset.");
        }

        private void NoOp()
        {
            m = 1; // One m-time taken.
        }

        private void LoadRegisterFromRegister(ref byte from, ref byte to)
        {
            to = from;
            m = 1; // One m-time taken.
        }

        private void LoadRegisterFromAddress(ref short address, ref byte register)
        {
            register = cartridge.ReadByte(address);
            m = 2; // Two m-time taken.
        }

        private void LoadMemoryFromRegister(ref short address, ref byte register)
        {
            cartridge.WriteByte(address, register);
            m = 2; // Two m-time taken.
        }

        private void LoadRegisterFromProgramCounter(ref byte register)
        {
            register = cartridge.ReadByte(PC++);
            m = 2; // Two m-time taken. Etc.
        }

        private bool TestBit(byte subject, int bit)
        {
            return (subject & (1 << bit)) != 0;
        }

        private void SetBit(ref byte subject, int bit)
        {
            subject |= (byte)(1 << bit);
        }

        private void ResetBit(ref byte subject, int bit)
        {
            subject &= (byte)(~(1 << bit));
        }

        internal string StepDebug()
        {
            throw new NotImplementedException();
        }
    }
}
