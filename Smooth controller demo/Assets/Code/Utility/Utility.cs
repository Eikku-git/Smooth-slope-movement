using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {

    public static Mesh CloneMesh(Mesh mesh) {
        Mesh meshClone = new Mesh();
        meshClone.vertices = mesh.vertices;
        meshClone.triangles = mesh.triangles;
        meshClone.uv = mesh.uv;
        meshClone.uv2 = mesh.uv2;
        meshClone.normals = mesh.normals;
        meshClone.tangents = mesh.tangents;
        return meshClone;
    }

    public static int BitToInt(bool[] bits) {
        int result = 0;
        for (int i = 0; i < bits.Length; i++) {
            result += (bits[i] ? 1 : 0) * (int)Math.Pow(2, i);
        }
        return result;
    }

    public static string RemoveStringSpaces(string str) {
        if (str == null) { return null; }
        string result = null;
        for (int i = 0; i < str.Length; i++) {
            if (str[i] !=  ' ') {
                result += str[i];
            }
        }
        return result;
    }

    private static readonly System.Random random = new();

    public static int RandomizeNew((int, int) range, int previous) {
        int dir = (random.Next() < int.MaxValue / 2 && previous != 0) || previous == range.Item2 - 1 ? -1 : 1;
        return random.Next(dir == -1 ? range.Item1 : previous + 1, dir == 1 ? range.Item2 : previous);
    }
}
