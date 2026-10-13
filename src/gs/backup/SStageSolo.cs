// Decompiled with JetBrains decompiler
// Type: gs.backup.SStageSolo
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace gs.backup
{

    public class SStageSolo
    {
      public const uint c_false = 0;
      public const uint c_true = 1;
      public const uint c_high_score_max_limit = 1000000000;
      public const uint c_high_score_unit = 10;
      public const uint c_fast_time_max_limit = 36000;
      private SStageSolo.SRecord[] m_record = AppMain.New<SStageSolo.SRecord>(2U);
      private uint m_is_new = 1;
      private uint m_is_high_score_use_supersonic = 1;
      private uint m_is_fast_time_use_supersonic = 1;
      private uint m_is_score_uploaded_once = 1;
      private uint m_is_time_uploaded_once = 1;
      private uint m_is_use_supersonic_once = 1;
      private uint m_reserve1 = 26;

      public bool IsNew() => this.m_is_new != 0U;

      public bool IsHighScoreUseSuperSonic() => this.m_is_high_score_use_supersonic != 0U;

      public bool IsFastTimeUseSuperSonic() => this.m_is_fast_time_use_supersonic != 0U;

      public bool IsScoreUploadedOnce() => this.m_is_score_uploaded_once != 0U;

      public bool IsTimeUploadedOnce() => this.m_is_time_uploaded_once != 0U;

      public bool IsUseSuperSonicOnce() => this.m_is_use_supersonic_once != 0U;

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            for (int index = 0; index < this.m_record.Length; ++index)
              binaryWriter.Write(this.m_record[index].getData());
            binaryWriter.Write(this.m_is_new);
            binaryWriter.Write(this.m_is_high_score_use_supersonic);
            binaryWriter.Write(this.m_is_fast_time_use_supersonic);
            binaryWriter.Write(this.m_is_score_uploaded_once);
            binaryWriter.Write(this.m_is_time_uploaded_once);
            binaryWriter.Write(this.m_is_use_supersonic_once);
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
            for (int index = 0; index < this.m_record.Length; ++index)
              this.m_record[index].setData(binaryReader.ReadBytes(28));
            this.m_is_new = binaryReader.ReadUInt32();
            this.m_is_high_score_use_supersonic = binaryReader.ReadUInt32();
            this.m_is_fast_time_use_supersonic = binaryReader.ReadUInt32();
            this.m_is_score_uploaded_once = binaryReader.ReadUInt32();
            this.m_is_time_uploaded_once = binaryReader.ReadUInt32();
            this.m_is_use_supersonic_once = binaryReader.ReadUInt32();
            this.m_reserve1 = binaryReader.ReadUInt32();
          }
        }
      }

      public void Init()
      {
        int length = this.m_record.Length;
        for (int index = 0; index < length; ++index)
        {
          this.m_record[index] = new SStageSolo.SRecord();
          this.m_record[index].high_score = 100000000U;
          this.m_record[index].fast_time = 36000U;
          this.m_record[index].is_high_score_enable = 0U;
          this.m_record[index].is_fast_time_enable = 0U;
          this.m_record[index].is_high_score_uploaded = 0U;
          this.m_record[index].is_fast_time_uploaded = 0U;
        }
        this.m_is_new = 1U;
        this.m_is_high_score_use_supersonic = 0U;
        this.m_is_fast_time_use_supersonic = 0U;
        this.m_is_score_uploaded_once = 0U;
        this.m_is_time_uploaded_once = 0U;
        this.m_is_use_supersonic_once = 0U;
      }

      private SStageSolo.SRecord getRecord(SStageSolo.ERecordKind.Type record_kind)
      {
        return this.m_record[(int) (uint) record_kind];
      }

      private SStageSolo.SRecord getRecord(bool is_supersonic)
      {
        return this.getRecord(is_supersonic ? SStageSolo.ERecordKind.Type.SuperSonic : SStageSolo.ERecordKind.Type.Sonic);
      }

      public bool IsHighScoreEnable(bool is_supersonic)
      {
        return this.getRecord(is_supersonic).is_high_score_enable != 0U;
      }

      public bool IsFastTimeEnable(bool is_supersonic)
      {
        return this.getRecord(is_supersonic).is_fast_time_enable != 0U;
      }

      public uint GetHighScore(bool is_supersonic) => this.getRecord(is_supersonic).high_score * 10U;

      public uint GetFastTime(bool is_supersonic) => this.getRecord(is_supersonic).fast_time;

      public bool IsHighScoreUploaded(bool is_supersonic)
      {
        return this.getRecord(is_supersonic).is_high_score_uploaded != 0U;
      }

      public bool IsFastTimeUploaded(bool is_supersonic)
      {
        return this.getRecord(is_supersonic).is_fast_time_uploaded != 0U;
      }

      public void SetNew(bool is_new) => this.m_is_new = is_new ? 1U : 0U;

      public void SetHighScore(uint high_score, bool is_use_supersonic)
      {
        SStageSolo.SRecord record = this.getRecord(is_use_supersonic);
        high_score = Math.Min(high_score, 1000000000U);
        record.high_score = high_score / 10U;
        record.is_high_score_enable = 1U;
        if (this.getRecord(!is_use_supersonic).high_score < record.high_score)
          this.m_is_high_score_use_supersonic = is_use_supersonic ? 1U : 0U;
        if (!is_use_supersonic)
          return;
        this.m_is_use_supersonic_once = 1U;
      }

      public void SetFastTime(uint fast_time, bool is_use_supersonic)
      {
        SStageSolo.SRecord record = this.getRecord(is_use_supersonic);
        fast_time = Math.Min(fast_time, 36000U);
        record.fast_time = fast_time;
        record.is_fast_time_enable = 1U;
        if (record.fast_time < this.getRecord(!is_use_supersonic).fast_time)
          this.m_is_fast_time_use_supersonic = is_use_supersonic ? 1U : 0U;
        if (!is_use_supersonic)
          return;
        this.m_is_use_supersonic_once = 1U;
      }

      public void SetHighScoreUploaded(bool is_supersonic, bool is_uploaded)
      {
        this.getRecord(is_supersonic).is_high_score_uploaded = is_uploaded ? 1U : 0U;
      }

      public void SetFastTimeUploaded(bool is_supersonic, bool is_uploaded)
      {
        this.getRecord(is_supersonic).is_fast_time_uploaded = is_uploaded ? 1U : 0U;
      }

      public void SetScoreUploadedOnce(bool is_uploaded_once)
      {
        this.m_is_score_uploaded_once = is_uploaded_once ? 1U : 0U;
      }

      public void SetTimeUploadedOnce(bool is_uploaded_once)
      {
        this.m_is_time_uploaded_once = is_uploaded_once ? 1U : 0U;
      }

      public void SetUseSuperSonicOnce(bool is_use_supersonic_once)
      {
        this.m_is_use_supersonic_once = is_use_supersonic_once ? 1U : 0U;
      }

      [StructLayout(LayoutKind.Sequential, Size = 1)]
      public struct ERecordKind
      {
        public enum Type
        {
          Sonic,
          SuperSonic,
          Max,
          None,
        }
      }

      public class SRecord
      {
        public uint high_score = 32 /*0x20*/;
        public uint fast_time = 16 /*0x10*/;
        public uint is_high_score_enable = 1;
        public uint is_fast_time_enable = 1;
        public uint is_high_score_uploaded = 1;
        public uint is_fast_time_uploaded = 1;
        public uint reserve1 = 12;

        public byte[] getData()
        {
          using (MemoryStream output = new MemoryStream())
          {
            using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
            {
              binaryWriter.Write(this.high_score);
              binaryWriter.Write(this.fast_time);
              binaryWriter.Write(this.is_high_score_enable);
              binaryWriter.Write(this.is_fast_time_enable);
              binaryWriter.Write(this.is_high_score_uploaded);
              binaryWriter.Write(this.is_fast_time_uploaded);
              binaryWriter.Write(this.reserve1);
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
              this.high_score = binaryReader.ReadUInt32();
              this.fast_time = binaryReader.ReadUInt32();
              this.is_high_score_enable = binaryReader.ReadUInt32();
              this.is_fast_time_enable = binaryReader.ReadUInt32();
              this.is_high_score_uploaded = binaryReader.ReadUInt32();
              this.is_fast_time_uploaded = binaryReader.ReadUInt32();
              this.reserve1 = binaryReader.ReadUInt32();
            }
          }
        }
      }
    }
}
