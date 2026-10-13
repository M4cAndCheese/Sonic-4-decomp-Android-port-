// Decompiled with JetBrains decompiler
// Type: FloatBuffer
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

public class FloatBuffer : ByteBuffer
{
    public new float this[int index]
    {
        get
        {
            return GetFloat(index * 4);
        }
        set
        {
            PutFloat(value, index * 4);
        }
    }

    public FloatBuffer(ByteBuffer buffer)
        : base(buffer.data, buffer.offset)
    {
    }
}
