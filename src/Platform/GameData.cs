// Datos del juego: todos los archivos van empaquetados dentro del APK (carpeta Content/).
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

public static class GameData
{
    // Datos listos si Kootenay.xnb está dentro del APK.
    public static bool Ready
    {
        get
        {
            try
            {
                using (TitleContainer.OpenStream("Content/Kootenay.xnb")) return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    // Abre un asset del juego desde el APK, p. ej. "Content\SOUND\x.wav" -> "Content/SOUND/x.wav".
    public static Stream OpenStream(string assetName)
    {
        string rel = assetName.Replace('\\', '/');
        if (rel.StartsWith("Content/")) rel = rel.Substring(8);
        rel = rel.TrimStart('/');
        // ContentManager.Load pide los assets sin extensión ("Kootenay"): el archivo es "Kootenay.xnb".
        if (!Path.HasExtension(rel)) rel += ".xnb";
        // Android distingue mayúsculas: el código pide "Sound/" y la carpeta es "SOUND".
        string real = Resolver(rel);

        // Los streams de assets de Android no permiten Length ni Seek; se copian a memoria.
        using (Stream src = TitleContainer.OpenStream("Content/" + real))
        {
            var ms = new MemoryStream();
            src.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }

    // Índice: ruta en minúsculas -> ruta real dentro de Content/.
    private static Dictionary<string, string> _indice;

    private static string Resolver(string rel)
    {
        if (_indice == null)
        {
            _indice = new Dictionary<string, string>();
            Recorrer("Content");
        }
        string real;
        return _indice.TryGetValue(rel.ToLowerInvariant(), out real) ? real : rel;
    }

    private static void Recorrer(string dir)
    {
        var assets = Android.App.Application.Context.Assets;
        foreach (string nombre in assets.List(dir))
        {
            string ruta = dir + "/" + nombre;
            if (assets.List(ruta).Length > 0)
            {
                Recorrer(ruta);
            }
            else
            {
                string relativa = ruta.Substring("Content/".Length);
                _indice[relativa.ToLowerInvariant()] = relativa;
            }
        }
    }
}

// ContentManager que lee los assets desde el APK.
public class GameDataContent : ContentManager
{
    public GameDataContent(IServiceProvider services) : base(services, "Content") { }

    protected override Stream OpenStream(string assetName)
    {
        return GameData.OpenStream(assetName);
    }
}
