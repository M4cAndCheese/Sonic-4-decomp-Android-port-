// Decompiled with JetBrains decompiler
// Type: UIntBuffer
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

public class UIntBuffer : ByteBuffer
{
    public new uint this[int index]
    {
        get
        {
            return GetUInt(index * 4);
        }
        set
        {
            PutUInt(value, index * 4);
        }
    }

    public UIntBuffer(ByteBuffer buffer)
        : base(buffer.data, buffer.offset)
    {
    }
}
