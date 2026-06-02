namespace RP.Math.Sample.SpaceFlight
{
    using System;
    using System.Drawing;            // GDI: Bitmap, Font, Graphics, Color, Rectangle, SizeF ...
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Microsoft.Xna.Framework.Graphics;

    // Alias the MonoGame core types we need so they never collide with the System.Drawing ones above.
    using XColor = Microsoft.Xna.Framework.Color;
    using XVec2 = Microsoft.Xna.Framework.Vector2;
    using XRect = Microsoft.Xna.Framework.Rectangle;

    /// <summary>
    /// A tiny monospaced bitmap font rasterised at runtime with System.Drawing, packed into a
    /// <see cref="Texture2D"/> atlas. This lets the telemetry HUD draw text without the XNA content
    /// pipeline, so the sample builds and runs with a bare <c>dotnet run</c>.
    /// </summary>
    internal sealed class BitmapFont : IDisposable
    {
        private const int First = 32, Last = 126, Columns = 16;
        private readonly Texture2D _atlas;
        private readonly int _cellW, _cellH;

        public int LineHeight => _cellH;
        public int CharWidth => _cellW;

        public BitmapFont(GraphicsDevice device, float pixelSize = 16f)
        {
            using var family = MonospaceFamily();
            using var font = new Font(family, pixelSize, FontStyle.Regular, GraphicsUnit.Pixel);

            // Measure a representative glyph for the (monospaced) cell size.
            using (var probe = new Bitmap(1, 1))
            using (var pg = Graphics.FromImage(probe))
            {
                SizeF s = pg.MeasureString("M", font);
                _cellW = (int)Math.Ceiling(s.Width) + 2;
                _cellH = (int)Math.Ceiling(font.GetHeight(pg)) + 2;
            }

            int count = Last - First + 1;
            int rows = (count + Columns - 1) / Columns;
            int w = Columns * _cellW, h = rows * _cellH;

            using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black); // white text on black; we read luminance back as alpha
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                using var brush = new SolidBrush(Color.White);
                for (int i = 0; i < count; i++)
                {
                    int col = i % Columns, row = i / Columns;
                    g.DrawString(((char)(First + i)).ToString(), font, brush, col * _cellW + 1, row * _cellH + 1);
                }
            }

            _atlas = ToTexture(device, bmp);
        }

        private static FontFamily MonospaceFamily()
        {
            foreach (var name in new[] { "Consolas", "Cascadia Mono", "Lucida Console", "Courier New" })
            {
                try { return new FontFamily(name); }
                catch (ArgumentException) { /* not installed, try next */ }
            }
            return new FontFamily(System.Drawing.Text.GenericFontFamilies.Monospace);
        }

        private static Texture2D ToTexture(GraphicsDevice device, Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int n = bmp.Width * bmp.Height;
                byte[] bgra = new byte[n * 4];
                Marshal.Copy(data.Scan0, bgra, 0, bgra.Length);

                var pixels = new XColor[n];
                for (int i = 0; i < n; i++)
                {
                    int lum = bgra[i * 4 + 2]; // R channel of white-on-black text == coverage
                    pixels[i] = new XColor(255, 255, 255, lum);
                }

                var tex = new Texture2D(device, bmp.Width, bmp.Height, false, SurfaceFormat.Color);
                tex.SetData(pixels);
                return tex;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>Draw a (possibly multi-line) string. Call inside a <c>SpriteBatch.Begin/End</c> with non-premultiplied alpha.</summary>
        public void Draw(SpriteBatch batch, string text, XVec2 pos, XColor color, float scale = 1f)
        {
            float x = pos.X, y = pos.Y;
            foreach (char ch in text)
            {
                if (ch == '\n') { x = pos.X; y += _cellH * scale; continue; }
                if (ch >= First && ch <= Last)
                {
                    int idx = ch - First;
                    var src = new XRect((idx % Columns) * _cellW, (idx / Columns) * _cellH, _cellW, _cellH);
                    batch.Draw(_atlas, new XVec2(x, y), src, color, 0f, XVec2.Zero, scale, SpriteEffects.None, 0f);
                }
                x += _cellW * scale;
            }
        }

        public void Dispose() => _atlas?.Dispose();
    }
}
