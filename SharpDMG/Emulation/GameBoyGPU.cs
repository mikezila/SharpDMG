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
        Mode GPUMode { get; set; }
        int ModeClock { get; set; }
        int Line { get; set; }
        public bool NewFrame { get; private set; }

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

        enum Mode
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
            TileSet = new GBTile[385];
            BlackScreen();
            GPUMode = Mode.HBlank;
            ModeClock = 0;
            NewFrame = false;

            Palette = new Color[4];

            Palette[0] = GBWhite;
            Palette[1] = GBLightGray;
            Palette[2] = GBDarkGray;
            Palette[3] = GBBlack;
        }

        public void UpdatePalette(byte colors)
        {
            throw new NotImplementedException();
        }

        internal void Step(int ticks)
        {
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
                                NewFrame = true;
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

        private void BlackScreen()
        {
            for (int y = 0; y < FrameBuffer.Height; y++)
                for (int x = 0; x < FrameBuffer.Width; x++)
                    FrameBuffer.SetPixel(x, y, Color.Black);
        }

        private void UpdateTiles()
        {
            for (int i = 0; i < 384; i++)
            {
                TileSet[i] = new GBTile();
                TileSet[i].Pixels = Memory.ReadVRAMTile(i);
                TileSet[i].UpdateRaster(Palette);
                //TileSet[i].Raster.Save("tiledump/" + i + ".bmp");
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
            return background;
        }

        private void RenderScan()
        {
            UpdateTiles();
            Graphics g = Graphics.FromImage(FrameBuffer);
            g.DrawImage(UpdateBackgroundMap(), ScrollX, ScrollY);
        }
    }
}
