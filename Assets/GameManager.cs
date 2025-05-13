using UnityEngine;
using System.Collections.Generic;
using proceduralMountain;

public class GameManager : MonoBehaviour
{
    [Header("User UI")]
    public GameObject brushPrefab;

    public static GameManager instance;
    public enum GameState{
        Menu,
        Playing,
        EditTerrain,
        DestroyTree,
        PlaceTree,
        CreateTrail,
        EditTrail,
    }

    public GameObject brush;
    public MeshGenerator.VertexData[] selectedVertices;
    private MeshGenerator.VertexData[] previousSelectedVertices;

    public GameState currentGameState = GameState.Menu;
    
    MeshGenerator meshGenerator;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want it to persist across scenes
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
        }
        meshGenerator = FindObjectOfType<MeshGenerator>().GetComponent<MeshGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameState();
    }

    private void UpdateGameState(){
        switch(currentGameState){
            case GameState.Menu:
                UpdateMenu();
                break;
            case GameState.Playing:
                //switch game state
                CanGoToMenu();
                CanGoToCreateTrail();
                CanGoToEditTrail();
                CanGoToDestroyTree();
                CanGoToPlaceTree();
                CanGoToEditTerrain();

                UpdatePlaying();
                
                break;
            case GameState.EditTerrain:
                //escape back
                CanGoToMenu();
                CanGoToPlaying();

                UpdateEditTerrain();

                break;
            case GameState.DestroyTree:
                //escape back
                CanGoToMenu();
                CanGoToPlaying();

                UpdateDestroyTree();

                break;
            case GameState.PlaceTree:
                //escape back
                CanGoToMenu();
                CanGoToPlaying();

                UpdatePlaceTree();

                break;
            case GameState.CreateTrail:
                //escape back
                CanGoToMenu();
                CanGoToPlaying();

                UpdateCreateTrail();

                break;
            case GameState.EditTrail:
                //escape back
                CanGoToMenu();
                CanGoToPlaying();

                UpdateEditTrail();

                break;
        }
    }

    public void ChangeGameState(GameState newState){
        currentGameState = newState;
        Debug.Log("GameState changed to: " + newState);
        switch(newState){
            case GameState.Menu:
                Time.timeScale = 0;
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                break;
            case GameState.EditTerrain:
                Time.timeScale = 0;
                //instantiate brush prefab
                brush = Instantiate(brushPrefab);
                break;
            case GameState.DestroyTree:
                Time.timeScale = 0;
                break;  
            case GameState.PlaceTree:
                Time.timeScale = 0;
                break;
            case GameState.CreateTrail:
                Time.timeScale = 0;
                break;
            case GameState.EditTrail:
                Time.timeScale = 0;
                break;
        }
    }
    public void EndGameState(){
        switch(currentGameState){
            case GameState.EditTerrain:
                Destroy(brush);
                break;
            case GameState.DestroyTree:
                break;
            case GameState.PlaceTree:
                break;
            case GameState.CreateTrail:
                break;
            case GameState.EditTrail:
                break;
        }
    }

    // State Updates
    private void UpdateMenu(){}
    private void UpdatePlaying(){}
    private void UpdateEditTerrain(){
        if (brush == null) return; // Safety check

        //raycast from mouse position to terrain, if hit, set brush position to hit position; else, set to (0,0,-100)
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) {
            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Mountain") {
                brush.transform.position = hit.point;
                // Reset color of previously selected vertices
                if (previousSelectedVertices != null)
                {
                    foreach (var v in previousSelectedVertices)
                    {
                        v.SetColor(Color.white); // Or your default color
                    }
                }

                Vector3 brushPosition = brush.transform.position;
                float brushRadius = brush.transform.localScale.x * 0.5f;

                MeshGenerator.VertexData[] allVertices = meshGenerator.vertices2D;
                List<MeshGenerator.VertexData> selected = new List<MeshGenerator.VertexData>();
                foreach (var v in allVertices)
                {
                    Vector3 vertexWorldPos = meshGenerator.transform.TransformPoint(v.position);
                    if (Vector3.Distance(vertexWorldPos, brushPosition) <= brushRadius)
                    {
                        selected.Add(v);
                        v.SetColor(Color.red); // Set selected color
                    }
                }
                selectedVertices = selected.ToArray();
                meshGenerator.UpdateMeshColors(); // Make sure this updates the mesh

                previousSelectedVertices = selectedVertices;
            }
        } else {
            brush.transform.position = new Vector3(0, 0, -100);
        }
    }
    private void UpdateDestroyTree(){}
    private void UpdatePlaceTree(){}
    private void UpdateCreateTrail(){}
    private void UpdateEditTrail(){}
    
    // change state with key
    private void CanGoToMenu(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            ChangeGameState(GameState.Menu);
        }
    }

    private void CanGoToPlaying(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            ChangeGameState(GameState.Playing);
        }
    }

    private void CanGoToEditTerrain(){
        if(Input.GetKeyDown(KeyCode.T)){
            ChangeGameState(GameState.EditTerrain);
        }
    }

    private void CanGoToDestroyTree(){
        if(Input.GetKeyDown(KeyCode.D)){
            ChangeGameState(GameState.DestroyTree);
        }
    }

    private void CanGoToPlaceTree(){
        if(Input.GetKeyDown(KeyCode.P)){
            ChangeGameState(GameState.PlaceTree);
        }
    }

    private void CanGoToCreateTrail(){
        if(Input.GetKeyDown(KeyCode.C)){
            ChangeGameState(GameState.CreateTrail);
        }
    }

    private void CanGoToEditTrail(){
        if(Input.GetKeyDown(KeyCode.E)){
            ChangeGameState(GameState.EditTrail);
        }
    }

    public void UpdateMeshColors()
    {
        ColorMap();
        UpdateMesh();
    }

}
