using System;
using UnityEngine;

[Serializable]
public struct SerializableMatrix4x4
{
    public float[] values;

    public static SerializableMatrix4x4 FromMatrix(Matrix4x4 m)
    {
        return new SerializableMatrix4x4
        {
            values = new float[]
            {
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33
            }
        };
    }

    public Matrix4x4 ToMatrix()
    {
        Matrix4x4 m = new Matrix4x4();
        m.m00 = values[0]; m.m01 = values[1]; m.m02 = values[2]; m.m03 = values[3];
        m.m10 = values[4]; m.m11 = values[5]; m.m12 = values[6]; m.m13 = values[7];
        m.m20 = values[8]; m.m21 = values[9]; m.m22 = values[10]; m.m23 = values[11];
        m.m30 = values[12]; m.m31 = values[13]; m.m32 = values[14]; m.m33 = values[15];
        return m;
    }
}
