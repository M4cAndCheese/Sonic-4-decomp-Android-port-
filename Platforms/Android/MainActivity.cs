using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Xna.Framework;

namespace Sonic4Android
{
    [Activity(
        Label = "Sonic 4 ep 1",
        Icon = "@mipmap/ic_launcher",
        MainLauncher = true,
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
        Exported = true)]
    public class MainActivity : AndroidGameActivity
    {
        private Sonic4Ep1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Captura excepciones que ocurren fuera de OnCreate (por ejemplo, al cargar contenido
            // en el hilo del juego) y las muestra en pantalla en vez de cerrar la app en silencio.
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;

            try
            {
                _game = new Sonic4Ep1();
                SetContentView((View)_game.Services.GetService(typeof(View)));
                _game.Run();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void OnUnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            ShowError(e.Exception);
        }

        private void ShowError(Exception ex)
        {
            RunOnUiThread(() =>
            {
                new AlertDialog.Builder(this)
                    .SetTitle("Error de Sonic 4 ep 1")
                    .SetMessage(ex.ToString())
                    .SetPositiveButton("Cerrar", (s, e) => Finish())
                    .SetCancelable(false)
                    .Show();
            });
        }
    }
}
