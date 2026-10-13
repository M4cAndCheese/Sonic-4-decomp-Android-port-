using System;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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
        private bool _iniciado;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
            Comprobar();
        }

        // Si los datos no están en la carpeta de la app, el juego no arranca.
        private void Comprobar()
        {
            if (_iniciado) return;

            try { Directory.CreateDirectory(GameData.Root); }
            catch (Exception) { }

            if (!GameData.Ready)
            {
                MostrarFaltanDatos();
                return;
            }

            _iniciado = true;
            StartGame();
        }

        private void MostrarFaltanDatos()
        {
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            layout.SetGravity(GravityFlags.Center);
            int pad = (int)(24 * Resources.DisplayMetrics.Density);
            layout.SetPadding(pad, pad, pad, pad);

            var texto = new TextView(this)
            {
                Text = "No hay datos del juego.\n\nCopia los archivos del juego (debe existir Kootenay.xnb) en:\n\n"
                       + GameData.Root
                       + "\n\nLuego pulsa Reintentar."
            };
            texto.Gravity = GravityFlags.Center;
            layout.AddView(texto);

            var reintentar = new Button(this) { Text = "Reintentar" };
            reintentar.Click += (s, e) => Comprobar();
            layout.AddView(reintentar);

            var cerrar = new Button(this) { Text = "Cerrar" };
            cerrar.Click += (s, e) => Finish();
            layout.AddView(cerrar);

            SetContentView(layout);
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
