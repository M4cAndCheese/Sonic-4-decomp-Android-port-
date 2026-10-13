// Stubs de la capa Xbox/Windows Phone para Android.
// Mantienen las firmas que usa el juego; no hacen nada.
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public enum MessageBoxIcon { None = 0, Alert = 1, Error = 2, Warning = 3 }

public class GameUpdateRequiredException : Exception
{
    public GameUpdateRequiredException(string message) : base(message) { }
}

public static class Guide
{
    public static bool IsVisible => false;
    public static bool IsTrialMode => false;
    public static bool IsScreenSaverEnabled { get; set; }

    public static IAsyncResult BeginShowMessageBox(string title, string text, IEnumerable<string> buttons,
        int focusButton, MessageBoxIcon icon, AsyncCallback callback, object state) => null;

    public static int? EndShowMessageBox(IAsyncResult result) => null;

    public static void ShowMarketplace(PlayerIndex player) { }
}

public abstract class XBOXLive
{
    public enum SigninStatus { None, SigningIn, Error, LIVE, Local, UpdateNeeded }

    public static SigninStatus signinStatus = SigninStatus.Local;
    public static bool displayTitleUpdateMessage = false;
    public static bool allowShowUpdate = false;

    public static bool isTrial(bool update) => false;
    public static bool isTrial() => false;
    public static void showGuide() { }
    public static void showUpdateMB() { }
    public static void HandleGameUpdateRequired(GameUpdateRequiredException e) { }
}

public class LiveFeature : XBOXLive
{
    public static Sonic4Ep1 GAME;
    public static int interruptMainLoop = 0;

    private static readonly LiveFeature instance = new LiveFeature();

    public static LiveFeature getInstance() => instance;
    public static bool isInterrupted() => false;
    internal static void endInterrupt() { }

    public bool GotAchievment(string achievementKey) => false;
    public void ShowAchievements() { }
    public void ShowLeaderboards() { }
    public bool saveLeaderBoardScore(int index, int value) => false;
    public bool InputOverride() => false;
    public void ShowOverride() { }
    public void clearHiScores() { }
}

public static class Upsell
{
    public static bool showUpsell = false;

    public static void launchUpsellScreen(object buyScreen) { }
    public static bool inputUpsellScreen() => false;
    public static void drawUpsellScreen() { }
}
