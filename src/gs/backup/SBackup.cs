// Decompiled with JetBrains decompiler
// Type: gs.backup.SBackup
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.IO;

namespace gs.backup
{

    public class SBackup
    {
      private const uint c_save_size = 1;
      private SSystem[] m_system = AppMain.New<SSystem>(1U);
      private SOption[] m_option = AppMain.New<SOption>(1U);
      private SStage[] m_stage = AppMain.New<SStage>(1U);
      private SSpecial[] m_special = AppMain.New<SSpecial>(1U);

      public uint GetSaveIndex() => 0;

      public SSystem GetSystem(uint save_index) => this.m_system[(int) save_index];

      public SSystem GetSystem() => this.GetSystem(this.GetSaveIndex());

      public SOption GetOption(uint save_index) => this.m_option[(int) save_index];

      public SOption GetOption() => this.GetOption(this.GetSaveIndex());

      public SStage GetStage(uint save_index) => this.m_stage[(int) save_index];

      public SStage GetStage() => this.GetStage(this.GetSaveIndex());

      public SSpecial GetSpecial(uint save_index) => this.m_special[(int) save_index];

      public SSpecial GetSpecial() => this.GetSpecial(this.GetSaveIndex());

      private void SetSaveIndex(uint save_index)
      {
      }

      public static SBackup CreateInstance() => (SBackup) AppMain.GsGetMainSysInfo().backup;

      public void Init()
      {
        for (int index = 0; index < 1; ++index)
        {
          this.m_system[index] = new SSystem();
          this.m_system[index].Init();
          this.m_option[index] = new SOption();
          this.m_option[index].Init();
          this.m_stage[index] = new SStage();
          this.m_stage[index].Init();
          this.m_special[index] = new SSpecial();
          this.m_special[index].Init();
        }
      }

      public byte[] getData()
      {
        using (MemoryStream output = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          {
            for (int index = 0; index < 1; ++index)
            {
              binaryWriter.Write(this.m_system[index].getData());
              binaryWriter.Write(this.m_option[index].getData());
              binaryWriter.Write(this.m_stage[index].getData());
              binaryWriter.Write(this.m_special[index].getData());
            }
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
            for (int index = 0; index < 1; ++index)
            {
              this.m_system[index].setData(binaryReader.ReadBytes(68));
              this.m_option[index].setData(binaryReader.ReadBytes(20));
              this.m_stage[index].setData(binaryReader.ReadBytes(1428));
              this.m_special[index].setData(binaryReader.ReadBytes(280));
            }
          }
        }
      }
    }
}
