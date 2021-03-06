using System;
using System.Collections.Generic;
using System.Dynamic;
using OSwizzling;
using UnityEngine;
public static class UNoise
{
    #region Random
    static readonly Vector3 s_RandomVec = new Vector3(12.0909f,89.233f,37.719f);
    static readonly float s_RandomValue = 143758.5453f;
    static float ValueUnit(float _dotValue) => UMath.Frac(Mathf.Sin(_dotValue) * s_RandomValue);
    public static float ValueUnit(float _x, float _y) => ValueUnit(new Vector2(_x, _y));
    public static float ValueUnit(float _x, float _y,float _z) => ValueUnit(new Vector3(_x, _y,_z));
    public static float ValueUnit(Vector2 _random) => ValueUnit(Vector2.Dot(_random, s_RandomVec));
    public static float ValueUnit(Vector3 _random) => ValueUnit(Vector3.Dot(_random, s_RandomVec));
    public static Vector2 VelueUnitVec2(Vector2 _random) => new Vector2(ValueUnit(new Vector2(_random.y, _random.x)), ValueUnit(_random));
    #endregion
    #region Perlin
    static readonly int[] s_PerlinPermutation = { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 };
    static readonly int[] s_PerlinPremutationRepeat = s_PerlinPermutation.Add(s_PerlinPermutation);
    static int Inc(int src) => (src + 1) % 255;
    static float Lerp(float a, float b, float x) => a + x * (b - a);
    static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    static float Gradient(int hash, float x, float y, float z)
    {
        switch (hash & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: throw new Exception("Invalid Gradient Result Here!");
        }
    }
    public static float PerlinUnit(float x, float y, float z)
    {
        int xi = (int)x & 255;
        int yi = (int)y & 255;
        int zi = (int)z & 255;
        float xf = x - (int)x;
        float yf = y - (int)y;
        float zf = z - (int)z;
        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);
        int[] p = s_PerlinPremutationRepeat;
        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[xi] + yi] + zi];
        aba = p[p[p[xi] + Inc(yi)] + zi];
        aab = p[p[p[xi] + yi] + Inc(zi)];
        abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
        baa = p[p[p[Inc(xi)] + yi] + zi];
        bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
        bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
        bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];
        float x1, x2, y1, y2;
        x1 = Lerp(Gradient(aaa, xf, yf, zf), Gradient(baa, xf - 1, yf, zf), u);
        x2 = Lerp(Gradient(aba, xf, yf - 1, zf), Gradient(bba, xf - 1, yf - 1, zf), u);
        y1 = Lerp(x1, x2, v);
        x1 = Lerp(Gradient(aab, xf, yf, zf - 1), Gradient(bab, xf - 1, yf, zf - 1), u);
        x2 = Lerp(Gradient(abb, xf, yf - 1, zf - 1), Gradient(bbb, xf - 1, yf - 1, zf - 1), u);
        y2 = Lerp(x1, x2, v);
        return Lerp(y1, y2, w);
    }
    #endregion
    #region Simplex
    const float c_Mod289 = 1.0f / 290f;
    static readonly float s_Sqrt3 = Mathf.Sqrt(3f);
    static readonly Vector4 s_Simplex_C = new Vector4((3f - s_Sqrt3) / 6f, (s_Sqrt3 - 1f) * .5f, -1f + 2f * ((3f - s_Sqrt3) / 6f), 1f / 41f);
    static readonly Vector3 s_Simplex_M1 = 1.79284291400159f.ToVector3();
    static readonly float s_Simplex_M2 = 0.85373472095314f;
    static float Mod289(float _x) { return _x - Mathf.Floor(_x * c_Mod289) * 289f; }
    static Vector2 Mod289(Vector2 _vec) => new Vector2(Mod289(_vec.x),Mod289(_vec.y));
    static Vector3 Mod289(Vector3 _vec) => new Vector3(Mod289(_vec.x), Mod289(_vec.y), Mod289(_vec.z));
    static Vector3 Permute(Vector3 vec) { return Mod289((vec * 34f + 1f.ToVector3()).Multiply(vec)); }
    public static float Simplex(float _x, float _y) => SimplexUnit(new Vector2(_x,_y));
    public static float SimplexUnit(Vector2 _v)
    {
        Vector2 i = Swizzling.Floor(_v + Vector2.Dot(_v, s_Simplex_C.y.ToVector2()).ToVector2());
        Vector2 x0 = _v - i + Vector2.Dot(i, s_Simplex_C.x.ToVector2()).ToVector2();
        Vector2 i1 = x0.x > x0.y ? new Vector2(1, 0) : new Vector2(0, 1);
        Vector4 x12 = new Vector4(x0.x, x0.y, x0.x, x0.y) + new Vector4(s_Simplex_C.x, s_Simplex_C.x, s_Simplex_C.z, s_Simplex_C.z);
        x12-=i1.ToVector4();
        i = Mod289(i);
        Vector3 p = Permute(Permute(new Vector3(i.y,i.y+i1.y,i.y+1))+new Vector3(i.x,i.x+i1.x,i.x+1));
        Vector2 x12xy = new Vector2(x12.x, x12.y);
        Vector2 x12zw = new Vector2(x12.z, x12.w);
        Vector3 m = Vector3.Max(0.5f.ToVector3()-new Vector3(Vector2.Dot(x0,x0),Vector2.Dot(x12xy,x12xy),Vector2.Dot(x12zw,x12zw)) ,Vector3.zero);
        m=m.Multiply(m);
        m = m.Multiply(m);
        Vector3 x = 2.0f * Swizzling.Frac(p*s_Simplex_C.w)-1.0f.ToVector3();
        Vector3 h = Swizzling.Abs(x)-0.5f.ToVector3();
        Vector3 ox = Swizzling.Floor(x + 0.5f.ToVector3());
        Vector3 a0 = x - ox;
        m = m.Multiply(s_Simplex_M1 - s_Simplex_M2 * (a0.Multiply(a0) + h.Multiply(h)));
        float gx = a0.x*x0.x+h.x*x0.y;
        Vector2 gyz = new Vector2(a0.y,a0.z)*new Vector2(x12.x,x12.z)+new Vector2(h.y,h.z)*new Vector2(x12.y,x12.w);
        return 130f * Vector3.Dot(m,new Vector3(gx,gyz.x,gyz.y));
    }
    #endregion
    #region Voronoi
    public static Vector2 VoronoiUnit(float _x, float _y) => VoronoiUnit(new Vector2( _x, _y));
    public  static Vector2 VoronoiUnit(Vector2 _v)
    {
        float sqrDstToCell = float.MaxValue;
        Vector2 baseCell = Swizzling.Floor(_v);
        Vector2 closetCell = baseCell;
        for (int i=-1;i<=1;i++)
            for(int j=-1;j<=1;j++)
            {
                Vector2 cell = baseCell+new Vector2(i,j);
                Vector2 cellPos = cell + VelueUnitVec2(cell);
                Vector2 toCell = cellPos - _v;
                float sqrDistance = toCell.sqrMagnitude;
                if (sqrDstToCell < sqrDistance)
                    continue;

                sqrDstToCell = sqrDistance;
                closetCell = cell;
            }
        return new Vector2(sqrDstToCell,  ValueUnit(closetCell) );
    }
    #endregion
}