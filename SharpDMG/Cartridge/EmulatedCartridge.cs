using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpDMG.Cartridge
{
    class EmulatedCartridge : ICartridge
    {
        private ICartridge cartridge;

        public byte[] VRAM { get { return new byte[1]; } }
        public byte[] OAM { get { return new byte[1]; } }

        public EmulatedCartridge(string filename)
        {
            byte[] ROM = File.ReadAllBytes(filename);
            if (ROM[0x0147] == (byte)MBCType.RomOnly)
                cartridge = new RomOnlyCartridge(ROM);
            else
                throw new NotImplementedException("Only RomOnly emulated cartridges are supported for now.  Try using Tetris.");
        }

        public byte ReadByte(ushort address)
        {
            return cartridge.ReadByte(address);
        }

        public void WriteByte(ushort address, byte data)
        {
            cartridge.WriteByte(address, data);
        }

        public void WriteWord(ushort address, ushort data)
        {
            cartridge.WriteWord(address, data);
        }

        public byte[] ReadVRAMTile(int tileIndex)
        {
            byte[] data = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                data[i] = cartridge.VRAM[i + (16 * tileIndex)];
            }
            return data;
        }

    }

    // No MBC or extra RAM.  Like Tetris.
    class RomOnlyCartridge : ICartridge
    {
        private byte[] ROM { get; set; }
        private byte[] RAM { get; set; }
        private byte[] ZeroPage { get; set; }
        public byte[] VRAM { get; set; }
        public byte[] OAM { get; set; }

        public RomOnlyCartridge(byte[] rom)
        {
            this.ROM = rom;

            // Internal RAM only, no extra in cartridge.
            RAM = new byte[0x2000];
            ZeroPage = new byte[128];
            //VRAM = new byte[0x2000];
            VRAM = File.ReadAllBytes("tetris.dump");
            OAM = new byte[160];
        }

        public byte[] ReadVRAMTile(int tileIndex)
        {
            return new byte[1];
        }

        public byte ReadByte(ushort address)
        {
            if (address < 0x8000)
                return ROM[address];
            else if (address >= 0xC000 && address < 0xE000)
                return RAM[address - 0xC000];
            else if (address >= 0xFF80 && address < 0xFFFF)
                return ZeroPage[address - 0xFF80];
            else if (address >= 0x8000 && address < 0xA000)
                return VRAM[address - 0x8000];
            else if (address >= 0xFE00 && address < 0xFEA0)
                return OAM[address - 0xFE00];
            else
                throw new Exception("ROM/RAM read out of range.");
        }

        public void WriteWord(ushort address, ushort data)
        {
            WriteByte(address, (byte)(data & 0xFF));
            WriteByte((ushort)(address - 1), (byte)(data >> 8));
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
            else
                throw new Exception("RAM write was out of range, or MBC switch was tried on non-MBC cartridge.");
        }
    }
}
