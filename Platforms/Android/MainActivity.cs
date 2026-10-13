using System;
using Android.App;
using Android.Content;
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
        private const int RequestDatos = 1;
        private Sonic4Ep1 _game;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;

            if (!GameData.Ready)
            {
                // Sin datos: pedir el zip antes de arrancar el juego.
                StartActivityForResult(new Intent(this, typeof(DataPickerActivity)), RequestDatos);
                return;
            }
            StartGame();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == RequestDatos && GameData.Ready)
            {
                StartGame();
            }
            else if (requestCode == RequestDatos)
            {
                Finish();
            }
        }

        private void StartGame()
        {
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
