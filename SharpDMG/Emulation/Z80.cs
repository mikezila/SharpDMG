using SharpDMG.Cartridge;
using System;

namespace SharpDMG.Emulation
{
    // This is a GameBoy Z80, not a Zilog Z80.
    // There are differences.
    class Z80
    {
        //Gameboy Cartridge
        //Can be either a real or emulated cartridge.
        EmulatedCartridge cartridge;

        //Has shit become real?
        public bool Crashed { get; private set; }
        public bool Halted { get; private set; }
        public bool InteruptsEnabled { get; private set; }

        //Registers
        byte a, b, c, d, e, h, l;

        public byte A { get { return a; } }
        public byte B { get { return b; } }
        public byte C { get { return c; } }
        public byte D { get { return d; } }
        public byte E { get { return e; } }
        public byte H { get { return h; } }
        public byte L { get { return l; } }

        //Flags
        //This can be used directly or queried/changed via the bool properites
        byte f;

        public byte F { get { return f; } }

        //Stack and program pointers
        public ushort PC { get; private set; }
        public ushort SP { get; private set; }

        //Running clocks
        public int mClock { get; set; }
        public int tClock { get; set; }

        //Cycle timers
        public int m { get; private set; }
        public int t { get { return m * 4; } }

        #region Shortcut properites for double registers and next byte/word
        public bool ZeroFlag
        {
            get { return ((f & (0x01 << 7)) != 0); }
            //get { return TestBit(f, 7); }
            private set { if (value) SetBit(ref f, 7); else ResetBit(ref f, 7); }
        }

        public bool SubtractionFlag
        {
            get { return ((f & (0x01 << 6)) != 0); }
            private set { if (value) SetBit(ref f, 6); else ResetBit(ref f, 6); }
        }

        public bool HalfCaryFlag
        {
            get { return ((f & (0x01 << 5)) != 0); }
            private set { if (value) SetBit(ref f, 5); else ResetBit(ref f, 5); }
        }

        public bool CarryFlag
        {
            get { return ((f & (0x01 << 4)) != 0); }
            private set { if (value) SetBit(ref f, 4); else ResetBit(ref f, 4); }
        }

        public ushort BC
        {
            get { return (ushort)(b << 8 | c); }
            private set { b = (byte)((value >> 8) & 0xFF); c = (byte)(value & 0xFF); }
        }

        public ushort DE
        {
            get { return (ushort)(d << 8 | e); }
            private set { d = (byte)((value >> 8) & 0xFF); e = (byte)(value & 0xFF); }
        }

        public ushort HL
        {
            get { return (ushort)(h << 8 | l); }
            private set { h = (byte)((value >> 8) & 0xFF); l = (byte)(value & 0xFF); }
        }

        public ushort AF
        {
            get { return (ushort)(a << 8 | f); }
            private set { a = (byte)((value >> 8) & 0xFF); f = (byte)(value & 0xFF); }
        }

        public byte PeekByte
        {
            get { return cartridge.ReadByte(PC); }
        }

        byte NextByte
        {
            get { return cartridge.ReadByte(PC++); }
        }

        ushort NextWord
        {
            get { return (ushort)(NextByte + (NextByte << 8)); }
        }

        // For debugging display
        public string NextThreeBytes()
        {
            return cartridge.ReadByte(PC).ToString("X2") + " " + cartridge.ReadByte((ushort)(PC + 1)).ToString("X2") + " " + cartridge.ReadByte((ushort)(PC + 2)).ToString("X2");
        }

        #endregion

        public Z80(EmulatedCartridge game)
        {
            cartridge = game;
            Reset();
        }

        private void Reset()
        {
            Crashed = false;
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
            h = 0;
            l = 0;
            SP = 0;
            PC = 0x0000;
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

        private void LoadRegisterFromAddress(ref byte register, ushort address)
        {
            register = cartridge.ReadByte(address);
            m = 2; // Two m-time taken.
        }

        private void LoadMemoryFromRegister(ushort address, byte register)
        {
            cartridge.WriteByte(address, register);
            m = 2; // Two m-time taken.
        }

        private void LoadRegisterFromProgramCounter(ref byte register)
        {
            register = NextByte;
            m = 2; // Two m-time taken. Etc.
        }

        private void LoadMemoryFromProgramCounter(ushort address)
        {
            cartridge.WriteByte(address, NextByte);
            m = 2;
        }

        private void ZPLoadRegisterFromMemory(ref byte register, byte address)
        {
            register = cartridge.ReadByte((ushort)(0xFF00 + address));
            m = 3;
        }

        private void ZPLoadMemoryFromRegister(byte address, byte register)
        {
            cartridge.WriteByte((ushort)(0xFF00 + address), register);
            m = 3;
        }
        #endregion

        #region Bit level Op Functions

        private void TestBitAtAddress(ushort address, int bit)
        {
            TestBit(cartridge.ReadByte(address), bit);
            m = 4;
        }

        private void TestBit(byte subject, int bit)
        {
            ZeroFlag = (subject & (1 << bit)) == 0;
            SubtractionFlag = false;
            HalfCaryFlag = true;
            m = 2;
        }

        private void SetBitAtAddress(ushort address, int bit)
        {
            byte subject = cartridge.ReadByte(address);
            subject |= (byte)(1 << bit);
            cartridge.WriteByte(address, subject);
            m = 4;
        }

        private void SetBit(ref byte subject, int bit)
        {
            subject |= (byte)(1 << bit);
            m = 2;
        }

        private void ResetBitAtAddress(ushort address, int bit)
        {
            byte subject = cartridge.ReadByte(address);
            subject &= (byte)(~(1 << bit));
            cartridge.WriteByte(address, subject);
            m = 4;
        }

        private void ResetBit(ref byte subject, int bit)
        {
            subject &= (byte)(~(1 << bit));
            m = 2;
        }

        private void ANDAddressWithRegister(ref byte to, ushort address)
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

        private void ORAddressWithRegister(ref byte to, ushort address)
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

        private void XORAddressWithRegister(ref byte to, ushort address)
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

        #region Bit Rotation

        private void RotateRegisterLeft(ref byte register)
        {
            bool prevCarry = CarryFlag;
            byte copy = register;
            CarryFlag = (copy << 1) > 255;
            copy = (byte)(copy << 1);
            if (prevCarry) copy++;

            register = copy;

            SubtractionFlag = false;
            HalfCaryFlag = false;
            m = 1;
        }

        private void RotateRegisterRight(ref byte register)
        {
            bool prevCarry = CarryFlag;
            register = (byte)((register >> 1) | (register << 7));
            ZeroFlag = false;
            SubtractionFlag = false;
            HalfCaryFlag = false;
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


        private void IncrementAddress(ushort address)
        {
            byte data = cartridge.ReadByte(address);
            data++;
            HalfCaryFlag = (data & 0x0F) == 0x0F;
            SubtractionFlag = false;
            ZeroFlag = (data == 0);
            cartridge.WriteByte(address, data);
            m = 3;
        }

        private void DecrementAddress(ushort address)
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

        private void AddAddressToRegister(ref byte register, ushort address)
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

        private void AddAddressToRegisterWithCarry(ref byte to, ushort address)
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

        private void CompareAddress(byte to, ushort address)
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
                CarryFlag = (((int)to - (int)from) < 0);
                to -= from;
            }
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = true;
            m = 1;
        }

        private void SubtractAddressFromRegister(ref byte to, ushort address)
        {
            byte from = cartridge.ReadByte(address);
            CarryFlag = (((int)to - (int)from - 1) < 0);
            to -= from;
            ZeroFlag = (to == 0);
            HalfCaryFlag = (to & 0x0F) == 0x0F;
            SubtractionFlag = true;
            m = 1;
        }

        private void SubtractAddressFromRegisterWithCarry(ref byte to, ushort address)
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

        #region 16-bit Load/Store

        private void StoreStackPointerAtAddress()
        {
            ushort address = NextWord;
            cartridge.WriteByte(address++,(byte)(SP>>8));
            cartridge.WriteByte(address, (byte)(SP & 0xFF));
            m = 5;
        }

        private void PushWordToStack(ushort data)
        {
            cartridge.WriteByte(--SP, (byte)(data >> 8));
            cartridge.WriteByte(--SP, (byte)(data & 0xFF));

            m = 4;
        }

        private ushort PopWordFromStack()
        {
            byte lowByte = cartridge.ReadByte(SP++);
            byte highByte = cartridge.ReadByte(SP++);
            ushort registerPair = (ushort)(highByte << 8 | lowByte);
            m = 4;

            return registerPair;
        }

        private void PushPCtoStack()
        {
            cartridge.WriteByte(--SP, (byte)(PC >> 8));
            cartridge.WriteByte(--SP, (byte)(PC & 0xFF));

        }

        private void PopPCFromStack()
        {
            byte lowByte = cartridge.ReadByte(SP++);
            byte highByte = cartridge.ReadByte(SP++);

            PC = (ushort)(highByte << 8 | lowByte);
        }

        #endregion

        #region 16-bit Math/Logic

        private void AddRegisterPairToHL(ushort from)
        {
            CarryFlag = (((int)HL + (int)from) > 255);
            HL += from;
            SubtractionFlag = false;
            HalfCaryFlag = (HL & 0x0F) == 0x0F;
            m = 2;
        }

        #endregion

        #region Jumps/Calls

        private void JumpRelative()
        {
            JumpRelativeHelper();
            m = 3;
        }

        private void JumpRelativeZero()
        {
            if (ZeroFlag)
            {
                JumpRelativeHelper();
                m = 3;
            }
            else
            {
                PC++;
                m = 2;
            }
        }

        private void JumpRelativeCarry()
        {
            if (CarryFlag)
            {
                JumpRelativeHelper();
                m = 3;
            }
            else
            {
                PC++;
                m = 3;
            }
        }

        private void JumpRelativeNotZero()
        {
            if (!ZeroFlag)
            {
                JumpRelativeHelper();
                m = 3;
            }
            else
            {
                PC++;
                m = 2;
            }
        }

        private void JumpRelativeNotCarry()
        {
            if (!CarryFlag)
            {
                JumpRelativeHelper();
                m = 3;
            }
            else
            {
                PC++;
                m = 3;
            }
        }

        // Kind of hacky, but eh.
        private void JumpRelativeHelper()
        {
            int i = NextByte;
            if (i > 127)
                i = -((~i + 1) & 0xFF);
            int tempPC = PC;
            tempPC += i;
            tempPC &= 0xFFFF;
            PC = (ushort)tempPC;
        }

        private void AddRelativeToSP()
        {
            int i = NextByte;
            if (i > 127)
                i = -((~i + 1) & 0xFF);
            int tempSP = SP;
            tempSP += i;
            tempSP &= 0xFFFF;
            SP = (ushort)tempSP;
            ZeroFlag = false;
            SubtractionFlag = false;
            //Other emulators leave this op  out of changing these flags
            //HalfCaryFlag = (SP & 0x0F) == 0x0F;
            //CarryFlag = tempSP > 255;
            m = 4;
        }

        private void JumpFromMemory(ushort address)
        {
            PC = cartridge.ReadByte(address);
            m = 1;
        }

        private void JumpFromImmediate()
        {
            PC = NextWord;
            m = 4;
        }

        private void JumpIfNotZero()
        {
            if (!ZeroFlag)
            {
                PC = NextWord;
                m = 4;

            }
            else
            {
                PC += 2;
                m = 3;
            }
        }

        private void JumpIfZero()
        {
            if (ZeroFlag)
            {
                PC = NextWord;
                m = 4;

            }
            else
            {
                PC += 2;
                m = 3;
            }
        }

        private void JumpIfNotCarry()
        {
            if (!CarryFlag)
            {
                PC = NextWord;
                m = 4;
            }
            else
            {
                PC += 2;
                m = 3;
            }
        }

        private void JumpIfCarry()
        {
            if (CarryFlag)
            {
                PC = NextWord;
                m = 4;
            }
            else
            {
                PC += 2;
                m = 3;
            }
        }

        private void CallRST(byte vector)
        {
            PushPCtoStack();
            PC = (ushort)vector;
            m = 4;
        }

        private void Call()
        {
            PushPCtoStack();
            PC = NextWord;
            m = 6;
        }

        private void CallNotZero()
        {
            if (!ZeroFlag)
            {
                PushPCtoStack();
                PC = NextWord;
                m = 6;
            }
            else
            {
                PC += 2;
                m = 4;
            }
        }

        private void CallZero()
        {
            if (ZeroFlag)
            {
                PushPCtoStack();
                PC = NextWord;
                m = 6;
            }
            else
            {
                PC += 2;
                m = 4;
            }
        }

        private void CallNotCarry()
        {
            if (!CarryFlag)
            {
                PushPCtoStack();
                PC = NextWord;
                m = 6;
            }
            else
            {
                PC += 2;
                m = 4;
            }
        }

        private void CallCarry()
        {
            if (CarryFlag)
            {
                PushPCtoStack();
                PC = NextWord;
                m = 6;
            }
            else
            {
                PC += 2;
                m = 4;
            }
        }

        private void Return()
        {
            PopPCFromStack();
            m = 4;
        }

        private void ReturnFromI()
        {
            PopPCFromStack();
            m = 4;
        }

        private void ReturnNotZero()
        {
            if (!ZeroFlag)
            {
                PopPCFromStack();
                m = 5;
            }
            else
            {
                m = 2;
            }
        }

        private void ReturnZero()
        {
            if (ZeroFlag)
            {
                PopPCFromStack();
                m = 5;
            }
            else
            {
                m = 2;
            }
        }

        private void ReturnNotCarry()
        {
            if (!CarryFlag)
            {
                PopPCFromStack();
                m = 5;
            }
            else
            {
                m = 2;
            }
        }

        private void ReturnCarry()
        {
            if (CarryFlag)
            {
                PopPCFromStack();
                m = 5;
            }
            else
            {
                m = 2;
            }
        }

        #endregion

        internal void Step()
        {
            byte op = NextByte;
            switch (op)
            {
                #region No Op
                // Real NOP
                case 0x00:
                    { NoOp(); break; }

                // Removed/undefined opcode.  Fake NOP
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

                #endregion

                #region Jumps and calls

                // Three byte ops
                case 0xC2: { JumpIfNotZero(); break; }
                case 0xC3: { JumpFromImmediate(); break; }
                case 0xC4: { CallNotZero(); break; }
                case 0xCA: { JumpIfZero(); break; }
                case 0xCC: { CallZero(); break; }
                case 0xCD: { Call(); break; }
                case 0xD2: { JumpIfNotCarry(); break; }
                case 0xD4: { CallNotCarry(); break; }
                case 0xDA: { JumpIfCarry(); break; }
                case 0xDC: { CallCarry(); break; }

                // Two byte ops
                case 0x18: { JumpRelative(); break; }
                case 0x20: { JumpRelativeNotZero(); break; }
                case 0x28: { JumpRelativeZero(); break; }
                case 0x30: { JumpRelativeNotCarry(); break; }
                case 0x38: { JumpRelativeCarry(); break; }

                // One byte ops
                case 0xC0: { ReturnNotZero(); break; }
                case 0xC7: { CallRST(0x00); break; }
                case 0xC8: { ReturnZero(); break; }
                case 0xC9: { Return(); break; }
                case 0xCF: { CallRST(0x08); break; }
                case 0xD0: { ReturnNotCarry(); break; }
                case 0xD7: { CallRST(0x10); break; }
                case 0xD8: { ReturnZero(); break; }
                case 0xD9: { ReturnFromI(); break; }
                case 0xDF: { CallRST(0x18); break; }
                case 0xE7: { CallRST(0x20); break; }
                case 0xE9: { JumpFromMemory(HL); break; }
                case 0xEF: { CallRST(0x28); break; }
                case 0xF7: { CallRST(0x30); break; }
                case 0xFF: { CallRST(0x38); break; }

                #endregion

                #region CB-Ops
                // Extended Bit-level Op table
                case 0xCB:
                    {
                        byte extOP = NextByte;
                        switch (extOP)
                        {
                            case 0x11: { RotateRegisterLeft(ref c); if (c == 0)ZeroFlag = true; m++; break; }
                            case 0x7C: { TestBit(h, 7); break; }
                            default:
                                {
                                    {
                                        throw new NotImplementedException("Missing Extended (CB) Opcode: " + extOP.ToString("X2") + " PC: " + PC.ToString("X4"));
                                    }
                                }
                        }
                        break;
                    }
                #endregion

                #region Misc/Control Ops
                // Misc/Control Instructions
                case 0x10:
                    {
                        //CPU STOP
                        PC++; // FIXME: Maybe only one byte?
                        m = 1;
                        break;
                    }
                case 0x76:
                    {
                        Halted = true;
                        m = 1;
                        break;
                    }

                case 0xF3:
                    {
                        InteruptsEnabled = false;
                        m = 1;
                        break;
                    }
                case 0xFB:
                    {
                        InteruptsEnabled = true;
                        m = 1;
                        break;
                    }
                #endregion

                #region 8-bit Math/Logic

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
                case 0x96: { SubtractAddressFromRegister(ref a, HL); break; }
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
                // I reuse the register+register ops here and just add another cycle.
                // The operations are equal otherwise.
                case 0xC6: { AddRegisterToRegister(ref a, NextByte); m++; break; }
                case 0xCE: { AddRegisterToRegisterWithCarry(ref a, NextByte); m++; break; }
                case 0xD6: { SubtractRegisterFromRegister(ref a, NextByte); m++; break; }
                case 0xDE: { SubtractRegisterFromRegisterWithCarry(ref a, NextByte); m++; break; }
                case 0xE6: { ANDRegisterWithRegister(ref a, NextByte); m++; break; }
                case 0xEE: { XORRegisterWithRegister(ref a, NextByte); m++; break; }
                case 0xF6: { ORRegisterWithRegister(ref a, NextByte); m++; break; }
                case 0xFE: { CompareRegister(a, NextByte); m++; break; }
                #endregion

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

                #region 16-bit Load/Store/Move

                // Three byte ops
                case 0x01: { BC = NextWord; m = 3; break; }
                case 0x08: { StoreStackPointerAtAddress(); break; }
                case 0x11: { DE = NextWord; m = 3; break; }
                case 0x21: { HL = NextWord; m = 3; break; }
                case 0x31: { SP = NextWord; m = 3; break; }

                // Two byte op
                // Load HL with SP+8bit immediate.
                // FIXME: Wrong carry flag check?
                case 0xF8:
                    {
                        HL = (ushort)(SP + NextByte);
                        ZeroFlag = false;
                        SubtractionFlag = false;
                        HalfCaryFlag = (HL & 0x0F) == 0x0F;
                        CarryFlag = HL > 255;
                        m = 2;
                        break;
                    }

                // One byte ops
                case 0xC1: { BC = PopWordFromStack(); break; }
                case 0xC5: { PushWordToStack(BC); break; }
                case 0xD1: { DE = PopWordFromStack(); break; }
                case 0xD5: { PushWordToStack(DE); break; }
                case 0xE1: { HL = PopWordFromStack(); break; }
                case 0xE5: { PushWordToStack(HL); break; }
                case 0xF1: { AF = PopWordFromStack(); break; }
                case 0xF5: { PushWordToStack(AF); break; }
                case 0xF9:
                    { // Load SP from HL direct.
                        SP = HL;
                        m = 2;
                        break;
                    }

                #endregion

                #region 16-bit Math/Logic

                // One byte ops
                case 0x03: { BC++; m = 2; break; }
                case 0x09: { AddRegisterPairToHL(BC); break; }
                case 0x0B: { BC--; m = 2; break; }
                case 0x13: { DE++; m = 2; break; }
                case 0x19: { AddRegisterPairToHL(DE); break; }
                case 0x1B: { DE--; m = 2; break; }
                case 0x23: { HL++; m = 2; break; }
                case 0x29: { AddRegisterPairToHL(HL); break; }
                case 0x2B: { HL--; m = 2; break; }
                case 0x33: { SP++; m = 2; break; }
                case 0x39: { AddRegisterPairToHL(SP); break; }
                case 0x3B: { SP--; m = 2; break; }

                // Two byte op
                case 0xE8: { AddRelativeToSP(); break; }

                #endregion

                // Non-CB bit level ops
                case 0x07:
                case 0x0F:
                    {
                        throw new NotImplementedException("Missing Opcode: " + op.ToString("X2"));
                    }
                case 0x17: { RotateRegisterLeft(ref a); break; }
                case 0x1F: //{ RotateRegisterRight(ref a); break; }


                // Shit has become real
                default:
                    {
                        Crashed = true;
                        break;
                    }
            }
        }
    }
}
