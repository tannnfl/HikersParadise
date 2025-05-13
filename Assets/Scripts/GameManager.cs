using UnityEngine;
using System.Collections.Generic;
using proceduralMountain;
using System;

public class GameManager : MonoBehaviour
{
    [Header("User UI")]
    public GameObject brushPrefab;

    public static GameManager instance;
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
    private GameState _currentState = GameState.Menu;
public GameState CurrentState => _currentState;
    
    MeshGenerator meshGenerator;

    public static event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        meshGenerator = FindFirstObjectByType<MeshGenerator>().GetComponent<MeshGenerator>();
        brush = Instantiate(brushPrefab, Vector3.zero, Quaternion.identity);
        brush.SetActive(false); // Start inactive
    }

    // Update is called once per frame
    void Update()
    {
        StateUpdate(_currentState);
        print(_currentState);
    }

    private void StateUpdate(GameState state)
{
    switch (state)
    {
        case GameState.Menu:
            UpdateMenu();
            break;
        case GameState.Playing:
            UpdatePlaying();
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
            if (PlayingUI != null) PlayingUI.SetActive(false);
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
                if (MenuUI != null) MenuUI.SetActive(true);
                Time.timeScale = 0;
                MoveBrushOffscreen();
                DeselectAllVertices();
                print("entermenu");
                break;
            case GameState.Playing:
                if (PlayingUI != null) PlayingUI.SetActive(true);
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
    private void UpdateDig(){}
    private void UpdateRaise(){}
    private void UpdateFlatten(){}
    private void UpdateSmooth(){}
    private void UpdatePaintGrass(){}
    private void UpdatePaintRock(){}
    private void UpdatePlaceTree(){}
    private void UpdateDemolish(){}
    private void UpdateCreateTrail(){}
    private void UpdateEditTrail(){
        
    }
    
    //handle button presses to change game state in all states.

    public void OnClickNewGame(){

    }
    public void OnClickResetGame(){

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
        }
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
