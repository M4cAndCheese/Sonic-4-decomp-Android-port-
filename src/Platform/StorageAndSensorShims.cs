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

        private string Full(string path) => Path.Combine(Root, path);

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
            : base(store.Root + Path.DirectorySeparatorChar + path, mode, access)
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
