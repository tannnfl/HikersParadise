using UnityEngine;

public class CameraDepth : MonoBehaviour
{
    
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
