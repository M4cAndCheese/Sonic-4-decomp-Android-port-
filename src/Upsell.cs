// Decompiled with JetBrains decompiler
// Type: Upsell
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;

public class Upsell
{
  private const int SS_MAX = 5;
  private const int CUR_STATE_NONE = -1;
  private const int CUR_STATE_PRESSED = 0;
  private const int CUR_STATE_RELEASED = 1;
  public static Texture2D bg;
  public static Texture2D cursor1;
  public static Texture2D cursor2;
  public static Texture2D button1;
  public static Texture2D button1hl;
  public static Texture2D button2;
  public static Texture2D button2hl;
  public static Texture2D screenshot;
  public static bool showUpsell = false;
  public static int ss_num = 1;
  public static int pressed_button = -1;
  public static Rectangle[] rects = new Rectangle[5]
  {
    new Rectangle(60, 245, 90, 30),
    new Rectangle(200, 230, 272, 52),
    new Rectangle(190, 145, 37, 59),
    new Rectangle(433, 145, 37, 59),
    new Rectangle(240 /*0xF0*/, 115, 180, 110)
  };
  public static bool[] hl_buttons = new bool[2];
  private static AppMain.DMS_BUY_SCR_WORK buy_scr_work;
  public static int px;
  public static int py;
  public static int cx;
  public static int cy;
  public static int curState;
  private static bool wasUpsell = false;
  public static int anm_progress = -1;

  public static void launchUpsellScreen(AppMain.DMS_BUY_SCR_WORK buy_scr)
  {
    AppMain.DmSndBgmPlayerBgmStop();
    Upsell.ss_num = 1;
    Upsell.buy_scr_work = buy_scr;
    Upsell.loadUpsellScreen();
  }

  public static void loadUpsellScreen()
  {
    try
    {
      Upsell.bg = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\UPSELL\\s4us_bg.png"));
      Upsell.cursor1 = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\UPSELL\\s4us_arrow.png"));
      Upsell.screenshot = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\UPSELL\\s4us_ss_{(object) Upsell.ss_num}.png"));
      string str = LiveFeature.lang_suffix[AppMain.GsEnvGetLanguage()];
      Upsell.button1 = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\UPSELL\\s4{str}_back.png"));
      Upsell.button1hl = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\UPSELL\\s4{str}_back_HL.png"));
      Upsell.button2 = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\UPSELL\\s4{str}_buy.png"));
      Upsell.button2hl = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\UPSELL\\s4{str}_buy_HL.png"));
      Upsell.showUpsell = true;
    }
    catch (Exception ex)
    {
    }
  }

    public static bool inputUpsellScreen()
    {
        if (!showUpsell)
        {
            return false;
        }
        pressed_button = -1;
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        {
            if (anm_progress != -1)
            {
                anm_progress = -1;
                return true;
            }
            disposeUpsellScreen();
            if (buy_scr_work != null)
            {
                buy_scr_work.result[0] = 2;
                AppMain.DmSndBgmPlayerPlayBgm(0);
            }
            else
            {
                AppMain.SyDecideEvtCase(1);
                AppMain.SyChangeNextEvt();
            }
            return true;
        }
        TouchCollection state = TouchPanel.GetState();
        if (state.Count == 0)
        {
            if (px == 0 && py == 0)
            {
                return true;
            }
            curState = 1;
            cx = px;
            cy = py;
            px = 0;
            py = 0;
        }
        else
        {
            TouchLocation touchLocation = state[0];
            if (touchLocation.State == TouchLocationState.Pressed || touchLocation.State == TouchLocationState.Moved)
            {
                curState = 0;
                cx = (int)touchLocation.Position.X;
                cy = (int)touchLocation.Position.Y;
            }
            if (touchLocation.State == TouchLocationState.Released || touchLocation.State == TouchLocationState.Invalid)
            {
                curState = 1;
                cx = px;
                cy = py;
                px = 0;
                py = 0;
            }
        }
        hl_buttons[0] = false;
        hl_buttons[1] = false;
        if (anm_progress > 100)
        {
            anm_progress = -anm_progress;
            curState = 1;
            cx = px;
            cy = py;
            px = 0;
            py = 0;
            return true;
        }
        if (anm_progress != -1)
        {
            return true;
        }
        for (int i = 0; i < 5; i++)
        {
            if (rects[i].Contains(cx, cy))
            {
                pressed_button = i;
                break;
            }
        }
        switch ((RectTypes)pressed_button)
        {
            case RectTypes.Back:
                if (curState == 0)
                {
                    hl_buttons[0] = true;
                    break;
                }
                disposeUpsellScreen();
                if (buy_scr_work != null)
                {
                    buy_scr_work.result[0] = 2;
                    AppMain.DmSndBgmPlayerPlayBgm(0);
                }
                else
                {
                    AppMain.SyDecideEvtCase(1);
                    AppMain.SyChangeNextEvt();
                }
                break;
            case RectTypes.CurLeft:
                if (curState == 1)
                {
                    ss_num--;
                    if (ss_num < 1)
                    {
                        ss_num = 5;
                    }
                    screenshot.Dispose();
                    screenshot = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\UPSELL\\s4us_ss_" + ss_num + ".png"));
                }
                break;
            case RectTypes.CurRight:
                if (curState == 1)
                {
                    ss_num++;
                    if (ss_num >= 5)
                    {
                        ss_num = 1;
                    }
                    screenshot.Dispose();
                    screenshot = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\UPSELL\\s4us_ss_" + ss_num + ".png"));
                }
                break;
            case RectTypes.Purchase:
                if (curState == 0)
                {
                    hl_buttons[1] = true;
                    break;
                }
                wasUpsell = true;
                XBOXLive.showGuide();
                break;
            case RectTypes.SS:
                if (curState == 0)
                {
                    anm_progress = 0;
                }
                break;
        }
        if (curState == 0)
        {
            px = cx;
            py = cy;
        }
        else
        {
            curState = -1;
        }
        return true;
    }

    public static void updateUpsellScreen()
  {
    if (Upsell.anm_progress != -1)
    {
      Upsell.anm_progress += 25;
      if (Upsell.anm_progress > (int) byte.MaxValue)
        Upsell.anm_progress = (int) byte.MaxValue;
      if (Upsell.anm_progress < -1)
      {
        Upsell.anm_progress += 25;
        if (Upsell.anm_progress > -50)
          Upsell.anm_progress = -1;
      }
    }
    if (!Upsell.wasUpsell || Guide.IsVisible || XBOXLive.isTrial(true))
      return;
    Upsell.disposeUpsellScreen();
    if (Upsell.buy_scr_work != null)
    {
      Upsell.buy_scr_work.result[0] = 0;
      AppMain.DmSndBgmPlayerPlayBgm(0);
    }
    else
    {
      AppMain.event_after_buy = true;
      AppMain.SyDecideEvtCase((short) 1);
      AppMain.SyChangeNextEvt();
    }
  }

  public static void drawUpsellScreen()
  {
    if (!Upsell.showUpsell)
      return;
    SpriteBatch spriteBatch = LiveFeature.GAME.spriteBatch;
    spriteBatch.Begin((SpriteSortMode) 1, BlendState.AlphaBlend);
    spriteBatch.Draw(Upsell.bg, Vector2.Zero, Color.White);
    spriteBatch.Draw(Upsell.hl_buttons[1] ? Upsell.button2hl : Upsell.button2, Upsell.rects[1], Color.White);
    spriteBatch.Draw(Upsell.hl_buttons[0] ? Upsell.button1hl : Upsell.button1, Upsell.rects[0], Color.White);
    spriteBatch.Draw(Upsell.cursor1, Upsell.rects[2], Color.White);
    spriteBatch.Draw(Upsell.cursor1, Upsell.rects[3], new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 1, 0.0f);
    spriteBatch.Draw(Upsell.screenshot, Upsell.rects[4], Color.White);
    int y1 = 5;
    int y2 = y1 + LiveFeature._drawWrapText("Download full Game!", 335, y1, 300, Color.White, true, LiveFeature.GAME.fnts[1], spriteBatch, 0.8f);
    int y3 = y2 + LiveFeature._drawWrapText("- 17 diverse stages", 195, y2, 300, Color.White, false, LiveFeature.GAME.fnts[0], spriteBatch, 0.8f);
    int y4 = y3 + LiveFeature._drawWrapText("- Two exclusive stages!", 195, y3, 300, Color.White, false, LiveFeature.GAME.fnts[0], spriteBatch, 0.8f);
    int y5 = y4 + LiveFeature._drawWrapText("- 7 special stages with tilt control!", 195, y4, 300, Color.White, false, LiveFeature.GAME.fnts[0], spriteBatch, 0.8f);
    int num1 = y5 + LiveFeature._drawWrapText("- Collect all 7 Chaos Emeralds to Unlock Super Sonic!", 195, y5, 300, Color.White, false, LiveFeature.GAME.fnts[0], spriteBatch, 0.8f);
    if (Upsell.anm_progress != -1)
    {
      int num2 = Math.Abs(Upsell.anm_progress);
      spriteBatch.Draw(Upsell.screenshot, Vector2.Zero, new Color(num2, num2, num2, num2));
    }
    LiveFeature.GAME.spriteBatch.End();
    Upsell.updateUpsellScreen();
  }

  public static void disposeUpsellScreen()
  {
    try
    {
      Upsell.showUpsell = false;
      ((GraphicsResource) Upsell.bg).Dispose();
      Upsell.bg = (Texture2D) null;
      ((GraphicsResource) Upsell.cursor1).Dispose();
      Upsell.cursor1 = (Texture2D) null;
      ((GraphicsResource) Upsell.screenshot).Dispose();
      Upsell.screenshot = (Texture2D) null;
      ((GraphicsResource) Upsell.button1).Dispose();
      Upsell.button1 = (Texture2D) null;
      ((GraphicsResource) Upsell.button1hl).Dispose();
      Upsell.button1hl = (Texture2D) null;
      ((GraphicsResource) Upsell.button2).Dispose();
      Upsell.button2 = (Texture2D) null;
      ((GraphicsResource) Upsell.button2hl).Dispose();
      Upsell.button2hl = (Texture2D) null;
    }
    catch (Exception ex)
    {
    }
  }

  private enum RectTypes
  {
    None = -1, // 0xFFFFFFFF
    Back = 0,
    Purchase = 1,
    CurLeft = 2,
    CurRight = 3,
    SS = 4,
    MAX = 5,
  }

  private enum HLTypes
  {
    Back,
    Purchase,
    MAX,
  }
}
