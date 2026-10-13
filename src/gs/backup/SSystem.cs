// Decompiled with JetBrains decompiler
// Type: gs.backup.SSystem
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;

namespace gs.backup
{

    public class SSystem
    {
      public const uint c_false = 0;
      public const uint c_true = 1;
      public const uint c_player_stock_limit = 1000;
      public const uint c_killed_limit = 1000;
      public const uint c_clear_count_limit = 2;
      private uint m_player_stock = 10;
      private uint m_killed = 10;
      private uint m_clear_count = 2;
      private uint m_announce_open_zone_select = 1;
      private uint m_announce_open_zone1_boss = 1;
      private uint m_announce_open_zone2_boss = 1;
      private uint m_announce_open_zone3_boss = 1;
      private uint m_announce_open_zone4_boss = 1;
      private uint m_announce_open_final_zone = 1;
      private uint m_announce_open_supersonic = 1;
      private uint m_announce_open_specialstage = 1;
      private uint m_reserve1 = 2;
      private uint m_announcetruck_tilt = 1;
      private uint m_announcetruck_flick = 1;
      private uint m_announcespecial_stage_tilt = 1;
      private uint m_announcespecial_stage_flick = 1;
      private uint m_reserve2 = 28;

      public uint GetPlayerStock() => this.m_player_stock;

      public uint GetKilled() => this.m_killed;

      public uint GetClearCount() => this.m_clear_count;

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            binaryWriter.Write(this.m_player_stock);
            binaryWriter.Write(this.m_killed);
            binaryWriter.Write(this.m_clear_count);
            binaryWriter.Write(this.m_announce_open_zone_select);
            binaryWriter.Write(this.m_announce_open_zone1_boss);
            binaryWriter.Write(this.m_announce_open_zone2_boss);
            binaryWriter.Write(this.m_announce_open_zone3_boss);
            binaryWriter.Write(this.m_announce_open_zone4_boss);
            binaryWriter.Write(this.m_announce_open_final_zone);
            binaryWriter.Write(this.m_announce_open_supersonic);
            binaryWriter.Write(this.m_announce_open_specialstage);
            binaryWriter.Write(this.m_reserve1);
            binaryWriter.Write(this.m_announcetruck_tilt);
            binaryWriter.Write(this.m_announcetruck_flick);
            binaryWriter.Write(this.m_announcespecial_stage_tilt);
            binaryWriter.Write(this.m_announcespecial_stage_flick);
            binaryWriter.Write(this.m_reserve2);
          }
          return output.ToArray();
        }
      }

      public void setData(byte[] data)
      {
        using (MemoryStream input = new MemoryStream(data))
        {
          using (BinaryReader binaryReader = new BinaryReader((Stream) input))
          {
            this.m_player_stock = binaryReader.ReadUInt32();
            this.m_killed = binaryReader.ReadUInt32();
            this.m_clear_count = binaryReader.ReadUInt32();
            this.m_announce_open_zone_select = binaryReader.ReadUInt32();
            this.m_announce_open_zone1_boss = binaryReader.ReadUInt32();
            this.m_announce_open_zone2_boss = binaryReader.ReadUInt32();
            this.m_announce_open_zone3_boss = binaryReader.ReadUInt32();
            this.m_announce_open_zone4_boss = binaryReader.ReadUInt32();
            this.m_announce_open_final_zone = binaryReader.ReadUInt32();
            this.m_announce_open_supersonic = binaryReader.ReadUInt32();
            this.m_announce_open_specialstage = binaryReader.ReadUInt32();
            this.m_reserve1 = binaryReader.ReadUInt32();
            this.m_announcetruck_tilt = binaryReader.ReadUInt32();
            this.m_announcetruck_flick = binaryReader.ReadUInt32();
            this.m_announcespecial_stage_tilt = binaryReader.ReadUInt32();
            this.m_announcespecial_stage_flick = binaryReader.ReadUInt32();
            this.m_reserve2 = binaryReader.ReadUInt32();
          }
        }
      }

      public static SSystem CreateInstance(uint save_index)
      {
        return SBackup.CreateInstance().GetSystem(save_index);
      }

      public static SSystem CreateInstance() => SBackup.CreateInstance().GetSystem();

      public void Init()
      {
        this.m_player_stock = 3U;
        this.m_killed = 0U;
        this.m_clear_count = 0U;
        this.m_announce_open_zone_select = 0U;
        this.m_announce_open_zone1_boss = 0U;
        this.m_announce_open_zone2_boss = 0U;
        this.m_announce_open_zone3_boss = 0U;
        this.m_announce_open_zone4_boss = 0U;
        this.m_announce_open_final_zone = 0U;
        this.m_announce_open_supersonic = 0U;
        this.m_announce_open_specialstage = 0U;
        this.m_announcetruck_tilt = 0U;
        this.m_announcetruck_flick = 0U;
        this.m_announcespecial_stage_tilt = 0U;
        this.m_announcespecial_stage_flick = 0U;
      }

      public bool IsAnnounce(SSystem.EAnnounce index)
      {
        uint num;
        switch (index)
        {
          case SSystem.EAnnounce.OpenZoneSelect:
            num = this.m_announce_open_zone_select;
            break;
          case SSystem.EAnnounce.OpenZone1Boss:
            num = this.m_announce_open_zone1_boss;
            break;
          case SSystem.EAnnounce.OpenZone2Boss:
            num = this.m_announce_open_zone2_boss;
            break;
          case SSystem.EAnnounce.OpenZone3Boss:
            num = this.m_announce_open_zone3_boss;
            break;
          case SSystem.EAnnounce.OpenZone4Boss:
            num = this.m_announce_open_zone4_boss;
            break;
          case SSystem.EAnnounce.OpenFinalZone:
            num = this.m_announce_open_final_zone;
            break;
          case SSystem.EAnnounce.OpenSuperSonic:
            num = this.m_announce_open_supersonic;
            break;
          case SSystem.EAnnounce.OpenSpecialStage:
            num = this.m_announce_open_specialstage;
            break;
          case SSystem.EAnnounce.TruckTilt:
            num = this.m_announcetruck_tilt;
            break;
          case SSystem.EAnnounce.TruckFlick:
            num = this.m_announcetruck_flick;
            break;
          case SSystem.EAnnounce.SpecialStageTilt:
            num = this.m_announcespecial_stage_tilt;
            break;
          case SSystem.EAnnounce.SpecialStageFlick:
            num = this.m_announcespecial_stage_flick;
            break;
          default:
            num = 0U;
            break;
        }
        return num != 0U;
      }

      public void SetPlayerStock(uint player_stock)
      {
        player_stock = Math.Min(player_stock, 1000U);
        this.m_player_stock = player_stock;
      }

      public void SetKilled(uint killed)
      {
        killed = Math.Min(killed, 1000U);
        this.m_killed = killed;
      }

      public void SetClearCount(uint count)
      {
        count = Math.Min(count, 2U);
        this.m_clear_count = count;
      }

      public void SetAnnounce(SSystem.EAnnounce index, bool is_announce)
      {
        uint num = is_announce ? 1U : 0U;
        switch (index)
        {
          case SSystem.EAnnounce.OpenZoneSelect:
            this.m_announce_open_zone_select = num;
            break;
          case SSystem.EAnnounce.OpenZone1Boss:
            this.m_announce_open_zone1_boss = num;
            break;
          case SSystem.EAnnounce.OpenZone2Boss:
            this.m_announce_open_zone2_boss = num;
            break;
          case SSystem.EAnnounce.OpenZone3Boss:
            this.m_announce_open_zone3_boss = num;
            break;
          case SSystem.EAnnounce.OpenZone4Boss:
            this.m_announce_open_zone4_boss = num;
            break;
          case SSystem.EAnnounce.OpenFinalZone:
            this.m_announce_open_final_zone = num;
            break;
          case SSystem.EAnnounce.OpenSuperSonic:
            this.m_announce_open_supersonic = num;
            break;
          case SSystem.EAnnounce.OpenSpecialStage:
            this.m_announce_open_specialstage = num;
            break;
          case SSystem.EAnnounce.TruckTilt:
            this.m_announcetruck_tilt = num;
            break;
          case SSystem.EAnnounce.TruckFlick:
            this.m_announcetruck_flick = num;
            break;
          case SSystem.EAnnounce.SpecialStageTilt:
            this.m_announcespecial_stage_tilt = num;
            break;
          case SSystem.EAnnounce.SpecialStageFlick:
            this.m_announcespecial_stage_flick = num;
            break;
        }
      }

      public enum EAnnounce
      {
        OpenZoneSelect,
        OpenZone1Boss,
        OpenZone2Boss,
        OpenZone3Boss,
        OpenZone4Boss,
        OpenFinalZone,
        OpenSuperSonic,
        OpenSpecialStage,
        TruckTilt,
        TruckFlick,
        SpecialStageTilt,
        SpecialStageFlick,
        Max,
        None,
      }
    }
}
