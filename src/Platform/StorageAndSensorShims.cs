// Reemplazos de APIs de Windows Phone para Android.
// IsolatedStorage -> archivos en el directorio privado de la app.
// Accelerometer   -> sin sensor (el juego sigue funcionando sin él).
using System;
using System.IO;

namespace System.IO.IsolatedStorage
{
    public class IsolatedStorageFile : IDisposable
    {
        public string Root { get; private set; }

        private IsolatedStorageFile(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public static IsolatedStorageFile GetUserStoreForApplication()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return new IsolatedStorageFile(Path.Combine(dir, "savedata"));
        }

        // La ruta de guardado es "/sonic4_ep1.dat". Path.Combine ignora la raíz cuando la segunda ruta empieza por '/',
        // así que FileExists/DeleteFile miraban en la raíz del sistema y nunca encontraban el archivo real:
        // la carga siempre fallaba y el borrado no tocaba el guardado. Se quitan las barras iniciales.
        private static string Relative(string path) => (path ?? "").TrimStart('/', '\\');

        private string Full(string path) => Path.Combine(Root, Relative(path));

        public bool FileExists(string path) => File.Exists(Full(path));

        public void DeleteFile(string path)
        {
            if (File.Exists(Full(path)))
                File.Delete(Full(path));
        }

        public IsolatedStorageFileStream CreateFile(string path) =>
            new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.ReadWrite, this);

        public IsolatedStorageFileStream OpenFile(string path, FileMode mode) =>
            new IsolatedStorageFileStream(path, mode, FileAccess.ReadWrite, this);

        public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access) =>
            new IsolatedStorageFileStream(path, mode, access, this);

        public void Dispose() { }
    }

    public class IsolatedStorageFileStream : FileStream
    {
        public IsolatedStorageFileStream(string path, FileMode mode, FileAccess access, IsolatedStorageFile store)
            : base(Path.Combine(store.Root, path.TrimStart('/', '\\')), mode, access)
        {
        }
    }
}

namespace Microsoft.Devices.Sensors
{
    public class Accelerometer : IDisposable
    {
        public event EventHandler<EventArgs> CurrentValueChanged;
        public void Start() { }
        public void Stop() { }
        public void Dispose() { }
    }
}
