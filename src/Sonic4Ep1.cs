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
using System.Globalization;
using System.IO;
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
    private Texture2D pixelTex;
    private Texture2D gearTex;
    private Vector2 stickCenter = new Vector2(72f, 212f);
    private Vector2 jumpCenter = new Vector2(414f, 236f);
    private float stickSize = 84f;
    private float jumpSize = 84f;
    private Vector2 stickPos;
    private float jumpScale = 1f;
    private bool jumpHeld;
    // Hoja nueva (512x512): aro gris con flechas, aro gris liso, bola brillante.
    private static readonly Rectangle SrcStickRing = new Rectangle(0, 0, 256, 256);
    private static readonly Rectangle SrcJumpRing = new Rectangle(256, 0, 256, 256);
    private static readonly Rectangle SrcBall = new Rectangle(12, 322, 176, 176);
    // Diagnóstico de toques (se ve arriba a la izquierda). Poner en false cuando ya no haga falta.
    private const bool DEBUG_TOUCH = true;
    private bool dbgOverride;
    private bool dbgRan;
    // Toques de este fotograma: los que siguen presionados y los que son nuevos (no estaban en el anterior).
    private TouchCollection frameTouches;
    private readonly List<TouchLocation> newTouches = new List<TouchLocation>();
    private HashSet<int> prevTouchIds = new HashSet<int>();
    private bool menuOpen;
    private bool editing;
    private bool wideView = true;
    private int dragId = -1;
    private int dragKind = -1; // 0 mover joystick, 1 mover salto, 2 redimensionar joystick, 3 redimensionar salto
    private static readonly Rectangle GearRect = new Rectangle(438, 52, 32, 32);
    private static readonly Rectangle ListoRect = new Rectangle(190, 8, 100, 28);
    private static readonly Rectangle MenuPanel = new Rectangle(140, 60, 200, 190);
    private static readonly Rectangle MenuVistaRect = new Rectangle(160, 100, 160, 26);
    private static readonly Rectangle MenuEditRect = new Rectangle(160, 134, 160, 26);
    private static readonly Rectangle MenuResetRect = new Rectangle(160, 168, 160, 26);
    private static readonly Rectangle MenuCloseRect = new Rectangle(160, 202, 160, 26);

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
            using (var st = TitleContainer.OpenStream("Content/ui/pad_hq.png"))
            {
                padSheet = Texture2D.FromStream(base.GraphicsDevice, st);
            }
        }
        catch (Exception)
        {
            padSheet = null; // Sin la hoja de sprites, el juego usa el pad del motor.
        }
        pixelTex = new Texture2D(base.GraphicsDevice, 1, 1);
        pixelTex.SetData(new Color[] { Color.White });
        gearTex = MakeGear(base.GraphicsDevice, 64);
        LoadLayout();
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
        UpdateTouchFrame();
        // El engranaje usa los toques nuevos de este fotograma (no depende del estado Pressed).
        if (!menuOpen && !editing && AppMain.IsStagePlaying())
        {
            foreach (TouchLocation t in newTouches)
            {
                if (InRect(t.Position, GearRect))
                {
                    menuOpen = true;
                }
            }
        }
        // Los toques se entregan siempre: en el menú principal is_show_ui es verdadero y el menú también los necesita.
        if (inputDataRead)
        {
            inputDataRead = false;
            dbgOverride = LiveFeature.getInstance().InputOverride();
            dbgRan = false;
            if (!dbgOverride && !Upsell.inputUpsellScreen())
            {
                AppMain.onTouchEvents();
                dbgRan = true;
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
            if (wideView)
            {
                spriteBatch.Draw(blurTarget, GraphicsDevice.Viewport.Bounds, new Color(110, 110, 110));
            }
            spriteBatch.Draw(gameTarget, GetGameRect(), Color.White);
            spriteBatch.End();
            DrawControls(GetGameRect());
            DrawDebug(GetGameRect());
        }
        LiveFeature.getInstance().ShowOverride();
        Upsell.drawUpsellScreen();
        base.Draw(gameTime);
    }

    // ---------- Controles táctiles: juego, menú del engranaje y edición ----------
    // Coordenadas del juego (480x288). Se dibuja con una matriz que escala al área del juego en pantalla.

    private static bool InRect(Vector2 p, Rectangle rc)
    {
        return p.X >= rc.X && p.X <= rc.Right && p.Y >= rc.Y && p.Y <= rc.Bottom;
    }

    private static float Clamp(float v, float lo, float hi)
    {
        return Math.Max(lo, Math.Min(hi, v));
    }

    private static Texture2D MakeGear(GraphicsDevice gd, int size)
    {
        Texture2D t = new Texture2D(gd, size, size);
        Color[] c = new Color[size * size];
        float cx = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cx;
                float r = (float)Math.Sqrt(dx * dx + dy * dy);
                float ang = (float)Math.Atan2(dy, dx);
                float outer = Math.Cos(8 * ang) > 0.2 ? size * 0.48f : size * 0.38f;
                bool solid = r <= outer && r >= size * 0.16f;
                c[y * size + x] = solid ? new Color(235, 235, 235, 225) : Color.Transparent;
            }
        }
        t.SetData(c);
        return t;
    }

    private static string LayoutPath
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "pad_layout.txt"); }
    }

    private static string F(float v)
    {
        return v.ToString(CultureInfo.InvariantCulture);
    }

    private void SaveLayout()
    {
        try
        {
            File.WriteAllLines(LayoutPath, new string[]
            {
                "jump " + F(jumpCenter.X) + "," + F(jumpCenter.Y) + "," + F(jumpSize),
                "stick " + F(stickCenter.X) + "," + F(stickCenter.Y) + "," + F(stickSize),
                "wide " + (wideView ? "1" : "0")
            });
        }
        catch (Exception)
        {
            // Sin almacenamiento: el diseño solo dura mientras la app está abierta.
        }
    }

    private void LoadLayout()
    {
        try
        {
            if (!File.Exists(LayoutPath))
            {
                return;
            }
            foreach (string line in File.ReadAllLines(LayoutPath))
            {
                string[] p = line.Split(' ');
                if (p.Length < 2)
                {
                    continue;
                }
                if (p[0] == "wide")
                {
                    wideView = p[1] == "1";
                    continue;
                }
                string[] v = p[1].Split(',');
                if (v.Length < 3)
                {
                    continue;
                }
                float x = float.Parse(v[0], CultureInfo.InvariantCulture);
                float y = float.Parse(v[1], CultureInfo.InvariantCulture);
                float sz = float.Parse(v[2], CultureInfo.InvariantCulture);
                if (p[0] == "jump")
                {
                    jumpCenter = new Vector2(x, y);
                    jumpSize = sz;
                }
                else if (p[0] == "stick")
                {
                    stickCenter = new Vector2(x, y);
                    stickSize = sz;
                }
            }
            stickPos = stickCenter;
        }
        catch (Exception)
        {
            // Archivo dañado: se usa el diseño por defecto.
        }
    }

    private void ResetLayout()
    {
        jumpCenter = new Vector2(414f, 236f);
        jumpSize = 84f;
        stickCenter = new Vector2(72f, 212f);
        stickSize = 84f;
        stickPos = stickCenter;
        SaveLayout();
    }

    private void UpdateTouchFrame()
    {
        frameTouches = GetGameTouches();
        newTouches.Clear();
        HashSet<int> cur = new HashSet<int>();
        foreach (TouchLocation t in frameTouches)
        {
            if (t.State == TouchLocationState.Released || t.State == TouchLocationState.Invalid)
            {
                continue;
            }
            cur.Add(t.Id);
            if (!prevTouchIds.Contains(t.Id))
            {
                newTouches.Add(t);
            }
        }
        prevTouchIds = cur;
    }

    private void DrawDebug(Rectangle r)
    {
        if (!DEBUG_TOUCH || fntKootenay == null || pixelTex == null)
        {
            return;
        }
        float k = r.Width / (float)RENDER_W;
        Matrix m = Matrix.CreateScale(k) * Matrix.CreateTranslation(r.X, r.Y, 0f);
        TouchCollection touches = GetGameTouches();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, m);
        foreach (TouchLocation t in touches)
        {
            spriteBatch.Draw(pixelTex, new Rectangle((int)t.Position.X - 3, (int)t.Position.Y - 3, 6, 6), Color.Red);
        }
        string info = "toques " + touches.Count;
        if (touches.Count > 0)
        {
            info += " juego " + (int)touches[0].Position.X + "," + (int)touches[0].Position.Y + " " + touches[0].State;
        }
        info += " | ov " + dbgOverride + " ejec " + dbgRan + " ui " + AppMain.g_ao_sys_global.is_show_ui + " partida " + AppMain.IsStagePlaying();
        spriteBatch.DrawString(fntKootenay, info, new Vector2(4, 4), Color.Yellow, 0f, Vector2.Zero, 0.35f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }

    private void DrawControls(Rectangle r)
    {
        if (padSheet == null)
        {
            return;
        }
        // Solo durante la partida: en el título, menús y opciones no se dibuja nada.
        if (!AppMain.IsStagePlaying())
        {
            menuOpen = false;
            editing = false;
            dragId = -1;
            return;
        }
        float k = r.Width / (float)RENDER_W;
        Matrix m = Matrix.CreateScale(k) * Matrix.CreateTranslation(r.X, r.Y, 0f);
        TouchCollection touches = frameTouches;

        // El salto del motor usa la misma zona que se dibuja.
        AppMain.SetJumpKeyRect(
            (int)Math.Max(0f, jumpCenter.X - jumpSize / 2f),
            (int)Math.Max(0f, jumpCenter.Y - jumpSize / 2f),
            (int)Math.Min(RENDER_W, jumpCenter.X + jumpSize / 2f),
            (int)Math.Min(GAME_H, jumpCenter.Y + jumpSize / 2f));

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, m);
        if (menuOpen)
        {
            HandleMenu(newTouches);
            DrawMenu();
        }
        else if (editing)
        {
            HandleEdit(touches, newTouches);
            DrawEdit();
        }
        else
        {
            HandlePlay(touches);
            DrawPlay();
        }
        spriteBatch.End();
    }

    private void HandlePlay(TouchCollection touches)
    {
        Vector2 stickTarget = stickCenter;
        jumpHeld = false;
        foreach (TouchLocation t in touches)
        {
            if (t.State == TouchLocationState.Released || t.State == TouchLocationState.Invalid)
            {
                continue;
            }
            if (InRect(t.Position, GearRect))
            {
                continue;
            }
            if (Vector2.Distance(t.Position, jumpCenter) < jumpSize / 2f)
            {
                jumpHeld = true;
            }
            else if (t.Position.X < 200f)
            {
                Vector2 d = t.Position - stickCenter;
                float len = d.Length();
                float R = stickSize * 0.44f;
                if (len > R)
                {
                    d = d / len * R;
                }
                stickTarget = stickCenter + d;
            }
        }
        stickPos = Vector2.Lerp(stickPos, stickTarget, 0.35f);
        jumpScale += ((jumpHeld ? 0.92f : 1f) - jumpScale) * 0.3f;
    }

    private void DrawPlay()
    {
        Color c = new Color(255, 255, 255, 230);
        spriteBatch.Draw(gearTex, GearRect, new Color(255, 255, 255, 190));
        DrawStick(stickCenter, stickPos, c);
        DrawJump(jumpCenter, jumpSize, jumpScale, c);
    }

    private void DrawStick(Vector2 basePos, Vector2 knobPos, Color c)
    {
        spriteBatch.Draw(padSheet, basePos, SrcStickRing, c, 0f, new Vector2(128, 128), stickSize / 256f, SpriteEffects.None, 0f);
        spriteBatch.Draw(padSheet, knobPos, SrcBall, c, 0f, new Vector2(88, 88), stickSize * 0.5f / 176f, SpriteEffects.None, 0f);
    }

    private void DrawJump(Vector2 pos, float size, float scale, Color c)
    {
        spriteBatch.Draw(padSheet, pos, SrcJumpRing, c, 0f, new Vector2(128, 128), size / 256f * scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(padSheet, pos, SrcBall, c, 0f, new Vector2(88, 88), size * 0.7f / 176f * scale, SpriteEffects.None, 0f);
    }

    private void HandleMenu(List<TouchLocation> newTouches)
    {
        foreach (TouchLocation t in newTouches)
        {
            if (InRect(t.Position, MenuVistaRect))
            {
                wideView = !wideView;
                SaveLayout();
            }
            else if (InRect(t.Position, MenuEditRect))
            {
                menuOpen = false;
                editing = true;
            }
            else if (InRect(t.Position, MenuResetRect))
            {
                ResetLayout();
            }
            else if (InRect(t.Position, MenuCloseRect))
            {
                menuOpen = false;
            }
        }
    }

    private void DrawMenu()
    {
        spriteBatch.Draw(pixelTex, MenuPanel, new Color(16, 20, 18, 235));
        DrawButton(MenuVistaRect, wideView ? "Vista: Ancha" : "Vista: Normal");
        DrawButton(MenuEditRect, "Editar controles");
        DrawButton(MenuResetRect, "Restablecer diseno");
        DrawButton(MenuCloseRect, "Cerrar");
    }

    private void DrawButton(Rectangle rc, string text)
    {
        spriteBatch.Draw(pixelTex, rc, new Color(60, 72, 66, 235));
        if (fntKootenay != null)
        {
            Vector2 size = fntKootenay.MeasureString(text) * 0.55f;
            Vector2 pos = new Vector2(rc.X + rc.Width / 2f, rc.Y + rc.Height / 2f);
            spriteBatch.DrawString(fntKootenay, text, pos, Color.White, 0f, size / 0.55f / 2f, 0.55f, SpriteEffects.None, 0f);
        }
    }

    private void HandleEdit(TouchCollection touches, List<TouchLocation> newTouches)
    {
        // Empezar a arrastrar: solo con toques nuevos.
        foreach (TouchLocation t in newTouches)
        {
            if (dragId != -1)
            {
                break;
            }
            if (InRect(t.Position, ListoRect))
            {
                editing = false;
                SaveLayout();
                return;
            }
            Vector2 jh = jumpCenter + new Vector2(jumpSize / 2f, jumpSize / 2f);
            Vector2 sh = stickCenter + new Vector2(stickSize / 2f, stickSize / 2f);
            if (Vector2.Distance(t.Position, jh) < 18f) { dragKind = 3; dragId = t.Id; }
            else if (Vector2.Distance(t.Position, jumpCenter) < jumpSize / 2f) { dragKind = 1; dragId = t.Id; }
            else if (Vector2.Distance(t.Position, sh) < 18f) { dragKind = 2; dragId = t.Id; }
            else if (Vector2.Distance(t.Position, stickCenter) < stickSize / 2f) { dragKind = 0; dragId = t.Id; }
        }

        // Seguir el arrastre con el dedo que lo empezó.
        bool found = false;
        foreach (TouchLocation t in touches)
        {
            if (dragId == -1 || t.Id != dragId)
            {
                continue;
            }
            found = true;
            if (t.State == TouchLocationState.Released || t.State == TouchLocationState.Invalid)
            {
                dragId = -1;
                SaveLayout();
                continue;
            }
            Vector2 p = t.Position;
            switch (dragKind)
            {
                case 0:
                    stickCenter = new Vector2(Clamp(p.X, stickSize / 2f, 260f), Clamp(p.Y, stickSize / 2f, GAME_H - stickSize / 2f));
                    break;
                case 1:
                    jumpCenter = new Vector2(Clamp(p.X, jumpSize / 2f, RENDER_W - jumpSize / 2f), Clamp(p.Y, jumpSize / 2f, GAME_H - jumpSize / 2f));
                    break;
                case 2:
                    stickSize = Clamp(2f * Vector2.Distance(p, stickCenter), 40f, 140f);
                    break;
                case 3:
                    jumpSize = Clamp(2f * Vector2.Distance(p, jumpCenter), 36f, 150f);
                    break;
            }
        }
        if (dragId != -1 && !found)
        {
            dragId = -1;
            SaveLayout();
        }
        stickPos = stickCenter;
    }

    private void DrawEdit()
    {
        Color c = new Color(255, 255, 255, 230);
        DrawStick(stickCenter, stickCenter, c);
        DrawJump(jumpCenter, jumpSize, 1f, c);
        DrawFrame(new Rectangle((int)(stickCenter.X - stickSize / 2f), (int)(stickCenter.Y - stickSize / 2f), (int)stickSize, (int)stickSize));
        DrawFrame(new Rectangle((int)(jumpCenter.X - jumpSize / 2f), (int)(jumpCenter.Y - jumpSize / 2f), (int)jumpSize, (int)jumpSize));
        spriteBatch.Draw(pixelTex, ListoRect, new Color(191, 232, 138, 235));
        if (fntKootenay != null)
        {
            Vector2 size = fntKootenay.MeasureString("Listo") * 0.55f;
            spriteBatch.DrawString(fntKootenay, "Listo", new Vector2(ListoRect.X + ListoRect.Width / 2f, ListoRect.Y + ListoRect.Height / 2f), new Color(10, 15, 12), 0f, size / 0.55f / 2f, 0.55f, SpriteEffects.None, 0f);
        }
    }

    private void DrawFrame(Rectangle rc)
    {
        Color col = new Color(191, 232, 138, 220);
        spriteBatch.Draw(pixelTex, new Rectangle(rc.X, rc.Y, rc.Width, 2), col);
        spriteBatch.Draw(pixelTex, new Rectangle(rc.X, rc.Bottom - 2, rc.Width, 2), col);
        spriteBatch.Draw(pixelTex, new Rectangle(rc.X, rc.Y, 2, rc.Height), col);
        spriteBatch.Draw(pixelTex, new Rectangle(rc.Right - 2, rc.Y, 2, rc.Height), col);
        spriteBatch.Draw(pixelTex, new Rectangle(rc.Right - 9, rc.Bottom - 9, 10, 10), col);
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
