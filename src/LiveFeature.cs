// Decompiled with JetBrains decompiler
// Type: LiveFeature
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sonic4ep1;
using System;
using System.Collections.Generic;
using System.Text;

public class LiveFeature : XBOXLive
{
  private const int screenw = 480;
  private const int screenh = 288;
  private const int title_y = 22;
  private const int title_x = 15;
  private const int center_x = 240 /*0xF0*/;
  private const int pad_x1 = 10;
  private const int pad_w = 230;
  private const int pad_h = 170;
  private const int pad_h2 = 220;
  private const int pad_x2 = 240 /*0xF0*/;
  private const int pad_y = 55;
  private const int pad_y2 = 15;
  private const int title_y1 = 27;
  private const int title_y2 = 60;
  private const int clip_x = 15;
  private const int clip_y = 80 /*0x50*/;
  private const int clip_w = 450;
  private const int clip_h = 140;
  private const int title_y3 = 47;
  private const int title_y4 = 67;
  private const int title_y5 = 87;
  private const int arrow_left_x = 165;
  private const int arrow_right_x = 285;
  private const int arrow_y = 240 /*0xF0*/;
  private const int left_x = 30;
  private const int list_y = 80 /*0x50*/;
  private const int bottom_y = 220;
  private const int draw_width = 420;
  private const int icon_paddle = 40;
  private static LiveFeature instance;
  private SignedInGamer gamer;
  private GamerProfile profile;
  private static readonly string[] zone_names = new string[6]
  {
    Strings.ID_STAGE1,
    Strings.ID_STAGE2,
    Strings.ID_STAGE3,
    Strings.ID_STAGE4,
    Strings.ID_FINALZONE,
    Strings.ID_SSTAGE
  };
  private LiveFeature.HISCORE_ENTRY[][] hiScoresTables = new LiveFeature.HISCORE_ENTRY[48 /*0x30*/][];
  public static Sonic4Ep1 GAME;
  private LiveFeature.LBStatus[] l_status = new LiveFeature.LBStatus[48 /*0x30*/];
  public int startedRead = -1;
  public string readErrorMSG;
  public static readonly string[] lang_suffix = new string[11]
  {
    "ja",
    "us",
    "fr",
    "it",
    "ge",
    "es",
    "fi",
    "pt",
    "ru",
    "cn",
    "hk"
  };
  public static int interruptMainLoop = 0;
  public static int achievements_total;
  public static int achievements_current;
  public static Texture2D[] a_images;
  public static Texture2D a_bg;
  private int pX;
  private int pY;
  private int cX;
  private int cY;
  private int deltaX;
  private int deltaY;
  private int offsetY;
  private int curStage;
  private static Texture2D arrowImg;
  private static Texture2D arrowImg2;
  private static Texture2D titleImg;
  private static Texture2D nums;
  private static LiveFeature.AchievementText[] achievementTextArray;
  public static readonly int[,] table_x = new int[4, 2]
  {
    {
      60,
      70
    },
    {
      145,
      160 /*0xA0*/
    },
    {
      260,
      275
    },
    {
      385,
      395
    }
  };
  public static int arrow_offset = 0;
  public static readonly Rectangle arrow1_Left = new Rectangle(165, 240 /*0xF0*/, 40, 40);
  public static readonly Rectangle arrow1_Right = new Rectangle(285, 240 /*0xF0*/, 40, 40);
  public static readonly Rectangle arrow2_Up = new Rectangle(430, 60, 35, 35);
  public static readonly Rectangle arrow2_Down = new Rectangle(430, 180, 35, 35);
  public static Rectangle num_src_rect = new Rectangle(0, 0, 16 /*0x10*/, 32 /*0x20*/);
  public static Rectangle num_dst_rect = new Rectangle(0, 240 /*0xF0*/, 16 /*0x10*/, 32 /*0x20*/);
  public static readonly Color transparent_achiev = new Color(128 /*0x80*/, 128 /*0x80*/, 128 /*0x80*/, 128 /*0x80*/);

  public LiveFeature()
  {
    try
    {
      XBOXLive.signinStatus = XBOXLive.SigninStatus.SigningIn;
      XBOXLive.isTrial(true);
      for (int index = 0; index < 48 /*0x30*/; ++index)
      {
        this.hiScoresTables[index] = new LiveFeature.HISCORE_ENTRY[0];
        this.l_status[index] = LiveFeature.LBStatus.NotRead;
      }
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
      XBOXLive.signinStatus = XBOXLive.SigninStatus.Error;
    }
    LiveFeature.instance = this;
    XBOXLive.instanceXBOX = (XBOXLive) this;
  }

    private void GamerSignedInCallback(object sender, SignedInEventArgs args)
    {
        XBOXLive.isTrial(true);
        try
        {
            if (XBOXLive.signinStatus == SigninStatus.UpdateNeeded)
            {
                return;
            }
            SignedInGamer signedInGamer = args.Gamer;
            if (signedInGamer == null)
            {
                return;
            }
            gamer = signedInGamer;
            if (!Guide.IsTrialMode)
            {
                gamer.BeginGetAchievements(GetAchievementsCallback, gamer);
            }
            if (gamer.IsSignedInToLive)
            {
                XBOXLive.signinStatus = SigninStatus.LIVE;
                GamerPrivilegeSetting allowProfileViewing = gamer.Privileges.AllowProfileViewing;
                if (allowProfileViewing == GamerPrivilegeSetting.Everyone || allowProfileViewing == GamerPrivilegeSetting.FriendsOnly)
                {
                    gamer.BeginGetProfile(GetProfileCallback, gamer);
                }
            }
            else
            {
                XBOXLive.signinStatus = SigninStatus.Local;
            }
        }
        catch (GameUpdateRequiredException e)
        {
            XBOXLive.HandleGameUpdateRequired(e);
        }
        catch (Exception)
        {
            XBOXLive.signinStatus = SigninStatus.Error;
        }
    }

    private void GetProfileCallback(IAsyncResult result)
  {
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded || !(result.AsyncState is SignedInGamer asyncState))
        return;
      this.profile = ((Gamer) asyncState).EndGetProfile(result);
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
    }
  }

  public LiveFeature.LBStatus getLeaderBoardStatus(int mode) => this.l_status[mode];

  public void startReadingLeaderBoard(int mode)
  {
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.Local)
        return;
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded)
      {
        this.l_status[mode] = LiveFeature.LBStatus.ReadFail;
      }
      else
      {
        this.readErrorMSG = (string) null;
        SignedInGamer signedInGamer = Gamer.SignedInGamers[(PlayerIndex) 0];
        if (signedInGamer != null)
        {
          LeaderboardReader.BeginRead(mode <= 23 ? LeaderboardIdentity.Create((LeaderboardKey) 0, mode) : LeaderboardIdentity.Create((LeaderboardKey) 2, mode), (Gamer) signedInGamer, 100, new AsyncCallback(this.LeaderboardReadCallback), (object) signedInGamer);
          this.l_status[mode] = LiveFeature.LBStatus.StartedRead;
        }
        else
          this.l_status[mode] = LiveFeature.LBStatus.ReadFail;
        this.startedRead = mode;
      }
    }
    catch (GameUpdateRequiredException ex)
    {
      this.l_status[mode] = LiveFeature.LBStatus.ReadFail;
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
      this.l_status[mode] = LiveFeature.LBStatus.ReadFail;
    }
  }

  private void LeaderboardReadCallback(IAsyncResult result)
  {
    int index1 = -1;
    if (this.startedRead != -1)
      index1 = this.startedRead;
    try
    {
      SignedInGamer asyncState = result.AsyncState as SignedInGamer;
      LeaderboardReader leaderboardReader = LeaderboardReader.EndRead(result);
      LeaderboardIdentity leaderboardIdentity = leaderboardReader.LeaderboardIdentity;
      index1 = ((LeaderboardIdentity) leaderboardIdentity).GameMode;
      this.hiScoresTables[index1] = new LiveFeature.HISCORE_ENTRY[leaderboardReader.Entries.Count];
      for (int index2 = 0; index2 < leaderboardReader.Entries.Count; ++index2)
      {
        LeaderboardEntry entry = leaderboardReader.Entries[index2];
        bool flag = entry.Gamer.Gamertag == ((Gamer) asyncState).Gamertag;
        this.hiScoresTables[index1][index2].index = index2 + 1;
        this.hiScoresTables[index1][index2].isMe = flag;
        this.hiScoresTables[index1][index2].name = entry.Gamer.Gamertag;
        this.hiScoresTables[index1][index2].value = index1 <= 23 ? entry.Columns.GetValueInt32("BestScore") : entry.Columns.GetValueInt32("BestTime");
        this.hiScoresTables[index1][index2].date = entry.Columns.GetValueDateTime("LastPlayedDateTime").ToLocalTime();
      }
      this.l_status[index1] = LiveFeature.LBStatus.ReadSuccess;
    }
    catch (GameUpdateRequiredException ex)
    {
      if (index1 == -1)
        index1 = 0;
      this.l_status[index1] = LiveFeature.LBStatus.ReadFail;
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
      if (index1 == -1)
        index1 = 0;
      this.l_status[index1] = LiveFeature.LBStatus.ReadFail;
      this.readErrorMSG = ex.Message;
    }
    this.startedRead = -1;
  }

  public bool saveLeaderBoardScore(int index, int value)
  {
    if (XBOXLive.isTrial())
      return false;
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded)
        return false;
      SignedInGamer signedInGamer = Gamer.SignedInGamers[(PlayerIndex) 0];
      if (signedInGamer != null)
      {
        if (index > 23)
        {
          LeaderboardIdentity leaderboardIdentity = LeaderboardIdentity.Create((LeaderboardKey) 2, index);
          LeaderboardEntry leaderboard = ((Gamer) signedInGamer).LeaderboardWriter.GetLeaderboard(leaderboardIdentity);
          leaderboard.Rating = (long) value;
          leaderboard.Columns.SetValue("Time", value);
          leaderboard.Columns.SetValue("TimeStamp", DateTime.Now);
        }
        else
        {
          LeaderboardIdentity leaderboardIdentity = LeaderboardIdentity.Create((LeaderboardKey) 0, index);
          LeaderboardEntry leaderboard = ((Gamer) signedInGamer).LeaderboardWriter.GetLeaderboard(leaderboardIdentity);
          leaderboard.Rating = (long) value;
          leaderboard.Columns.SetValue("Score", value);
          leaderboard.Columns.SetValue("TimeStamp", DateTime.Now);
        }
      }
      return true;
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
      return false;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public static LiveFeature getInstance()
  {
    if (LiveFeature.instance == null)
      LiveFeature.instance = new LiveFeature();
    return LiveFeature.instance;
  }

  private void GetAchievementsCallback(IAsyncResult result)
  {
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded || !(result.AsyncState is SignedInGamer asyncState))
        return;
      foreach (Achievement achievement in asyncState.EndGetAchievements(result))
      {
        string key = achievement.Key;
        for (int index = 0; index < AppMain.achievements.Length; ++index)
        {
          if (AppMain.achievements[index].id == key)
            AppMain.gs_trophy_acquisition_tbl[index] = !achievement.IsEarned ? 0 : 2;
        }
      }
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
    }
  }

  public bool GotAchievment(string achievementKey)
  {
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded || this.gamer == null || XBOXLive.isTrial())
        return false;
      this.gamer.BeginAwardAchievement(achievementKey, new AsyncCallback(this.AwardAchievementCallback), (object) this.gamer);
      return true;
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
    }
    return false;
  }

  private void AwardAchievementCallback(IAsyncResult result)
  {
    try
    {
      if (XBOXLive.signinStatus == XBOXLive.SigninStatus.UpdateNeeded || !(result.AsyncState is SignedInGamer asyncState))
        return;
      asyncState.EndAwardAchievement(result);
      this.gamer.BeginGetAchievements(new AsyncCallback(this.GetAchievementsCallback), (object) this.gamer);
    }
    catch (GameUpdateRequiredException ex)
    {
      XBOXLive.HandleGameUpdateRequired(ex);
    }
    catch (Exception ex)
    {
    }
  }

  public void clearHiScores()
  {
    for (int index = 0; index < 48 /*0x30*/; ++index)
    {
      this.l_status[index] = LiveFeature.LBStatus.NotRead;
      this.hiScoresTables[index] = (LiveFeature.HISCORE_ENTRY[]) null;
    }
  }

  public override void _initTextDialog(
    out string dlgYes,
    out string dlgNo,
    out string dlgCaption,
    out string dlgText)
  {
    dlgYes = Strings.ID_YES;
    dlgNo = Strings.ID_NO;
    dlgCaption = Strings.ID_UPDATE_CAPTION;
    dlgText = Strings.ID_UPDATE_TEXT;
  }

  internal void ShowAchievements()
  {
    LiveFeature.interruptMainLoop = 1;
    this.offsetY = 0;
    int length = AppMain.achievements.Length;
    LiveFeature.achievements_total = 0;
    LiveFeature.achievements_current = 0;
    if (LiveFeature.a_images == null)
      LiveFeature.a_images = new Texture2D[AppMain.achievements.Length];
    LiveFeature.achievementTextArray = new LiveFeature.AchievementText[length];
    for (int index = 0; index < length; ++index)
    {
      LiveFeature.achievements_total += AppMain.achievements[index].cost;
      if (AppMain.gs_trophy_acquisition_tbl[index] == 2)
        LiveFeature.achievements_current += AppMain.achievements[index].cost;
      LiveFeature.a_images[index] = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\LIVE\\live_a{(object) (index + 1)}.png"));
      LiveFeature.achievementTextArray[index].verticalSize = -1;
    }
    if (LiveFeature.a_bg == null)
      LiveFeature.a_bg = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\LIVE\\tab.png"));
    if (LiveFeature.arrowImg2 == null)
      LiveFeature.arrowImg2 = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\LIVE\\arrow2.png"));
    if (LiveFeature.titleImg != null)
      return;
    LiveFeature.titleImg = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream($"Content\\LIVE\\ach_top_{LiveFeature.lang_suffix[AppMain.GsEnvGetLanguage()]}.png"));
  }

  internal void ShowLeaderboards()
  {
    LiveFeature.interruptMainLoop = 2;
    this.offsetY = 0;
    this.curStage = 24;
    this.clearHiScores();
    if (LiveFeature.arrowImg == null)
      LiveFeature.arrowImg = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\LIVE\\arrow.png"));
    if (LiveFeature.a_bg == null)
      LiveFeature.a_bg = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\LIVE\\tab.png"));
    if (LiveFeature.nums == null)
      LiveFeature.nums = Texture2D.FromStream(LiveFeature.GAME.GraphicsDevice, TitleContainer.OpenStream("Content\\LIVE\\nums.png"));
    this.startReadingLeaderBoard(this.curStage);
  }

    public bool InputOverride()
    {
        if (interruptMainLoop == 0)
        {
            return false;
        }
        MouseState state = Mouse.GetState();
        cX = state.X;
        cY = state.Y;
        if (((state.LeftButton == ButtonState.Pressed && pX != 0 && pY != 0) || state.LeftButton == ButtonState.Released) && cY > 240 && cX > 350)
        {
            return false;
        }
        if (interruptMainLoop == 2)
        {
            if (AppMain.GsTrialIsTrial() || XBOXLive.signinStatus == SigninStatus.Local)
            {
                return false;
            }
            if (state.LeftButton == ButtonState.Pressed)
            {
                pX = cX;
                pY = cY;
            }
            if (state.LeftButton == ButtonState.Released && pX != 0 && pY != 0)
            {
                bool flag = false;
                if (arrow1_Left.Contains(cX, cY))
                {
                    curStage--;
                    if (curStage < 24)
                    {
                        curStage = 47;
                    }
                    flag = true;
                }
                else if (arrow1_Right.Contains(cX, cY))
                {
                    curStage++;
                    if (curStage > 47)
                    {
                        curStage = 24;
                    }
                    flag = true;
                }
                cX = (pX = (pY = (cY = 0)));
                if (flag && l_status[curStage] != LBStatus.ReadSuccess && l_status[curStage] != LBStatus.ReadFail)
                {
                    startReadingLeaderBoard(curStage);
                }
            }
        }
        else
        {
            if (state.LeftButton == ButtonState.Released)
            {
                pX = (pY = (cY = (cX = 0)));
                deltaX = 0;
                deltaY = 0;
                return true;
            }
            if (state.LeftButton == ButtonState.Pressed)
            {
                if (pX == 0 && pY == 0)
                {
                    pX = cX;
                    pY = cY;
                    if (arrow2_Up.Contains(cX, cY))
                    {
                        offsetY += 5;
                        if (offsetY > 0)
                        {
                            offsetY = 0;
                        }
                        pX = 0;
                        pY = 0;
                    }
                    if (arrow2_Down.Contains(cX, cY))
                    {
                        offsetY -= 5;
                        pX = 0;
                        pY = 0;
                    }
                }
                else
                {
                    deltaX = pX - cX;
                    deltaY = pY - cY;
                    offsetY -= deltaY;
                    if (offsetY > 0)
                    {
                        offsetY = 0;
                    }
                    pX = cX;
                    pY = cY;
                }
            }
        }
        return true;
    }
    private void _drawAchievements(SpriteBatch spriteBatch, SpriteFont[] fonts)
    {
        int num = 0;
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        try
        {
            if (AppMain.dm_xbox_show_progress != 100)
            {
                spriteBatch.Draw(a_bg, new Rectangle(10, 55, 230, (int)(170f * ((float)AppMain.dm_xbox_show_progress / 100f))), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                spriteBatch.Draw(a_bg, new Rectangle(240, 55, 230, (int)(170f * ((float)AppMain.dm_xbox_show_progress / 100f))), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }
            else
            {
                spriteBatch.Draw(a_bg, new Rectangle(10, 55, 230, 170), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                spriteBatch.Draw(a_bg, new Rectangle(240, 55, 230, 170), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }
            spriteBatch.Draw(titleImg, new Rectangle(15, 22, 128, 16), Color.White);
            if (AppMain.dm_xbox_show_progress == 100)
            {
                string text = achievements_current + "/" + achievements_total + " G";
                Vector2 vector = fonts[1].MeasureString(text);
                spriteBatch.DrawString(fonts[0], text, new Vector2(240 - ((int)vector.X >> 1), 60f), Color.White);
            }
        }
        finally
        {
            spriteBatch.End();
        }
        if (AppMain.dm_xbox_show_progress == 100)
        {
            GAME.GraphicsDevice.ScissorRectangle = new Rectangle(15, 80, 450, 140);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, GAME.scissorState);
            try
            {
                num = offsetY + 80;
                Rectangle destinationRectangle = new Rectangle(30, num, 35, 35);
                for (int i = 0; i < AppMain.achievements.Length; i++)
                {
                    bool flag = AppMain.gs_trophy_acquisition_tbl[i] == 2;
                    spriteBatch.Draw(a_images[i], destinationRectangle, flag ? Color.White : transparent_achiev);
                    if (!flag)
                    {
                        Color gray = Color.Gray;
                    }
                    else
                    {
                        Color white = Color.White;
                    }
                    string text = AppMain.achievements[i].name + " (" + AppMain.achievements[i].cost + " G)";
                    Vector2 vector = fonts[1].MeasureString(text);
                    if (num > 0 && num < 220)
                    {
                        spriteBatch.DrawString(fonts[1], text, new Vector2(70f, num), Color.White);
                    }
                    num += (int)vector.Y;
                    if ((num > 0 && num < 220) || achievementTextArray[i].verticalSize == -1)
                    {
                        if (achievementTextArray[i].text == null)
                        {
                            achievementTextArray[i].text = _wrapString(AppMain.achievements[i].description, 380, fonts[0]);
                        }
                        achievementTextArray[i].verticalSize = _drawWrapText(achievementTextArray[i].text, 70, num, 380, Color.White, false, fonts[0], spriteBatch);
                    }
                    num = (destinationRectangle.Y = num + achievementTextArray[i].verticalSize);
                }
                if (num < 220)
                {
                    offsetY -= num - 220;
                }
            }
            finally
            {
                spriteBatch.End();
                GAME.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, 480, 288);
            }
        }
        if (AppMain.dm_xbox_show_progress != 100)
        {
            return;
        }
        arrow_offset += 2;
        if (arrow_offset > 16)
        {
            arrow_offset = -16;
        }
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        try
        {
            if (offsetY != 0)
            {
                Rectangle destinationRectangle2 = arrow2_Up;
                destinationRectangle2.Y -= Math.Abs(arrow_offset);
                spriteBatch.Draw(arrowImg2, destinationRectangle2, Color.White);
            }
            if (num > 220)
            {
                Rectangle destinationRectangle2 = arrow2_Down;
                destinationRectangle2.Y += Math.Abs(arrow_offset);
                spriteBatch.Draw(arrowImg2, destinationRectangle2, null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
            }
        }
        finally
        {
            spriteBatch.End();
        }
    }

    private void _drawLeaderboards(SpriteBatch spriteBatch, SpriteFont[] fonts)
  {
    spriteBatch.Begin((SpriteSortMode) 1, BlendState.AlphaBlend);
    try
    {
      if (AppMain.dm_xbox_show_progress != 100)
      {
        spriteBatch.Draw(LiveFeature.a_bg, new Rectangle(10, 55 - (int) (40.0 * (double) ((float) AppMain.dm_xbox_show_progress / 100f)), 230, (int) (220.0 * (double) ((float) AppMain.dm_xbox_show_progress / 100f))), new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 0, 0.0f);
        spriteBatch.Draw(LiveFeature.a_bg, new Rectangle(240 /*0xF0*/, 55 - (int) (40.0 * (double) ((float) AppMain.dm_xbox_show_progress / 100f)), 230, (int) (220.0 * (double) ((float) AppMain.dm_xbox_show_progress / 100f))), new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 1, 0.0f);
      }
      else
      {
        spriteBatch.Draw(LiveFeature.a_bg, new Rectangle(10, 15, 230, 220), new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 0, 0.0f);
        spriteBatch.Draw(LiveFeature.a_bg, new Rectangle(240 /*0xF0*/, 15, 230, 220), new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 1, 0.0f);
      }
      if (AppMain.dm_xbox_show_progress != 100)
        return;
      int curStage = this.curStage;
      if (curStage > 23)
        curStage -= 24;
      string str1;
      if (curStage < 16 /*0x10*/)
      {
        string zoneName = LiveFeature.zone_names[curStage / 4];
        if (curStage % 4 == 3)
          str1 = string.Format(zoneName, (object) Strings.ID_BOSS);
        else
          str1 = string.Format(zoneName, (object) string.Format(Strings.ID_ACT, (object[]) new string[1]
          {
            string.Concat((object) (curStage % 4 + 1))
          }));
      }
      else if (curStage == 16 /*0x10*/)
        str1 = Strings.ID_FINALZONE;
      else
        str1 = string.Format(LiveFeature.zone_names[5], (object[]) new string[1]
        {
          string.Concat((object) (curStage - 16 /*0x10*/))
        });
      Vector2 vector2 = fonts[2].MeasureString(str1);
      spriteBatch.DrawString(fonts[2], str1, new Vector2((float) (240 /*0xF0*/ - ((int) vector2.X >> 1)), 27f), Color.White);
      LiveFeature.arrow_offset += 2;
      if (LiveFeature.arrow_offset > 16 /*0x10*/)
        LiveFeature.arrow_offset = -16;
      Rectangle arrow1Left = LiveFeature.arrow1_Left;
      arrow1Left.X -= Math.Abs(LiveFeature.arrow_offset);
      spriteBatch.Draw(LiveFeature.arrowImg, arrow1Left, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 0, 0.0f);
      Rectangle arrow1Right = LiveFeature.arrow1_Right;
      arrow1Right.X += Math.Abs(LiveFeature.arrow_offset);
      spriteBatch.Draw(LiveFeature.arrowImg, arrow1Right, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, (SpriteEffects) 1, 0.0f);
      string str2 = string.Concat((object) (curStage + 1));
      int index1 = 0;
      int length = str2.Length;
      while (index1 < str2.Length)
      {
        LiveFeature.num_dst_rect.X = 240 /*0xF0*/ - 10 * length;
        LiveFeature.num_src_rect.X = str2[index1] != '0' ? 16 /*0x10*/ * ((int) str2[index1] - 49) : 144 /*0x90*/;
        spriteBatch.Draw(LiveFeature.nums, LiveFeature.num_dst_rect, new Rectangle?(LiveFeature.num_src_rect), Color.White);
        ++index1;
        --length;
      }
      LiveFeature.num_src_rect.X = 160 /*0xA0*/;
      LiveFeature.num_src_rect.Width = 32 /*0x20*/;
      LiveFeature.num_dst_rect.X = 240 /*0xF0*/;
      spriteBatch.Draw(LiveFeature.nums, LiveFeature.num_dst_rect, new Rectangle?(LiveFeature.num_src_rect), Color.White);
      LiveFeature.num_src_rect.Width = 16 /*0x10*/;
      LiveFeature.num_dst_rect.X = 250;
      LiveFeature.num_src_rect.X = 16 /*0x10*/;
      spriteBatch.Draw(LiveFeature.nums, LiveFeature.num_dst_rect, new Rectangle?(LiveFeature.num_src_rect), Color.White);
      LiveFeature.num_dst_rect.X = 260;
      LiveFeature.num_src_rect.X = 48 /*0x30*/;
      spriteBatch.Draw(LiveFeature.nums, LiveFeature.num_dst_rect, new Rectangle?(LiveFeature.num_src_rect), Color.White);
      int index2 = AppMain.g_gs_env_language == 0 || AppMain.g_gs_env_language == 1 || AppMain.g_gs_env_language == 4 || AppMain.g_gs_env_language == 5 ? 0 : 1;
      string idLbRank = Strings.ID_LB_RANK;
      vector2 = fonts[1].MeasureString(idLbRank);
      spriteBatch.DrawString(fonts[1], idLbRank, new Vector2((float) (LiveFeature.table_x[0, index2] - ((int) vector2.X >> 1)), 47f), Color.White);
      string idLbGtag = Strings.ID_LB_GTAG;
      vector2 = fonts[1].MeasureString(idLbGtag);
      spriteBatch.DrawString(fonts[1], idLbGtag, new Vector2((float) (LiveFeature.table_x[1, index2] - ((int) vector2.X >> 1)), 47f), Color.White);
      string idBesttime = Strings.ID_BESTTIME;
      vector2 = fonts[1].MeasureString(idBesttime);
      spriteBatch.DrawString(fonts[1], idBesttime, new Vector2((float) (LiveFeature.table_x[2, index2] - ((int) vector2.X >> 1)), 47f), Color.White);
      string idLbDate = Strings.ID_LB_DATE;
      vector2 = fonts[1].MeasureString(idLbDate);
      spriteBatch.DrawString(fonts[1], idLbDate, new Vector2((float) (LiveFeature.table_x[3, index2] - ((int) vector2.X >> 1)), 47f), Color.White);
      int num = 67;
      if (this.l_status[this.curStage] == LiveFeature.LBStatus.StartedRead)
        LiveFeature._drawWrapText(Strings.ID_LOADING, 240 /*0xF0*/, 144 /*0x90*/, 450, Color.White, true, fonts[1], spriteBatch);
      else if (this.l_status[this.curStage] == LiveFeature.LBStatus.ReadFail)
        LiveFeature._drawWrapText(Strings.ID_LB_UNABLE, 240 /*0xF0*/, 144 /*0x90*/, 450, Color.White, true, fonts[1], spriteBatch);
      else if (this.hiScoresTables[this.curStage] != null && this.hiScoresTables[this.curStage].Length != 0)
      {
        if (AppMain.dm_xbox_show_progress != 100)
          return;
        Color white = Color.White;
        for (int index3 = 0; index3 < this.hiScoresTables[this.curStage].Length; ++index3)
        {
          if (index3 < 5 || index3 >= 5 && this.hiScoresTables[this.curStage][index3].isMe)
          {
            Color color = this.hiScoresTables[this.curStage][index3].isMe ? new Color(154 - Math.Abs(LiveFeature.arrow_offset << 3), (int) byte.MaxValue, 100 - Math.Abs(LiveFeature.arrow_offset << 2)) : Color.WhiteSmoke;
            string str3 = string.Concat((object) this.hiScoresTables[this.curStage][index3].index);
            vector2 = fonts[0].MeasureString(str3);
            spriteBatch.DrawString(fonts[0], str3, new Vector2((float) (LiveFeature.table_x[0, index2] - ((int) vector2.X >> 1)), (float) num), color);
            string name = this.hiScoresTables[this.curStage][index3].name;
            vector2 = fonts[0].MeasureString(name);
            spriteBatch.DrawString(fonts[0], name, new Vector2((float) (LiveFeature.table_x[1, index2] - ((int) vector2.X >> 1)), (float) num), color);
            string str4;
            if (this.curStage > 23)
            {
              ushort min = 0;
              ushort sec = 0;
              ushort msec = 0;
              AppMain.AkUtilFrame60ToTime((uint) this.hiScoresTables[this.curStage][index3].value, ref min, ref sec, ref msec);
              StringBuilder stringBuilder = new StringBuilder();
              if (min < (ushort) 10)
                stringBuilder.Append("0");
              stringBuilder.Append(min);
              stringBuilder.Append("'");
              if (sec < (ushort) 10)
                stringBuilder.Append("0");
              stringBuilder.Append(sec);
              stringBuilder.Append("''");
              if (msec < (ushort) 10)
                stringBuilder.Append("0");
              stringBuilder.Append(msec);
              str4 = stringBuilder.ToString();
            }
            else
              str4 = string.Concat((object) this.hiScoresTables[this.curStage][index3].value);
            vector2 = fonts[0].MeasureString(str4);
            spriteBatch.DrawString(fonts[0], str4, new Vector2((float) (LiveFeature.table_x[2, index2] - ((int) vector2.X >> 1)), (float) num), color);
            string str5 = string.Concat((object) this.hiScoresTables[this.curStage][index3].date);
            vector2 = fonts[0].MeasureString(str5);
            spriteBatch.DrawString(fonts[0], str5, new Vector2((float) (LiveFeature.table_x[3, index2] - ((int) vector2.X >> 1)), (float) num), color);
            num += (int) vector2.Y;
          }
        }
      }
      else
        LiveFeature._drawWrapText(Strings.ID_LB_NORECORDS, 240 /*0xF0*/, 144 /*0x90*/, 450, Color.White, true, fonts[1], spriteBatch);
    }
    finally
    {
      spriteBatch.End();
    }
  }

  public void ShowOverride()
  {
    if (LiveFeature.interruptMainLoop == 0)
      return;
    if (LiveFeature.interruptMainLoop == 2)
    {
      this._drawLeaderboards(LiveFeature.GAME.spriteBatch, LiveFeature.GAME.fnts);
    }
    else
    {
      if (LiveFeature.interruptMainLoop != 1)
        return;
      this._drawAchievements(LiveFeature.GAME.spriteBatch, LiveFeature.GAME.fnts);
    }
  }

  public static bool isInterrupted() => LiveFeature.interruptMainLoop != 0;

  internal static void endInterrupt()
  {
    if (LiveFeature.interruptMainLoop == 1)
    {
      int length = AppMain.achievements.Length;
      for (int index = 0; index < length; ++index)
      {
        ((GraphicsResource) LiveFeature.a_images[index]).Dispose();
        LiveFeature.a_images[index] = (Texture2D) null;
      }
      LiveFeature.a_images = (Texture2D[]) null;
      ((GraphicsResource) LiveFeature.a_bg).Dispose();
      LiveFeature.a_bg = (Texture2D) null;
      ((GraphicsResource) LiveFeature.arrowImg2).Dispose();
      LiveFeature.arrowImg2 = (Texture2D) null;
      ((GraphicsResource) LiveFeature.titleImg).Dispose();
      LiveFeature.titleImg = (Texture2D) null;
      LiveFeature.achievementTextArray = (LiveFeature.AchievementText[]) null;
    }
    if (LiveFeature.interruptMainLoop == 2)
    {
      ((GraphicsResource) LiveFeature.a_bg).Dispose();
      LiveFeature.a_bg = (Texture2D) null;
      ((GraphicsResource) LiveFeature.arrowImg).Dispose();
      LiveFeature.arrowImg = (Texture2D) null;
      ((GraphicsResource) LiveFeature.nums).Dispose();
      LiveFeature.nums = (Texture2D) null;
      LiveFeature.getInstance().clearHiScores();
    }
    LiveFeature.interruptMainLoop = 0;
  }

  public static string[] _wrapString(string s, int width, SpriteFont font)
  {
    List<string> stringList = new List<string>(5);
    s = s.Trim();
    string[] strArray = s.Split(new char[1]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);
    int num1 = 0;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string str in strArray)
    {
      Vector2 vector2 = font.MeasureString(str + " ");
      if ((double) vector2.X + (double) num1 > (double) width)
      {
        int num2 = 0;
        stringList.Add(stringBuilder.ToString());
        stringBuilder.Remove(0, stringBuilder.Length);
        stringBuilder.Append(str);
        stringBuilder.Append(" ");
        num1 = num2 + (int) vector2.X;
      }
      else
      {
        stringBuilder.Append(str);
        stringBuilder.Append(" ");
        num1 += (int) vector2.X;
        if (str.Contains("\r\n"))
          num1 = 0;
      }
    }
    stringList.Add(stringBuilder.ToString());
    return stringList.ToArray();
  }

  public static int _drawWrapText(
    string text,
    int x,
    int y,
    int width,
    Color color,
    bool isXCenter,
    SpriteFont font,
    SpriteBatch sb,
    float corrector)
  {
    return LiveFeature._drawWrapText(LiveFeature._wrapString(text, width, font), x, y, width, color, isXCenter, font, sb, corrector);
  }

  public static int _drawWrapText(
    string text,
    int x,
    int y,
    int width,
    Color color,
    bool isXCenter,
    SpriteFont font,
    SpriteBatch sb)
  {
    return LiveFeature._drawWrapText(LiveFeature._wrapString(text, width, font), x, y, width, color, isXCenter, font, sb);
  }

    public static int _drawWrapText(string[] s, int x, int y, int width, Color color, bool isXCenter, SpriteFont font, SpriteBatch sb, float corrector)
    {
        Vector2 position = new Vector2(x, y);
        int num = 0;
        foreach (string text in s)
        {
            Vector2 vector = font.MeasureString(text);
            if (isXCenter)
            {
                position.X = x - ((int)vector.X >> 1);
            }
            sb.DrawString(font, text, position, color);
            position.Y += vector.Y * corrector;
            num += (int)(vector.Y * corrector);
        }
        return num;
    }

    public static int _drawWrapText(
    string[] s,
    int x,
    int y,
    int width,
    Color color,
    bool isXCenter,
    SpriteFont font,
    SpriteBatch sb)
  {
    return LiveFeature._drawWrapText(s, x, y, width, color, isXCenter, font, sb, 1f);
  }

  public enum Stages
  {
    Zone11_Score,
    Zone12_Score,
    Zone13_Score,
    Zone14_Score,
    Zone21_Score,
    Zone22_Score,
    Zone23_Score,
    Zone24_Score,
    Zone31_Score,
    Zone32_Score,
    Zone33_Score,
    Zone34_Score,
    Zone41_Score,
    Zone42_Score,
    Zone43_Score,
    Zone44_Score,
    ZoneF_Score,
    ZoneSS1_Score,
    ZoneSS2_Score,
    ZoneSS3_Score,
    ZoneSS4_Score,
    ZoneSS5_Score,
    ZoneSS6_Score,
    ZoneSS7_Score,
    Zone11_Time,
    Zone12_Time,
    Zone13_Time,
    Zone14_Time,
    Zone21_Time,
    Zone22_Time,
    Zone23_Time,
    Zone24_Time,
    Zone31_Time,
    Zone32_Time,
    Zone33_Time,
    Zone34_Time,
    Zone41_Time,
    Zone42_Time,
    Zone43_Time,
    Zone44_Time,
    ZoneF_Time,
    ZoneSS1_Time,
    ZoneSS2_Time,
    ZoneSS3_Time,
    ZoneSS4_Time,
    ZoneSS5_Time,
    ZoneSS6_Time,
    ZoneSS7_Time,
    LeaderboardsCount,
  }

  public enum LBStatus
  {
    NotRead,
    StartedRead,
    ReadSuccess,
    ReadFail,
  }

  public struct HISCORE_ENTRY
  {
    public int index;
    public bool isMe;
    public string name;
    public int value;
    public DateTime date;
  }

  private struct AchievementText
  {
    public string[] text;
    public int verticalSize;
  }
}
