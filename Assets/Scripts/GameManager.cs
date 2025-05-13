using UnityEngine;
using System.Collections.Generic;
using AnnaUtility.ProceduralMountain;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    float t;
    [Header("User UI")]
    public GameObject brushPrefab;
    public enum GameState{
        Menu,//special state for menu.
        Playing,//special state for playing
        Dig,
        Raise,
        Flatten,
        Smooth,
        PaintGrass,
        PaintRock,
        PlaceTree,
        Demolish,
        CreateTrail,
        EditTrail,
    }

    
    public MeshGenerator.VertexData[] selectedVertices;
    private MeshGenerator.VertexData[] previousSelectedVertices;

    private GameObject brush;
    public GameObject MenuUI;
    public GameObject PlayingUI;
    [Header("Painting Colors")]
    public Color GrassColor = new Color(0.3f, 0.7f, 0.2f, 1f);
    public Color StoneColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private GameState _currentState = GameState.Menu;
    
    MeshGenerator meshGenerator;

    public static event Action<GameState> OnGameStateChanged;

    private MeshGenerator.VertexData closestVertex; // Store the closest vertex

    void Awake()
    {
        meshGenerator = FindFirstObjectByType<MeshGenerator>().GetComponent<MeshGenerator>();
        brush = Instantiate(brushPrefab, Vector3.zero, Quaternion.identity);
        brush.SetActive(false); // Start inactive
    }

    // Update is called once per frame
    void Update()
    {
        // Keyboard shortcuts for state changes
        if (Input.GetKeyDown(KeyCode.G)) ChangeGameState(GameState.Dig);
        if (Input.GetKeyDown(KeyCode.R)) ChangeGameState(GameState.Raise);
        if (Input.GetKeyDown(KeyCode.F)) ChangeGameState(GameState.Flatten);
        if (Input.GetKeyDown(KeyCode.S)) ChangeGameState(GameState.Smooth);
        if (Input.GetKeyDown(KeyCode.H)) ChangeGameState(GameState.PaintGrass);
        if (Input.GetKeyDown(KeyCode.K)) ChangeGameState(GameState.PaintRock);
        if (Input.GetKeyDown(KeyCode.T)) ChangeGameState(GameState.PlaceTree);
        if (Input.GetKeyDown(KeyCode.D)) ChangeGameState(GameState.Demolish);
        if (Input.GetKeyDown(KeyCode.C)) ChangeGameState(GameState.CreateTrail);
        if (Input.GetKeyDown(KeyCode.E)) ChangeGameState(GameState.EditTrail);
        StateUpdate(_currentState);
        print(_currentState);
    }

    private void StateUpdate(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                UpdateMenu();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.Playing:
                UpdatePlaying();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Menu);
                break;
            case GameState.Dig:
                UpdateBrush(Color.white);
                UpdateDig();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.Raise:
                UpdateBrush(Color.white);
                UpdateRaise();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.Flatten:
                UpdateBrush(Color.white);
                UpdateFlatten();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.Smooth:
                UpdateBrush(Color.white);
                UpdateSmooth();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.PaintGrass:
                UpdateBrush(Color.green);
                UpdatePaintGrass();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.PaintRock:
                UpdateBrush(Color.gray);
                UpdatePaintRock();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.PlaceTree:
                UpdateBrush(Color.green);
                UpdatePlaceTree();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.Demolish:
                UpdateBrush(Color.red);
                UpdateDemolish();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.CreateTrail:
                UpdateBrush(Color.yellow);
                UpdateCreateTrail();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
            case GameState.EditTrail:
                UpdateBrush(Color.white);
                UpdateEditTrail();
                if (Input.GetKeyDown(KeyCode.Escape)) ChangeGameState(GameState.Playing);
                break;
        }
    }

        public void ChangeGameState(GameState newState)
    {
        if (_currentState == newState)
        {
            Debug.Log("Already in state: " + newState);
            return;
        }

        // Exit logic for the old state
        OnStateExit(_currentState);

        // Enter logic for the new state
        _currentState = newState;
        OnStateEnter(_currentState);

        Debug.Log("GameState changed to: " + _currentState);
        OnGameStateChanged?.Invoke(_currentState);
    }
        private void OnStateExit(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                if (MenuUI != null) MenuUI.SetActive(false);
                print("exitmenu");
                break;
            case GameState.Playing:
                print("exitplaying");
                break;
            default:
                print("exitstate" + state);
                break;
        }
    }

    private void OnStateEnter(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                if (PlayingUI != null) PlayingUI.SetActive(false);
                if (MenuUI != null) MenuUI.SetActive(true);
                Time.timeScale = 0;
                MoveBrushOffscreen();
                DeselectAllVertices();
                print("entermenu");
                break;
            case GameState.Playing:
                if (PlayingUI != null) PlayingUI.SetActive(true);
                if (MenuUI != null) MenuUI.SetActive(false);
                Time.timeScale = 1;
                MoveBrushOffscreen();
                DeselectAllVertices();
                print("enterplaying");
                break;
            default:
                // Edit states: hide both UIs, show brush
                Time.timeScale = 0;
                print("enterstate" + state);
                // Brush will be positioned by UpdateBrush when needed
                break;
        }
    }

    // State Updates
    private void UpdateMenu(){}
    private void UpdatePlaying(){}
    private void UpdateDig(){
        if (Input.GetMouseButton(0)){
            t -= Time.deltaTime;
            if(t <= 0f){
                foreach (var v in selectedVertices){
                    v.position.y -= 1;
                }
                meshGenerator.UpdateMesh();
                t = 0.1f; // Repeat every 0.1 seconds while holding
            }
        } else {
            t = 0f; // Reset timer when mouse is released
        }
    }
    private void UpdateRaise(){
        if (Input.GetMouseButton(0)){
            t -= Time.deltaTime;
            if(t <= 0f){
                foreach (var v in selectedVertices){
                    v.position.y += 1;
                }
                meshGenerator.UpdateMesh();
                t = 0.1f; // Repeat every 0.1 seconds while holding
            }
        } else {
            t = 0f; // Reset timer when mouse is released
        }
    }
    private void UpdateFlatten(){
        if (Input.GetMouseButton(0)){
            t -= Time.deltaTime;
            if(t <= 0f && closestVertex != null){
                float targetHeight = closestVertex.position.y;
                foreach (var v in selectedVertices){
                    v.position.y = targetHeight;
                }
                meshGenerator.UpdateMesh();
                t = 0.1f; // Repeat every 0.1 seconds while holding
            }
        } else {
            t = 0f; // Reset timer when mouse is released
        }
    }
    private void UpdateSmooth(){
        if (Input.GetMouseButton(0)){
                foreach (var v in selectedVertices){
                    float sum = v.position.y;
                    int count = 1;
                    // Find neighbors in the 2D array
                    for (int dz = -1; dz <= 1; dz++){
                        for (int dx = -1; dx <= 1; dx++){
                            if (Mathf.Abs(dx) + Mathf.Abs(dz) != 1) continue; // Only up/down/left/right
                            int nx = Mathf.RoundToInt(v.position.x) + dx;
                            int nz = Mathf.RoundToInt(v.position.z) + dz;
                            if (nx >= 0 && nx <= meshGenerator.xSize && nz >= 0 && nz <= meshGenerator.zSize){
                                var neighbor = meshGenerator.vertices2D[nx, nz];
                                sum += neighbor.position.y;
                                count++;
                            }
                        }
                    }
                    v.position.y = sum / count;
                }
                meshGenerator.UpdateMesh();
                t = 0.1f; // Repeat every 0.1 seconds while holding
        }
    }
    private void UpdatePaintGrass()
    {
        if (Input.GetMouseButton(0)){
                foreach (var v in selectedVertices){
                    v.SetColor(GrassColor);
                    v.StoreOriginalColor();
                    Debug.Log("Set color to: " + GrassColor);
                }
                //meshGenerator.UpdateMesh();
                meshGenerator.ApplyVertexRealColors();
                t = 0.1f;
        }
    }
    private void UpdatePaintRock()
    {
        Debug.Log("UpdatePaintRock called, selectedVertices: " + (selectedVertices != null ? selectedVertices.Length.ToString() : "null"));
        if (Input.GetMouseButton(0)){
            t -= Time.deltaTime;
            if(t <= 0f){
                if (selectedVertices == null){
                    Debug.Log("selectedVertices is null");
                    return;
                }
                foreach (var v in selectedVertices){
                    v.SetColor(StoneColor);
                    v.StoreOriginalColor();
                    Debug.Log("Set color to: " + StoneColor);
                }
                //meshGenerator.UpdateMesh();
                meshGenerator.ApplyVertexRealColors();
                t = 0.1f;
            }
        } else {
            t = 0f;
        }
    }
    private void UpdatePlaceTree()
    {
        if (Input.GetMouseButton(0))
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                foreach (var v in selectedVertices)
                {
                    if (v.tree == null && meshGenerator.treePrefabs != null && meshGenerator.treePrefabs.Length > 0)
                    {
                        int prefabIndex = UnityEngine.Random.Range(0, meshGenerator.treePrefabs.Length);
                        GameObject tree = Instantiate(meshGenerator.treePrefabs[prefabIndex], meshGenerator.transform.TransformPoint(v.position), Quaternion.identity, meshGenerator.transform);
                        v.tree = tree;
                    }
                }
                t = 0.1f;
            }
        }
        else
        {
            t = 0f;
        }
    }
    private void UpdateDemolish()
    {
        if (Input.GetMouseButton(0))
        {
            t -= Time.deltaTime;
            if (t <= 0f)
            {
                foreach (var v in selectedVertices)
                {
                    if (v.tree != null)
                    {
                        Destroy(v.tree);
                        v.tree = null;
                    }
                }
                t = 0.1f;
            }
        }
        else
        {
            t = 0f;
        }
    }
    private void UpdateCreateTrail(){}
    private void UpdateEditTrail(){
        
    }
    
    //handle button presses to change game state in all states.

    public void OnClickNewGame(){
        meshGenerator.StartNewGame();
    }
    public void OnClickResetGame(){
        meshGenerator.ResetGame();
    }
    public void OnClickBackToPlaying(){
        ChangeGameState(GameState.Playing);
    }
    public void OnClickDig() { ChangeGameState(GameState.Dig); }
    public void OnClickRaise() { ChangeGameState(GameState.Raise); }
    public void OnClickFlatten() { ChangeGameState(GameState.Flatten); }
    public void OnClickSmooth() { ChangeGameState(GameState.Smooth); }
    public void OnClickPaintGrass() { ChangeGameState(GameState.PaintGrass); }
    public void OnClickPaintRock() { ChangeGameState(GameState.PaintRock); }
    public void OnClickPlaceTree() { ChangeGameState(GameState.PlaceTree); }
    public void OnClickDemolish() { ChangeGameState(GameState.Demolish); }
    public void OnClickCreateTrail() { ChangeGameState(GameState.CreateTrail); }
    public void OnClickEditTrail() { ChangeGameState(GameState.EditTrail); }
    public void OnClickBack() { ChangeGameState(GameState.Playing); }

    public void UpdateBrush(Color highlightColor)
    {
        if (brush == null) return; // Safety check

        //raycast from mouse position to terrain, if hit, set brush position to hit position; else, set to (0,0,-100)
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //find the vertex data most close to the hit point
        closestVertex = null;
        float closestDistance = float.MaxValue;

        if (Physics.Raycast(ray, out hit)) {
            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Mountain") {
                brush.transform.position = hit.point;

                // Reset color of previously selected vertices to their original colors
                if (previousSelectedVertices != null)
                {
                    foreach (var v in previousSelectedVertices)
                    {
                        v.RestoreOriginalColor();
                    }
                }

                Vector3 brushPosition = brush.transform.position;
                float brushRadius = brush.transform.localScale.x * 0.5f;

                // Flatten the 2D array to a list for selected vertices
                List<MeshGenerator.VertexData> selected = new List<MeshGenerator.VertexData>();
                for (int z = 0; z <= meshGenerator.zSize; z++)
                {
                    for (int x = 0; x <= meshGenerator.xSize; x++)
                    {
                        var v = meshGenerator.vertices2D[x, z];
                        Vector3 vertexWorldPos = meshGenerator.transform.TransformPoint(v.position);
                        float dist = Vector3.Distance(vertexWorldPos, hit.point);
                        if (dist < closestDistance) {
                            closestDistance = dist;
                            closestVertex = v;
                        }
                        if (Vector3.Distance(vertexWorldPos, brushPosition) <= brushRadius)
                        {
                            selected.Add(v);
                            v.StoreOriginalColor(); // Store the current color before changing it
                            v.SetColor(highlightColor); // Set selected color
                        }
                    }
                }
                selectedVertices = selected.ToArray();
                meshGenerator.ApplyVertexColors(); // Update mesh colors

                previousSelectedVertices = selectedVertices;
            }
        } else {
            brush.transform.position = new Vector3(0, 0, -100);
            
            // Reset colors when brush is not over terrain
            if (previousSelectedVertices != null)
            {
                foreach (var v in previousSelectedVertices)
                {
                    v.RestoreOriginalColor();
                }
                meshGenerator.ApplyVertexColors();
                previousSelectedVertices = null;
                selectedVertices = null;
            }
            closestVertex = null;
        }

        Debug.Log("UpdateBrush: selectedVertices count = " + (selectedVertices != null ? selectedVertices.Length.ToString() : "null"));
    }

    // Deselect all vertices and restore their original colors
    private void DeselectAllVertices()
    {
        if (previousSelectedVertices != null)
        {
            foreach (var v in previousSelectedVertices)
            {
                v.RestoreOriginalColor();
            }
            meshGenerator.ApplyVertexColors();
            previousSelectedVertices = null;
            selectedVertices = null;
        }
    }

    private void MoveBrushOffscreen()
    {
        if (brush != null)
            brush.transform.position = new Vector3(0, 0, -100);
    }
}
