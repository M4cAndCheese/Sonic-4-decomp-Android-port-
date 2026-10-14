// Texturas HD (DDS DXT1/DXT5) empaquetadas en Content/HD/<SHA1>.DDS.
// La clave es el SHA-1 de los bytes del PNG original dentro del AMB (desde la cabecera PNG hasta IEND).
// Así cada textura se identifica por su contenido, y no hay ambigüedad entre zonas con el mismo nombre.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Android.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class HdTextures
{
    public static bool Enabled = true;

    // Las DDS del paquete iPad vienen con las filas invertidas respecto al PNG del AMB.
    public static bool FlipY = true;

    // Texturas (SHA1 del PNG) que se dejan sin voltear. Vacío = todas se voltean.
    private static readonly HashSet<string> NoFlip = new HashSet<string>
    {
    };

    private static void FlipRows(Color[] px, int w, int h)
    {
        for (int y = 0; y < h / 2; y++)
        {
            int a = y * w, b = (h - 1 - y) * w;
            for (int x = 0; x < w; x++)
            {
                Color t = px[a + x];
                px[a + x] = px[b + x];
                px[b + x] = t;
            }
        }
    }

    private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
    private static readonly HashSet<string> missing = new HashSet<string>();
    private static readonly byte[] IEND = { (byte)'I', (byte)'E', (byte)'N', (byte)'D' };

    // data: bytes del AMB; offset: inicio del PNG dentro de data (amb.offsets[i]).
    public static Texture2D TryLoadPng(GraphicsDevice gd, byte[] data, int offset)
    {
        if (!Enabled || gd == null || data == null || offset < 0 || offset >= data.Length) return null;

        int end = IndexOf(data, IEND, offset);
        if (end < 0) return null;
        end += 8; // IEND + CRC
        if (end > data.Length) return null;

        string key;
        using (var sha = SHA1.Create())
        {
            key = BitConverter.ToString(sha.ComputeHash(data, offset, end - offset)).Replace("-", "");
        }

        Texture2D tex;
        if (cache.TryGetValue(key, out tex)) return tex;
        if (missing.Contains(key)) return null;

        try
        {
            using (Stream s = GameData.OpenStream("HD/" + key + ".DDS"))
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                tex = Decode(gd, ms.ToArray(), FlipY && !NoFlip.Contains(key));
            }
        }
        catch (Exception)
        {
            tex = null;
        }

        // Diagnóstico: ver con "adb logcat -s S4HD" si la textura HD se aplica.
        Log.Info("S4HD", (tex != null ? "HD aplicada " : "sin HD ") + key.Substring(0, 12) + " (" + offset + ")");
        if (tex != null) cache[key] = tex;
        else missing.Add(key);
        return tex;
    }

    private static int IndexOf(byte[] data, byte[] pattern, int start)
    {
        for (int i = start; i <= data.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (data[i + j] != pattern[j]) { match = false; break; }
            }
            if (match) return i;
        }
        return -1;
    }

    private static Texture2D Decode(GraphicsDevice gd, byte[] d, bool flip)
    {
        if (d == null || d.Length < 128 || d[0] != 'D' || d[1] != 'D' || d[2] != 'S' || d[3] != ' ')
        {
            return null;
        }
        int height = BitConverter.ToInt32(d, 12);
        int width = BitConverter.ToInt32(d, 16);
        string fourCC = Encoding.ASCII.GetString(d, 84, 4);
        bool dxt5 = fourCC == "DXT5";
        if (!dxt5 && fourCC != "DXT1") return null;

        Color[] pixels = DecodeDxt(d, 128, width, height, dxt5);
        if (flip) FlipRows(pixels, width, height);
        var tex = new Texture2D(gd, width, height, false, SurfaceFormat.Color);
        tex.SetData(pixels);
        return tex;
    }

    // Decodificador DXT1/DXT5 (solo el nivel de mipmap principal).
    private static Color[] DecodeDxt(byte[] d, int off, int w, int h, bool dxt5)
    {
        var px = new Color[w * h];
        int bw = (w + 3) / 4;
        int bh = (h + 3) / 4;
        int p = off;
        int[] cr = new int[4], cg = new int[4], cb = new int[4], ca = new int[4];
        int[] al = new int[8];

        for (int by = 0; by < bh; by++)
        {
            for (int bx = 0; bx < bw; bx++)
            {
                ulong alphaBits = 0;
                if (dxt5)
                {
                    int a0 = d[p], a1 = d[p + 1];
                    for (int i = 0; i < 6; i++) alphaBits |= (ulong)d[p + 2 + i] << (8 * i);
                    p += 8;
                    al[0] = a0;
                    al[1] = a1;
                    if (a0 > a1)
                    {
                        for (int i = 1; i <= 6; i++) al[i + 1] = ((7 - i) * a0 + i * a1) / 7;
                    }
                    else
                    {
                        for (int i = 1; i <= 4; i++) al[i + 1] = ((5 - i) * a0 + i * a1) / 5;
                        al[6] = 0;
                        al[7] = 255;
                    }
                }

                int c0 = d[p] | (d[p + 1] << 8);
                int c1 = d[p + 2] | (d[p + 3] << 8);
                uint idx = BitConverter.ToUInt32(d, p + 4);
                p += 8;

                Rgb565(c0, 0, cr, cg, cb);
                Rgb565(c1, 1, cr, cg, cb);
                cr[2] = (2 * cr[0] + cr[1]) / 3; cg[2] = (2 * cg[0] + cg[1]) / 3; cb[2] = (2 * cb[0] + cb[1]) / 3;
                ca[0] = ca[1] = ca[2] = 255;
                if (dxt5 || c0 > c1)
                {
                    cr[3] = (cr[0] + 2 * cr[1]) / 3; cg[3] = (cg[0] + 2 * cg[1]) / 3; cb[3] = (cb[0] + 2 * cb[1]) / 3;
                    ca[3] = 255;
                }
                else
                {
                    // DXT1 de 3 colores: el índice 3 es transparente.
                    cr[2] = (cr[0] + cr[1]) / 2; cg[2] = (cg[0] + cg[1]) / 2; cb[2] = (cb[0] + cb[1]) / 2;
                    cr[3] = 0; cg[3] = 0; cb[3] = 0; ca[3] = 0;
                }

                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        int px_x = bx * 4 + x;
                        int px_y = by * 4 + y;
                        if (px_x >= w || px_y >= h) continue;
                        int ci = (int)((idx >> (2 * (4 * y + x))) & 3);
                        int ai = (int)((alphaBits >> (3 * (4 * y + x))) & 7);
                        int alpha = dxt5 ? al[ai] : ca[ci];
                        px[px_y * w + px_x] = new Color((byte)cr[ci], (byte)cg[ci], (byte)cb[ci], (byte)alpha);
                    }
                }
            }
        }
        return px;
    }

    private static void Rgb565(int c, int i, int[] r, int[] g, int[] b)
    {
        int rr = (c >> 11) & 31, gg = (c >> 5) & 63, bb = c & 31;
        r[i] = (rr << 3) | (rr >> 2);
        g[i] = (gg << 2) | (gg >> 4);
        b[i] = (bb << 3) | (bb >> 2);
    }
}
