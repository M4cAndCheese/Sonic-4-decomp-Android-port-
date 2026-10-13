// Decompiled with JetBrains decompiler
// Type: GLIndexBuffer_ByteBuffer
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using mpp;

public struct GLIndexBuffer_ByteBuffer : OpenGL.GLIndexBuffer
{
    private ByteBuffer data_;

    private int dataSize_;

    public int Size
    {
        get
        {
            return dataSize_ / 2;
        }
    }

    public GLIndexBuffer_ByteBuffer(ByteBuffer data, int dataSize)
    {
        data_ = data;
        dataSize_ = dataSize;
    }

    public void ExtractTo(ushort[] dst)
    {
        int num = 0;
        for (int i = 0; i < dataSize_; i += 2)
        {
            dst[num] = data_.GetUShort(i);
            num++;
        }
    }
}
