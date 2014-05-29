using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDMG.Cartridge;

namespace SharpDMG.Emulation
{
    // This is a GameBoy Z80, not a Zilog Z80.
    // There are differences.
    class Z80
    {
        //Gameboy Cartridge
        //Can be either a real or emulated cartridge.
        ICartridge cartridge;

        //Has shit become real?
        public bool Crashed { get; private set; }
        public bool Halted { get; private set; }

        //Registers
        byte a, b, c, d, e, h, l;

        //Flags
        //This can be used directly or queried/changed via the bool properites
        byte f;

        //Stack and program pointers
        public short PC { get; private set; }
        short sp;
        Stack<short> stack;

        //Cycle timers
        public int m { get; private set; }
        public int t { get { return m * 4; } }

        #region Shortcut properites for double registers and next byte/word
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

        byte NextByte
        {
            get { return cartridge.ReadByte(PC++); }
        }

        short NextWord
        {
            get { return (short)(NextByte + (NextByte << 8)); }
        }
        #endregion

        public Z80()
        {
            cartridge = new EmulatedCartridge("tetris.gb");
            stack = new Stack<short>();
            Reset();
        }

        private void Reset()
        {
            Crashed = false;
            stack.Clear();
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
            h = 0;
            l = 0;
            sp = 0;
            PC = 0x0100;
            m = 0;
            f = 0;
            Console.WriteLine("Z80 core reset.");
        }

        #region Misc/Control Op Functions

        private void NoOp()
        {
            m = 1; // One m-time taken.
        }

        private void SetCarryFlag()
        {
            CarryFlag = true;
            m = 1;
        }

        private void ClearCarryFlag()
        {
            CarryFlag = false;
            m = 1;
        }

        #endregion

        #region 8-bit Load/Store Op Fuctions
        private void LoadRegisterFromRegister(ref byte to, byte from)
        {
            to = from;
            m = 1; // One m-time taken.
        }

        private void LoadRegisterFromAddress(ref byte register, short address)
        {
            register = cartridge.ReadByte(address);
            m = 2; // Two m-time taken.
        }

        private void LoadMemoryFromRegister(short address, byte register)
        {
            cartridge.WriteByte(address, register);
            m = 2; // Two m-time taken.
        }

        private void LoadRegisterFromProgramCounter(ref byte register)
        {
            register = NextByte;
            m = 2; // Two m-time taken. Etc.
        }

        private void LoadMemoryFromProgramCounter(short address)
        {
            cartridge.WriteByte(address, NextByte);
            m = 2;
        }

        private void ZPLoadRegisterFromMemory(ref byte register, byte address)
        {
            register = cartridge.ReadByte(0xFF00 + address);
            m = 3;
        }

        private void ZPLoadMemoryFromRegister(byte address, byte register)
        {
            cartridge.WriteByte(0xFF00 + address, register);
            m = 3;
        }
        #endregion

        #region Bit level Op Functions

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

        private void ANDAddressWithRegister(ref byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            to &= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = true;
            CarryFlag = true;
            m = 2;
        }

        private void ANDRegisterWithRegister(ref byte to, byte from)
        {
            to &= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = true;
            CarryFlag = true;
            m = 1;
        }

        private void ORAddressWithRegister(ref byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            to |= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = false;
            CarryFlag = true;
            m = 2;
        }

        private void ORRegisterWithRegister(ref byte to, byte from)
        {
            to |= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = false;
            CarryFlag = true;
            m = 1;
        }

        private void XORAddressWithRegister(ref byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            to ^= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = false;
            CarryFlag = false;
            m = 2;
        }

        private void XORRegisterWithRegister(ref byte to, byte from)
        {
            to ^= from;
            ZeroFlag = to == 0;
            SubtractionFlag = false;
            HalfCaryFlag = false;
            CarryFlag = false;
            m = 1;
        }

        #endregion

        #region 8-bit Math Op Functions

        //Maybe not right?  Stolen.
        //Some emulators skip implementing this opcode anyway.
        private void DecimallyAdjustA()
        {
            int highNibble = a >> 4;
            int lowNibble = a & 0x0F;
            bool _FC = true;
            if (SubtractionFlag)
            {
                if (CarryFlag)
                {
                    if (HalfCaryFlag)
                    {
                        a += 0x9A;
                    }
                    else
                    {
                        a += 0xA0;
                    }
                }
                else
                {
                    _FC = false;
                    if (HalfCaryFlag)
                    {
                        a += 0xFA;
                    }
                    else
                    {
                        a += 0x00;
                    }
                }
            }
            else if (CarryFlag)
            {
                if (HalfCaryFlag || lowNibble > 9)
                {
                    a += 0x66;
                }
                else
                {
                    a += 0x60;
                }
            }
            else if (HalfCaryFlag)
            {
                if (highNibble > 9)
                {
                    a += 0x66;
                }
                else
                {
                    a += 0x06;
                    _FC = false;
                }
            }
            else if (lowNibble > 9)
            {
                if (highNibble < 9)
                {
                    _FC = false;
                    a += 0x06;
                }
                else
                {
                    a += 0x66;
                }
            }
            else if (highNibble > 9)
            {
                a += 0x60;
            }
            else
            {
                _FC = false;
            }

            HalfCaryFlag = false;
            CarryFlag = _FC;
            ZeroFlag = a == 0;
            m = 1;
        }

        private void IncrementRegister(ref byte register)
        {
            register++;
            HalfCaryFlag = (register & 0x0F) == 0x0F;
            SubtractionFlag = false;
            ZeroFlag = (register == 0);
            m = 1;
        }

        private void DecrementRegister(ref byte register)
        {
            register--;
            HalfCaryFlag = (register & 0x0F) == 0x0F;
            SubtractionFlag = true;
            ZeroFlag = (register == 0);
            m = 1;
        }


        private void IncrementAddress(short address)
        {
            byte data = cartridge.ReadByte(address);
            data++;
            HalfCaryFlag = (data & 0x0F) == 0x0F;
            SubtractionFlag = false;
            ZeroFlag = (data == 0);
            cartridge.WriteByte(address, data);
            m = 3;
        }

        private void DecrementAddress(short address)
        {
            byte data = cartridge.ReadByte(address);
            data--;
            HalfCaryFlag = (data & 0x0F) == 0x0F;
            SubtractionFlag = true;
            ZeroFlag = (data == 0);
            cartridge.WriteByte(address, data);
            m = 3;
        }

        private void ComplementRegister(ref byte register)
        {
            register = (byte)(~register);
            SubtractionFlag = true;
            HalfCaryFlag = true;
            m = 1;
        }

        private void AddRegisterToRegister(ref byte to, byte from)
        {
            CarryFlag = (((int)to + (int)from) > 255);
            to += from;
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = false;
            m = 1;
        }

        private void AddAddressToRegister(ref byte register, short address)
        {
            byte data = cartridge.ReadByte(address);
            CarryFlag = (((int)register + (int)data) > 255);
            register += data;
            ZeroFlag = (register == 0);
            HalfCaryFlag = (register & 0x0F) == 0x0F;
            SubtractionFlag = false;
            m = 2;
        }

        private void AddRegisterToRegisterWithCarry(ref byte to, byte from)
        {
            if (CarryFlag)
            {
                CarryFlag = (((int)to + (int)from + 1) > 255);                
                to += (byte)(from + 1);
            }
            else
            {
                CarryFlag = (((int)to + (int)from + 1) > 255);
                to += from;
            }
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = false;
            m = 1;
        }

        private void AddAddressToRegisterWithCarry(ref byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            if (CarryFlag)
            {
                CarryFlag = (((int)to + (int)from + 1) > 255);
                to += (byte)(from + 1);
            }
            else
            {
                CarryFlag = (((int)to + (int)from + 1) > 255);
                to += from;
            }
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = false;
            m = 2;
        }

        private void SubtractRegisterFromRegister(ref byte to, byte from)
        {
            CarryFlag = ((int)to - (int)from) < 0;
            to -= from;
            ZeroFlag = to == 0;
            SubtractionFlag = true;
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            m = 1;
        }

        private void CompareRegister(byte to, byte from)
        {
            CarryFlag = ((int)to - (int)from) < 0;
            to -= from;
            ZeroFlag = to == 0;
            SubtractionFlag = true;
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            m = 1;
        }

        private void CompareAddress(byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            CarryFlag = ((int)to - (int)from) < 0;
            to -= from;
            ZeroFlag = to == 0;
            SubtractionFlag = true;
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            m = 1;
        }

        private void SubtractRegisterFromRegisterWithCarry(ref byte to, byte from)
        {
            if (CarryFlag)
            {
                CarryFlag = (((int)to - (int)from - 1) < 0);
                to -= (byte)(from + 1);
            }
            else
            {
                CarryFlag = (((int)to - (int)from - 1) < 0);
                to -= from;
            }
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = true;
            m = 1;
        }

        private void SubtractAddressFromRegisterWithCarry(ref byte to, short address)
        {
            byte from = cartridge.ReadByte(address);
            if (CarryFlag)
            {
                CarryFlag = (((int)to - (int)from - 1) < 0);
                to -= (byte)(from + 1);
            }
            else
            {
                CarryFlag = (((int)to - (int)from - 1) < 0);
                to -= from;
            }
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = true;
            m = 1;
        }

        #endregion

        internal void Step()
        {
            switch (NextByte)
            {
                // No Op
                case 0x00:
                    { NoOp(); break; }

                case 0xD3:
                case 0xDB:
                case 0xDD:
                case 0xE3:
                case 0xE4:
                case 0xEB:
                case 0xEC:
                case 0xED:
                case 0xF4:
                case 0xFC:
                case 0xFD:
                    { NoOp(); break; }

                // Jumps and calls

                // Three byte ops
                case 0xC2:
                case 0xC3:
                case 0xC4:
                case 0xCA:
                case 0xCC:
                case 0xCD:
                case 0xD2:
                case 0xD4:
                case 0xDA:
                case 0xDC:
                    {
                        PC += 2;
                        break;
                    }

                // Two byte ops
                case 0x18:
                case 0x20:
                case 0x28:
                case 0x30:
                case 0x38:
                    {
                        PC++;
                        break;
                    }

                // One byte ops
                case 0xC0:
                case 0xC7:
                case 0xC8:
                case 0xC9:
                case 0xCF:
                case 0xD0:
                case 0xD7:
                case 0xD8:
                case 0xD9:
                case 0xDF:
                case 0xE7:
                case 0xE9:
                case 0xEF:
                case 0xF7:
                case 0xFF:
                    break;

                // Misc/Control Instructions
                case 0x10:
                    {
                        //CPU STOP
                        PC++; // FIXME: Maybe only one byte?
                        break;
                    }
                case 0x76:
                    {
                        Halted = true;
                        break;
                    }
                case 0xCB:
                    {
                        PC++;
                        break;
                    }
                case 0xF3:
                    break;
                case 0xFB:
                    break;

                // 8-bit Math/Logic

                // One byte ops
                case 0x04: { IncrementRegister(ref b); break; }
                case 0x05: { DecrementRegister(ref b); break; }
                case 0x0C: { IncrementRegister(ref c); break; }
                case 0x0D: { DecrementRegister(ref c); break; }
                case 0x14: { IncrementRegister(ref d); break; }
                case 0x15: { DecrementRegister(ref d); break; }
                case 0x1C: { IncrementRegister(ref e); break; }
                case 0x1D: { DecrementRegister(ref e); break; }
                case 0x24: { IncrementRegister(ref h); break; }
                case 0x25: { DecrementRegister(ref h); break; }
                case 0x27: { DecimallyAdjustA(); break; }
                case 0x2C: { IncrementRegister(ref l); break; }
                case 0x2D: { DecrementRegister(ref l); break; }
                case 0x2F: { ComplementRegister(ref a); break; }
                case 0x34: { IncrementAddress(HL); break; }
                case 0x35: { DecrementAddress(HL); break; }
                case 0x37: { SetCarryFlag(); break; }
                case 0x3C: { IncrementRegister(ref a); break; }
                case 0x3D: { DecrementRegister(ref a); break; }
                case 0x3F: { ClearCarryFlag(); break; }
                case 0x80: { AddRegisterToRegister(ref a, b); break; }
                case 0x81: { AddRegisterToRegister(ref a, c); break; }
                case 0x82: { AddRegisterToRegister(ref a, d); break; }
                case 0x83: { AddRegisterToRegister(ref a, e); break; }
                case 0x84: { AddRegisterToRegister(ref a, h); break; }
                case 0x85: { AddRegisterToRegister(ref a, l); break; }
                case 0x86: { AddAddressToRegister(ref a, HL); break; }
                case 0x87: { AddRegisterToRegister(ref a, a); break; }
                case 0x88: { AddRegisterToRegisterWithCarry(ref a, b); break; }
                case 0x89: { AddRegisterToRegisterWithCarry(ref a, c); break; }
                case 0x8A: { AddRegisterToRegisterWithCarry(ref a, d); break; }
                case 0x8B: { AddRegisterToRegisterWithCarry(ref a, e); break; }
                case 0x8C: { AddRegisterToRegisterWithCarry(ref a, h); break; }
                case 0x8D: { AddRegisterToRegisterWithCarry(ref a, l); break; }
                case 0x8E: { AddAddressToRegisterWithCarry(ref a, HL); break; }
                case 0x8F: { AddRegisterToRegisterWithCarry(ref a, a); break; }
                case 0x90: { SubtractRegisterFromRegister(ref a, b); break; }
                case 0x91: { SubtractRegisterFromRegister(ref a, c); break; }
                case 0x92: { SubtractRegisterFromRegister(ref a, d); break; }
                case 0x93: { SubtractRegisterFromRegister(ref a, e); break; }
                case 0x94: { SubtractRegisterFromRegister(ref a, h); break; }
                case 0x95: { SubtractRegisterFromRegister(ref a, l); break; }
                case 0x96: { SubtractAddressFromRegisterWithCarry(ref a, HL); break; }
                case 0x97: { SubtractRegisterFromRegister(ref a, a); break; }
                case 0x98: { SubtractRegisterFromRegisterWithCarry(ref a, b); break; }
                case 0x99: { SubtractRegisterFromRegisterWithCarry(ref a, c); break; }
                case 0x9A: { SubtractRegisterFromRegisterWithCarry(ref a, d); break; }
                case 0x9B: { SubtractRegisterFromRegisterWithCarry(ref a, e); break; }
                case 0x9C: { SubtractRegisterFromRegisterWithCarry(ref a, h); break; }
                case 0x9D: { SubtractRegisterFromRegisterWithCarry(ref a, l); break; }
                case 0x9E: { SubtractAddressFromRegisterWithCarry(ref a, HL); break; }
                case 0x9F: { SubtractRegisterFromRegisterWithCarry(ref a, a); break; }
                case 0xA0: { ANDRegisterWithRegister(ref a, b); break; }
                case 0xA1: { ANDRegisterWithRegister(ref a, c); break; }
                case 0xA2: { ANDRegisterWithRegister(ref a, d); break; }
                case 0xA3: { ANDRegisterWithRegister(ref a, e); break; }
                case 0xA4: { ANDRegisterWithRegister(ref a, h); break; }
                case 0xA5: { ANDRegisterWithRegister(ref a, l); break; }
                case 0xA6: { ANDAddressWithRegister(ref a, HL); break; }
                case 0xA7: { ANDRegisterWithRegister(ref a, a); break; }
                case 0xA8: { XORRegisterWithRegister(ref a, b); break; }
                case 0xA9: { XORRegisterWithRegister(ref a, c); break; }
                case 0xAA: { XORRegisterWithRegister(ref a, d); break; }
                case 0xAB: { XORRegisterWithRegister(ref a, e); break; }
                case 0xAC: { XORRegisterWithRegister(ref a, h); break; }
                case 0xAD: { XORRegisterWithRegister(ref a, l); break; }
                case 0xAE: { XORAddressWithRegister(ref a, HL); break; }
                case 0xAF: { XORRegisterWithRegister(ref a, a); break; }
                case 0xB0: { ORRegisterWithRegister(ref a, b); break; }
                case 0xB1: { ORRegisterWithRegister(ref a, c); break; }
                case 0xB2: { ORRegisterWithRegister(ref a, d); break; }
                case 0xB3: { ORRegisterWithRegister(ref a, e); break; }
                case 0xB4: { ORRegisterWithRegister(ref a, h); break; }
                case 0xB5: { ORRegisterWithRegister(ref a, l); break; }
                case 0xB6: { ORAddressWithRegister(ref a, HL); break; }
                case 0xB7: { ORRegisterWithRegister(ref a, a); break; }
                case 0xB8: { CompareRegister(a, b); break; }
                case 0xB9: { CompareRegister(a, c); break; }
                case 0xBA: { CompareRegister(a, d); break; }
                case 0xBB: { CompareRegister(a, e); break; }
                case 0xBC: { CompareRegister(a, h); break; }
                case 0xBD: { CompareRegister(a, l); break; }
                case 0xBE: { CompareAddress(a, HL); break; }
                case 0xBF: { CompareRegister(a, a); break; }

                // Two byte ops
                case 0xC6:
                case 0xCE:
                case 0xD6:
                case 0xDE:
                case 0xE6:
                case 0xEE:
                case 0xF6:
                case 0xFE:
                    {
                        PC++;
                        break;
                    }

                # region 8-bit Load/Store
                // 8-bit Load/Store

                // Three byte ops
                case 0xEA: { LoadMemoryFromRegister(NextWord, a); break; }
                case 0xFA: { LoadRegisterFromAddress(ref a, NextWord); break; }

                // Two byte ops
                case 0x06: { LoadRegisterFromProgramCounter(ref b); break; }
                case 0x0E: { LoadRegisterFromProgramCounter(ref c); break; }
                case 0x16: { LoadRegisterFromProgramCounter(ref d); break; }
                case 0x1E: { LoadRegisterFromProgramCounter(ref e); break; }
                case 0x26: { LoadRegisterFromProgramCounter(ref h); break; }
                case 0x2E: { LoadRegisterFromProgramCounter(ref l); break; }
                case 0x36: { LoadMemoryFromProgramCounter(HL); break; }
                case 0x3E: { LoadRegisterFromProgramCounter(ref a); break; }
                case 0xE0: { ZPLoadMemoryFromRegister(NextByte, a); break; }
                case 0xE2: { ZPLoadMemoryFromRegister(c, a); break; }
                case 0xF0: { ZPLoadRegisterFromMemory(ref a, NextByte); break; }
                case 0xF2: { ZPLoadRegisterFromMemory(ref a, c); break; }

                // One byte ops
                case 0x02: { LoadMemoryFromRegister(BC, a); break; }
                case 0x0A: { LoadRegisterFromAddress(ref a, BC); break; }
                case 0x12: { LoadMemoryFromRegister(DE, a); break; }
                case 0x1A: { LoadRegisterFromAddress(ref a, DE); break; }
                case 0x22: { LoadMemoryFromRegister(HL, a); HL++; break; }
                case 0x2A: { LoadRegisterFromAddress(ref a, HL); HL++; break; }
                case 0x32: { LoadMemoryFromRegister(HL, a); HL--; break; }
                case 0x3A: { LoadRegisterFromAddress(ref a, HL); HL--; break; }
                case 0x40: { LoadRegisterFromRegister(ref b, b); break; }
                case 0x41: { LoadRegisterFromRegister(ref b, c); break; }
                case 0x42: { LoadRegisterFromRegister(ref b, d); break; }
                case 0x43: { LoadRegisterFromRegister(ref b, e); break; }
                case 0x44: { LoadRegisterFromRegister(ref b, h); break; }
                case 0x45: { LoadRegisterFromRegister(ref b, l); break; }
                case 0x46: { LoadRegisterFromAddress(ref b, HL); break; }
                case 0x47: { LoadRegisterFromRegister(ref b, a); break; }
                case 0x48: { LoadRegisterFromRegister(ref c, b); break; }
                case 0x49: { LoadRegisterFromRegister(ref c, c); break; }
                case 0x4A: { LoadRegisterFromRegister(ref c, d); break; }
                case 0x4B: { LoadRegisterFromRegister(ref c, e); break; }
                case 0x4C: { LoadRegisterFromRegister(ref c, h); break; }
                case 0x4D: { LoadRegisterFromRegister(ref c, l); break; }
                case 0x4E: { LoadRegisterFromAddress(ref c, HL); break; }
                case 0x4F: { LoadRegisterFromRegister(ref c, a); break; }
                case 0x50: { LoadRegisterFromRegister(ref d, b); break; }
                case 0x51: { LoadRegisterFromRegister(ref d, c); break; }
                case 0x52: { LoadRegisterFromRegister(ref d, d); break; }
                case 0x53: { LoadRegisterFromRegister(ref d, e); break; }
                case 0x54: { LoadRegisterFromRegister(ref d, h); break; }
                case 0x55: { LoadRegisterFromRegister(ref d, l); break; }
                case 0x56: { LoadRegisterFromAddress(ref d, HL); break; }
                case 0x57: { LoadRegisterFromRegister(ref d, a); break; }
                case 0x58: { LoadRegisterFromRegister(ref e, b); break; }
                case 0x59: { LoadRegisterFromRegister(ref e, c); break; }
                case 0x5A: { LoadRegisterFromRegister(ref e, d); break; }
                case 0x5B: { LoadRegisterFromRegister(ref e, e); break; }
                case 0x5C: { LoadRegisterFromRegister(ref e, h); break; }
                case 0x5D: { LoadRegisterFromRegister(ref e, l); break; }
                case 0x5E: { LoadRegisterFromAddress(ref e, HL); break; }
                case 0x5F: { LoadRegisterFromRegister(ref e, a); break; }
                case 0x60: { LoadRegisterFromRegister(ref h, b); break; }
                case 0x61: { LoadRegisterFromRegister(ref h, c); break; }
                case 0x62: { LoadRegisterFromRegister(ref h, d); break; }
                case 0x63: { LoadRegisterFromRegister(ref h, e); break; }
                case 0x64: { LoadRegisterFromRegister(ref h, h); break; }
                case 0x65: { LoadRegisterFromRegister(ref h, l); break; }
                case 0x66: { LoadRegisterFromAddress(ref h, HL); break; }
                case 0x67: { LoadRegisterFromRegister(ref h, a); break; }
                case 0x68: { LoadRegisterFromRegister(ref l, b); break; }
                case 0x69: { LoadRegisterFromRegister(ref l, c); break; }
                case 0x6A: { LoadRegisterFromRegister(ref l, d); break; }
                case 0x6B: { LoadRegisterFromRegister(ref l, e); break; }
                case 0x6C: { LoadRegisterFromRegister(ref l, h); break; }
                case 0x6D: { LoadRegisterFromRegister(ref l, l); break; }
                case 0x6E: { LoadRegisterFromAddress(ref l, HL); break; }
                case 0x6F: { LoadRegisterFromRegister(ref l, a); break; }
                case 0x70: { LoadMemoryFromRegister(HL, b); break; }
                case 0x71: { LoadMemoryFromRegister(HL, c); break; }
                case 0x72: { LoadMemoryFromRegister(HL, d); break; }
                case 0x73: { LoadMemoryFromRegister(HL, e); break; }
                case 0x74: { LoadMemoryFromRegister(HL, h); break; }
                case 0x75: { LoadMemoryFromRegister(HL, l); break; }
                case 0x77: { LoadMemoryFromRegister(HL, a); break; }
                case 0x78: { LoadRegisterFromRegister(ref a, b); break; }
                case 0x79: { LoadRegisterFromRegister(ref a, c); break; }
                case 0x7A: { LoadRegisterFromRegister(ref a, d); break; }
                case 0x7B: { LoadRegisterFromRegister(ref a, e); break; }
                case 0x7C: { LoadRegisterFromRegister(ref a, h); break; }
                case 0x7D: { LoadRegisterFromRegister(ref a, l); break; }
                case 0x7E: { LoadRegisterFromAddress(ref a, HL); break; }
                case 0x7F: { LoadRegisterFromRegister(ref a, a); break; }
                #endregion

                // 16-bit Load/Store/Move

                // Three byte ops
                case 0x01:
                case 0x08:
                case 0x11:
                case 0x21:
                case 0x31:
                    {
                        PC += 2;
                        break;
                    }

                // Two byte op
                case 0xF8:
                    {
                        PC++;
                        break;
                    }

                // One byte ops
                case 0xC1:
                case 0xC5:
                case 0xD1:
                case 0xD5:
                case 0xE1:
                case 0xE5:
                case 0xF1:
                case 0xF5:
                case 0xF9:
                    break;

                // 16-bit Math/Logic

                // One byte ops
                case 0x03:
                case 0x09:
                case 0x0B:
                case 0x13:
                case 0x19:
                case 0x1B:
                case 0x23:
                case 0x29:
                case 0x2B:
                case 0x33:
                case 0x39:
                case 0x3B:
                    break;

                case 0xE8:
                    {
                        PC++;
                        break;
                    }

                // Non-CB bit level ops
                case 0x07:
                case 0x0F:
                case 0x17:
                case 0x1F:
                    break;

                // Shit has become real
                default:
                    {
                        Crashed = true;
                        Halted = true;
                        break;
                    }
            }
        }
    }
}
