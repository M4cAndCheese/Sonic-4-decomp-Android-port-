// Datos del juego desde un zip elegido por el usuario.
// Si un archivo no está en la carpeta de datos, se usa el de los assets de la APK.
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

public static class GameData
{
    public static string Root
    {
        get
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(dir, "gamedata");
        }
    }

    // Marca de datos completos: el archivo que siempre carga el juego al arrancar.
    public static bool Ready => File.Exists(Path.Combine(Root, "Kootenay.xnb"));

    public static Stream OpenStream(string assetName)
    {
        string rel = assetName.Replace('\\', '/');
        if (rel.StartsWith("Content/")) rel = rel.Substring(8);
        rel = rel.TrimStart('/');

        if (rel.Length > 0 && !rel.Contains(".."))
        {
            string p = Path.Combine(Root, rel);
            if (File.Exists(p)) return File.OpenRead(p);
        }
        return TitleContainer.OpenStream(assetName);
    }

    // Extrae el zip en una carpeta temporal y ubica la carpeta que contiene Kootenay.xnb.
    public static void ExtractZip(Stream zipStream)
    {
        string tmp = Root + "_tmp";
        if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
        Directory.CreateDirectory(tmp);
        string tmpFull = Path.GetFullPath(tmp) + Path.DirectorySeparatorChar;

        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                string dest = Path.GetFullPath(Path.Combine(tmp, entry.FullName.Replace('\\', '/')));
                // Evita rutas que salgan de la carpeta de destino (zip slip).
                if (!dest.StartsWith(tmpFull, StringComparison.Ordinal)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                entry.ExtractToFile(dest, true);
            }
        }

        string marca = Directory.GetFiles(tmp, "Kootenay.xnb", SearchOption.AllDirectories).FirstOrDefault();
        if (marca == null)
        {
            Directory.Delete(tmp, true);
            throw new InvalidDataException("El zip no contiene Kootenay.xnb. Elige el zip con los datos del juego.");
        }

        string contenido = Path.GetDirectoryName(marca);
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
        Directory.Move(contenido, Root);
        if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
    }
}

// ContentManager que lee desde GameData en vez de solo desde los assets de la APK.
public class GameDataContent : ContentManager
{
    public GameDataContent(IServiceProvider services) : base(services, "Content") { }

    protected internal override Stream OpenStream(string assetName)
    {
        return GameData.OpenStream(assetName);
    }

    // Instala los datos desde una carpeta de preparación: localiza Kootenay.xnb y mueve su carpeta a Root.
    public static void Install(string staging)
    {
        string marca = Directory.GetFiles(staging, "Kootenay.xnb", SearchOption.AllDirectories).FirstOrDefault();
        if (marca == null)
        {
            Directory.Delete(staging, true);
            throw new InvalidDataException("La carpeta no contiene Kootenay.xnb. Elige la carpeta donde descomprimiste main.obb.");
        }
        string contenido = Path.GetDirectoryName(marca);
        if (Directory.Exists(Root)) Directory.Delete(Root, true);
        Directory.Move(contenido, Root);
        if (Directory.Exists(staging)) Directory.Delete(staging, true);
    }
}

// ContentManager que lee desde GameData en vez de solo desde los assets de la APK.
