// Decompiled with JetBrains decompiler
// Type: GLNormalData
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using mpp;
using System;

public class GLNormalData : GLVector3Data, OpenGL.GLVertexData
{
    protected OpenGL.GLVertexElementType[] compType_ = new OpenGL.GLVertexElementType[1] { OpenGL.GLVertexElementType.Normal };

    public OpenGL.GLVertexElementType[] DataComponents
    {
        get
        {
            return compType_;
        }
    }

    public GLNormalData(ByteBuffer buffer, int size, uint type, int stride, int elCount)
        : base(buffer, size, type, stride, elCount)
    {
    }

    public virtual void ExtractTo(OpenGL.Vertex[] dst, int count)
    {
        for (int i = 0; i < count; i++)
        {
            dst[i].Normal = data_[i];
        }
    }

    public void ExtractTo(OpenGL.VertexPosTexColNorm[] dst, int dstOffset, int count)
    {
        throw new InvalidOperationException();
    }
}