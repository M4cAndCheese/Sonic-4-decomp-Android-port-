# Sonic 4 Episodio I — Android (MonoGame)

Port nativo de la decompilación C# de Sonic 4 Ep. I (XNA, Windows Phone) a Android con MonoGame.

- `src/`: código decompilado, en proceso de adaptación.
- `Platforms/Android/`: actividad, manifest y recursos.
- `Content/`: assets del juego. Se empaquetan dentro del APK; no hay que copiar nada al teléfono.
- `.github/workflows/build-android.yml`: compila el APK en GitHub Actions y lo sube como artefacto.

El APK no pide permisos de almacenamiento ni muestra selector de archivos.
