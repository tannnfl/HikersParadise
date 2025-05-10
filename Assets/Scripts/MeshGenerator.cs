using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int xSize = 20;
    public int zSize = 20;
    
    [Header("Terrain Controls")]
    [Range(0.01f, 1f)]
    public float noiseScale = 0.3f;
    [Range(0.1f, 100f)]
    public float heightMultiplier = 3f;
    [Range(0.1f, 5f)]
    public float edgeFalloffStrength = 2f;

    [Header("Fractal Noise Controls")]
    [Range(1, 8)]
    public int octaves = 4;
    [Range(0.1f, 1f)]
    public float persistence = 0.5f;
    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Header("Layer Visibility")]
    public bool showBaseLayer = true;
    public bool showDetailLayers = true;

    [Header("Base Layer Controls")]
    [Range(0.01f, 1f)]
    public float baseNoiseScale = 0.3f;
    [Range(0.1f, 100f)]
    public float baseHeight = 3f;
    public Vector2 baseOffset = Vector2.zero;

    [Header("Detail Layer Controls")]
    public bool showDetailLayer = true;
    [Range(0.01f, 1f)]
    public float detailNoiseScale = 0.1f;
    [Range(0.01f, 10f)]
    public float detailHeight = 1f;
    public Vector2 detailOffset = Vector2.zero;
    [Range(1, 8)]
    public int detailOctaves = 3;
    [Range(0.1f, 1f)]
    public float detailPersistence = 0.5f;
    [Range(1f, 4f)]
    public float detailLacunarity = 2f;

    [Header("Fine Detail Layer")]
    public bool fineDetailLayerEnabled = true;
    [Range(0.01f, 1f)]
    public float fineDetailScale = 0.05f;
    [Range(0.01f, 5f)]
    public float fineDetailHeight = 0.5f;
    public Vector2 fineDetailOffset = Vector2.zero;
    [Range(1, 8)]
    public int fineDetailOctaves = 2;
    [Range(0.1f, 1f)]
    public float fineDetailPersistence = 0.5f;
    [Range(1f, 4f)]
    public float fineDetailLacunarity = 2f;

    public Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    
    public Material testMaterial;
    
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

    void OnValidate()
    {
        /*
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Generated Mesh";
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = mesh;
        }

        CreateMesh();
        UpdateMesh();
        */
    }

    //create a mesh with perlin noise and fall off on the edges
    public void CreateMesh(){
        //create vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for(int i = 0, z = 0; z <= zSize; z++){
            for(int x = 0; x <= xSize; x++){
                float y = 0f;

                // Base Layer
                if (showBaseLayer)
                {
                    float baseSampleX = x * baseNoiseScale + baseOffset.x;
                    float baseSampleZ = z * baseNoiseScale + baseOffset.y;
                    float baseValue = Mathf.PerlinNoise(baseSampleX, baseSampleZ) * 2f - 1f;
                    y += baseValue * baseHeight;
                }

                // Detail Layer (fractal)
                if (showDetailLayer)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    float maxAmplitude = 0f;

                    for (int o = 0; o < detailOctaves; o++)
                    {
                        float sampleX = x * detailNoiseScale * frequency + detailOffset.x;
                        float sampleZ = z * detailNoiseScale * frequency + detailOffset.y;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
                        noiseHeight += perlinValue * amplitude;
                        maxAmplitude += amplitude;
                        amplitude *= detailPersistence;
                        frequency *= detailLacunarity;
                    }
                    if (maxAmplitude > 0f)
                        noiseHeight /= maxAmplitude;
                    y += noiseHeight * detailHeight;
                }

                // Fine Detail Layer (fractal)
                if (fineDetailLayerEnabled)
                {
                    float amp = 1f, freq = 1f, sum = 0f, ampSum = 0f;
                    for (int o = 0; o < fineDetailOctaves; o++)
                    {
                        float fx = x * fineDetailScale * freq + fineDetailOffset.x;
                        float fz = z * fineDetailScale * freq + fineDetailOffset.y;
                        float fVal = Mathf.PerlinNoise(fx, fz) * 2f - 1f;
                        sum += fVal * amp;
                        ampSum += amp;
                        amp *= fineDetailPersistence;
                        freq *= fineDetailLacunarity;
                    }
                    if (ampSum > 0f) sum /= ampSum;
                    y += sum * fineDetailHeight;
                }

                //fall off on the edges, make the edges of both row and column less steep, and the center more steep, make the edges a smooth curve
                float edgeX = Mathf.Abs(Mathf.Sin(Mathf.PI * x / xSize));
                float edgeZ = Mathf.Abs(Mathf.Sin(Mathf.PI * z / zSize));
                float edgeFallOff = Mathf.Pow(edgeX, edgeFalloffStrength) * Mathf.Pow(edgeZ, edgeFalloffStrength);
                y *= Mathf.Max(edgeFallOff, 0.01f);
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
