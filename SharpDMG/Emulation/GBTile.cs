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
        public byte[] Pixels { get; set; }
        public Bitmap Raster { get; private set; }

        public GBTile()
        {
            Raster = new Bitmap(8, 8);
            Pixels = new byte[16];
        }

        private static bool TestBit(byte subject, int bit)
        {
            return (subject & (1 << bit)) != 0;
        }

        public void UpdateRaster(Color[] palette)
        {
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
        }
    }
}
