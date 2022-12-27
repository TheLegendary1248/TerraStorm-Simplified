using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSplash : FXGenerator
{
    public override void FixedUpdate()
    {
        framesAlive++;
        mesh.Clear();
        Vector3[] verts = new Vector3[17];
        verts[16] = Vector2.zero;
        int[] tris = new int[16 * 3];
        for (int i = 0; i < 16; i++)
        {
            float ang = (i / 16f) * (Mathf.PI * 2);
            float magnitude = (i % 2 == 1 ? 3 : 1) * (Mathf.PerlinNoise(Time.time * 7.5f + i, Time.time) + 0.5f);
            float angOffset = Mathf.PerlinNoise(Time.time * 3.5f + i, Time.time) * 0.8f;
            Vector2 normal = new Vector2(Mathf.Sin(ang + angOffset) * magnitude, Mathf.Cos(ang + angOffset) * magnitude);
            verts[i] = normal;
            tris[i * 3] = i;
            tris[(i * 3) + 1] = (i + 1) % 16;
            tris[(i * 3) + 2] = 16;
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.UploadMeshData(false);
        if (framesAlive >= lifetime) Destroy(gameObject);
    }

}
