using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Painter : MonoBehaviour
{
    public float penSize = 0.2f;
    public int mat = 0;
    public bool erase = false;
    public bool continous = false;
    private void Update()
    {
        if (continous ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
        {
            Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CirclePen(v);
            //World.main.AddSub(new SubColumn(v.y - (penSize / 2), v.y + (penSize / 2), 0), false, v.x);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            penSize += Time.unscaledDeltaTime * penSize;
        }
        else if(Input.GetKey(KeyCode.E))
        {
            penSize -= Time.unscaledDeltaTime * penSize;
        }
        if (Input.GetKeyDown(KeyCode.Space)) erase = !erase;
        if (Input.GetKeyDown(KeyCode.UpArrow)) mat++;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) mat--;
        if (Input.GetKeyDown(KeyCode.C)) continous = !continous;
            
    }
    /// <summary>
    /// Draws on the world with a circle using an algorithm
    /// </summary>
    /// <param name="pos"></param>
    
    void CirclePen(Vector2 pos)
    {
        //Debug.Log("START CIRCLE");
        World w = World.main;
        float startRight;
        int leftIndex = w.GetIndex(pos.x);
        float startLeft = w.GetColumnPos(leftIndex);
        if (startLeft > pos.x) { startRight = startLeft; startLeft = startRight - w.ColumnWidth; leftIndex--; }
        else startRight = startLeft + w.ColumnWidth;
        float leftDif = pos.x - startLeft;
        float rightDif = startRight - pos.x;
        int rad = Mathf.CeilToInt(penSize * 6);//The radius of the circle in steps
        //Draw towards the left
        for (int i = 0; i < rad; i++)
        {
            Profiler.BeginSample($"Left {i}");
            int index = leftIndex - i;
            if (index < 0 || index > w.ColumnCount) { Profiler.EndSample(); continue;  }
            float dif = leftDif + (i * w.ColumnWidth);
            //Debug.Log($"LEFT, {dif}, {index}");
            if (dif > penSize) { Profiler.EndSample(); break; }//No more circle left
            float vertChord = Mathf.Sqrt((penSize * penSize) - (dif * dif));
            if (erase) w.EraseSub(new SubColumn(pos.y - vertChord, pos.y + vertChord, 0), index);
            else w.AddSub(new SubColumn(pos.y - vertChord, pos.y + vertChord, 0), false, index);
            Profiler.EndSample();
                
        }
        
        //Draw towards the right
        for (int i = 0; i < rad; i++)
        {
            Profiler.BeginSample($"Right {i}");
            int index = leftIndex + i + 1;
            if (index < 0 || index > w.ColumnCount) { Profiler.EndSample(); continue;  }
            float dif = rightDif + (i * w.ColumnWidth);
            //Debug.Log($"RIGHT, {dif}, {index}");
            if (dif > penSize) { Profiler.EndSample(); break;  } //No more circle left
            float vertChord = Mathf.Sqrt((penSize * penSize) - (dif * dif));
            if (erase) w.EraseSub(new SubColumn(pos.y - vertChord, pos.y + vertChord, 0), index);
            else w.AddSub(new SubColumn(pos.y - vertChord, pos.y + vertChord, 0), false, index);
            Profiler.EndSample();
        }
        w.UpdateAllMesh();
        
    }
}
