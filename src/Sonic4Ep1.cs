// Decompiled with JetBrains decompiler
// Type: Sonic4Ep1
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using mpp;

public class Sonic4Ep1 : Game
{
    public static Sonic4Ep1 pInstance;

    private GraphicsDeviceManager graphics;

    private SpriteFont fntKootenay;

    public SpriteFont[] fnts = new SpriteFont[3];

    private int GCCount;

    private WeakReference wr = new WeakReference(new object());

    private double _lastUpdateMilliseconds;

    public static bool cheat = false;

    public RasterizerState scissorState;

    private AppMain appMain;

    public SpriteBatch spriteBatch;

    public ContentManager GameContent;

    public const int GAME_W = 480;
    public const int GAME_H = 288;
    // Ancho real del render: se calcula según la proporción de la pantalla (ultrawide = más ancho).
    // Con alto fijo a 288 px el juego no se estira ni se recorta: solo se ve más escenario a los lados.
    public static int RENDER_W = GAME_W;
    private RenderTarget2D gameTarget;
    private Rectangle gameRect;
    private RenderTarget2D blurTarget;
    private bool prevBackPressed;

    // Controles grises del port de Sonic 1 2013 (dibujados encima del pad del motor, que queda oculto).
    private Texture2D padSheet;
    private Vector2 stickCenter = new Vector2(62f, 226f);
    private Vector2 jumpCenter = new Vector2(436f, 264f);
    private Vector2 stickPos;
    private float jumpScale = 1f;

    protected float deviceMusicVolume;

    protected bool storeSystemVolume = true;

    private Vector3 accel;

    private static bool inputDataRead = true;

    public Sonic4Ep1()
    {
        pInstance = this;
        GameContent = new GameDataContent(Services);
        graphics = new GraphicsDeviceManager(this);
        // Backbuffer al tamaño real de la pantalla (si no, Android escala 480x288 y mete bandas negras).
        var dm = Android.App.Application.Context.Resources.DisplayMetrics;
        graphics.PreferredBackBufferWidth = Math.Max(dm.WidthPixels, dm.HeightPixels);
        graphics.PreferredBackBufferHeight = Math.Min(dm.WidthPixels, dm.HeightPixels);
        graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
        graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
        graphics.SynchronizeWithVerticalRetrace = true;
        graphics.IsFullScreen = true;
        base.IsMouseVisible = true;
        base.Content.RootDirectory = "Content";
        base.TargetElapsedTime = TimeSpan.FromTicks(333333L);
        base.Activated += OnActivated;
        base.Deactivated += OnDeactivated;
    }

    private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        PresentationParameters presentationParameter = e.GraphicsDeviceInformation.PresentationParameters;
        presentationParameter.BackBufferWidth = graphics.PreferredBackBufferWidth;
        presentationParameter.BackBufferHeight = graphics.PreferredBackBufferHeight;
    }

    protected override void Initialize()
    {
        scissorState = new RasterizerState
        {
            ScissorTestEnable = true,
            CullMode = CullMode.None
        };
        Guide.IsScreenSaverEnabled = false;
        LiveFeature.GAME = this;
        LiveFeature.getInstance();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        //IL_00a1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ab: Expected O, but got Unknown
        spriteBatch = new SpriteBatch(base.GraphicsDevice);
        // Igual que el widescreen de SMW: la altura se queda en 288 (escala original) y el ancho
        // se toma de la proporción real del área de dibujo. Así no hay zoom ni estiramiento.
        // El juego se dibuja en su resolución original 15:9 (480x288): sin estirar y con la geometría del motor intacta.
        RENDER_W = GAME_W;
        gameTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W, GAME_H, false, SurfaceFormat.Color, DepthFormat.None);
        // Versión mínima del frame para hacer el fondo difuminado de los laterales.
        blurTarget = new RenderTarget2D(base.GraphicsDevice, 32, 18, false, SurfaceFormat.Color, DepthFormat.None);
        stickPos = stickCenter;
        try
        {
            using (var st = TitleContainer.OpenStream("Content/ui/controls_2013.png"))
            {
                padSheet = Texture2D.FromStream(base.GraphicsDevice, st);
            }
        }
        catch (Exception)
        {
            padSheet = null; // Sin la hoja de sprites, el juego usa el pad del motor.
        }
        fntKootenay = GameContent.Load<SpriteFont>("Kootenay");
        fnts[0] = GameContent.Load<SpriteFont>("small");
        fnts[1] = GameContent.Load<SpriteFont>("medium");
        fnts[2] = GameContent.Load<SpriteFont>("large");
        try
        {
            appMain = new AppMain(this, graphics, base.GraphicsDevice);
            appMain.AppInit("");
        }
        catch (Exception)
        {
        }
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
        AppMain.isForeground = false;
        if (SaveState.saveLater)
        {
            SaveState._saveFile(SaveState.save);
        }
        if (!Guide.IsVisible)
        {
            storeSystemVolume = true;
            try
            {
                if (!AppMain.g_ao_sys_global.is_playing_device_bgm_music)
                {
                    MediaPlayer.Pause();
                }
                MediaPlayer.Volume = deviceMusicVolume;
                return;
            }
            catch (Exception)
            {
                return;
            }
        }
        storeSystemVolume = false;
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        AppMain.isForeground = true;
        if (storeSystemVolume)
        {
            deviceMusicVolume = MediaPlayer.Volume;
        }
        if ((AppMain.g_gm_main_system.game_flag & 0x40) == 0)
        {
            AppMain.g_pause_flag = true;
        }
    }

    protected override void UnloadContent()
    {
    }

    protected override void Update(GameTime gameTime)
    {
        AppMain.lastGameTime = gameTime;
        // El Back se registra al presionarlo (flanco), también dentro de menús y opciones.
        bool backNow = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
        if (backNow && !prevBackPressed)
        {
            AppMain.back_key_is_pressed = true;
        }
        prevBackPressed = backNow;
        if (!AppMain.g_ao_sys_global.is_show_ui && inputDataRead)
        {
            inputDataRead = false;
            if (!LiveFeature.getInstance().InputOverride() && !Upsell.inputUpsellScreen())
            {
                AppMain.onTouchEvents();
            }
            AppMain.amIPhoneAccelerate(ref accel);
        }
        try
        {
            base.Update(gameTime);
        }
        catch (GameUpdateRequiredException e)
        {
            XBOXLive.HandleGameUpdateRequired(e);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        inputDataRead = true;
        OpenGL.drawPrimitives_Count = 0;
        OpenGL.drawVertexBuffer_Count = 0;
        if (gameTarget != null)
        {
            GraphicsDevice.SetRenderTarget(gameTarget);
        }
        appMain.AppMainLoop();
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        if (gameTarget != null)
        {
            // Fondo difuminado: el frame reducido y estirado cubre los laterales sin tocar el juego.
            GraphicsDevice.SetRenderTarget(blurTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(gameTarget, blurTarget.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(blurTarget, GraphicsDevice.Viewport.Bounds, new Color(110, 110, 110));
            spriteBatch.Draw(gameTarget, GetGameRect(), Color.White);
            spriteBatch.End();
            DrawPadOverlay(GetGameRect());
        }
        LiveFeature.getInstance().ShowOverride();
        Upsell.drawUpsellScreen();
        base.Draw(gameTime);
    }

    // Dibuja la cruceta y el salto grises (port 2013) con movimiento suave.
    // Las coordenadas del juego son 480x288; r es el rectángulo donde se dibuja el juego en pantalla.
    private void DrawPadOverlay(Rectangle r)
    {
        if (padSheet == null)
        {
            return;
        }
        float k = r.Width / (float)RENDER_W;
        TouchCollection touches = GetGameTouches();
        Vector2 stickTarget = stickCenter;
        bool jumpTouched = false;
        foreach (TouchLocation t in touches)
        {
            if (t.State == TouchLocationState.Released || t.State == TouchLocationState.Invalid)
            {
                continue;
            }
            if (Vector2.Distance(t.Position, jumpCenter) < 34f)
            {
                jumpTouched = true;
            }
            else if (t.Position.X < 200f)
            {
                Vector2 d = t.Position - stickCenter;
                float len = d.Length();
                if (len > 28f)
                {
                    d = d / len * 28f;
                }
                stickTarget = stickCenter + d;
            }
        }
        stickPos = Vector2.Lerp(stickPos, stickTarget, 0.35f);
        jumpScale += ((jumpTouched ? 0.92f : 1f) - jumpScale) * 0.3f;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
        Color c = new Color(255, 255, 255, 215);
        Vector2 baseC = new Vector2(r.X, r.Y) + stickCenter * k;
        spriteBatch.Draw(padSheet, baseC, new Rectangle(2, 2, 64, 64), c, 0f, new Vector2(32, 32), 64f * k / 64f, SpriteEffects.None, 0f);
        Vector2 knobC = new Vector2(r.X, r.Y) + stickPos * k;
        spriteBatch.Draw(padSheet, knobC, new Rectangle(332, 2, 42, 42), c, 0f, new Vector2(21, 21), 42f * k / 42f, SpriteEffects.None, 0f);
        Rectangle jumpSrc = jumpTouched ? new Rectangle(58, 68, 54, 54) : new Rectangle(2, 68, 54, 54);
        Vector2 jumpC = new Vector2(r.X, r.Y) + jumpCenter * k;
        spriteBatch.Draw(padSheet, jumpC, jumpSrc, c, 0f, new Vector2(27, 27), 54f * k / 54f * jumpScale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }

    // Área donde se dibuja el juego. Escala uniforme (sin estirar) y sin recortar:
    // como RENDER_W sigue la proporción de la pantalla, no quedan bandas negras.
    public Rectangle GetGameRect()
    {
        Viewport vp = GraphicsDevice.Viewport;
        float scale = Math.Min(vp.Width / (float)RENDER_W, vp.Height / (float)GAME_H);
        int w = (int)Math.Round(RENDER_W * scale);
        int h = (int)Math.Round(GAME_H * scale);
        gameRect = new Rectangle((vp.Width - w) / 2, (vp.Height - h) / 2, w, h);
        return gameRect;
    }

    // Toques en coordenadas de pantalla -> coordenadas del juego (RENDER_W x 288).
    public TouchCollection GetGameTouches()
    {
        Rectangle r = GetGameRect();
        float sx = RENDER_W / (float)r.Width;
        float sy = GAME_H / (float)r.Height;
        List<TouchLocation> list = new List<TouchLocation>();
        foreach (TouchLocation t in TouchPanel.GetState())
        {
            Vector2 p = new Vector2((t.Position.X - r.X) * sx, (t.Position.Y - r.Y) * sy);
            list.Add(new TouchLocation(t.Id, t.State, p));
        }
        return new TouchCollection(list.ToArray());
    }
}
