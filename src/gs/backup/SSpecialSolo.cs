// Decompiled with JetBrains decompiler
// Type: gs.backup.SSpecialSolo
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;

namespace gs.backup
{

    public class SSpecialSolo
    {
      public const uint c_false = 0;
      public const uint c_true = 1;
      public const uint c_high_score_max_limit = 1000000000;
      public const uint c_high_score_unit = 10;
      public const uint c_fast_time_max_limit = 36000;
      public uint m_high_score = 32 /*0x20*/;
      public uint m_fast_time = 16 /*0x10*/;
      public uint m_is_high_score_enable = 1;
      public uint m_is_fast_time_enable = 1;
      public uint m_is_high_score_uploaded = 1;
      public uint m_is_fast_time_uploaded = 1;
      public uint m_emerald_stage = 4;
      public uint m_is_score_uploaded_once = 1;
      public uint m_is_time_uploaded_once = 1;
      public uint m_reserve1 = 24;

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            binaryWriter.Write(this.m_high_score);
            binaryWriter.Write(this.m_fast_time);
            binaryWriter.Write(this.m_is_high_score_enable);
            binaryWriter.Write(this.m_is_fast_time_enable);
            binaryWriter.Write(this.m_is_high_score_uploaded);
            binaryWriter.Write(this.m_is_fast_time_uploaded);
            binaryWriter.Write(this.m_emerald_stage);
            binaryWriter.Write(this.m_is_score_uploaded_once);
            binaryWriter.Write(this.m_is_time_uploaded_once);
            binaryWriter.Write(this.m_reserve1);
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
            this.m_high_score = binaryReader.ReadUInt32();
            this.m_fast_time = binaryReader.ReadUInt32();
            this.m_is_high_score_enable = binaryReader.ReadUInt32();
            this.m_is_fast_time_enable = binaryReader.ReadUInt32();
            this.m_is_high_score_uploaded = binaryReader.ReadUInt32();
            this.m_is_fast_time_uploaded = binaryReader.ReadUInt32();
            this.m_emerald_stage = binaryReader.ReadUInt32();
            this.m_is_score_uploaded_once = binaryReader.ReadUInt32();
            this.m_is_time_uploaded_once = binaryReader.ReadUInt32();
            this.m_reserve1 = binaryReader.ReadUInt32();
          }
        }
      }

      public void Init()
      {
        this.m_high_score = 100000000U;
        this.m_fast_time = 36000U;
        this.m_is_high_score_enable = 0U;
        this.m_is_fast_time_enable = 0U;
        this.m_is_high_score_uploaded = 0U;
        this.m_is_fast_time_uploaded = 0U;
        this.m_emerald_stage = 0U;
        this.m_is_score_uploaded_once = 0U;
        this.m_is_time_uploaded_once = 0U;
      }

      public bool IsNew() => false;

      public bool IsHighScoreEnable() => this.m_is_high_score_enable != 0U;

      public bool IsFastTimeEnable() => this.m_is_fast_time_enable != 0U;

      public uint GetHighScore() => this.m_high_score * 10U;

      public uint GetFastTime() => this.m_fast_time;

      public bool IsHighScoreUploaded() => this.m_is_high_score_uploaded != 0U;

      public bool IsFastTimeUploaded() => this.m_is_fast_time_uploaded != 0U;

      public bool IsGetEmerald() => this.m_emerald_stage != 0U;

      public EEmeraldStage.Type GetEmeraldStage() => (EEmeraldStage.Type) this.m_emerald_stage;

      public bool IsScoreUploadedOnce() => this.m_is_score_uploaded_once != 0U;

      public bool IsTimeUploadedOnce() => this.m_is_time_uploaded_once != 0U;

      public void SetNew(bool val)
      {
      }

      public void SetHighScore(uint high_score)
      {
        high_score = Math.Min(high_score, 1000000000U);
        this.m_high_score = high_score / 10U;
        this.m_is_high_score_enable = 1U;
      }

      public void SetFastTime(uint fast_time)
      {
        fast_time = Math.Min(fast_time, 36000U);
        this.m_fast_time = fast_time;
        this.m_is_fast_time_enable = 1U;
      }

      public void SetHighScoreUploaded(bool is_uploaded)
      {
        this.m_is_high_score_uploaded = is_uploaded ? 1U : 0U;
      }

      public void SetFastTimeUploaded(bool is_uploaded)
      {
        this.m_is_fast_time_uploaded = is_uploaded ? 1U : 0U;
      }

      public void SetEmeraldStage(EEmeraldStage.Type emerald_stage)
      {
        this.m_emerald_stage = Math.Min((uint) emerald_stage, 13U);
      }

      public void SetScoreUploadedOnce(bool is_uploaded_once)
      {
        this.m_is_score_uploaded_once = is_uploaded_once ? 1U : 0U;
      }

      public void SetTimeUploadedOnce(bool is_uploaded_once)
      {
        this.m_is_time_uploaded_once = is_uploaded_once ? 1U : 0U;
      }
    }
}
