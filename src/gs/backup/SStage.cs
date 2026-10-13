// Decompiled with JetBrains decompiler
// Type: gs.backup.SStage
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace gs.backup
{

    public class SStage
    {
      private const uint c_size = 17;
      private SStageSolo[] m_stage = AppMain.New<SStageSolo>(17U);

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            for (int index = 0; index < 17; ++index)
              binaryWriter.Write(this.m_stage[index].getData());
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
            for (int index = 0; index < 17; ++index)
              this.m_stage[index].setData(binaryReader.ReadBytes(84));
          }
        }
      }

      public SStageSolo getStageSolo(uint index) => this.m_stage[(int) index];

      public SStageSolo getStageSolo(SStage.EStage.Type index) => this.getStageSolo((uint) index);

      public SStageSolo this[int index]
      {
        get => this.m_stage[index];
        set => this.m_stage[index] = value;
      }

      public static uint GetSize() => 17;

      public static SStage CreateInstance(uint save_index)
      {
        return SBackup.CreateInstance().GetStage(save_index);
      }

      public static SStage CreateInstance() => SBackup.CreateInstance().GetStage();

      public void Init()
      {
        for (int index = 0; index < this.m_stage.Length; ++index)
        {
          this.m_stage[index] = new SStageSolo();
          this.m_stage[index].Init();
        }
      }

      [StructLayout(LayoutKind.Sequential, Size = 1)]
      public struct EStage
      {
        public enum Type
        {
          Zone1Act1,
          Zone1Act2,
          Zone1Act3,
          Zone1Boss,
          Zone2Act1,
          Zone2Act2,
          Zone2Act3,
          Zone2Boss,
          Zone3Act1,
          Zone3Act2,
          Zone3Act3,
          Zone3Boss,
          Zone4Act1,
          Zone4Act2,
          Zone4Act3,
          Zone4Boss,
          Final,
          Max,
          None,
        }
      }
    }
}
