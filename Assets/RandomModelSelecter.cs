using UnityEngine;

public class RandomModelSelecter : MonoBehaviour
{
    public GameObject[] models;
    public MeshFilter meshFilter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // select a random model from the array
        int randomIndex = Random.Range(0, models.Length);
        // instantiate the selected model
       meshFilter = GetComponent<MeshFilter>();
       if(meshFilter.mesh == null)
       {
        meshFilter.mesh = models[randomIndex].GetComponent<MeshFilter>().mesh;
       }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
