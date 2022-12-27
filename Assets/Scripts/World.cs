using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public MaterialColumn this[int i]
    {
        get { return columns[i]; }
        set { columns[i] = value; }
    }
    /// <summary>
    /// Represents the main instantiated world
    /// </summary>
    public static World main;
    /// <summary>
    /// The number of columns in this world
    /// </summary>
    public int ColumnCount = 100;
    /// <summary>
    /// The left reach of said World in world space
    /// </summary>
    public float LeftBorder = -50f;
    /// <summary>
    /// The right reach of said World in world space
    /// </summary>
    public float RightBorder = 50f;
    public float ColumnWidth => (RightBorder - LeftBorder) / ColumnCount;
    public MaterialColumn[] columns;
    public event System.Action UpdateMesh; //Simultaneously updates all affected collumns to update their mesh
    // Start is called before the first frame update
    void Start()
    {
        columns = new MaterialColumn[ColumnCount];
        if (main == null) main = this;
    }
    public void UpdateAllMesh() => UpdateMesh?.Invoke();
    /// <summary>
    /// Add a subcolumn depending on the XPos
    /// </summary>
    /// <param name="sub">The subcolumn to be added</param>
    /// <param name="keep">Whether painting should keep already existing material</param>
    public void AddSub(SubColumn sub, bool keep, float xPos)
    {
        int i = GetIndex(xPos);
        if (columns[i] == null) CreateColumn(i);
        columns[i].AddSub(sub, keep);
        columns[i - 1]?.AddToUpdate();
    }
    ///<summary>Add a subcolumn by index in the World's column array</summary>
    public void AddSub(SubColumn sub, bool keep, int index)
    {
        if (columns[index] == null) CreateColumn(index);
        columns[index].AddSub(sub, keep);
        if((index - 1) >= 0)columns[index - 1]?.AddToUpdate();
    }
    public void EraseSub(SubColumn sub, float xPos)
    {
        int i = GetIndex(xPos);
        if (columns[i] == null) CreateColumn(i);
        columns[i].Erase(sub);
        columns[i - 1]?.AddToUpdate();
    }
    public void EraseSub(SubColumn sub, int index)
    {
        if (columns[index] == null) CreateColumn(index);
        columns[index].Erase(sub);
        columns[index - 1]?.AddToUpdate();
    }
    
    public int GetIndex(float xPos) => Mathf.RoundToInt(((xPos - LeftBorder) / (RightBorder - LeftBorder)) * ColumnCount);
    public float GetColumnPos(int index) => Mathf.Lerp(LeftBorder, RightBorder, (float)index / ColumnCount);
    public void CreateColumn(int index)
    {
        GameObject gb = new GameObject(index.ToString(), typeof(MaterialColumn));
        gb.transform.parent = this.transform;
        gb.transform.position = new Vector2(Mathf.Lerp(LeftBorder,RightBorder, (float)index / ColumnCount), 0);
        columns[index] = gb.GetComponent<MaterialColumn>();
        columns[index].index = index;
        columns[index].world = this;
    }
    public void DeleteColumn(int index)
    {
        columns[index] = null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        if (main == this) main = null;
    }
}
