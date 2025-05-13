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
    public GameState currentGameState = GameState.Menu;
    
    MeshGenerator meshGenerator;

    public static event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        Debug.Log("GameManager Awake: " + this);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Destroying duplicate GameManager: " + this);
            Destroy(gameObject);
        }
        meshGenerator = FindFirstObjectByType<MeshGenerator>().GetComponent<MeshGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGameState();
        if(brush != null){
            print(brush);
        }
        print(brush);
    }

    private void UpdateGameState(){
        if(currentGameState != GameState.Menu && currentGameState != GameState.Playing){
            UpdateBrush(Color.white);
            if(Input.GetKeyDown(KeyCode.Escape)){
                ChangeGameState(GameState.Playing);
            }
        }
        switch(currentGameState){
            case GameState.Menu:
                UpdateMenu();
                break;
            case GameState.Playing:
                //switch game state
                UpdatePlaying();
                break;
            case GameState.Dig:
                UpdateDig();
                break;
            case GameState.Raise:
                UpdateRaise();
                break;
            case GameState.Flatten:
                UpdateFlatten();
                break;
            case GameState.Smooth:
                UpdateSmooth();
                break;
            case GameState.PaintGrass:
                UpdatePaintGrass();
                break;
            case GameState.PaintRock:
                UpdatePaintRock();
                break;
            case GameState.PlaceTree:
                UpdatePlaceTree();
                break;
            case GameState.Demolish:
                UpdateDemolish();
                break;
            case GameState.CreateTrail:
                UpdateCreateTrail();
                break;
            case GameState.EditTrail:
                UpdateEditTrail();
                break;
        }
    }

    public void ChangeGameState(GameState newState){
        EndGameState();

        // Always destroy the brush when entering Playing or Menu
        if ((newState == GameState.Playing || newState == GameState.Menu) && brush != null) {
            Destroy(brush);
            Debug.Log("Brush destroyed");
            //brush = null;
        }

        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);

        if(newState == GameState.Playing){
            Time.timeScale = 1;
        }

        if(newState != GameState.Menu && newState != GameState.Playing){
            brush = Instantiate(brushPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("Brush instantiated: " + brush);
        }
        Debug.Log("GameState changed to: " + newState);
        switch(newState){
            case GameState.Menu:
                Time.timeScale = 0;
                break;
            case GameState.Playing: 
                Time.timeScale = 1;
                break;
            case GameState.Dig:
                Time.timeScale = 0;
                break;
            case GameState.Raise:
                Time.timeScale = 0;
                break;
            case GameState.Flatten:
                Time.timeScale = 0;
                break;
            case GameState.Smooth:
                Time.timeScale = 0;
                break;
            case GameState.PaintGrass:
                Time.timeScale = 0; 
                break;
            case GameState.PaintRock:
                Time.timeScale = 0;
                break;
            case GameState.PlaceTree:
                Time.timeScale = 0;
                break;
            case GameState.Demolish:
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
        if (brush != null) Destroy(brush);
        switch(currentGameState){
            case GameState.Menu:
                if(MenuUI != null){
                    MenuUI.SetActive(false);
                }
                break;
            case GameState.Playing:
                Time.timeScale = 0;
                if(PlayingUI != null){
                    PlayingUI.SetActive(false);
                }
                break;
            case GameState.Dig:
            case GameState.Raise:
            case GameState.Flatten:
            case GameState.Smooth:
            case GameState.PaintGrass:
            case GameState.PaintRock:
            case GameState.PlaceTree:
            case GameState.Demolish:
            case GameState.CreateTrail:
            case GameState.EditTrail:
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
    public void OnClickDig(){
        ChangeGameState(GameState.Dig);
    }
    public void OnClickRaise(){
        ChangeGameState(GameState.Raise);
    }
    public void OnClickFlatten(){
        ChangeGameState(GameState.Flatten);
    }
    public void OnClickSmooth(){
        ChangeGameState(GameState.Smooth);
    }
    public void OnClickPaintGrass(){
        ChangeGameState(GameState.PaintGrass);
    }
    public void OnClickPaintRock(){
        ChangeGameState(GameState.PaintRock);
    }
    public void OnClickPlaceTree(){
        ChangeGameState(GameState.PlaceTree);
    }
    public void OnClickDemolish(){
        ChangeGameState(GameState.Demolish);
    }
    public void OnClickCreateTrail(){
        ChangeGameState(GameState.CreateTrail);
    }
    public void OnClickEditTrail(){
        ChangeGameState(GameState.EditTrail);
    }
    public void OnClickBack(){
        if(currentGameState != GameState.Menu && currentGameState != GameState.Playing){
            ChangeGameState(GameState.Playing);
        }
    }

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

    private void CanGoToDig(){
        if(Input.GetKeyDown(KeyCode.G)){
            ChangeGameState(GameState.Dig);
        }
    }

    private void CanGoToRaise(){
        if(Input.GetKeyDown(KeyCode.R)){
            ChangeGameState(GameState.Raise);
        }
    }

    private void CanGoToFlatten(){
        if(Input.GetKeyDown(KeyCode.F)){
            ChangeGameState(GameState.Flatten);
        }
    }

    private void CanGoToSmooth(){
        if(Input.GetKeyDown(KeyCode.S)){
            ChangeGameState(GameState.Smooth);
        }
    }

    private void CanGoToPaintGrass(){
        if(Input.GetKeyDown(KeyCode.H)){
            ChangeGameState(GameState.PaintGrass);
        }
    }

    private void CanGoToPaintRock(){
        if(Input.GetKeyDown(KeyCode.K)){
            ChangeGameState(GameState.PaintRock);
        }
    }

    private void CanGoToDemolish(){
        if(Input.GetKeyDown(KeyCode.D)){
            ChangeGameState(GameState.Demolish);
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

    public void UpdateBrush(Color highlightColor)
    {
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
                        v.SetColor(highlightColor); // Or your default color
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
                            v.SetColor(Color.red); // Set selected color
                        }
                    }
                }
                selectedVertices = selected.ToArray();
                meshGenerator.ApplyVertexColors(); // Update mesh colors

                previousSelectedVertices = selectedVertices;
            }
        } else {
            brush.transform.position = new Vector3(0, 0, -100);
        }
    }

}
