using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDMG.Cartridge
{
    public enum SuperGameBoyFunctions : byte
    {
        GameBoy = 0x00,
        SuperGameBoy = 0x03
    }

    public enum GameBoyColorFunctions : byte
    {
        GameBoy = 0x00,
        GameBoyColor = 0x80
    }

    public enum ROMBankCount : byte
    {
        Two = 0x00,
        Four = 0x01,
        Eight = 0x02,
        Sixteen = 0x03,
        ThirtyTwo = 0x04,
        SixtyFour = 0x05,
        OneTwentyEight = 0x06,
        SeventyTwo = 0x52,
        Eighty = 0x53,
        NinteySix = 0x54
    }

    public enum RAMBankCount : byte
    {
        None = 0x00,
        One2KB = 0x01,
        One8KB = 0x02,
        Four = 0x03,
        Sixteen = 0x04
    }

    public enum Region : byte
    {
        Japan = 0x00,
        World = 0x01
    }

    public enum MBCType : byte
    {
        RomOnly = 0x00,
        MBC1 = 0x01,
        MBC1Ram = 0x02,
        MBC1RamBattery = 0x05,
        MBC2 = 0x05,
        MBC2Batter = 0x06,
        ROMRam = 0x08,
        ROMRamBattery = 0x09,
        MMM01 = 0x0B,
        MMM01SRAM = 0x0C,
        MMM01SRAMBattery = 0x0D,
        MBC3TimerBattery = 0x0F,
        MBC3TimerRamBattery = 0x10,
        MBC3 = 0x11,
        MBC3Ram = 0x12,
        MBC3RamBattery = 0x013,
        MBC5 = 0x19,
        MBC5Ram = 0x1A,
        MBC5RamBattery = 0x1B,
        MBC5Rumble = 0x1C,
        MBC5RumbleSRAM = 0x1D,
        MBC5RumbleSRAMBattery = 0x1E,
        PocketCamera = 0x1F,
        BandaiTAMA5 = 0xFD,
        HudsonHuC3 = 0xFE,
        HudsonHuC1 = 0XFF
    }

    interface ICartridge
    {
        byte ReadByte(ushort address);
        void WriteByte(ushort address, byte data);
        byte[] ReadVRAMTile(int tileIndex);

        byte[] VRAM { get; }
    }
}
