using UnityEngine;

public class CameraController : MonoBehaviour
{
    //always face the center of the map; player can rotate it around the center by moving mouse with holding right click
    public MeshGenerator meshGenerator;
    public float rotationSpeed = 10f;
    public float zoomSpeed = 10f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float currentZoom = 10f; 
    public Vector3 center;

    void Start()
    {
        Vector3 sum = Vector3.zero;
        foreach (var v in meshGenerator.vertices)
            sum += v;
        center = sum / meshGenerator.vertices.Length;
    }

    // Update is called once per frame
    void Update()
    {
        //look at the center
        transform.LookAt(center);
        //rotate around the center
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.RotateAround(center, Vector3.up, mouseX * rotationSpeed);
            transform.RotateAround(center, Vector3.right, mouseY * rotationSpeed);
        }
    }
    
}
