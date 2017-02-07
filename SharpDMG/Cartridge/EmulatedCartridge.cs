using System;
using System.IO;

namespace SharpDMG.Cartridge
{
    // No MBC or extra RAM.  Like Tetris.
    class EmulatedCartridge : ICartridge
    {
        private byte[] ROM { get; set; }
        private byte[] RAM { get; set; }
        private byte[] ZeroPage { get; set; }
        public byte[] VRAM { get; private set; }
        private byte[] OAM { get; set; }
        private byte[] HARDWAREIO { get; set; }
        private byte InterruptRegister { get; set; }

        private byte[] BIOS { get; set; }

        private bool biosActive = true;

        public EmulatedCartridge(string path)
        {
            ROM = File.ReadAllBytes(path);

            // Internal RAM only, no extra in cartridge.
            RAM = new byte[0x2000];
            ZeroPage = new byte[128];
            VRAM = new byte[0x2000];
            OAM = new byte[160];
            HARDWAREIO = new byte[128];
            BIOS = File.ReadAllBytes("bios.bin");
        }

        public byte[] ReadVRAMTile(int tileIndex)
        {
            byte[] data = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                data[i] = VRAM[i + (16 * tileIndex)];
            }
            return data;
        }

        public byte ReadByte(ushort address)
        {
            if (biosActive && address < 0x100)
                return BIOS[address];
            else if (address < 0x8000)
                return ROM[address];
            else if (address >= 0xC000 && address < 0xE000)
                return RAM[address - 0xC000];
            else if (address >= 0xFF80 && address < 0xFFFF)
                return ZeroPage[address - 0xFF80];
            else if (address >= 0x8000 && address < 0xA000)
                return VRAM[address - 0x8000];
            else if (address >= 0xFE00 && address < 0xFEA0)
                return OAM[address - 0xFE00];
            else if (address >= 0xFF00 && address < 0xFF80)
                return HARDWAREIO[address - 0xFF00];
            else if (address == 0xFFFF)
                return InterruptRegister;
            else
                throw new Exception("ROM/RAM read out of range.");
        }

        public void WriteByte(ushort address, byte data)
        {
            if (address >= 0xC000 && address < 0xE000)
                RAM[address - 0xC000] = data;
            else if (address >= 0xFF80 && address < 0xFFFF)
                ZeroPage[address - 0xFF80] = data;
            else if (address >= 0x8000 && address < 0xA000)
                VRAM[address - 0x8000] = data;
            else if (address >= 0xFE00 && address < 0xFEA0)
                OAM[address - 0xFE00] = data;
            else if (address >= 0xFF00 && address < 0xFF80)
                HARDWAREIO[address - 0xFF00] = data;
            else if (address == 0xFFFF)
                InterruptRegister = data;
            else
                throw new Exception("RAM write was out of range, or MBC switch was tried on non-MBC cartridge.");
        }
    }
}
