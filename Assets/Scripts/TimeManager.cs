using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int i;
    [RuntimeInitializeOnLoadMethod]
    static void SelfLoad()
    {
        new GameObject("Time Manager").AddComponent(typeof(TimeManager));
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            i = 0;
            Time.timeScale = 1;
        }
    }
    private void FixedUpdate()
    {
        if(i >= 59)
        {
            Time.timeScale = 0;
        }
        i++;
    }
}
