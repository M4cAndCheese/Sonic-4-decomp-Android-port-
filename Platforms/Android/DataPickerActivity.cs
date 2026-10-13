using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;

namespace Sonic4Android
{
    // Aparece en el cajón de apps como "Elegir datos" y también se abre
    // automáticamente cuando el juego no tiene datos.
    [Activity(
        Label = "Elegir datos (carpeta)",
        Theme = "@android:style/Theme.DeviceDefault.Light.DarkActionBar",
        Exported = true)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryLauncher })]
    public class DataPickerActivity : Activity
    {
        private const int RequestCarpeta = 2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState == null)
            {
                Toast.MakeText(this, "Elige la carpeta sonic4-data en Descargas", ToastLength.Long).Show();
                var intent = new Intent(Intent.ActionOpenDocumentTree);
                intent.PutExtra("android.provider.extra.INITIAL_URI",
                    Android.Net.Uri.Parse("content://com.android.externalstorage.documents/document/primary%3ADownload"));
                StartActivityForResult(intent, RequestCarpeta);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != RequestCarpeta || resultCode != Result.Ok || data?.Data == null)
            {
                Finish();
                return;
            }

            var treeUri = data.Data;
            Toast.MakeText(this, "Copiando datos, espera un momento…", ToastLength.Long).Show();

            Task.Run(() =>
            {
                string staging = GameData.Root + "_import";
                try
                {
                    if (Directory.Exists(staging)) Directory.Delete(staging, true);
                    string rootId = DocumentsContract.GetTreeDocumentId(treeUri);
                    CopiarArbol(treeUri, rootId, staging);
                    GameData.Install(staging);
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Datos listos", ToastLength.Short).Show();
                        SetResult(Result.Ok);
                        Finish();
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "No se pudieron cargar los datos: " + ex.Message, ToastLength.Long).Show();
                        Finish();
                    });
                }
            });
        }

        // Copia recursivamente el contenido de la carpeta elegida al almacenamiento privado.
        private void CopiarArbol(Android.Net.Uri treeUri, string docId, string destDir)
        {
            Directory.CreateDirectory(destDir);
            var hijos = DocumentsContract.BuildChildDocumentsUriUsingTree(treeUri, docId);
            var columnas = new[]
            {
                DocumentsContract.Document.ColumnDocumentId,
                DocumentsContract.Document.ColumnDisplayName,
                DocumentsContract.Document.ColumnMimeType
            };

            using (var cursor = ContentResolver.Query(hijos, columnas, null, null, null))
            {
                if (cursor == null) return;
                while (cursor.MoveToNext())
                {
                    string id = cursor.GetString(0);
                    string nombre = cursor.GetString(1);
                    string mime = cursor.GetString(2);

                    if (DocumentsContract.Document.MimeTypeDir.Equals(mime))
                    {
                        CopiarArbol(treeUri, id, Path.Combine(destDir, nombre));
                    }
                    else
                    {
                        var docUri = DocumentsContract.BuildDocumentUriUsingTree(treeUri, id);
                        using (var entrada = ContentResolver.OpenInputStream(docUri))
                        using (var salida = File.Create(Path.Combine(destDir, nombre)))
                        {
                            entrada.CopyTo(salida);
                        }
                    }
                }
            }
        }
    }
}
