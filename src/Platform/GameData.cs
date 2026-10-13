// Datos del juego: todos los archivos van empaquetados dentro del APK (carpeta Content/).
using System;
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
        return TitleContainer.OpenStream("Content/" + rel);
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
