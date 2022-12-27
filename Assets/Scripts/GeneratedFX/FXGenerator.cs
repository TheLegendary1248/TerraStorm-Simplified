using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FXGenerator : MonoBehaviour
{
    public int framesAlive;
    public int lifetime;
    public MeshRenderer render;
    public MeshFilter filter;
    public Mesh mesh;
    object[] props;
    /// <summary>
    /// Use this method to generate the effect with some initialized values
    /// </summary>
    /// <param name="props"></param>
    public virtual void Initialize(object[] props)
    {
        this.props = props;
    }
    /// <summary>
    /// Use this method to alter the effect with given properties
    /// </summary>
    /// <param name="props"></param>
    public virtual void Alter(object[] props)
    {

    }
    public void Start()
    {
        filter = gameObject.AddComponent<MeshFilter>();
        render = gameObject.AddComponent<MeshRenderer>();
        filter.mesh = mesh = new Mesh();
    }
    public virtual void FixedUpdate() { }
    static void Generate()
    {

    }
    private void OnDestroy()
    {
        Destroy(mesh);
    }
}
