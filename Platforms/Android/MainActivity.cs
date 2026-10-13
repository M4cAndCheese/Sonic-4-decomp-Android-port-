using System;
using System.IO;
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
        private Sonic4Ep1 _game;
        private bool _iniciado;
        private bool _esperandoPermiso;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledException;
            Comprobar();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_esperandoPermiso)
            {
                _esperandoPermiso = false;
                Comprobar();
            }
        }

        // Permiso -> carpeta Sonic4-data -> datos. Sin datos no arranca el juego.
        private void Comprobar()
        {
            if (_iniciado) return;

            if (!TieneAcceso())
            {
                _esperandoPermiso = true;
                PedirAcceso();
                return;
            }

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

        private bool TieneAcceso()
        {
            if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.R)
                return Android.OS.Environment.IsExternalStorageManager;
            return CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage)
                   == Android.Content.PM.Permission.Granted;
        }

        private void PedirAcceso()
        {
            if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.R)
            {
                Toast.MakeText(this, "Concede acceso a todos los archivos para crear Sonic4-data", ToastLength.Long).Show();
                var intent = new Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission,
                    Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
            }
            else
            {
                RequestPermissions(new[]
                {
                    Android.Manifest.Permission.WriteExternalStorage,
                    Android.Manifest.Permission.ReadExternalStorage
                }, 1);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            _esperandoPermiso = false;
            Comprobar();
        }

        private void MostrarFaltanDatos()
        {
            RunOnUiThread(() =>
            {
                new AlertDialog.Builder(this)
                    .SetTitle("Faltan datos del juego")
                    .SetMessage("No hay datos del juego, ponlos en Sonic4-data en la raíz del dispositivo\n\n(" + GameData.Root + ")")
                    .SetPositiveButton("Reintentar", (s, e) => Comprobar())
                    .SetNegativeButton("Cerrar", (s, e) => Finish())
                    .SetCancelable(false)
                    .Show();
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
