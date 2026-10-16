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
    // Capa del HUD: se dibuja sin zoom encima del escenario.
    private RenderTarget2D hudTarget;
    // Interruptor para aislar problemas: false devuelve el HUD al escenario con zoom.
    public const bool HUD_LAYER_ENABLED = false;
    // Interruptor: false usa el fondo de carga anterior (desenfocado) en vez de negro.
    public const bool LOADING_BLACK_BASE = false;
    private RenderTargetBinding[] hudPrevTargets;
    private Viewport hudPrevViewport;
    // Mantiene el fondo de carga unos fotogramas más para que los laterales no parpadeen.
    private int bootHoldFrames;
    private Rectangle gameRect;
    private RenderTarget2D blurTarget;
    private RenderTarget2D softTarget;
    // Filtro suave tipo "vaselina": el escenario se reduce y se vuelve a estirar (más borroso). Opcional.
    // Suavizado activo por defecto: evita el pixelado al escalar a la pantalla.
    private bool softFilter = true;
    private bool prevBackPressed;

    // Controles grises del port de Sonic 1 2013 (dibujados encima del pad del motor, que queda oculto).
    private Texture2D padSheet;
    private string padError;
    // Escala 3D ajustable desde el engranaje (1.00 = la del motor sin corrección extra).
    private static readonly float[] Scale3DSteps = { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 1.1f, 1.2f };
    private int scale3DIndex = 0;
    // Pausa por toque: se ignora un rato después de volver de la pausa o al empezar la partida.
    private int pauseCooldown;
    private bool wasStageActive;
    // Tras volver de la pausa, los dedos que ya estaban apoyados no cuentan hasta soltarlos.
    private bool touchLockedUntilRelease;
    // Cuándo se desactivó la app: paneles del sistema (volumen, no molestar) no deben pausar el juego.
    private DateTime deactivatedAt = DateTime.MinValue;
    private Texture2D pixelTex;
    private Texture2D gearTex;
    // Diseño por defecto, medido de la captura de referencia (coordenadas del motor, alto GAME_H = 320).
    // d-pad abajo a la izquierda y salto abajo a la derecha, mismo tamaño y posición que la captura.
    private const float DefEdge = 74f;         // centro X del d-pad desde el borde izquierdo
    private const float DefJumpEdge = 74.9f;   // centro X del salto desde el borde derecho (ancla a RENDER_W)
    private const float DefStickY = 245f;      // centro Y del d-pad
    private const float DefJumpY = 250.5f;     // centro Y del salto
    private const float DefStickSize = 83.4f;  // diámetro del d-pad
    private const float DefJumpSize = 71.3f;   // diámetro del salto
    private Vector2 stickCenter = new Vector2(DefEdge, DefStickY);
    private Vector2 jumpCenter = new Vector2(DefJumpEdge, DefJumpY);
    private float stickSize = DefStickSize;
    private float jumpSize = DefJumpSize;
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
    // Editor de HUD: tamaño (1 = original) y desplazamiento en píxeles de juego (+ derecha / + abajo).
    private bool hudEditing;
    private float hudScale = HUD_DEFAULT_SCALE;
    private float hudOffX = HUD_DEFAULT_OFF_X;
    private float hudOffY = HUD_DEFAULT_OFF_Y;
    // Signos del desplazamiento: si al mover el HUD va al revés, cambia a -1.
    public const float HUD_X_SIGN = 1f;
    public const float HUD_Y_SIGN = 1f;
    private const float HUD_STEP_PX = 4f;
    private const float HUD_SCALE_STEP = 0.05f;
    private const float HUD_MIN_SCALE = 0.5f;
    private const float HUD_MAX_SCALE = 1.5f;
    private const float HUD_MAX_OFFSET_PX = 200f;
    // Tamaño por defecto del HUD: 70% del original (el original se ve grande en pantalla de celular).
    // Posición de referencia tomada de la captura 1146581 (estimada a ojo, ±3 px).
    private const float HUD_DEFAULT_SCALE = 1f;
    private const float HUD_DEFAULT_OFF_X = -3f;
    private const float HUD_DEFAULT_OFF_Y = 0f;
    // Caja editable del HUD (píxeles de juego, a escala 1).
    private const float HUD_BOX_W = 440f;
    private const float HUD_BOX_H = 64f;
    private int hudDragId = -1;
    private int hudDragMode = -1;   // 0 mover, 1 escalar (esquina)
    private Vector2 hudDragLast;
    private float hudDragStartScale = 1f;
    private float hudDragStartDist = 1f;
    private static readonly Rectangle HudEditDoneBase = new Rectangle(190, 280, 100, 28);
    private static readonly Rectangle HudEditResetBase = new Rectangle(70, 280, 100, 28);
    private bool wideView = true;
    private int dragId = -1;
    private int dragKind = -1; // 0 mover joystick, 1 mover salto, 2 redimensionar joystick, 3 redimensionar salto
    // Rectángulos diseñados para 480 px. El engranaje va anclado a la derecha y el resto centrado.
    private static readonly Rectangle GearBase = new Rectangle(438, 52, 32, 32);
    private static readonly Rectangle ListoBase = new Rectangle(190, 8, 100, 28);
    private static readonly Rectangle MenuPanelBase = new Rectangle(140, 80, 200, 228);
    private static readonly Rectangle MenuVistaBase = new Rectangle(160, 92, 160, 26);
    private static readonly Rectangle MenuSoftBase = new Rectangle(160, 122, 160, 26);
    private static readonly Rectangle MenuEditBase = new Rectangle(160, 152, 160, 26);
    private static readonly Rectangle MenuResetBase = new Rectangle(160, 182, 160, 26);
    private static readonly Rectangle MenuScaleBase = new Rectangle(160, 212, 160, 26);
    private static readonly Rectangle MenuHudBase = new Rectangle(160, 242, 160, 26);
    private static readonly Rectangle MenuCloseBase = new Rectangle(160, 272, 160, 26);
    // Panel del editor de HUD.
    private static readonly Rectangle HudPanelBase = new Rectangle(140, 60, 200, 196);
    private static readonly Rectangle HudPlusBase = new Rectangle(160, 70, 76, 26);
    private static readonly Rectangle HudMinusBase = new Rectangle(244, 70, 76, 26);
    private static readonly Rectangle HudUpBase = new Rectangle(196, 104, 88, 24);
    private static readonly Rectangle HudLeftBase = new Rectangle(160, 132, 76, 24);
    private static readonly Rectangle HudRightBase = new Rectangle(244, 132, 76, 24);
    private static readonly Rectangle HudDownBase = new Rectangle(196, 160, 88, 24);
    private static readonly Rectangle HudExportBase = new Rectangle(160, 192, 76, 26);
    private static readonly Rectangle HudImportBase = new Rectangle(244, 192, 76, 26);
    private static readonly Rectangle HudResetBase = new Rectangle(160, 224, 76, 26);
    private static readonly Rectangle HudDoneBase = new Rectangle(244, 224, 76, 26);

    // Botón de pausa dibujado por el motor: en 480 px va en x=215..331; con ultrawide se desplaza CenterDX.
    private Rectangle PauseRect { get { return new Rectangle(CenterDX + 215, 0, 116, 60); } }

    private static Rectangle ShiftX(Rectangle r, int dx)
    {
        return new Rectangle(r.X + dx, r.Y, r.Width, r.Height);
    }

    private int CenterDX { get { return (RENDER_W - GAME_W) / 2; } }
    private Rectangle GearRect { get { return ShiftX(GearBase, (int)Math.Round(RENDER_W - GAME_W - CROP_RIGHT)); } }
    private Rectangle ListoRect { get { return ShiftX(ListoBase, CenterDX); } }
    private Rectangle MenuPanel { get { return ShiftX(MenuPanelBase, CenterDX); } }
    private Rectangle MenuVistaRect { get { return ShiftX(MenuVistaBase, CenterDX); } }
    private Rectangle MenuSoftRect { get { return ShiftX(MenuSoftBase, CenterDX); } }
    private Rectangle MenuEditRect { get { return ShiftX(MenuEditBase, CenterDX); } }
    private Rectangle MenuResetRect { get { return ShiftX(MenuResetBase, CenterDX); } }
    private Rectangle MenuCloseRect { get { return ShiftX(MenuCloseBase, CenterDX); } }
    private Rectangle MenuHudRect { get { return ShiftX(MenuHudBase, CenterDX); } }
    private Rectangle HudPanel { get { return ShiftX(HudPanelBase, CenterDX); } }
    private Rectangle HudPlusRect { get { return ShiftX(HudPlusBase, CenterDX); } }
    private Rectangle HudMinusRect { get { return ShiftX(HudMinusBase, CenterDX); } }
    private Rectangle HudUpRect { get { return ShiftX(HudUpBase, CenterDX); } }
    private Rectangle HudLeftRect { get { return ShiftX(HudLeftBase, CenterDX); } }
    private Rectangle HudRightRect { get { return ShiftX(HudRightBase, CenterDX); } }
    private Rectangle HudDownRect { get { return ShiftX(HudDownBase, CenterDX); } }
    private Rectangle HudExportRect { get { return ShiftX(HudExportBase, CenterDX); } }
    private Rectangle HudImportRect { get { return ShiftX(HudImportBase, CenterDX); } }
    private Rectangle HudResetRect { get { return ShiftX(HudResetBase, CenterDX); } }
    private Rectangle HudDoneRect { get { return ShiftX(HudDoneBase, CenterDX); } }
    private Rectangle MenuScaleRect { get { return ShiftX(MenuScaleBase, CenterDX); } }

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
        // Proporción del área REAL de dibujo (no del backbuffer pedido), para que el alto ocupe toda la pantalla.
        Viewport dev = base.GraphicsDevice.Viewport;
        float screenAspect = dev.Width / (float)dev.Height;
        RENDER_W = ULTRAWIDE_ENABLED ? Math.Max(GAME_W, Math.Min(MAX_RENDER_W, (int)Math.Round(GAME_H * screenAspect))) : GAME_W;
        // Resolución interna: aprox. la altura real de la pantalla, limitada por el tamaño máximo de textura.
        // Ultra HD: 2x la altura de la pantalla, limitado por MAX_TARGET_DIM.
        // Render a resolución nativa (480x320): los assets del motor son de esa resolución. Con muestreo de píxel
        // cercano (modo nítido) el escalado a la pantalla se ve como pixel art; con filtro suave se mezcla.
        // Render al doble de resolución interna: se reduce a la pantalla con menos pixelado.
        RENDER_SCALE = 3;
        gameTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.Depth24);
        hudTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.None);
        AppMain.pauseKeyX = (uint)PauseRect.X;
        // Versión mínima del frame para hacer el fondo difuminado de los laterales.
        blurTarget = new RenderTarget2D(base.GraphicsDevice, 32, 18, false, SurfaceFormat.Color, DepthFormat.None);
        softTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.None);
        SetDefaultLayout();
        try
        {
            using (var st = GameData.OpenStream("ui/pad_hq.png"))
            {
                padSheet = Texture2D.FromStream(base.GraphicsDevice, st);
            }
            padError = null;
        }
        catch (Exception e)
        {
            padSheet = null; // Sin la hoja de sprites, el juego usa el pad del motor.
            padError = e.GetType().Name + ": " + e.Message;
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
        deactivatedAt = DateTime.UtcNow;
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
        // Solo pausa si la app estuvo realmente fuera (más de 1.5 s); los paneles del sistema no pausan.
        bool realSwitch = deactivatedAt == DateTime.MinValue || (DateTime.UtcNow - deactivatedAt).TotalSeconds > 1.5;
        if (realSwitch && (AppMain.g_gm_main_system.game_flag & 0x40) == 0)
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
        bool stageActive = !menuOpen && !editing && AppMain.IsStageTouchActive();
        if (pauseCooldown > 0)
        {
            pauseCooldown--;
        }
        // Al empezar la partida o al volver de la pausa, los toques no cuentan como pausa durante un momento.
        if (stageActive && !wasStageActive)
        {
            pauseCooldown = 45;
            touchLockedUntilRelease = TouchPanel.GetState().Count > 0;
        }
        wasStageActive = stageActive;
        if (stageActive && pauseCooldown == 0)
        {
            foreach (TouchLocation t in newTouches)
            {
                if (InRect(t.Position, PauseRect))
                {
                    AppMain.g_pause_flag = true;
                    pauseCooldown = 60;
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
        if (hudTarget != null)
        {
            GraphicsDevice.SetRenderTarget(hudTarget);
            GraphicsDevice.Clear(Color.Transparent);
        }
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
            // En pantallas de arranque el fondo se ve a brillo completo y no hay fondo difuminado tapando los laterales.
            bool bootNow = AppMain.IsBootScreenNow();
            if (bootNow)
            {
                bootHoldFrames = 10;
            }
            else if (bootHoldFrames > 0)
            {
                bootHoldFrames--;
            }
            bool boot = bootNow || bootHoldFrames > 0;
            // El fondo difuminado usa muestreo lineal (es borroso a propósito); la capa del juego usa el modo elegido.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
            if (AppMain.IsSonicTeamLogoNow())
            {
                spriteBatch.Draw(pixelTex, GraphicsDevice.Viewport.Bounds, Color.White);
            }
            else if (LOADING_BLACK_BASE && AppMain.IsLoadingScreenNow())
            {
                // Carga: base negra fija, así los laterales no cambian entre fotogramas.
                spriteBatch.Draw(pixelTex, GraphicsDevice.Viewport.Bounds, Color.Black);
            }
            else
            {
                spriteBatch.Draw(blurTarget, GraphicsDevice.Viewport.Bounds, (wideView && !boot) ? new Color(110, 110, 110) : Color.White);
            }
            spriteBatch.End();
            // Pantallas de arranque y carga: el contenido de 480 px se extiende a los laterales repitiendo su borde
            // izquierdo y derecho (barra negra/azul de la carga, fondo blanco del logo), sin el fondo difuminado.
            if (boot)
            {
                Rectangle bb = GetBaseRect();
                float k = bb.Width / (float)RENDER_W;
                int side = (int)Math.Round(CenterDX * k);
                if (side > 0)
                {
                    int leftCol = CenterDX * RENDER_SCALE + 1;
                    int rightCol = (CenterDX + GAME_W) * RENDER_SCALE - 2;
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, null, null);
                    spriteBatch.Draw(gameTarget, new Rectangle(bb.X, bb.Y, side, bb.Height), new Rectangle(leftCol, 0, 1, gameTarget.Height), Color.White);
                    spriteBatch.Draw(gameTarget, new Rectangle(bb.X + bb.Width - side, bb.Y, side, bb.Height), new Rectangle(rightCol, 0, 1, gameTarget.Height), Color.White);
                    spriteBatch.End();
                }
            }
            // Escala entera: píxeles nítidos. Escala fraccionaria: muestreo lineal para no dejar bordes en diente de sierra.
            // AlphaBlend: las zonas sin nivel son transparentes y dejan ver el fondo de los laterales.
            SamplerState gameSampler = (softFilter || !IsIntegerScale()) ? SamplerState.LinearClamp : SamplerState.PointClamp;
            Texture2D gameTex = softFilter && softTarget != null ? (Texture2D)softTarget : gameTarget;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, gameSampler, null, null);
            spriteBatch.Draw(gameTex, GetGameRect(), Color.White);
            spriteBatch.End();
            // HUD a escala original, encima del escenario (no se estira ni se agranda con el zoom).
            if (!boot && HUD_LAYER_ENABLED && hudTarget != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, gameSampler, null, null);
                spriteBatch.Draw(hudTarget, GetBaseRect(), Color.White);
                spriteBatch.End();
            }
            // En fases especiales no hay controles en pantalla: se juega moviendo el celular.
            if (!boot && !AppMain.IsSpecialStageNow())
            {
                DrawControls(GetBaseRect());
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

    private static string HudExportPath
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "layout_export.txt"); }
    }

    private void SaveLayout()
    {
        SaveLayoutTo(LayoutPath);
    }

    private void SaveLayoutTo(string path)
    {
        try
        {
            File.WriteAllLines(path, new string[]
            {
                "ver 5",
                "jump " + F(RENDER_W - jumpCenter.X) + "," + F(jumpCenter.Y) + "," + F(jumpSize),
                "stick " + F(stickCenter.X) + "," + F(stickCenter.Y) + "," + F(stickSize),
                "wide " + (wideView ? "1" : "0"),
                "soft " + (softFilter ? "1" : "0"),
                "k3d " + F(Scale3DSteps[scale3DIndex]),
                "hud_s " + F(hudScale),
                "hud_x " + F(hudOffX),
                "hud_y " + F(hudOffY)
            });
        }
        catch (Exception)
        {
            // Sin almacenamiento: el diseño solo dura mientras la app está abierta.
        }
    }

    private void LoadLayout()
    {
        LoadLayoutFrom(LayoutPath);
    }

    private void LoadLayoutFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }
            string[] lines = File.ReadAllLines(path);
            // Posiciones de botones guardadas en versiones anteriores se ignoran: se usa el diseño nuevo.
            // Vista, filtro y escala 3D de esas versiones sí se siguen leyendo.
            int ver = lines.Length > 0 && lines[0] == "ver 5" ? 5 : 0;
            bool current = ver == 5;
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
                if (p[0] == "hud_s" || p[0] == "hud_x" || p[0] == "hud_y")
                {
                    float hv;
                    if (float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out hv))
                    {
                        if (p[0] == "hud_s") { hudScale = Clamp(hv, HUD_MIN_SCALE, HUD_MAX_SCALE); }
                        else if (p[0] == "hud_x") { hudOffX = Clamp(hv, -HUD_MAX_OFFSET_PX, HUD_MAX_OFFSET_PX); }
                        else { hudOffY = Clamp(hv, -HUD_MAX_OFFSET_PX, HUD_MAX_OFFSET_PX); }
                    }
                    continue;
                }
                if (p[0] == "k3d")
                {
                    float k;
                    if (float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out k))
                    {
                        for (int i = 0; i < Scale3DSteps.Length; i++)
                        {
                            if (Math.Abs(Scale3DSteps[i] - k) < 0.001f) { scale3DIndex = i; }
                        }
                        OpenGL.Scale3D = Scale3DSteps[scale3DIndex];
                    }
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
                    // x es la distancia desde el borde derecho.
                    float jx = RENDER_W - x;
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
        jumpCenter = new Vector2(RENDER_W - DefJumpEdge, DefJumpY);
        jumpSize = DefJumpSize;
        stickCenter = new Vector2(DefEdge, DefStickY);
        stickSize = DefStickSize;
        stickPos = stickCenter;
    }

    private void ResetLayout()
    {
        SetDefaultLayout();
        ResetHudLayout();
        SaveLayout();
    }

    private void ResetHudLayout()
    {
        hudScale = HUD_DEFAULT_SCALE;
        hudOffX = HUD_DEFAULT_OFF_X;
        hudOffY = HUD_DEFAULT_OFF_Y;
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
    private const bool DEBUG_LAYOUT = false;

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
            "aspect proy " + F(AppMain.AMD_SCREEN_ASPECT) + "  2D " + AppMain.AMD_SCREEN_2D_WIDTH + "x" + AppMain.AMD_SCREEN_2D_HEIGHT + "\n" +
            "pad " + (padSheet != null ? "ok" : "ERROR " + padError) + "  partida " + AppMain.IsStageTouchActive();
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
            hudEditing = false;
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
        else if (hudEditing)
        {
            HandleHudEdit(touches, newTouches);
            DrawHudEdit();
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
            else if (InRect(t.Position, MenuScaleRect))
            {
                scale3DIndex = (scale3DIndex + 1) % Scale3DSteps.Length;
                OpenGL.Scale3D = Scale3DSteps[scale3DIndex];
                OpenGL.ReapplyProjectionAspect();
                SaveLayout();
            }
            else if (InRect(t.Position, MenuHudRect))
            {
                menuOpen = false;
                hudEditing = true;
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
        DrawButton(MenuScaleRect, "Escala 3D " + Scale3DSteps[scale3DIndex].ToString("0.00", CultureInfo.InvariantCulture));
        DrawButton(MenuHudRect, "Editar HUD");
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

    // Caja del HUD en coordenadas del juego: se mueve arrastrándola y la esquina inferior la escala.
    private Vector2 HudCenter()
    {
        return new Vector2(CenterDX + 240f + hudOffX, 32f + hudOffY);
    }

    private Rectangle HudBoxRect
    {
        get
        {
            Vector2 c = HudCenter();
            float w = HUD_BOX_W * hudScale;
            float h = HUD_BOX_H * hudScale;
            return new Rectangle((int)Math.Round(c.X - w / 2f), (int)Math.Round(c.Y - h / 2f), (int)Math.Round(w), (int)Math.Round(h));
        }
    }

    private static Rectangle HudHandleRect(Rectangle box)
    {
        return new Rectangle(box.Right - 14, box.Bottom - 14, 18, 18);
    }

    private Rectangle HudEditDoneRect { get { return ShiftX(HudEditDoneBase, CenterDX); } }
    private Rectangle HudEditResetRect { get { return ShiftX(HudEditResetBase, CenterDX); } }

    // Editor de HUD: arrastrar la caja mueve el HUD; arrastrar la esquina cambia el tamaño. Se guarda al soltar.
    private void HandleHudEdit(TouchCollection touches, List<TouchLocation> newTouches)
    {
        bool save = false;
        if (hudDragId == -1)
        {
            foreach (TouchLocation t in newTouches)
            {
                if (InRect(t.Position, HudEditDoneRect))
                {
                    hudEditing = false;
                    SaveLayout();
                    return;
                }
                if (InRect(t.Position, HudEditResetRect))
                {
                    ResetHudLayout();
                    save = true;
                    break;
                }
                Rectangle box = HudBoxRect;
                if (InRect(t.Position, HudHandleRect(box)))
                {
                    hudDragId = t.Id;
                    hudDragMode = 1;
                    hudDragLast = t.Position;
                    hudDragStartScale = hudScale;
                    hudDragStartDist = Math.Max(1f, Vector2.Distance(t.Position, HudCenter()));
                    break;
                }
                if (InRect(t.Position, box))
                {
                    hudDragId = t.Id;
                    hudDragMode = 0;
                    hudDragLast = t.Position;
                    break;
                }
            }
        }

        bool found = false;
        foreach (TouchLocation t in touches)
        {
            if (hudDragId == -1 || t.Id != hudDragId)
            {
                continue;
            }
            found = true;
            if (t.State == TouchLocationState.Released || t.State == TouchLocationState.Invalid)
            {
                hudDragId = -1;
                save = true;
                continue;
            }
            if (hudDragMode == 0)
            {
                Vector2 d = t.Position - hudDragLast;
                hudOffX = Clamp(hudOffX + d.X, -HUD_MAX_OFFSET_PX, HUD_MAX_OFFSET_PX);
                hudOffY = Clamp(hudOffY + d.Y, -HUD_MAX_OFFSET_PX, HUD_MAX_OFFSET_PX);
            }
            else
            {
                float dist = Vector2.Distance(t.Position, HudCenter());
                hudScale = (float)Math.Round(Clamp(hudDragStartScale * dist / hudDragStartDist, HUD_MIN_SCALE, HUD_MAX_SCALE), 2);
            }
            hudDragLast = t.Position;
        }
        if (hudDragId != -1 && !found)
        {
            hudDragId = -1;
            save = true;
        }
        if (save)
        {
            SaveLayout();
        }
    }

    private void DrawHudEdit()
    {
        Rectangle box = HudBoxRect;
        spriteBatch.Draw(pixelTex, box, new Color(255, 255, 255, 36));
        DrawFrame(box);
        spriteBatch.Draw(pixelTex, HudHandleRect(box), new Color(191, 232, 138, 235));
        DrawButton(HudEditResetRect, "Restab.");
        DrawButton(HudEditDoneRect, "Listo");
        if (fntKootenay != null)
        {
            string info = "Arrastra el HUD  |  esquina: tamano  |  " + Math.Round(hudScale * 100f) + "%";
            spriteBatch.DrawString(fntKootenay, info, new Vector2(CenterDX + 8, 258), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        }
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

    // Las piezas del HUD se dibujan en hudTarget mientras estén entre Begin y End.
    public void BeginHudPass()
    {
        if (!HUD_LAYER_ENABLED || hudTarget == null || GraphicsDevice == null)
        {
            return;
        }
        hudPrevTargets = GraphicsDevice.GetRenderTargets();
        hudPrevViewport = GraphicsDevice.Viewport;
        GraphicsDevice.SetRenderTarget(hudTarget);
    }

    public void EndHudPass()
    {
        if (!HUD_LAYER_ENABLED || hudTarget == null || GraphicsDevice == null)
        {
            return;
        }
        if (hudPrevTargets != null && hudPrevTargets.Length > 0)
        {
            GraphicsDevice.SetRenderTargets(hudPrevTargets);
        }
        else
        {
            GraphicsDevice.SetRenderTarget(null);
        }
        GraphicsDevice.Viewport = hudPrevViewport;
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

    // Sin recorte: el escenario de los laterales (franja extra del ultrawide) se ve completo.
    public const float CROP_LEFT = 0f;
    public const float CROP_RIGHT = 0f;
    // Desplazamiento vertical del área de juego (en unidades). Con 0 se ve el alto completo, 0 a 320.
    public const float CAM_TOP_UNITS = 0f;

    // Franja extra a cada lado del área 2D (en unidades de juego). La ortográfica 2D ya la dibuja;
    // aquí se amplía el área lógica (culling/scroll) para que el nivel y el fondo llenen los laterales.
    public static float WideExtra2D { get { return (RENDER_W - GAME_W) / 2f; } }

    // Zoom del escenario en niveles: oculta los bordes que el nivel no cubre en ultrawide.
    // Solo afecta a lo que se dibuja del juego; los controles y el toque siguen en el rectángulo base.
    // Zoom solo en niveles: tapa el borde del mapa en ultrawide. El HUD se redibuja sin zoom y los controles no cambian.
    private const float STAGE_ZOOM = 1.4f;
    // Alto de la franja de HUD superior (unidades de juego) que se muestra sin zoom.
    private const float HUD_BAND_UNITS = 88f;

    // Valores del editor de HUD que usa AppMain.AmMakeOrtho2D (unidades de ortho: 2.25 por píxel de juego).
    public float HudUserScale { get { return hudScale; } }
    public float HudZoomNow { get { return (AppMain.IsStageTouchActive() && !AppMain.IsBootScreenNow() && !AppMain.IsResultScreenNow()) ? STAGE_ZOOM : 1f; } }
    public float HudOffsetUnitsX { get { return hudOffX * 2.25f * HUD_X_SIGN; } }
    public float HudOffsetUnitsY { get { return hudOffY * 2.25f * HUD_Y_SIGN; } }

    // Área base donde se dibuja el juego: escala uniforme que llena el alto y ancho RENDER_W.
    private Rectangle GetBaseRect()
    {
        Viewport vp = GraphicsDevice.Viewport;
        float scale = vp.Height / (float)GAME_H;
        int w = (int)Math.Round(RENDER_W * scale);
        int h = (int)Math.Round(GAME_H * scale);
        int x = (vp.Width - w) / 2;
        int y = (vp.Height - h) / 2;
        return new Rectangle(x, y, w, h);
    }

    // Área donde se dibuja el escenario: el base, con zoom centrado en niveles.
    public Rectangle GetGameRect()
    {
        Rectangle b = GetBaseRect();
        // Sin zoom en pantallas de arranque y carga: ahí el contenido se extiende a los laterales sin estirarse.
        float z = (AppMain.IsStageTouchActive() && !AppMain.IsBootScreenNow() && !AppMain.IsResultScreenNow()) ? STAGE_ZOOM : 1f;
        int w = (int)Math.Round(b.Width * z);
        int h = (int)Math.Round(b.Height * z);
        int x = b.X + (b.Width - w) / 2;
        int y = b.Y + (b.Height - h) / 2;
        gameRect = new Rectangle(x, y, w, h);
        return gameRect;
    }

    // Toques para menús: el contenido de 480 px está centrado en el ancho real, así que se resta el margen.
    public TouchCollection GetMenuTouches()
    {
        Rectangle r = GetBaseRect();
        float sx = RENDER_W / (float)r.Width;
        float sy = GAME_H / (float)r.Height;
        float off = CenterDX;
        List<TouchLocation> list = new List<TouchLocation>();
        foreach (TouchLocation t in TouchPanel.GetState())
        {
            Vector2 p = new Vector2((t.Position.X - r.X) * sx - off, (t.Position.Y - r.Y) * sy);
            list.Add(new TouchLocation(t.Id, t.State, p));
        }
        return new TouchCollection(list.ToArray());
    }

    // Toques en coordenadas de pantalla -> coordenadas del juego (RENDER_W x GAME_H).
    public TouchCollection GetGameTouches()
    {
        if (touchLockedUntilRelease)
        {
            if (TouchPanel.GetState().Count == 0)
            {
                touchLockedUntilRelease = false;
            }
            return new TouchCollection(new TouchLocation[0]);
        }
        Rectangle r = GetBaseRect();
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
