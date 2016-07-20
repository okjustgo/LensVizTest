// Vector3Extensions.cs
// C#
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector2 xy(this Vector3 aVector)
    {
        return new Vector2(aVector.x, aVector.y);
    }
    public static Vector2 xz(this Vector3 aVector)
    {
        return new Vector2(aVector.x, aVector.z);
    }
    public static Vector2 yz(this Vector3 aVector)
    {
        return new Vector2(aVector.y, aVector.z);
    }
    public static Vector2 yx(this Vector3 aVector)
    {
        return new Vector2(aVector.y, aVector.x);
    }
    public static Vector2 zx(this Vector3 aVector)
    {
        return new Vector2(aVector.z, aVector.x);
    }
    public static Vector2 zy(this Vector3 aVector)
    {
        return new Vector2(aVector.z, aVector.y);
    }
}