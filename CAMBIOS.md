# Cambios de esta versión

## Pantalla achicada
- `Sonic4Ep1.GAME_H` pasa de 320 a 288. El motor dibuja con `m_area.bottom = 288` y el init de 2D fijaba 320,
  lo que dejaba una franja vacía abajo y hacía ver el juego más chico.
- `AppMain` (init de 2D/3D): alto 2D y aspecto de proyección usan `GAME_H` (288). El ancho sigue `RENDER_W`,
  así que el ultrawide no estira la escena.

## Fases especiales: acelerómetro real, sin controles táctiles
- `src/Platform/TiltSensor.cs`: lee el acelerómetro de Android (antes el shim no entregaba datos).
  Si el giro sale invertido, cambiar `TiltSensor.SteerSign` a `-1f`.
- `AppMain.SPSTAGE_FORCE_TILT = true`: en fases especiales siempre se usa la inclinación, aunque la opción
  guardada sea control táctil.
- No se dibuja el pad ni el engranaje en fases especiales. Los toques siguen activos en las zonas del motor
  (salto abajo a la derecha) pero no se ven. El botón de pausa se mantiene.

## Texturas HD (iPad)
- `src/Platform/HdTextures.cs`: decodificador DXT1/DXT5. La clave es el SHA-1 de los bytes del PNG original
  dentro del AMB (desde la cabecera PNG hasta IEND). Si existe `Content/HD/<SHA1>.DDS`, se usa en lugar de la PNG.
- `AppMain.amTextureLoad`: consulta `HdTextures.TryLoadPng(amb.data, amb.offsets[i])` antes de decodificar la PNG original.
- `Content/HD/`: 23 texturas emparejadas por AMB de origen y proporción verificada (2x o 4x):
  enemigos, cuerpos de jefes de zona 1/3/4, items, cuerdas, logo de título y flechas de selección de fase.
  Los nombres repetidos entre zonas (como EG_BODY, BOSS_01) ya no son ambiguos porque la clave es el contenido.

### Excluidas (layout distinto, no versión ampliada)
- BOSS_01 (zona 1), BOSS_04 (zona 4), EG_BODY/EG_HEAD/EG_WRIST (zona F): el DDS HD tiene proporción 2x1 o más
  y contenido distinto al PNG original.
- MOGURIN_CATERPILLAR: relación 2x1; posible equivalencia, pendiente de probar en el juego.

### Pendiente
- Sonic (SON_TEX, SSON_TEX) y el terreno de las zonas no tienen correspondencia en el paquete HD:
  el atlas del port (SONIC_DEF.PNG, 256x256) tiene otro layout que los DDS iPad.

## Correcciones de audio y render
- Música tras el 1UP (100 anillos): al tocar el jingle, la música se pausa y solo se reanuda cuando el
  jingle avisa que terminó, lo que en Android no ocurría. Ahora hay un respaldo por tiempo
  (`GMD_SOUND_1UP_JINGLE_RESUME_FRAME = 300`) que reanuda la música aunque el jingle no avise.
- "Diente de sierra": con alto 288 la escala al celular suele ser fraccionaria, y el muestreo de punto
  dejaba bordes escalonados. Si la escala no es entera, ahora se usa muestreo lineal.

## Diseño por defecto del pad (builds nuevos)
- `Sonic4Ep1.cs`: el d-pad y el salto usan el diseño medido de la captura (alto GAME_H = 320):
  d-pad en (68, 244) con diámetro 74; salto a 58 px del borde derecho, en y = 244, diámetro 74.
  Antes el salto tenía diámetro 62 y ancla 68 px.
- Un `pad_layout.txt` guardado en el dispositivo sigue teniendo prioridad sobre estos valores.

## Escenario aplastado en ultrawide
- `AppMain.amIPhoneInitNN`: el ancho lógico (`width_2d`, `draw_width`, `disp_width`) usa `Sonic4Ep1.RENDER_W`
  en lugar de 480. Antes la cámara y el 2D se calculaban con 480 y la escena se veía aplastada.
- Se quitó `Content/HD/` (texturas HD del iPad). El código de `HdTextures` queda inactivo porque no encuentra archivos.
