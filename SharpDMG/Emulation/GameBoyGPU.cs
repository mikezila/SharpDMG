using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpDMG.Cartridge;

namespace SharpDMG.Emulation
{
    class GameBoyGPU
    {
        public Bitmap FrameBuffer { get; private set; }
        private Graphics g;

        public Mode GPUMode { get; private set; }
        int ModeClock { get; set; }
        int Line { get; set; }

        // Interrupts
        public bool VBlankFired { get; set; }
        public bool HBlankFired { get; set; }

        // Scroll registers
        public int ScrollX { get; set; }
        public int ScrollY { get; set; }

        // So that they can be easily changed later.
        private Color GBBlack = Color.Black;
        private Color GBDarkGray = Color.DarkGray;
        private Color GBLightGray = Color.LightGray;
        private Color GBWhite = Color.White;

        // Tile set
        private GBTile[] TileSet { get; set; }

        // Palette
        private Color[] Palette { get; set; }

        // Main memory
        private ICartridge Memory { get; set; }

        public enum Mode
        {
            ScanLineOAM = 2,
            ScanLineVRAM = 3,
            HBlank = 0,
            VBlank = 1
        }

        public GameBoyGPU(ICartridge memory)
        {
            Memory = memory;
            Reset();
        }

        private void Reset()
        {
            FrameBuffer = new Bitmap(160, 144);
            g = Graphics.FromImage(FrameBuffer);
            TileSet = new GBTile[385];
            GPUMode = Mode.HBlank;
            ModeClock = 0;
            Line = 0;

            VBlankFired = false;
            HBlankFired = false;

            Palette = new Color[4];

            Palette[0] = GBWhite;
            Palette[1] = GBLightGray;
            Palette[2] = GBDarkGray;
            Palette[3] = GBBlack;

            for (int i = 0; i < 384; i++)
            {
                TileSet[i] = new GBTile();
            }

        }

        private static bool TestBit(byte subject, int bit)
        {
            return (subject & (1 << bit)) != 0;
        }

        public void UpdatePalette()
        {
            byte colors = Memory.ReadByte(0xFF47);

            for (int i = 0; i < 4; i++)
            {
                if (TestBit(colors, 0 + (i * 2)) && TestBit(colors, 1 + (i * 2)))
                    Palette[i] = GBBlack;
                else if (TestBit(colors, 0 + (i * 2)))
                    Palette[i] = GBLightGray;
                else if (TestBit(colors, 1 + (i * 2)))
                    Palette[i] = GBDarkGray;
                else
                    Palette[i] = GBWhite; 
            }
        }

        internal void Step(int ticks)
        {
            RenderScan();
            return;
            ModeClock += ticks;

            switch (GPUMode)
            {
                case Mode.ScanLineOAM:
                    {
                        if (ModeClock >= 80)
                        {
                            ModeClock = 0;
                            GPUMode = Mode.ScanLineVRAM;
                        }
                        break;
                    }
                case Mode.ScanLineVRAM:
                    {
                        if (ModeClock >= 172)
                        {
                            ModeClock = 0;
                            GPUMode = Mode.HBlank;
                            HBlankFired = true;
                            RenderScan();
                        }
                        break;
                    }
                case Mode.HBlank:
                    {
                        if (ModeClock >= 204)
                        {
                            ModeClock = 0;
                            Line++;

                            if (Line == 143)
                            {
                                GPUMode = Mode.VBlank;
                                VBlankFired = true;
                            }
                        }
                        else
                        {
                            GPUMode = Mode.ScanLineOAM;
                        }

                        break;
                    }
                case Mode.VBlank:
                    {
                        if (ModeClock >= 456)
                        {
                            ModeClock = 0;
                            Line++;

                            if (Line > 153)
                            {
                                GPUMode = Mode.HBlank;
                                Line = 0;
                            }
                        }
                        break;
                    }
            }
        }

        private void UpdateTiles()
        {
            for (int i = 0; i < 384; i++)
            {
                TileSet[i].UpdateRaster(Memory.ReadVRAMTile(i), Palette);
            }
        }

        private Bitmap UpdateBackgroundMap()
        {
            Bitmap background = new Bitmap(256, 256);
            Graphics g = Graphics.FromImage(background);
            int tile = 0;
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                {
                    g.DrawImage(TileSet[Memory.ReadByte((ushort)(0x9800 + tile))].Raster, new Point(y * 8, x * 8));
                    tile++;
                }
            g.Dispose();
            return background;
        }

        private void RenderScan()
        {
            UpdatePalette();
            UpdateTiles();
            
            g.DrawImage(UpdateBackgroundMap(), ScrollX, ScrollY);
        }
    }
}
