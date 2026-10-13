// Datos del juego: carpeta Sonic4-data en la raíz del almacenamiento del dispositivo.
// Si un archivo no está ahí, se usa el de los assets de la APK.
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

public static class GameData
{
    public const string NombreCarpeta = "Sonic4-data";

    // Carpeta de datos dentro de la app (no necesita permisos de almacenamiento):
    // /storage/emulated/0/Android/data/com.sonic4ep1.android/files/Sonic4-data
    public static string Root =>
        Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath, NombreCarpeta);

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

    // Instala los datos copiados a `staging` como la carpeta Sonic4-data.
    // Acepta la carpeta elegida directamente (con Kootenay.xnb dentro) o una que contenga Sonic4-data.
    public static void Install(string staging)
    {
        string origen = staging;
        if (!File.Exists(Path.Combine(origen, "Kootenay.xnb")))
        {
            string anidada = Path.Combine(staging, NombreCarpeta);
            if (File.Exists(Path.Combine(anidada, "Kootenay.xnb"))) origen = anidada;
            else throw new InvalidOperationException("La carpeta elegida no contiene Kootenay.xnb");
        }

        string viejo = Root + "_old";
        if (Directory.Exists(viejo)) Directory.Delete(viejo, true);
        if (Directory.Exists(Root)) Directory.Move(Root, viejo);
        try
        {
            Directory.Move(origen, Root);
        }
        catch
        {
            // Si falla, se restaura la copia anterior.
            if (Directory.Exists(viejo) && !Directory.Exists(Root)) Directory.Move(viejo, Root);
            throw;
        }

        if (Directory.Exists(viejo)) Directory.Delete(viejo, true);
        if (Directory.Exists(staging)) Directory.Delete(staging, true);
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
