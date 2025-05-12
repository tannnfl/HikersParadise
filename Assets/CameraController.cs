using UnityEngine;

public class CameraController : MonoBehaviour
{
    //always face the center of the map; player can rotate it around the center by moving mouse with holding right click
    public GameObject Center;
    public float rotationSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float currentZoom = 10f; 
    public float moveSpeed = 10f;
    public GameObject treePrefab;
    public Vector3[] vertices; // Your vertex positions
    public GameObject[] treeInstances; // Array to store tree GameObjects

    void Start()
    {
        treeInstances = new GameObject[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            // Instantiate tree at vertex position
            GameObject tree = Instantiate(treePrefab, vertices[i], Quaternion.identity);
            // Store the tree GameObject in the array at the vertex index
            treeInstances[i] = tree;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //look at the center
        transform.LookAt(Center.transform);
        //zoom in and out with mouse wheel
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        transform.position = Center.transform.position - transform.forward * currentZoom;
        //rotate up and down with mouse wheel press
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.RotateAround(Center.transform.position, Vector3.up, mouseX * rotationSpeed);
        }

        // move center with wasd
        if (Input.GetKey(KeyCode.W))
        {
            Center.transform.position += transform.forward * moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Center.transform.position -= transform.forward * moveSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Center.transform.position -= transform.right * moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Center.transform.position += transform.right * moveSpeed;
        }
        
    }
    
}
