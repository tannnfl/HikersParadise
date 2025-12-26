using UnityEngine;
using AnnaUtility.ProceduralMountain;
using System.Collections.Generic;

public class TrailManager : MonoBehaviour
{
    private MeshGenerator meshGenerator;
    private List<Trail> trails = new List<Trail>();

    public class Trail
    {
        private TrailManager manager;
        private List<MeshGenerator.VertexData> vertices;
        private List<MeshGenerator.VertexData> canAddVertices;//the 9 vertices around the last vertex
        public List<MeshGenerator.VertexData> Vertices{get{return vertices;}}
        public List<MeshGenerator.VertexData> CanAddVertices { get { return canAddVertices; } }
        public Trail(TrailManager manager){
            this.manager = manager;
            vertices = new List<MeshGenerator.VertexData>();
            canAddVertices = new List<MeshGenerator.VertexData>();
            manager.trails.Add(this);
        }

        public bool CanAddVertex(MeshGenerator.VertexData vertex){
            //check if the list is empty
            if(vertices.Count == 0) {
                Debug.Log("Adding first vertex: " + vertex.position);
                return true;}
            //check if the vertex is in canAddVertices list
            if(!canAddVertices.Contains(vertex)) {
                Debug.Log("Cannot add vertex: " + vertex.position + " because it is not in canAddVertices list");
                return false;}
            //check if the slope(height - height / distance) between new vertex and last vertex is less than 0.5
            float slope = manager.meshGenerator.GetSlope(vertex.position, vertices[vertices.Count - 1].position);
            if(slope > 0.5f) {
                Debug.Log("Cannot add vertex: " + vertex.position + " because the slope is too steep: " + slope);
                return false;}
            //check if the vertex forms an acute turn
            if (vertices.Count >= 2) {
                var A = vertices[vertices.Count - 2]; // 前一个拐点
                var B = vertices[vertices.Count - 1]; // 当前末点
                var C = vertex;                       // 候选新点
                if (FormsAcuteTurn(A, B, C)) {
                    Debug.Log("Cannot add vertex: " + vertex.position + " because it forms an acute turn at " + B.position);
                    return false;
                }
            }
            //add the vertex to the list
            return true;
        }
        public void AddVertex(MeshGenerator.VertexData vertex){
            if(!CanAddVertex(vertex)) {
                Debug.Log("Cannot add vertex: " + vertex.position);
                return;}
            Debug.Log("Adding vertex: " + vertex.position);
            vertices.Add(vertex);
            UpdateCanAddVertices(vertex);
        }
        private void UpdateCanAddVertices(MeshGenerator.VertexData vertex){
            //empty the list
            canAddVertices.Clear();
            //check the 9 vertices around the last vertex
            for(int i = -1; i <= 1; i++){
                for(int j = -1; j <= 1; j++){
                    if(i == 0 && j == 0) continue;
                    Debug.Log($"Checking neighbor offset ({i},{j}) for vertex at {vertex.position}");
                    int nx = vertex.xIndex + i;
                    int nz = vertex.zIndex + j;
                    if(nx < 0 || nx >= manager.meshGenerator.xSize || nz < 0 || nz >= manager.meshGenerator.zSize) continue;
                    var neighbor = manager.meshGenerator.vertices2D[nx, nz];
                    if(vertices.Contains(neighbor)) continue;
                    if(canAddVertices.Contains(neighbor)) continue;
                    canAddVertices.Add(neighbor);
                }
            }
            Debug.Log("Can add vertices: " + canAddVertices.Count);
        }
            // —— 关键：锐角判定（在 B 处看 A->B 与 B->C 的夹角）——
        private static bool FormsAcuteTurn(MeshGenerator.VertexData A,
                                        MeshGenerator.VertexData B,
                                        MeshGenerator.VertexData C)
        {
            // 用网格索引做整型向量，稳且无浮点误差
            var u = new Vector2Int(A.xIndex - B.xIndex, A.zIndex - B.zIndex);
            var v = new Vector2Int(C.xIndex - B.xIndex, C.zIndex - B.zIndex);

            // 任何一段为零向量就不当成锐角（等价于允许原地/重复点被其他逻辑拦掉）
            if (u == Vector2Int.zero || v == Vector2Int.zero) return false;

            int dot = u.x * v.x + u.y * v.y; // u·v
            return dot > 0;                  // >0 ⇒ 锐角
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshGenerator = FindFirstObjectByType<MeshGenerator>().GetComponent<MeshGenerator>();
        trails = new List<Trail>();
        if (meshGenerator.vertices2D == null)
        {
            Debug.LogError("MeshGenerator.vertices2D is not initialized yet!");
            return;
        }
        Trail trail = new Trail(this);
        int startX = Random.Range(0, meshGenerator.xSize);
        int startZ = Random.Range(0, meshGenerator.zSize);
        trail.AddVertex(meshGenerator.vertices2D[startX, startZ]);
        GenerateRandomTrail(trail);
        Debug.Log("Trail completed. Total vertices: " + trail.Vertices.Count);
    }

    public void GenerateRandomTrail(Trail trail)
    {
        while (trail.CanAddVertices.Count > 0)
        {
            // Shuffle the candidate list for randomness
            var candidates = new List<MeshGenerator.VertexData>(trail.CanAddVertices);
            int added = 0;
            while (candidates.Count > 0)
            {
                int idx = Random.Range(0, candidates.Count);
                var nextVertex = candidates[idx];
                candidates.RemoveAt(idx);

                if (trail.CanAddVertex(nextVertex))
                {
                    trail.AddVertex(nextVertex);
                    added++;
                    break; // Only add one per loop, then update candidates
                }
            }
            if (added == 0)
            {
                // No candidates could be added, so stop
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //draw the trail in playmode
    void OnDrawGizmos(){
        if(trails != null){
            Gizmos.color = Color.red;
            foreach(Trail trail in trails){
                var vertices = trail.Vertices;
                for(int i = 0; i < vertices.Count - 1; i++){
                    Gizmos.DrawLine(vertices[i].position, vertices[i+1].position);
                }
            }
        }
    }
}
