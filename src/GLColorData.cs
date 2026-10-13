// Decompiled with JetBrains decompiler
// Type: GLColorData
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using Microsoft.Xna.Framework;
using mpp;
using System;

public class GLColorData : OpenGL.GLVertexData
{
  protected readonly Color[] data_;
  protected OpenGL.GLVertexElementType[] compType_ = new OpenGL.GLVertexElementType[1]
  {
    OpenGL.GLVertexElementType.Color
  };

  public GLColorData(ByteBuffer buffer, int size, uint type, int stride, int elCount)
  {
    stride = stride == 0 ? OpenGL.SizeOf(type) * size : stride;
    this.data_ = new Color[elCount];
    switch (type)
    {
      case 5121:
        if (size > 3)
        {
          this.extract4ByteData(buffer, stride);
          break;
        }
        this.extract3ByteData(buffer, stride);
        break;
      case 5123:
        throw new NotImplementedException();
      case 5126:
        throw new NotImplementedException();
    }
  }

  public OpenGL.GLVertexElementType[] DataComponents => this.compType_;

  public int VertexCount => this.data_.Length;

  public void ExtractTo(OpenGL.Vertex[] dst, int count)
  {
    for (int index = 0; index < count; ++index)
      dst[index].Color = this.data_[index];
  }

  public void ExtractTo(OpenGL.VertexPosTexColNorm[] dst, int dstOffset, int count)
  {
    for (int index = 0; index < count; ++index)
      dst[index + dstOffset].Color = this.data_[index];
  }

  private void extract4ByteData(ByteBuffer buffer, int stride)
  {
    int length = this.data_.Length;
    int i = 0;
    for (int index = 0; index < length; ++index)
    {
      byte num1 = buffer[i];
      byte num2 = buffer[i + 1];
      byte num3 = buffer[i + 2];
      byte num4 = buffer[i + 3];
      this.data_[index] = new Color((int) num1, (int) num2, (int) num3, (int) num4);
      i += stride;
    }
  }

  private void extract3ByteData(ByteBuffer buffer, int stride)
  {
    int length = this.data_.Length;
    int i = 0;
    for (int index = 0; index < length; ++index)
    {
      byte num1 = buffer[i];
      byte num2 = buffer[i + 1];
      byte num3 = buffer[i + 2];
      this.data_[index] = new Color((int) num1, (int) num2, (int) num3);
      i += stride;
    }
  }
}
