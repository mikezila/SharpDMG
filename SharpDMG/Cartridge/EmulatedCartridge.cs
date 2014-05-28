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

        public EmulatedCartridge(string filename)
        {
            byte[] ROM = File.ReadAllBytes(filename);
            if (ROM[0x0147] == (byte)MBCType.RomOnly)
                cartridge = new RomOnlyCartridge(ROM);
            else
                throw new NotImplementedException("Only RomOnly emulated cartridges are supported for now.  Try using Tetris.");
        }

        public byte ReadByte(int address)
        {
            return cartridge.ReadByte(address);
        }

        public void WriteByte(int address, byte data)
        {
            cartridge.WriteByte(address, data);
        }
    }

    // No MBC or extra RAM.  Like Tetris.
    class RomOnlyCartridge : ICartridge
    {
        private byte[] ROM { get; set; }
        private byte[] RAM { get; set; }

        public RomOnlyCartridge(byte[] rom)
        {
            this.ROM = rom;
            RAM = new byte[0x2000];
        }

        public byte ReadByte(int address)
        {
            if (address < 0x8000)
                return ROM[address];
            else if (address >= 0xC000 && address < 0xE000)
                return RAM[address - 0xC000];
            else
                throw new Exception("Non ROM read made it through to cartridge.");
        }

        public void WriteByte(int address, byte data)
        {
            if (address >= 0xC000 && address < 0xE000)
                RAM[address - 0xC000] = data;
            else
                throw new Exception("RAM write was out of range, or MBC switch was tried on non-MBC cartridge.");
        }
    }
}
