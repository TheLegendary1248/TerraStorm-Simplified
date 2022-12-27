using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class is used to contain public properties about an entity.
/// Use this for properties of an object that are meant to be affected by gameplay
/// </summary>
public class Property : MonoBehaviour
{
    public Dictionary<int, int> props = new Dictionary<int, int>();
    /// <summary>
    /// Gets the property with the name given      
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public static int GetProperty(string i)
    {
        return 0;
    }
    /// <summary>
    /// Registers a property with the name given  
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static int RegisterProperty(string prop)
    {
        return 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
