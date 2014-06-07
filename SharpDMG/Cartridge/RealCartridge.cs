using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace SharpDMG.Cartridge
{
    // This provides a link to a real GameBoy cartridge.

    public class RealCartridge : ICartridge, IDisposable
    {
        private byte[] RAM { get; set; }
        private byte[] ZeroPage { get; set; }
        public byte[] VRAM { get; private set; }
        private byte[] OAM { get; set; }
        private byte[] HARDWAREIO { get; set; }
        private byte InterruptRegister { get; set; }

        private byte[] BIOS { get; set; }

        private bool biosActive = true;

        private SerialPort cartridge;

        // Number of bytes to request at a time when
        // requesting more than one byte at a time.
        const int blockSize = 128;

        public RealCartridge(string portName = "COM3")
        {
            cartridge = new SerialPort(portName, 9600);
            this.Open();

            SwitchBank(1);

            // Internal RAM only, no extra in cartridge.
            RAM = new byte[0x2000];
            ZeroPage = new byte[128];
            VRAM = new byte[0x2000];
            OAM = new byte[160];
            HARDWAREIO = new byte[128];
            BIOS = File.ReadAllBytes("bios.bin");
        }

        private void SwitchBank(int bank)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0x00;
            buffer[1] = 0x00;
            buffer[2] = (byte)bank;
            buffer[3] = 0x04;
            cartridge.Write(buffer, 0, 4);
        }

        private void SwitchRamBank(int bank)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0x00;
            buffer[1] = 0x00;
            buffer[2] = (byte)bank;
            buffer[3] = 0x06;
            cartridge.Write(buffer, 0, 4);
        }

        private byte[] ReadBytes(ushort address, int count)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)address;
            buffer[1] = (byte)(address >> 8);
            buffer[2] = (byte)count;
            buffer[3] = 0x01;
            cartridge.Write(buffer, 0, 4);
            byte[] incoming = new byte[count];
            for (int i = 0; i < incoming.Length; i++)
                incoming[i] = (byte)cartridge.ReadByte();

            return incoming;
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
                return ReadCartridgeByte(address);
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

        public byte ReadCartridgeByte(ushort address)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)address;
            buffer[1] = (byte)(address >> 8);
            buffer[2] = 0x00;
            buffer[3] = 0x02;
            cartridge.Write(buffer, 0, 4);
            byte data = (byte)cartridge.ReadByte();

            //Console.WriteLine("Read byte: " + data.ToString("X2") + " at " + address.ToString("X4"));

            return data;
        }

        public void WriteCartridgeByte(ushort address, byte data)
        {
            throw new NotImplementedException("Writing bytes to real cartridge is not supported yet.");
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            if (cartridge.IsOpen)
                cartridge.Close();
        }

        public void Open()
        {
            if (!cartridge.IsOpen)
                cartridge.Open();
        }
    }
}
