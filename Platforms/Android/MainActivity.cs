using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
        private const int RequestZip = 2;

        private Sonic4Ep1 _game;
        private bool _iniciado;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
            Comprobar();
        }

        // Sin datos se muestra la pantalla de importar; con datos arranca el juego.
        private void Comprobar()
        {
            if (_iniciado) return;

            try { Directory.CreateDirectory(GameData.Root); }
            catch (Exception) { }

            if (!GameData.Ready)
            {
                MostrarImportar();
                return;
            }

            _iniciado = true;
            StartGame();
        }

        // Pantalla sin datos: un texto y el botón para importar un .zip con los archivos del juego.
        private void MostrarImportar()
        {
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            layout.SetGravity(GravityFlags.Center);
            int pad = (int)(24 * Resources.DisplayMetrics.Density);
            layout.SetPadding(pad, pad, pad, pad);

            var texto = new TextView(this)
            {
                Text = "No hay datos del juego.\n\nImporta un archivo .zip con los archivos del juego (debe contener Kootenay.xnb)."
            };
            texto.Gravity = GravityFlags.Center;
            layout.AddView(texto);

            var boton = new Button(this) { Text = "Importar ZIP" };
            boton.Click += (s, e) => ElegirZip();
            layout.AddView(boton);

            SetContentView(layout);
        }

        private void ElegirZip()
        {
            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            intent.PutExtra(Intent.ExtraMimeTypes, new[] { "application/zip", "application/x-zip-compressed" });
            if (intent.ResolveActivity(PackageManager) == null)
            {
                // Algunos dispositivos no tienen el selector de documentos: se usa el de contenido.
                intent = new Intent(Intent.ActionGetContent);
                intent.SetType("application/zip");
            }
            StartActivityForResult(intent, RequestZip);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != RequestZip || resultCode != Result.Ok || data?.Data == null) return;
            ImportarZip(data.Data);
        }

        private void ImportarZip(Android.Net.Uri uri)
        {
            Toast.MakeText(this, "Importando datos, espera…", ToastLength.Long).Show();

            Task.Run(() =>
            {
                string tmpZip = Path.Combine(CacheDir.AbsolutePath, "import.zip");
                string staging = GameData.Root + "_import";
                try
                {
                    using (var entrada = ContentResolver.OpenInputStream(uri))
                    {
                        if (entrada == null) throw new IOException("No se pudo abrir el zip");
                        using (var salida = File.Create(tmpZip))
                        {
                            entrada.CopyTo(salida);
                        }
                    }

                    if (Directory.Exists(staging)) Directory.Delete(staging, true);
                    System.IO.Compression.ZipFile.ExtractToDirectory(tmpZip, staging);

                    // El zip puede traer los archivos en la raíz o dentro de una subcarpeta.
                    string[] marca = Directory.GetFiles(staging, "Kootenay.xnb", SearchOption.AllDirectories);
                    if (marca.Length == 0) throw new IOException("El zip no contiene Kootenay.xnb");

                    try
                    {
                        GameData.Install(Path.GetDirectoryName(marca[0]));
                    }
                    finally
                    {
                        if (Directory.Exists(staging)) Directory.Delete(staging, true);
                        if (File.Exists(tmpZip)) File.Delete(tmpZip);
                    }

                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Datos listos", ToastLength.Short).Show();
                        Comprobar();
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "No se pudo importar: " + ex.Message, ToastLength.Long).Show();
                        MostrarImportar();
                    });
                }
            });
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
