using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SharpDMG.Emulation
{
    class GBTile
    {
        public byte[] Pixels { get; private set; }
        private Color[] prevPalette { get; set; }
        public Bitmap Raster { get; private set; }

        public bool Dirty { get; set; }

        public GBTile()
        {
            Raster = new Bitmap(8, 8);
            Pixels = new byte[16];
            prevPalette = new Color[4];
        }

        //Row is zero-indexed, from the top.  So 0-7.
        public Bitmap GetRow(int row)
        {
            Bitmap pixels = new Bitmap(8, 1);

            for (int x = 0; x < 8; x++)
                pixels.SetPixel(x, 0, Raster.GetPixel(x, row));

            return pixels;
        }

        private static bool TestBit(byte subject, int bit)
        {
            return (subject & (1 << bit)) != 0;
        }

        public void UpdateRaster(byte[] rawPixels, Color[] palette)
        {
            Dirty = (!Enumerable.SequenceEqual(Pixels, rawPixels) || !Enumerable.SequenceEqual(prevPalette, palette));

            if (!Dirty)
                return;

            Pixels = rawPixels;
            prevPalette = palette;

            // Work out the intensity (0-3) of a pixel based on the bytes and store that
            // in an array we can use to fill the bitmap using a palette.
            int[] colors = new int[64];
            int pixel = 0;
            for (int line = 0; line < 16; line += 2)
            {
                for (int bit = 7; bit >= 0; bit--)
                {
                    if (TestBit(Pixels[line], bit) && TestBit(Pixels[line + 1], bit))
                        colors[pixel++] = 3;
                    else if (TestBit(Pixels[line + 1], bit))
                        colors[pixel++] = 2;
                    else if (TestBit(Pixels[line], bit))
                        colors[pixel++] = 1;
                    else
                        colors[pixel++] = 0;
                }
            }

            pixel = 0;

            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    Raster.SetPixel(x, y, palette[colors[pixel++]]);
                }

            Dirty = false;
        }
    }
}
