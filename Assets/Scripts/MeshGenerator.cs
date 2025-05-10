using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int xSize = 20;
    public int zSize = 20;
    
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    
    void Start()
    { 
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        UpdateMesh();
    }


    void Update()
    {
        UpdateMesh();
    }

    //create a mesh with perlin noise and fall off on the edges
    public void CreateMesh(){
        //create vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for(int i = 0, z = 0; z <= zSize; z++){
            for(int x = 0; x <= xSize; x++){
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 3f;
                //fall off on the edges
                float edgeFallOff = Mathf.Clamp01(Mathf.Abs(x - xSize / 2) / (xSize / 2));
                y *= edgeFallOff;
                vertices[i] = new Vector3(x, y, z);// the y value = height = noise value
                i++;
            }
        }

        //create triangles
        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

    }

    public void UpdateMesh(){
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos(){
        if(vertices == null) return;
        Gizmos.color = Color.black;
        for(int i = 0; i < vertices.Length; i++){
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
