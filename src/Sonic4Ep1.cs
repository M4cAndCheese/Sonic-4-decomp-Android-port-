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
    // Alto lógico del motor: 320. La ortho 2D usa 720 unidades de alto y 1080 de ancho (2.25 u/px en ambos ejes).
    // Con 288 el eje vertical quedaba a 2.5 u/px y la imagen se veía achicada.
    public const int GAME_H = 320;
    // Ancho real del render: se calcula según la proporción de la pantalla (ultrawide = más ancho).
    // Con alto fijo a GAME_H el juego no se estira ni se recorta: solo se ve más escenario a los lados.
    public static int RENDER_W = GAME_W;
    // Ancho máximo del mundo visible (21:9 a 320 px de alto ronda 746; 24:9 ronda 853).
    public const int MAX_RENDER_W = 864;
    // Ultrawide sin estirar: el escenario se ensancha según la pantalla, la escala vertical no cambia.
    public const bool ULTRAWIDE_ENABLED = true;
    // Factor de resolución interna: el juego se dibuja a más píxeles y se reduce a la pantalla sin perder nitidez.
    public static int RENDER_SCALE = 1;
    // Límite de tamaño de textura del render target (seguro en casi todos los Android).
    public const int MAX_TARGET_DIM = 4096;
    private RenderTarget2D gameTarget;
    private Rectangle gameRect;
    private RenderTarget2D blurTarget;
    private RenderTarget2D softTarget;
    // Filtro suave tipo "vaselina": el escenario se reduce y se vuelve a estirar (más borroso). Opcional.
    private bool softFilter;
    private bool prevBackPressed;

    // Controles grises del port de Sonic 1 2013 (dibujados encima del pad del motor, que queda oculto).
    private Texture2D padSheet;
    private Texture2D pixelTex;
    private Texture2D gearTex;
    // Diseño por defecto, medido de la captura del dispositivo (coordenadas del motor, alto GAME_H = 320).
    // d-pad abajo a la izquierda y salto abajo a la derecha. Lo usan los builds que no tengan pad_layout.txt.
    private const float DefEdge = 68f;        // centro X del d-pad desde el borde izquierdo
    private const float DefJumpEdge = 58f;    // centro X del salto desde el borde derecho (ancla a RENDER_W)
    private const float DefY = 244f;          // centro Y de ambos botones
    private const float DefStickSize = 74f;   // diámetro del d-pad
    private const float DefJumpSize = 74f;    // diámetro del salto
    private Vector2 stickCenter = new Vector2(68f, 244f);
    private Vector2 jumpCenter = new Vector2(625f, 244f);
    private float stickSize = 74f;
    private float jumpSize = 74f;
    private Vector2 stickPos;
    private float jumpScale = 1f;
    private bool jumpHeld;
    // Hoja nueva (512x512): aro gris con flechas, aro gris liso, bola brillante.
    private static readonly Rectangle SrcStickRing = new Rectangle(0, 0, 256, 256);
    private static readonly Rectangle SrcJumpRing = new Rectangle(256, 256, 256, 256);
    private static readonly Rectangle SrcBall = new Rectangle(12, 322, 176, 176);
    // Diagnóstico de toques (se ve arriba a la izquierda). Poner en false cuando ya no haga falta.
    private const bool DEBUG_TOUCH = false;
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
    // Rectángulos diseñados para 480 px. El engranaje va anclado a la derecha y el resto centrado.
    private static readonly Rectangle GearBase = new Rectangle(438, 52, 32, 32);
    private static readonly Rectangle ListoBase = new Rectangle(190, 8, 100, 28);
    private static readonly Rectangle MenuPanelBase = new Rectangle(140, 60, 200, 218);
    private static readonly Rectangle MenuVistaBase = new Rectangle(160, 100, 160, 26);
    private static readonly Rectangle MenuSoftBase = new Rectangle(160, 134, 160, 26);
    private static readonly Rectangle MenuEditBase = new Rectangle(160, 168, 160, 26);
    private static readonly Rectangle MenuResetBase = new Rectangle(160, 202, 160, 26);
    private static readonly Rectangle MenuCloseBase = new Rectangle(160, 236, 160, 26);

    private Rectangle PauseRect { get { return new Rectangle(RENDER_W / 2 - 58, 0, 116, 60); } }

    private static Rectangle ShiftX(Rectangle r, int dx)
    {
        return new Rectangle(r.X + dx, r.Y, r.Width, r.Height);
    }

    private int CenterDX { get { return (RENDER_W - GAME_W) / 2; } }
    private Rectangle GearRect { get { return ShiftX(GearBase, RENDER_W - GAME_W); } }
    private Rectangle ListoRect { get { return ShiftX(ListoBase, CenterDX); } }
    private Rectangle MenuPanel { get { return ShiftX(MenuPanelBase, CenterDX); } }
    private Rectangle MenuVistaRect { get { return ShiftX(MenuVistaBase, CenterDX); } }
    private Rectangle MenuSoftRect { get { return ShiftX(MenuSoftBase, CenterDX); } }
    private Rectangle MenuEditRect { get { return ShiftX(MenuEditBase, CenterDX); } }
    private Rectangle MenuResetRect { get { return ShiftX(MenuResetBase, CenterDX); } }
    private Rectangle MenuCloseRect { get { return ShiftX(MenuCloseBase, CenterDX); } }

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
        // Tamaño REAL de la pantalla (incluye barra de navegación/recorte). Con DisplayMetrics comunes el backbuffer
        // tiene otra proporción que la superficie y Android lo estira al presentar: escenas aplastadas.
        var dm = new Android.Util.DisplayMetrics();
        var wm = Android.Runtime.Extensions.JavaCast<Android.Views.IWindowManager>(
            Android.App.Application.Context.GetSystemService(Android.Content.Context.WindowService));
        if (wm != null && wm.DefaultDisplay != null)
        {
            wm.DefaultDisplay.GetRealMetrics(dm);
        }
        else
        {
            dm = Android.App.Application.Context.Resources.DisplayMetrics;
        }
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
        TiltSensor.Instance.Start();
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
        // Ultrawide sin estirar: el alto se queda en 288 y el ancho sigue la proporción real de la pantalla.
        float screenAspect = graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
        RENDER_W = ULTRAWIDE_ENABLED ? Math.Max(GAME_W, Math.Min(MAX_RENDER_W, (int)Math.Round(GAME_H * screenAspect))) : GAME_W;
        // Resolución interna: aprox. la altura real de la pantalla, limitada por el tamaño máximo de textura.
        // Ultra HD: 2x la altura de la pantalla, limitado por MAX_TARGET_DIM.
        // Render a resolución nativa (480x320): los assets del motor son de esa resolución. Con muestreo de píxel
        // cercano (modo nítido) el escalado a la pantalla se ve como pixel art; con filtro suave se mezcla.
        RENDER_SCALE = 1;
        gameTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.Depth24);
        // Versión mínima del frame para hacer el fondo difuminado de los laterales.
        blurTarget = new RenderTarget2D(base.GraphicsDevice, 32, 18, false, SurfaceFormat.Color, DepthFormat.None);
        softTarget = new RenderTarget2D(base.GraphicsDevice, Math.Max(1, RENDER_W * 3 / 4), GAME_H * 3 / 4, false, SurfaceFormat.Color, DepthFormat.None);
        SetDefaultLayout();
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
        TiltSensor.Instance.Stop();
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
        TiltSensor.Instance.Start();
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
        // Botón de pausa del HUD (centrado arriba). La zona fija del motor no coincide con el ultrawide,
        // así que aquí se detecta el toque y se pide la pausa directamente.
        if (!menuOpen && !editing && AppMain.IsStageTouchActive())
        {
            foreach (TouchLocation t in newTouches)
            {
                if (InRect(t.Position, PauseRect))
                {
                    AppMain.g_pause_flag = true;
                }
            }
        }
        // El engranaje usa los toques nuevos de este fotograma (no depende del estado Pressed).
        if (!menuOpen && !editing && AppMain.IsStageTouchActive() && !AppMain.IsSpecialStageNow())
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
            // Acelerómetro real: en fases especiales el motor lo usa para girar al inclinar el celular.
            accel = TiltSensor.Instance.Value;
            accel.Y *= TiltSensor.SteerSign;
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
        if (gameTarget != null && softFilter && softTarget != null)
        {
            GraphicsDevice.SetRenderTarget(softTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(gameTarget, softTarget.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
        }
        if (gameTarget != null)
        {
            // Fondo difuminado: el frame reducido y estirado cubre los laterales sin tocar el juego.
            GraphicsDevice.SetRenderTarget(blurTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(gameTarget, blurTarget.Bounds, Color.White);
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            // El fondo difuminado usa muestreo lineal (es borroso a propósito); la capa del juego usa el modo elegido.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            if (wideView)
            {
                spriteBatch.Draw(blurTarget, GraphicsDevice.Viewport.Bounds, new Color(110, 110, 110));
            }
            spriteBatch.End();
            // Escala entera: píxeles nítidos. Escala fraccionaria: muestreo lineal para no dejar bordes en diente de sierra.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, (softFilter || !IsIntegerScale()) ? SamplerState.LinearClamp : SamplerState.PointClamp, null, null);
            spriteBatch.Draw(softFilter && softTarget != null ? (Texture2D)softTarget : gameTarget, GetGameRect(), Color.White);
            spriteBatch.End();
            // En fases especiales no hay controles en pantalla: se juega moviendo el celular.
            if (!AppMain.IsSpecialStageNow())
            {
                DrawControls(GetGameRect());
            }
            DrawDebug(GetGameRect());
        }
        LiveFeature.getInstance().ShowOverride();
        Upsell.drawUpsellScreen();
        base.Draw(gameTime);
    }

    // ---------- Controles táctiles: juego, menú del engranaje y edición ----------
    // Coordenadas del juego (480x320). Se dibuja con una matriz que escala al área del juego en pantalla.

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
                "ver 4",
                "jump " + F(RENDER_W - jumpCenter.X) + "," + F(jumpCenter.Y) + "," + F(jumpSize),
                "stick " + F(stickCenter.X) + "," + F(stickCenter.Y) + "," + F(stickSize),
                "wide " + (wideView ? "1" : "0"),
                "soft " + (softFilter ? "1" : "0")
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
            string[] lines = File.ReadAllLines(LayoutPath);
            // Disposiciones de versiones anteriores (sin la línea ver 3) se ignoran y se usa el diseño nuevo.
            int ver = lines.Length > 0 && lines[0] == "ver 4" ? 4 : (lines.Length > 0 && lines[0] == "ver 3" ? 3 : 0);
            bool current = ver != 0;
            foreach (string line in lines)
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
                if (p[0] == "soft")
                {
                    softFilter = p[1] == "1";
                    continue;
                }
                if (!current)
                {
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
                    // ver 4: x es la distancia desde el borde derecho. ver 3: x era absoluto en el ancho de ese momento.
                    float jx = ver == 4 ? RENDER_W - x : x;
                    jumpCenter = new Vector2(jx, y);
                    jumpSize = sz;
                }
                else if (p[0] == "stick")
                {
                    stickCenter = new Vector2(x, y);
                    stickSize = sz;
                }
            }
            // Que el botón nunca quede fuera de la pantalla actual (layouts guardados con otro ancho).
            jumpCenter = new Vector2(Clamp(jumpCenter.X, jumpSize / 2f, RENDER_W - jumpSize / 2f),
                                     Clamp(jumpCenter.Y, jumpSize / 2f, GAME_H - jumpSize / 2f));
            stickCenter = new Vector2(Clamp(stickCenter.X, stickSize / 2f, RENDER_W * 0.5f),
                                      Clamp(stickCenter.Y, stickSize / 2f, GAME_H - stickSize / 2f));
            stickPos = stickCenter;
        }
        catch (Exception)
        {
            // Archivo dañado: se usa el diseño por defecto.
        }
    }

    private void SetDefaultLayout()
    {
        jumpCenter = new Vector2(RENDER_W - DefJumpEdge, DefY);
        jumpSize = DefJumpSize;
        stickCenter = new Vector2(DefEdge, DefY);
        stickSize = DefStickSize;
        stickPos = stickCenter;
    }

    private void ResetLayout()
    {
        SetDefaultLayout();
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

    // Diagnóstico de proporciones: se activa con DEBUG_LAYOUT. Muestra tamaños reales de backbuffer, viewport y escala.
    private const bool DEBUG_LAYOUT = true;

    private void DrawLayoutDebug(Rectangle r)
    {
        if (!DEBUG_LAYOUT || fntKootenay == null)
        {
            return;
        }
        Viewport vp = GraphicsDevice.Viewport;
        string info =
            "backbuffer " + graphics.PreferredBackBufferWidth + "x" + graphics.PreferredBackBufferHeight + "\n" +
            "viewport " + vp.Width + "x" + vp.Height + "\n" +
            "RENDER " + RENDER_W + "x" + GAME_H + "  scale " + F(r.Width / (float)RENDER_W) + "," + F(r.Height / (float)GAME_H) + "\n" +
            "gameRect " + r.Width + "x" + r.Height + " @" + r.X + "," + r.Y + "\n" +
            "aspect proy " + F(AppMain.AMD_SCREEN_ASPECT) + "  2D " + AppMain.AMD_SCREEN_2D_WIDTH + "x" + AppMain.AMD_SCREEN_2D_HEIGHT;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        spriteBatch.DrawString(fntKootenay, info, new Vector2(8, vp.Height * 0.55f), Color.Yellow, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }

    private void DrawDebug(Rectangle r)
    {
        DrawLayoutDebug(r);
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
        info += " | ov " + dbgOverride + " ejec " + dbgRan + " " + AppMain.DebugStageState();
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
        if (!AppMain.IsStageTouchActive())
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

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, m);
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

    // Zona de dirección del motor (CPadVirtualPad): va centrada en la perilla que se dibuja.
    // El motor compara la posición del toque, transformada como (y - 16) / 1.05, contra el centro del área,
    // así que esa transformación se compensa aquí. Medio ancho 90 y medio alto 100 dan una banda izquierda/derecha de ±40 px.
    public static float[] GetDPadArea(Vector2 center)
    {
        float mid = (center.Y - 16f) / 1.05f;
        return new float[] { center.X - 90f, mid - 100f, center.X + 90f, mid + 100f };
    }

    private void HandlePlay(TouchCollection touches)
    {
        // Que el motor lea la cruceta donde está el joystick que se ve (antes su zona estaba fija y había que empujar de más).
        AppMain.CPadVirtualPad.CreateInstance().Create(GetDPadArea(stickCenter));
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
            else if (t.Position.X < RENDER_W * 0.5f)
            {
                Vector2 d = t.Position - stickCenter;
                float len = d.Length();
                // Recorrido de la perilla: con radio de perilla 0.23 x tamaño, el borde queda dentro del aro (0.5 x tamaño).
                float R = stickSize * 0.25f;
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
        spriteBatch.Draw(padSheet, knobPos, SrcBall, c, 0f, new Vector2(88, 88), stickSize * 0.66f / 176f, SpriteEffects.None, 0f);
    }

    private void DrawJump(Vector2 pos, float size, float scale, Color c)
    {
        // Sin cuadro alrededor al presionar: solo se aclara el aro del botón.
        if (jumpHeld)
        {
            c = Color.White;
        }
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
            else if (InRect(t.Position, MenuSoftRect))
            {
                softFilter = !softFilter;
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
        DrawButton(MenuSoftRect, softFilter ? "Filtro: Suave" : "Filtro: Nitido");
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
                    stickCenter = new Vector2(Clamp(p.X, stickSize / 2f, RENDER_W * 0.5f), Clamp(p.Y, stickSize / 2f, GAME_H - stickSize / 2f));
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

    private bool IsIntegerScale()
    {
        Rectangle r = GetGameRect();
        float s = r.Width / (float)RENDER_W;
        return Math.Abs(s - Math.Round(s)) < 0.001f;
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

    // Toques en coordenadas de pantalla -> coordenadas del juego (RENDER_W x GAME_H).
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
