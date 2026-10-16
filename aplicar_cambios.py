# Aplica los cambios de capas (HUD / UI / zoom 1.0) a un repo de Sonic 4.
# Uso (desde la raíz del repo):  python aplicar_cambios.py
# Tolera finales de línea CRLF (Windows) y LF. No borra nada que no reemplace.
import os, sys

def leer(path):
    with open(path, 'rb') as f:
        raw = f.read()
    crlf = b'\r\n' in raw
    text = raw.decode('utf-8').replace('\r\n', '\n')
    return text, crlf

def escribir(path, text, crlf):
    if crlf:
        text = text.replace('\n', '\r\n')
    with open(path, 'wb') as f:
        f.write(text.encode('utf-8'))

fallos = []

def reemplazar(text, old, new, etiqueta):
    n = text.count(old)
    if n != 1:
        fallos.append('%s: se esperaba 1 coincidencia, hay %d' % (etiqueta, n))
        return text
    return text.replace(old, new)

def cambios_sonic4ep1(s):
    s = reemplazar(s, "    private RenderTarget2D hudTarget;\n",
        "    private RenderTarget2D hudTarget;\n    // Capa de UI (pausa, resultados, menús): escala propia (UI_ZOOM), separada del HUD.\n    private RenderTarget2D uiTarget;\n", 'campo uiTarget')
    s = reemplazar(s, "        hudTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.None);\n        AppMain.pauseKeyX",
        "        hudTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.None);\n        uiTarget = new RenderTarget2D(base.GraphicsDevice, RENDER_W * RENDER_SCALE, GAME_H * RENDER_SCALE, false, SurfaceFormat.Color, DepthFormat.None);\n        AppMain.pauseKeyX", 'creación uiTarget')
    s = reemplazar(s, """        if (hudTarget != null)
        {
            GraphicsDevice.SetRenderTarget(hudTarget);
            GraphicsDevice.Clear(Color.Transparent);
        }
""", """        if (hudTarget != null)
        {
            GraphicsDevice.SetRenderTarget(hudTarget);
            GraphicsDevice.Clear(Color.Transparent);
        }
        if (uiTarget != null)
        {
            GraphicsDevice.SetRenderTarget(uiTarget);
            GraphicsDevice.Clear(Color.Transparent);
        }
""", 'limpieza uiTarget')
    s = reemplazar(s, """                spriteBatch.Draw(hudTarget, HudDrawRect(), Color.White);
                spriteBatch.End();
            }
""", """                spriteBatch.Draw(hudTarget, HudDrawRect(), Color.White);
                spriteBatch.End();
            }
            // Pausa, resultados y menús: capa propia, escalada con UI_ZOOM (no depende del HUD ni del nivel).
            if (!boot && HUD_LAYER_ENABLED && uiTarget != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, gameSampler, null, null);
                spriteBatch.Draw(uiTarget, UiDrawRect(), Color.White);
                spriteBatch.End();
            }
""", 'composición UI')
    s = reemplazar(s, "    public const float HUD_ZOOM = 0.75f;\n",
        "    public const float HUD_ZOOM = 1f;\n    // Zoom propio de pausa, resultados y menús (1 = tamaño base). Bájalo si no caben con el zoom del nivel.\n    public const float UI_ZOOM = 0.75f;\n", 'constantes de zoom')
    s = reemplazar(s, """    private Rectangle HudDrawRect()
    {
        Rectangle b = GetBaseRect();
        if (HUD_ZOOM == 1f)
        {
            return b;
        }
        int w = (int)Math.Round(b.Width * HUD_ZOOM);
        int h = (int)Math.Round(b.Height * HUD_ZOOM);
        return new Rectangle(b.X + (b.Width - w) / 2, b.Y, w, h);
    }
""", """    private Rectangle HudDrawRect()
    {
        return LayerRect(HUD_ZOOM);
    }

    // Pausa, resultados y menús: su propio zoom (UI_ZOOM), anclado arriba y centrado.
    private Rectangle UiDrawRect()
    {
        return LayerRect(UI_ZOOM);
    }

    private Rectangle LayerRect(float zoom)
    {
        Rectangle b = GetBaseRect();
        if (zoom == 1f)
        {
            return b;
        }
        int w = (int)Math.Round(b.Width * zoom);
        int h = (int)Math.Round(b.Height * zoom);
        return new Rectangle(b.X + (b.Width - w) / 2, b.Y, w, h);
    }
""", 'rectángulos por capa')
    s = reemplazar(s, """    public void BeginHudPass()
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
        }""", """    // ui = true: las piezas van a la capa de UI (pausa, resultados); false: a la capa del HUD.
    public void BeginHudPass(bool ui = false)
    {
        RenderTarget2D target = ui ? uiTarget : hudTarget;
        if (!HUD_LAYER_ENABLED || target == null || GraphicsDevice == null)
        {
            return;
        }
        hudPrevTargets = GraphicsDevice.GetRenderTargets();
        hudPrevViewport = GraphicsDevice.Viewport;
        GraphicsDevice.SetRenderTarget(target);
    }

    public void EndHudPass()
    {
        if (!HUD_LAYER_ENABLED || (hudTarget == null && uiTarget == null) || GraphicsDevice == null)
        {
            return;
        }""", 'pases de capa')
    s = reemplazar(s, "    private const float STAGE_ZOOM = 1.4f;",
        "    private const float STAGE_ZOOM = 1.0f;", 'STAGE_ZOOM')
    return s

def cambios_apmain(s):
    return reemplazar(s, "\t\tinst.BeginHudPass();\n",
        "\t\t// Pausa y resultados (uiLayer sin hud) van a su capa propia; el HUD a la suya.\n\t\tinst.BeginHudPass(!command.hud);\n", 'BeginHudPass en AppMain')

def cambios_cambios_md(s):
    return s + """

## Capas separadas: HUD, pausa/resultados y zoom del nivel
- `Sonic4Ep1.STAGE_ZOOM` pasa de 1.4 a 1.0: el nivel se ve sin acercar, como en la captura.
- `Sonic4Ep1.HUD_ZOOM` pasa de 0.75 a 1.0: el HUD se ve a tamaño de la captura.
- Nuevo `Sonic4Ep1.UI_ZOOM` (0.75): escala propia de la pausa, la pantalla de resultados y los menús.
  Están en su propia textura (`uiTarget`). Si no caben bien con el zoom del nivel, bajar `UI_ZOOM`.
- `AppMain._amDrawInLayer`: `BeginHudPass(!command.hud)` manda pausa/resultados a `uiTarget` y el HUD a `hudTarget`.
"""

def aplicar(ruta, fn):
    if not os.path.exists(ruta):
        fallos.append('%s: no existe (¿estás en la raíz del repo?)' % ruta)
        return
    texto, crlf = leer(ruta)
    nuevo = fn(texto)
    if nuevo == texto:
        print('sin cambios: %s' % ruta)
        return
    escribir(ruta, nuevo, crlf)
    print('actualizado: %s' % ruta)

aplicar(os.path.join('src', 'Sonic4Ep1.cs'), cambios_sonic4ep1)
aplicar(os.path.join('src', 'AppMain.cs'), cambios_apmain)
aplicar('CAMBIOS.md', cambios_cambios_md)

if fallos:
    print('\nNo se pudo aplicar algo. Copia este mensaje completo:')
    for f in fallos:
        print(' - ' + f)
    sys.exit(1)
print('\nListo. Ahora: git add -A && git commit -m "Capas separadas: HUD, UI y zoom 1.0" && git push')
