using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MaterialColumn : MonoBehaviour
{
    public SubColumn this[int i]
    {
        get { return materials[i]; }
        set { materials[i] = value; }
    }
    Mesh mesh;
    MeshFilter mFilter;
    MeshRenderer render;
    public List<SubColumn> materials = new List<SubColumn>();
    int[] updateSubs;
    public int index;
    public World world;
    bool markedToUpdate = false;
    List<PolygonCollider2D> body = new List<PolygonCollider2D>();
    private void Awake()
    {
        mFilter = GetComponent<MeshFilter>();
        render = GetComponent<MeshRenderer>();
        //TEST MESH
        mesh = new Mesh();
        
        mFilter.mesh = mesh;
        render.material = (Material)Resources.Load("Materials/Default");
    }
    /// <summary>
    /// Adds a sub of material in the column
    /// </summary>
    /// <param name="sub">The sub to be added</param>
    /// <param name="keep">Whether already existing material that overlaps will be kept</param>
    public void AddSub(SubColumn sub, bool keep)
    {
        if (materials.Count == 0) { materials.Add(sub); if (!markedToUpdate) world.UpdateMesh += RebuildMesh; return; }
        int i = 0; //Will be used as the lower bound
        int k; //Higher bound           
        //Binary Search for a location first


        //SEARCH FOR SPOT ALGORITHM
        bool higherIsAdded = false;
        SubColumn lower = sub;
        SubColumn higher = sub;
        for (; i < materials.Count; i++)
        {
            lower = materials[i];
            if (lower.Min > sub.Min) //If our min is lower than the other's min
            {
                //Create the index iterator for the following 'for'
                k = i;
                //Check if we're not at start of list
                if (i != 0)
                    if (materials[i - 1].Max > sub.Min) { lower = materials[--i]; k = i; }  //If we overlap the previous sub. Reduce 'i' so we make sure to delete it in RemoveRange
                    else lower = sub; //If not, the lower sub is ours
                else lower = sub;
                //This checks overlap with the following subs
                for (; k < materials.Count; k++)
                {
                    higher = materials[k];
                    if (higher.Max > sub.Max) //The checked sub is higher than the added one
                    {
                        if (higher.Min >= sub.Max) //Whether the higher checked sub doesn't overlap ours
                        { higher = sub; higherIsAdded = true; }
                        else //Else it does
                        { k++; }
                        break;
                    }
                }
                //If we reached the end of the list and the higher sub column is the add one
                if (k == materials.Count) if (sub.Max >= higher.Max) { higher = sub; higherIsAdded = false; }
                //ADDITION ALGORITHM
                goto skip;
                #pragma warning disable CS0162 // Unreachable code detected
                if (keep)
                #pragma warning restore CS0162 // Unreachable code detected
                {
                    
                }
                else
                {
                    bool lowerIsSame = materials[i].MaterialID == sub.MaterialID;
                    bool higherIsSame = materials[higherIsAdded ? k : k - 1].MaterialID == sub.MaterialID;
                    //Every material in range is going to be either destroyed or modified so adios
                    
                    materials.RemoveRange(i, k - i);
                }
                skip:
                materials.RemoveRange(i, k - i);
                materials.Insert(i, new SubColumn(lower.Min, higher.Max, 1));
                if (!markedToUpdate) world.UpdateMesh += RebuildMesh;
                return; //lala
            }
        }
        //The added sub reaches higher than all of them 
        SubColumn topmost = materials[materials.Count - 1];
        if (topmost.Max > sub.Min) //If there's overlap with the topmost column
        {
            if (topmost.Max > sub.Max) { } //If the topmost column completely covers ours
            else { materials[materials.Count - 1] = new SubColumn(topmost.Min, sub.Max, 0); }
        }
        else materials.Add(sub);
        if(!markedToUpdate)world.UpdateMesh += RebuildMesh;
    }
    public void AddToUpdate() { if (!markedToUpdate) world.UpdateMesh += RebuildMesh; }
    public void RebuildMesh() //Ensure to deal with collider here as well. TRY TO NOT USE HARDCODED NUMBERS
    {
        if (materials.Count == 0)
        {
            world.UpdateMesh -= RebuildMesh;
            return;
        }
        /* General Schema for the 5-vert per connection concept
         * 
         *    / 4 \        4 - The fifth vert that connects two overlaps on the same sub
         *   /     \
         *  3 - - - 1       1 and 0 are the right side verts for the sub min and max
         *  |       |
         *  |  (2)  |       (2) represents the midpoint in case there's no overlap
         *  |       |
         *  2 - - - 0       2 and 3 are the left side verts for the sub min and max
         */
        float half = world.ColumnWidth / 2f; //Half Column width
        float full = world.ColumnWidth; //Full Column width
        float midPt(float x, float y) => Mathf.Lerp(x, y, 0.5f);
        mesh.Clear();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        int bodyIndex = 0;
        void AddPart(Vector2[] pts)
        {
            PolygonCollider2D part;
            if (bodyIndex >= body.Count)
            {
                part = (PolygonCollider2D)gameObject.AddComponent(typeof(PolygonCollider2D));
                body.Add(part);
            }
            else part = body[bodyIndex];
            part.points = pts;
            bodyIndex++;
        }
        //goto skip; //Skips to blocky mesh build (prototypal)
        //goto five: //Skips to five-vert idea
        if (index < world.ColumnCount - 1 && world[index + 1] != null && world[index + 1].materials.Count != 0) //OPTIMIZE THISSSSSSSSS(or at least maybe rewrite it in C/C++)
        { 
            MaterialColumn neighbor = world[index + 1];
            List<SubColumn> theyMaterials = neighbor.materials;
            int theyIndex = 0; //Index on their materials
            int ourIndex = 0; //Index on our materials
             
            int total = 0; //Total 5-pairs. Will be incremented by five
            SubColumn our = materials[ourIndex];
            SubColumn they = theyMaterials[theyIndex];
            bool ourChanged = true; //Indicates if our index changed in an iteration, otherwise no
            bool overlapped = false;
            int failOverlap = 1; //If at 3 or above, the last incremented side requires a single triangle for the previous iteration
            //Debug.Log($"Building mesh for column {index}");
            
            while (true)
            {
                //Debug.Log($"Indices {ourIndex} : {theyIndex} | [{our.Min}, {our.Max}], [{they.Min}, {they.Max}]");
                //Overlap check
                if (our.Min < they.Max & they.Min < our.Max) //Hey, we have overlap
                {
                    //Debug.Log("Overlap detected");
                    
                    if(failOverlap >= 2)//In the case that we failed overlap twice, don't forget to build a triangle for the previous sub
                    {
                        //Debug.Log($"Building single triangle, for our? {ourChanged}");
                        SubColumn single;
                        if (ourChanged)
                        {
                            single = materials[ourIndex - 1];
                            AddPart(new Vector2[] { new Vector2(0, single.Min), new Vector2(0, single.Max), new Vector2(half, midPt(single.Max, single.Min))});
                            verts.AddRange(new Vector3[] { new Vector2(0, single.Min), new Vector2(0, single.Max), new Vector2(half, midPt(single.Max, single.Min)), Vector2.zero });
                            uv.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(half, 0.5f), Vector2.zero });
                            tris.AddRange(new int[] { total, total + 1, total + 2 });
                        }
                        else
                        {
                            single = theyMaterials[theyIndex - 1];
                            AddPart(new Vector2[] { new Vector2(full, single.Min), new Vector2(full, single.Max), new Vector2(half, midPt(single.Max, single.Min)) });
                            verts.AddRange(new Vector3[] { new Vector2(full, single.Min), new Vector2(full, single.Max), new Vector2(half, midPt(single.Max, single.Min)), Vector2.zero });
                            uv.AddRange(new Vector2[] { new Vector2(full, 0), new Vector2(full, 1), new Vector2(half, 0.5f), Vector2.zero });
                            tris.AddRange(new int[] { total, total + 1, total + 2 });
                        }
                        total += 4;
                    }
                    float mid = 0f;
                    byte indicator = 0;
                    if (overlapped)//If the previous check did overlap
                    {
                        //Debug.Log($"Overlapped previously, set midpt at our? {ourChanged}");
                        if (ourChanged)//If we incremented last
                        {
                            
                            mid = midPt(our.Min, materials[ourIndex - 1].Max);
                            verts[verts.Count - 3] = new Vector3(full, mid);
                            Vector2[] arr = body[bodyIndex - 1].points;
                            arr[1] = new Vector2(full, mid);
                            body[bodyIndex - 1].points = arr;
                            indicator = 1;
                        }
                        else //If they incremented last
                        {
                            mid = midPt(they.Min, theyMaterials[theyIndex - 1].Max);
                            verts[verts.Count - 1] = new Vector2(0, mid);
                            Vector2[] arr = body[bodyIndex - 1].points;
                            arr[2] = new Vector2(0, mid);
                            body[bodyIndex - 1].points = arr;
                            indicator = 2;
                        }
                    }
                    AddPart(new Vector2[] { new Vector2(full, indicator == 1 ? mid : they.Min), new Vector2(full, they.Max), new Vector2(0, our.Max), new Vector2(0, indicator == 2 ? mid : our.Min) });
                    verts.AddRange(new Vector3[] { new Vector2(full, indicator == 1 ? mid : they.Min), new Vector2(full, they.Max), new Vector2(0, indicator == 2 ? mid : our.Min), new Vector2(0, our.Max) });
                    uv.AddRange(new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1) });
                    tris.AddRange(new int[] { total, total + 1, total + 2, total + 1, total + 2, total + 3 });
                    total += 4;
                    overlapped = true;
                    failOverlap = 0;
                }
                else //No overlap
                {
                    //Debug.Log("Fail overlap", gameObject);
                    failOverlap++;
                    if(failOverlap >= 3) //Build a single triangle for the previous incremented side
                    {
                        //Debug.Log($"Building single triangle, for our? {ourChanged}");
                        SubColumn single;
                        if (ourChanged)
                        {
                            single = materials[ourIndex - 1];
                            AddPart(new Vector2[] { new Vector2(0, single.Min), new Vector2(0, single.Max), new Vector2(half, midPt(single.Max, single.Min)) });
                            verts.AddRange(new Vector3[] { new Vector2(0, single.Min), new Vector2(0, single.Max), new Vector2(half, midPt(single.Max, single.Min)), Vector2.zero });
                            uv.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(half, 0.5f), Vector2.zero });
                            tris.AddRange(new int[] { total, total + 1, total + 2 });
                        }
                        else
                        {
                            single = theyMaterials[theyIndex - 1];
                            AddPart(new Vector2[] { new Vector2(full, single.Min), new Vector2(full, single.Max), new Vector2(half, midPt(single.Max, single.Min)) });
                            verts.AddRange(new Vector3[] { new Vector2(full, single.Min), new Vector2(full, single.Max), new Vector2(half, midPt(single.Max, single.Min)), Vector2.zero });
                            uv.AddRange(new Vector2[] { new Vector2(full, 0), new Vector2(full, 1), new Vector2(half, 0.5f), Vector2.zero });
                            tris.AddRange(new int[] { total, total + 1, total + 2 });
                        }
                        total += 4;
                    }
                    overlapped = false;
                }
                //Increment stage
                if (our.Max < they.Max) //Our is lower, so increment our index
                {
                    ourChanged = true;
                    ourIndex++;
                    if (ourIndex >= materials.Count) break;
                    else our = materials[ourIndex]; 
                }
                else //They are lower, so increment their index
                {
                    ourChanged = false;
                    theyIndex++;
                    if (theyIndex >= theyMaterials.Count) break;
                    else they = theyMaterials[theyIndex];
                }
            }
            //If we broke out of the while loop from ourIndex being larger than our list(aka no more subs on our side)
            //Debug.Log("Work on remaining tris");
            if (!overlapped) { --theyIndex; --ourIndex; }
            if(ourChanged) for (int i = ++theyIndex; i < theyMaterials.Count; i++) //Then work on their remaining subs
                {
                    //Debug.Log($"Doing {i}");
                    they = theyMaterials[i];
                    AddPart(new Vector2[] { new Vector2(full, they.Min), new Vector2(full, they.Max), new Vector2(half, midPt(they.Max, they.Min)) });
                    verts.AddRange(new Vector3[] { new Vector2(full, they.Min), new Vector2(full, they.Max), new Vector2(half, midPt(they.Max, they.Min)), Vector2.zero});
                    uv.AddRange(new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero });
                    tris.AddRange(new int[] { total, total + 1, total + 2 });
                    total += 4;
                }
            else for (int i = ++ourIndex; i < materials.Count; i++) //Otherwise ours
                {
                    //Debug.Log($"Doing {i}");
                    our = materials[i];
                    AddPart(new Vector2[] { new Vector2(0, our.Min), new Vector2(0, our.Max), new Vector2(half, midPt(our.Max, our.Min)) });
                    verts.AddRange(new Vector3[] { new Vector2(0, our.Min), new Vector2(0, our.Max), new Vector2(half, midPt(our.Max, our.Min)), Vector2.zero });
                    uv.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f), Vector2.zero});
                    tris.AddRange(new int[] { total, total + 1, total + 2 });
                    total += 4;
                }
        }
        else //If there's nothing to look forward to, just make some triangles
        {
            //Debug.Log($"nothing to look forward to");
            for (int i = 0; i < materials.Count; i++)
            {
                SubColumn s = materials[i];
                AddPart(new Vector2[] { new Vector2(0, s.Min), new Vector2(0, s.Max), new Vector2(half, Mathf.Lerp(s.Max, s.Min, 0.5f)) });
                verts.AddRange(new Vector3[] { new Vector2(0, s.Min), new Vector2(0, s.Max), new Vector2(half, Mathf.Lerp(s.Max, s.Min, 0.5f)) });
                uv.AddRange(new Vector2[] { new Vector2(0f, 0), new Vector2(0f, 1), new Vector2(half, 0.5f) });
                tris.AddRange(new int[] { (i * 3), (i * 3) + 1, (i * 3) + 2 });
            }
        }
        for (int i = bodyIndex; i < body.Count; i++) //Disable any attached polygon coll
        {
            Destroy(body[i]);
        }
        body.RemoveRange(bodyIndex, body.Count - bodyIndex);
        goto pass;
        /*
         //----FIVE VERTEX CONCEPT-----//
        if (index < world.ColumnCount - 1 && world[index + 1] != null) //OPTIMIZE THISSSSSSSSS(or at least maybe rewrite it in C/C++)
        {
            MaterialColumn neighbor = world[index + 1];
            int they = 0; //Index on their materials
            int total = 0; //Total 5-pairs. Will be incremented by five
            for (int my = 0; my < materials.Count; my++)
            {
                SubColumn our = materials[my];
                SubColumn next = neighbor.materials[they];
                while (next.Max < our.Min) //While the neighbor's columns don't overlap and are below. Create single triangles for these
                {
                    verts.AddRange(new Vector3[] { new Vector2(full, next.Min), new Vector2(full, next.Max), new Vector2(half, midPt(next.Max, next.Min)), Vector3.zero, Vector3.zero});
                    uv.AddRange(new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero });
                    tris.AddRange(new int[]{ total, total + 1, total + 2});
                    total += 5;
                    if (++they >= neighbor.materials.Count) break; //Reached end of neighbor's sub list
                    next = neighbor.materials[they];
                }
                bool overlapped = false; //Used to detect if we overlapped more than once, hence stitch them together
                while (next.Min <= our.Max) //While the neighbor's columns DO overlap {PROBABLY WRITE A FOR LOOP HERE TO GET MULTIPLE OVERLAPS}
                {
                    if (!overlapped)
                    {
                        verts.AddRange(new Vector3[] { new Vector2(full, next.Min), new Vector2(full, next.Max), new Vector2(0, our.Min), new Vector2(0, our.Max), Vector2.zero });
                        uv.AddRange(new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1), Vector2.zero });
                        tris.AddRange(new int[] { total, total + 1, total + 2, total + 1, total + 2, total + 3 });
                        overlapped = true; 
                    }
                    else
                    {
                        float mid = midPt(next.Min, verts[verts.Count - 4].y);
                        verts[verts.Count - 2] = new Vector3(0, mid); //We need to go back and change the our max vector to a mid point
                        verts[verts.Count - 1] = new Vector3(half, mid);
                        //Make sure to add that those triangles from earlier to stitch them together
                        //                      / Lower Triangle Stitch - - - - / Higher Triangle Stitch
                        tris.AddRange(new int[] { total - 1, total - 2, total - 4, total - 1, total - 2});
                        verts.AddRange(new Vector3[] { new Vector2(full, next.Min), new Vector2(full, next.Max), new Vector2(0, our.Min), new Vector2(0, our.Max), Vector2.zero });
                    }
                    total += 5;
                    if (++they >= neighbor.materials.Count) break; //Reached end of neighbor's sub list
                    next = neighbor.materials[they];
                }
            }
        }
        else //If there's nothing to look forward to, just make some triangles
        {
            for (int i = 0; i < materials.Count; i++)
            {
                SubColumn s = materials[i];
                verts.AddRange(new Vector3[] { new Vector2(0, s.Min), new Vector2(0, s.Max), new Vector2(0.1f, Mathf.Lerp(s.Max, s.Min, 0.5f)) });
                uv.AddRange(new Vector2[] { new Vector2(0f, 0), new Vector2(0f, 1), new Vector2(0.1f, 0.5f)});
                tris.AddRange(new int[] { (i * 3), (i * 3) + 1, (i * 3) + 2 });
            } 
        }
        goto pass;
        skip:
        //---DEBUG BLOCKY MESH BUILD---//
        for (int i = 0; i < materials.Count; i++)
        {
            SubColumn s = materials[i];
            verts.AddRange(new Vector3[] { new Vector2(-0.1f, s.Min), new Vector2(-0.1f, s.Max), new Vector2(0.1f, s.Min), new Vector2(0.1f, s.Max) });
            uv.AddRange(new Vector2[] { new Vector2(-0.1f, 0), new Vector2(-0.1f, 1), new Vector2(0.1f, 0), new Vector2(0.1f, 1) });
            tris.AddRange(new int[] { (i * 4), (i * 4) + 1, (i * 4) + 2, (i * 4) + 1, (i * 4) + 3, (i * 4) + 2 });
        }
        */
        pass:
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uv.ToArray();
        SubMeshDescriptor sub = new SubMeshDescriptor(0, tris.Count, MeshTopology.Triangles);
        //sub.firstVertex = 0;
        //sub.vertexCount = verts.Count;
        mesh.SetSubMesh(0, sub);
        mesh.UploadMeshData(false);
        markedToUpdate = false;
        world.UpdateMesh -= RebuildMesh;
    }
    public void Erase(SubColumn sub) => Erase(sub.Min, sub.Max);
    public void Erase(float min, float max)
    {
        if (materials.Count == 0) { return; }
        //BINARY SEARCH ATTEMPT - COME BACK TO THIS AT SOME POINT
        int low = 0; //Will be used as the lower bound
        int high = materials.Count - 1; //Higher bound 
        //Binary Search lower bound
        while (low <= high) //Checks what subs are COMPLETELY above MIN
        {
            int mid = low + (high - low) / 2;

            if (materials[mid].Min <= min)
                low = mid + 1;
            else
                high = mid - 1;
        }
        SubColumn lowerSub = new SubColumn();
        bool modifyLow = false;
        if (low < materials.Count) lowerSub = materials[low];
        if (low != 0) //If low wasn't set to start of the list
        {
            if (materials[low - 1].Max > min) //If we overlap the lower sub
            {
                lowerSub = materials[--low];
                modifyLow = true;
            }
        }
        high = materials.Count - 1; //Reset the binary search algo, however don't reset low for what should be obvious reasons
        int binLow = low; //Create a new lower bound for the second binary search
        while (binLow <= high) //Checks what subs are COMPLETELY below MAX
        {
            int mid = binLow + (high - binLow) / 2;

            if (materials[mid].Max <= max)
                binLow = mid + 1;
            else
                high = mid - 1;
        }

        SubColumn higherSub = new SubColumn();
        bool modifyHigh = false;
        if (high >= 0) higherSub = materials[high];

            
        if (high != materials.Count - 1) //If high wasn't set to end of the list
        {
            if (materials[high + 1].Min < max) //If we overlap the higher sub
            {
                higherSub = materials[++high];
                modifyHigh = true;
            }
        }

        if (low > high) return;
        //Debug.Log($"{low}, {high}, {modifyLow}, {modifyHigh}");
        materials.RemoveRange(low, (high - low) + 1);
        if (modifyHigh) materials.Insert(low, new SubColumn(max, higherSub.Max, higherSub.MaterialID));
        if (modifyLow) materials.Insert(low, new SubColumn(lowerSub.Min, min, lowerSub.MaterialID));
        if (materials.Count == 0)
        {
            Destroy(gameObject);
        }
        else if (!markedToUpdate) world.UpdateMesh += RebuildMesh;
    }
    public void SimUpdate()
    { }
    private void OnDestroy()
    {
        world.DeleteColumn(index);
        if (markedToUpdate) world.UpdateMesh -= RebuildMesh;
    }
    public void OnDrawGizmos()
    {
        for (int i = 0; i < materials.Count; i++)
        {
            SubColumn s = materials[i];
            Gizmos.DrawWireCube(transform.position + new Vector3(world.ColumnWidth / 2f, s.Min + ((s.Max - s.Min) / 2f), 0), new Vector2(world.ColumnWidth, s.Max - s.Min));
        }
    }

}
/// <summary>
/// Represents a sub-column of material in a MaterialColumn
/// </summary>
[System.Serializable]
public struct SubColumn
{
    /// <summary>
    /// The 
    /// </summary>
    public float Max { get => _Max; set { if (value < _Min) throw new System.Exception("Tried setting a subcolumn's max lower than it's min"); _Max = value; } }
    /// <summary>
    /// 
    /// </summary>
    public float Min { get => _Min; set { if (value > _Max) throw new System.Exception("Tried setting a subcolumn's min higher than it's max"); _Max = value; } }
    [SerializeField]
    float _Min;
    [SerializeField]
    float _Max;
    /// <summary>
    /// The material represented by this sub column
    /// </summary>
    public short MaterialID;
    public SubColumn(float min, float max, short matID)
    {
        if (min > max) throw new System.Exception("This literally can't exist");
        _Min = min;
        _Max = max;
        MaterialID = matID;
    }
}