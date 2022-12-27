using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class MeshColor : MonoBehaviour
{
    public MeshRenderer render;
    public Color[] colors;
    private void OnValidate()
    {
        if (!render) render = GetComponent<MeshRenderer>();
        for (int i = 0; i < colors.Length && i < render.sharedMaterials.Length; i++)
        {
            if (render.sharedMaterials[i].GetInstanceID() > 0) render.sharedMaterials[i] = new Material(render.sharedMaterials[i]);
            render.sharedMaterials[i].color = colors[i];
        }
    }
    private void OnDestroy()
    {
        for (int i = 0; i < render.materials.Length; i++)
        {
            Destroy(render.materials[i]);
        }
    }
}
