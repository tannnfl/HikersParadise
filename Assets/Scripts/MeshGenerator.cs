using UnityEngine;
using UnityEditor;

namespace AnnaUtility.ProceduralMountain
{
    public class MeshGenerator : MonoBehaviour
    {
        [Header("Mesh Settings")]
        public int xSize = 20;
        public int zSize = 20;
        public float edgeFalloffStrength = 2f;
        public Gradient gradient;

        [Header("Base Layer Controls")]
        public bool showBaseLayer = true;
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

        [Header("Tree Placement")]
        public GameObject[] treePrefabs;
        [Range(0.01f, 1f)]
        public float treeNoiseScale = 0.1f;
        [Range(0f, 1f)]
        public float treeNoiseThreshold = 0.3f;
        [Range(0f, 2f)]
        public float treeRandomRadius = 0.5f;
        [Range(0f, 2f)]
        public float slopeThreshold = 0.5f;

        [Header("Grass Placement")]
        public GameObject[] grassPrefabs;
        [Range(0.01f, 1f)]
        public float grassNoiseScale = 0.15f;
        [Range(0f, 1f)]
        public float grassNoiseThreshold = 0.2f;
        [Range(0f, 2f)]
        public float grassRandomRadius = 0.3f;
        [Range(0f, 2f)]
        public float grassSlopeThreshold = 0.7f;
        //mesh components
        public Mesh mesh;

        public class VertexData
        {
            public Vector3 position;
            public float slope;
            public Color color;
            public bool isHighlighted = false;
            public GameObject tree;
            private MeshGenerator parent;
            private Color originalColor;

            public VertexData(Vector3 position, float slope, Color color, MeshGenerator parent)
            {
                this.position = position;
                this.slope = slope;
                this.color = color;
                this.tree = null;
                this.parent = parent;
                this.originalColor = color;
            }

            public Color GetDisplayColor()
            {
                return isHighlighted ? Color.red : color;
            }

            public void SetColor(Color c)
            {
                this.color = c;
            }

            public void StoreOriginalColor()
            {
                originalColor = color;
            }

            public void RestoreOriginalColor()
            {
                color = originalColor;
            }

            public void SetHeight(float h)
            {
                this.position.y = h;
            }
            
            public void PlaceTree()
            {
                if (parent.treePrefabs != null && parent.treePrefabs.Length > 0)
                {
                    this.tree = UnityEngine.Object.Instantiate(parent.treePrefabs[Random.Range(0, parent.treePrefabs.Length)], this.position, Quaternion.identity);
                    this.tree.transform.parent = parent.transform;
                }
            }

            public void DestroyTree()
            {
                if (this.tree != null)
                {
                    UnityEngine.Object.Destroy(this.tree);
                    this.tree = null;
                }
            }
        }
        public VertexData[,] vertices2D;
        public int[] triangles;
        //mesh properties
        public float minHeight, maxHeight;
        //mesh colors
        private Vector3[] vertices;
        private Color[] colors;
        
        void Start()
        { 
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            GenerateNewMountain();
        }


        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)){
                GenerateNewMountain();
            }
        }

        public void GenerateNewMountain(){
            CreateMeshShape();
            CreateMeshTriangles();
            ResetMeshProperties();
            ColorMap();
            UpdateMesh();
            PlaceTreesWithRaycast();
            //PlaceGrassWithRaycast();
        }

        //Mesh Properties Calculations
        public void ResetMeshProperties(){
            CalculateVerticesMinMaxHeight();
            CalculateVerticesSlope();
        }

        //create a mesh with perlin noise and fall off on the edges
        private void CreateMeshShape(){
            //create vertices2D
            vertices2D = new VertexData[xSize + 1, zSize + 1];
            for(int z = 0; z <= zSize; z++){
                for(int x = 0; x <= xSize; x++){
                    float y = 0f;

                    // Base Layer
                    if (showBaseLayer)
                    {
                        float baseSampleX = x * baseNoiseScale + baseOffset.x;
                        float baseSampleZ = z * baseNoiseScale + baseOffset.y;
                        float baseValue = Mathf.PerlinNoise(baseSampleX, baseSampleZ);
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
                    Vector3 pos = new Vector3(x, y, z);// the y value = height = noise value
                    vertices2D[x, z] = new VertexData(pos, 0f, Color.black, this);
                }
            }
            // Flatten 2D array to 1D for Unity mesh
            vertices = new Vector3[(xSize + 1) * (zSize + 1)];
            int i = 0;
            for(int z = 0; z <= zSize; z++)
                for(int x = 0; x <= xSize; x++)
                    vertices[i++] = vertices2D[x, z].position;
        }

        private void CreateMeshTriangles(){
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

        private void UpdateMesh(){
            // Rebuild the class-level arrays from VertexData
            int vertCount = (xSize + 1) * (zSize + 1);
            for (int i = 0, z = 0; z <= zSize; z++) {
                for (int x = 0; x <= xSize; x++, i++) {
                    vertices[i] = vertices2D[x, z].position;
                    colors[i] = vertices2D[x, z].color;
                }
            }
            mesh.vertices = vertices;//build based on verticesdatas[,]
            mesh.triangles = triangles;
            mesh.colors = colors;//build based on verticesdatas[,]
            mesh.RecalculateNormals();

            // Update or add MeshCollider
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = null; // Clear first
            meshCollider.sharedMesh = mesh;
        }

        //procedural generate details
        private void ColorMap(){
            colors = new Color[vertices.Length];
            int i = 0;
            for (int z = 0; z <= zSize; z++) {
                for (int x = 0; x <= xSize; x++) {
                    VertexData v = vertices2D[x, z];
                    float height = Mathf.InverseLerp(minHeight, maxHeight, v.position.y);
                    Color col = gradient.Evaluate(height);
                    if (v.slope > slopeThreshold) col = Color.grey;
                    v.SetColor(col);
                    vertices2D[x, z] = v;
                    colors[i++] = col;
                }
            }
        }
        private void PlaceTreesWithRaycast()
        {
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                Debug.LogError("No MeshCollider found!");
                return;
            }

            // Destroy old tree parent if it exists
            Transform oldParent = transform.Find("Trees");
            if (oldParent != null)
            {
                DestroyImmediate(oldParent.gameObject);
            }
            // Parent for all trees
            GameObject treeParent = new GameObject("Trees");
            treeParent.transform.parent = this.transform;

            for (int i = 0; i < vertices.Length; i++)
            {
                int x = i % (xSize + 1);
                int z = i / (xSize + 1);
                // Only allow tree if slope is under threshold
                if (vertices2D[x, z].slope > slopeThreshold) continue;
                float perlinValue = Mathf.PerlinNoise(vertices[i].x * treeNoiseScale, vertices[i].z * treeNoiseScale);
                if (perlinValue > treeNoiseThreshold && treePrefabs != null && treePrefabs.Length > 0)
                {
                    // Probability increases as perlinValue approaches 1
                    float probability = Mathf.InverseLerp(treeNoiseThreshold, 1f, perlinValue);
                    if (Random.value < probability)
                    {
                        Vector3 rayOrigin = vertices[i] + Vector3.up * 100f;
                        Ray ray = new Ray(rayOrigin, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 200f))
                        {
                            if (hit.collider == meshCollider)
                            {
                                vertices2D[x, z].PlaceTree();
                                vertices2D[x, z].tree.transform.position = hit.point;
                                Quaternion randomYRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                                vertices2D[x, z].tree.transform.rotation = randomYRot;
                                vertices2D[x, z].tree.transform.parent = treeParent.transform;
                            }
                        }
                    }
                }
            }
        }

        private void PlaceGrassWithRaycast()
        {
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                Debug.LogError("No MeshCollider found!");
                return;
            }

            // Destroy old grass parent if it exists
            Transform oldParent = transform.Find("Grass");
            if (oldParent != null)
            {
                DestroyImmediate(oldParent.gameObject);
            }
            // Parent for all grass
            GameObject grassParent = new GameObject("Grass");
            grassParent.transform.parent = this.transform;

            for (int i = 0; i < vertices.Length; i++)
            {
                int x = i % (xSize + 1);
                int z = i / (xSize + 1);
                // Only allow grass if slope is under threshold
                if (vertices2D[x, z].slope > grassSlopeThreshold) continue;
                float perlinValue = Mathf.PerlinNoise(vertices[i].x * grassNoiseScale, vertices[i].z * grassNoiseScale);
                if (perlinValue > grassNoiseThreshold && grassPrefabs != null && grassPrefabs.Length > 0)
                {
                    float probability = Mathf.InverseLerp(grassNoiseThreshold, 1f, perlinValue);
                    if (Random.value < probability)
                    {
                        Vector2 randomCircle = Random.insideUnitCircle * grassRandomRadius;
                        Vector3 randomPos = vertices[i] + new Vector3(randomCircle.x, 0, randomCircle.y);

                        Vector3 rayOrigin = randomPos + Vector3.up * 100f;
                        Ray ray = new Ray(rayOrigin, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 200f))
                        {
                            if (hit.collider == meshCollider)
                            {
                                int prefabIndex = Random.Range(0, grassPrefabs.Length);
                                GameObject prefabToUse = grassPrefabs[prefabIndex];
                                if (prefabToUse != null)
                                {
                                    Quaternion randomYRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                                    GameObject grass = Instantiate(prefabToUse, hit.point, randomYRot, grassParent.transform);
                                    grass.transform.localScale = Vector3.one * 0.3f;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CalculateVerticesMinMaxHeight(){
            //go through all the vertices and find the min and max height
            minHeight = 0;
            maxHeight = 0;
            foreach(Vector3 vertex in vertices){
                if(vertex.y < minHeight) minHeight = vertex.y;
                if(vertex.y > maxHeight) maxHeight = vertex.y;
            }
        }

        private void CalculateVerticesSlope(){
            // Calculate slope for each vertex
            for (int z = 0; z <= zSize; z++) {
                for (int x = 0; x <= xSize; x++) {
                    float totalSlope = 0f;
                    int validNeighbors = 0;
                    Vector3 current = vertices2D[x, z].position;
                    int[,] neighbors = new int[,] { {1,0}, {-1,0}, {0,1}, {0,-1} };
                    for (int n = 0; n < 4; n++) {
                        int nx = x + neighbors[n,0];
                        int nz = z + neighbors[n,1];
                        if (nx >= 0 && nx <= xSize && nz >= 0 && nz <= zSize) {
                            Vector3 neighbor = vertices2D[nx, nz].position;
                            float heightDiff = Mathf.Abs(current.y - neighbor.y);
                            float dist = Vector2.Distance(new Vector2(x, z), new Vector2(nx, nz));
                            float slope = heightDiff / dist;
                            totalSlope += slope;
                            validNeighbors++;
                        }
                    }
                    float avgSlope = validNeighbors > 0 ? totalSlope / validNeighbors : 0f;
                    var v = vertices2D[x, z];
                    v.slope = avgSlope;
                    vertices2D[x, z] = v;
                }
            }
        }
        private void OnDrawGizmos(){
            if(vertices == null) return;
            Gizmos.color = Color.black;
            for(int i = 0; i < vertices.Length; i++){
                Gizmos.DrawSphere(vertices[i], 0.1f);
            }
        }

        public void ApplyVertexColors()
        {
            int i = 0;
            for (int z = 0; z <= zSize; z++)
                for (int x = 0; x <= xSize; x++)
                    colors[i++] = vertices2D[x, z].GetDisplayColor();
            mesh.colors = colors;
        }

    }
}