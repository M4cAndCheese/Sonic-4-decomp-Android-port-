// Decompiled with JetBrains decompiler
// Type: gs.backup.SSpecial
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;

namespace gs.backup
{

    public class SSpecial
    {
      private const uint c_size = 7;
      private SSpecialSolo[] m_stage = AppMain.New<SSpecialSolo>(7U);

      private SSpecialSolo getSpecialSolo(uint index) => this.m_stage[(int) index];

      private SSpecialSolo getSpecialSolo(SStage.EStage.Type index) => this.getSpecialSolo(index);

      public SSpecialSolo this[int index]
      {
        get => this.m_stage[index];
        set => this.m_stage[index] = value;
      }

      public static uint GetSize() => 7;

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            for (int index = 0; index < this.m_stage.Length; ++index)
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
            for (int index = 0; index < this.m_stage.Length; ++index)
              this.m_stage[index].setData(binaryReader.ReadBytes(40));
          }
        }
      }

      public static SSpecial CreateInstance(uint save_index)
      {
        return SBackup.CreateInstance().GetSpecial(save_index);
      }

      public static SSpecial CreateInstance() => SBackup.CreateInstance().GetSpecial();

      public void Init()
      {
        for (int index = 0; index < 7; ++index)
        {
          this.m_stage[index] = new SSpecialSolo();
          this.m_stage[index].Init();
        }
      }
    }
}
