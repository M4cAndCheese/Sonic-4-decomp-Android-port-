// Acelerómetro real de Android para las fases especiales.
// Reemplaza al shim vacío de Microsoft.Devices.Sensors (que no entregaba datos).
// Entrega la gravedad en "g" con el mismo signo que espera el motor (convención iOS).
using System;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;

public sealed class TiltSensor : Java.Lang.Object, ISensorEventListener
{
    public static readonly TiltSensor Instance = new TiltSensor();

    // Si al girar el celular el personaje va hacia el lado contrario, cambiar a -1f.
    public static float SteerSign = 1f;

    // Última lectura en g.
    public Vector3 Value = Vector3.Zero;

    private SensorManager sensorManager;
    private Sensor accelerometer;
    private bool running;

    public void Start()
    {
        if (running) return;
        try
        {
            var ctx = Android.App.Application.Context;
            sensorManager = (SensorManager)ctx.GetSystemService(Context.SensorService);
            accelerometer = sensorManager?.GetDefaultSensor(SensorType.Accelerometer);
            if (accelerometer != null)
            {
                running = sensorManager.RegisterListener(this, accelerometer, SensorDelay.Game);
            }
        }
        catch (Exception)
        {
            running = false;
        }
    }

    public void Stop()
    {
        if (!running) return;
        try
        {
            sensorManager?.UnregisterListener(this);
        }
        catch (Exception) { }
        running = false;
    }

    public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) { }

    public void OnSensorChanged(SensorEvent e)
    {
        const float G = 9.80665f;
        var v = e.Values;
        // Android reporta +g hacia arriba; el motor espera el signo contrario.
        Value = new Vector3(-v[0] / G, -v[1] / G, -v[2] / G);
    }
}
