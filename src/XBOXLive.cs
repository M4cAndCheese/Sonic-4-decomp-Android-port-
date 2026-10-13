// Decompiled with JetBrains decompiler
// Type: XBOXLive
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Collections.Generic;
using System.Threading;

public abstract class XBOXLive
{
  protected static GamerServicesComponent gameService;
  protected static bool TrialModeCached = false;
  public static XBOXLive instanceXBOX;
  public static XBOXLive.SigninStatus signinStatus = XBOXLive.SigninStatus.None;
  public static bool displayTitleUpdateMessage = false;
  public static bool allowShowUpdate = false;
  public static int updateExceptCount = 1;

  public static bool isTrial(bool update)
  {
    if (update)
      XBOXLive.TrialModeCached = Guide.IsTrialMode;
    return XBOXLive.TrialModeCached;
  }

  public static bool isTrial() => XBOXLive.TrialModeCached;

  public static void showGuide()
  {
    if (!XBOXLive.isTrial() || Guide.IsVisible)
      return;
    Guide.ShowMarketplace((PlayerIndex) 0);
  }

  public static void HandleGameUpdateRequired(GameUpdateRequiredException e)
  {
    try
    {
      if (((GameComponent) XBOXLive.gameService).Enabled)
        XBOXLive.displayTitleUpdateMessage = true;
      XBOXLive.signinStatus = XBOXLive.SigninStatus.UpdateNeeded;
      ((GameComponent) XBOXLive.gameService).Enabled = false;
    }
    catch (Exception ex)
    {
    }
  }

  public abstract void _initTextDialog(
    out string dlgYes,
    out string dlgNo,
    out string dlgCaption,
    out string dlgText);

  public static void showUpdateMB()
  {
    if (!XBOXLive.displayTitleUpdateMessage || !XBOXLive.allowShowUpdate || Guide.IsVisible)
      return;
    XBOXLive.displayTitleUpdateMessage = false;
    AppMain.g_ao_sys_global.is_show_ui = true;
    string dlgYes = "";
    string dlgNo = "";
    string dlgCaption = "";
    string dlgText = "";
    if (XBOXLive.instanceXBOX != null)
      XBOXLive.instanceXBOX._initTextDialog(out dlgYes, out dlgNo, out dlgCaption, out dlgText);
    Guide.BeginShowMessageBox(dlgCaption, dlgText, (IEnumerable<string>) new List<string>()
    {
      dlgYes,
      dlgNo
    }, 1, (MessageBoxIcon) 3, new AsyncCallback(XBOXLive.UpdateDialogGetMBResult), (object) null);
  }

  protected static void UpdateDialogGetMBResult(IAsyncResult userResult)
  {
    AppMain.g_ao_sys_global.is_show_ui = false;
    int? nullable = Guide.EndShowMessageBox(userResult);
    try
    {
      if (!nullable.HasValue || nullable.Value != 0)
        return;
      if (Guide.IsTrialMode)
      {
        int num = 10;
        while (Guide.IsVisible && num > 0)
        {
          --num;
          Thread.Sleep(100);
        }
        Guide.ShowMarketplace((PlayerIndex) 0);
      }
      else 
      { 
            
      }
    }
    catch (Exception ex)
    {
    }
  }

  public static void tryUpdate()
  {
    if (XBOXLive.updateExceptCount < 0)
      XBOXLive.updateExceptCount = 0;
    if (XBOXLive.updateExceptCount == 0)
      throw new GameUpdateRequiredException("Text Exception");
  }

  public enum SigninStatus
  {
    None,
    SigningIn,
    Local,
    LIVE,
    Error,
    UpdateNeeded,
  }
}
