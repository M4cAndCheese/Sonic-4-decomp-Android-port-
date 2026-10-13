// Datos del juego: carpeta Sonic4-data en la raíz del almacenamiento del dispositivo.
// Si un archivo no está ahí, se usa el de los assets de la APK.
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

public static class GameData
{
    public const string NombreCarpeta = "Sonic4-data";

    // Raíz del almacenamiento compartido, p. ej. /storage/emulated/0/Sonic4-data
    public static string Root =>
        Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, NombreCarpeta);

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
}

// ContentManager que lee desde Sonic4-data en vez de solo desde los assets de la APK.
public class GameDataContent : ContentManager
{
    public GameDataContent(IServiceProvider services) : base(services, "Content") { }

    protected override Stream OpenStream(string assetName)
    {
        return GameData.OpenStream(assetName);
    }
}
