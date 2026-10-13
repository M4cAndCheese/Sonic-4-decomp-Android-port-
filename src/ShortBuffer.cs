// Decompiled with JetBrains decompiler
// Type: ShortBuffer
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

public class ShortBuffer : ByteBuffer
{
    public new short this[int index]
    {
        get
        {
            return GetShort(index * 2);
        }
        set
        {
            PutShort(value, index * 2);
        }
    }

    public ShortBuffer(ByteBuffer buffer)
        : base(buffer.data, buffer.offset)
    {
    }
}
