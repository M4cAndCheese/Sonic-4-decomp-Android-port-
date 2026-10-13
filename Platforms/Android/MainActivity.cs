using Android.Views;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Xna.Framework;

namespace Sonic4Android
{
    [Activity(
        Label = "Sonic 4 ep 1",
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
            _game = new Sonic4Ep1();
            SetContentView((View)_game.Services.GetService(typeof(View)));
            _game.Run();
        }
    }
}
