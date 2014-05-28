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
        public string GameName { get; private set; }
        public int RomSize { get; private set; }
        private SerialPort cartridge;

        // Number of bytes to request at a time when
        // requesting more than one byte at a time.
        const int blockSize = 128;

        public RealCartridge(string portName = "COM3")
        {
            cartridge = new SerialPort(portName, 57600);
        }

        private string ReadGamename()
        {
            const int TITLE_LENGTH = 16;

            byte[] rawName = ReadBytes(0x0134, TITLE_LENGTH);

            return System.Text.Encoding.UTF8.GetString(rawName).Trim();
        }

        private void DumpRamBank()
        {
            List<byte> dumpedRom = new List<byte>();
            for (int i = 0xA000; i < 0xBFFF; i += blockSize)
                dumpedRom.AddRange(ReadBytes(i, blockSize));
            File.WriteAllBytes("dump.gb", dumpedRom.ToArray());
        }

        private void DumpBank0()
        {
            List<byte> dumpedRom = new List<byte>();
            for (int i = 0x000; i < 0x3FFF; i += blockSize)
                dumpedRom.AddRange(ReadBytes(i, blockSize));
            File.WriteAllBytes("dump.gb", dumpedRom.ToArray());
        }

        private void DumpSwitchedBank(int bank)
        {
            SwitchBank(bank);

            List<byte> dumpedBank = new List<byte>();
            for (int i = 0x4000; i < 0x7FFF; i += blockSize)
                dumpedBank.AddRange(ReadBytes(i, blockSize));

            using (var RomFile = File.OpenWrite("dump.gb"))
            {
                RomFile.Seek(0, SeekOrigin.End);
                foreach (byte gbByte in dumpedBank)
                {
                    RomFile.WriteByte(gbByte);
                }
            }
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

        private byte[] ReadBytes(int address, int count)
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

        public byte ReadByte(int address)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)address;
            buffer[1] = (byte)(address >> 8);
            buffer[2] = 0x00;
            buffer[3] = 0x02;
            cartridge.Write(buffer, 0, 4);
            byte data = (byte)cartridge.ReadByte();
            return data;
        }

        public void WriteByte(int address, byte data)
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
