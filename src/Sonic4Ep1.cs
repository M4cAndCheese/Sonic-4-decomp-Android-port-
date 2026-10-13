// Decompiled with JetBrains decompiler
// Type: Sonic4Ep1
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

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

    public const int GAME_W = 480;
    public const int GAME_H = 288;
    private RenderTarget2D gameTarget;
    private Rectangle gameRect;

    protected float deviceMusicVolume;

    protected bool storeSystemVolume = true;

    private Vector3 accel;

    private static bool inputDataRead = true;

    public Sonic4Ep1()
    {
        pInstance = this;
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 480;
        graphics.PreferredBackBufferHeight = 288;
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
        gameTarget = new RenderTarget2D(base.GraphicsDevice, GAME_W, GAME_H, false, SurfaceFormat.Color, DepthFormat.None);
        fntKootenay = base.Content.Load<SpriteFont>("Kootenay");
        fnts[0] = base.Content.Load<SpriteFont>("small");
        fnts[1] = base.Content.Load<SpriteFont>("medium");
        fnts[2] = base.Content.Load<SpriteFont>("large");
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
        if (!AppMain.g_ao_sys_global.is_show_ui && inputDataRead)
        {
            inputDataRead = false;
            if (!LiveFeature.getInstance().InputOverride() && !Upsell.inputUpsellScreen())
            {
                AppMain.onTouchEvents();
            }
            AppMain.amIPhoneAccelerate(ref accel);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AppMain.back_key_is_pressed = true;
            }
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(gameTarget, GetGameRect(), Color.White);
            spriteBatch.End();
        }
        LiveFeature.getInstance().ShowOverride();
        Upsell.drawUpsellScreen();
        base.Draw(gameTime);
    }

    // Área donde se dibuja el juego: proporción 480:288 centrada, sin estirar y sin bordes.
    // Se escala para cubrir toda la pantalla; lo que sobra se recorta por igual en los bordes.
    public Rectangle GetGameRect()
    {
        Viewport vp = GraphicsDevice.Viewport;
        float scale = Math.Max(vp.Width / (float)GAME_W, vp.Height / (float)GAME_H);
        int w = (int)(GAME_W * scale);
        int h = (int)(GAME_H * scale);
        gameRect = new Rectangle((vp.Width - w) / 2, (vp.Height - h) / 2, w, h);
        return gameRect;
    }

    // Toques en coordenadas de pantalla -> coordenadas del juego (480x288).
    public TouchCollection GetGameTouches()
    {
        Rectangle r = GetGameRect();
        float sx = GAME_W / (float)r.Width;
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
