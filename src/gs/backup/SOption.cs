// Decompiled with JetBrains decompiler
// Type: gs.backup.SOption
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace gs.backup
{

    public class SOption
    {
      public const uint c_false = 0;
      public const uint c_true = 1;
      public const uint c_volume_bgm_max_limit = 100;
      public const uint c_volume_bgm_unit = 10;
      public const uint c_volume_se_max_limit = 100;
      public const uint c_volume_se_unit = 10;
      public const uint c_name_length_limit = 10;
      public const uint c_name_length_limit_pad = 16 /*0x10*/;
      private uint m_is_vibration = 1;
      private uint m_volume_bgm = 4;
      private uint m_volume_se = 4;
      private uint m_control = 2;
      private uint m_reserve1 = 21;

      public bool IsVibration() => this.m_is_vibration != 0U;

      public uint GetVolumeBgm() => this.m_volume_bgm * 10U;

      public uint GetVolumeSe() => this.m_volume_se * 10U;

      public SOption.EControl.Type GetControl() => (SOption.EControl.Type) this.m_control;

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            binaryWriter.Write(this.m_is_vibration);
            binaryWriter.Write(this.m_volume_bgm);
            binaryWriter.Write(this.m_volume_se);
            binaryWriter.Write(this.m_control);
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
            this.m_is_vibration = binaryReader.ReadUInt32();
            this.m_volume_bgm = binaryReader.ReadUInt32();
            this.m_volume_se = binaryReader.ReadUInt32();
            this.m_control = binaryReader.ReadUInt32();
            this.m_reserve1 = binaryReader.ReadUInt32();
          }
        }
      }

      public static SOption CreateInstance(uint save_index)
      {
        return SBackup.CreateInstance().GetOption(save_index);
      }

      public static SOption CreateInstance() => SBackup.CreateInstance().GetOption();

      public void Init()
      {
        this.m_is_vibration = 1U;
        this.m_volume_bgm = 10U;
        this.m_volume_se = 10U;
        this.m_control = 1U;
      }

      private void SetVibration(bool is_vibration) => this.m_is_vibration = is_vibration ? 1U : 0U;

      public void SetVolumeBgm(uint volume_bgm)
      {
        volume_bgm = Math.Min(volume_bgm, 100U);
        this.m_volume_bgm = volume_bgm / 10U;
      }

      public void SetVolumeSe(uint volume_se)
      {
        volume_se = Math.Min(volume_se, 100U);
        this.m_volume_se = volume_se / 10U;
      }

      public void SetControl(SOption.EControl.Type control)
      {
        control = (SOption.EControl.Type) Math.Min((uint) control, 3U);
        this.m_control = (uint) control;
      }

      [StructLayout(LayoutKind.Sequential, Size = 1)]
      public struct EControl
      {
        public enum Type
        {
          Tilt,
          VirtualPadDown,
          VirtualPadUp,
          Max,
          None,
        }
      }
    }
}
