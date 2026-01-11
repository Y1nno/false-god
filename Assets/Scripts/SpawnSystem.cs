using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public GameObject Wraith_Prefab;
    
    public Transform Spawnpoint_1;

    void Start()
    {
        
        Instantiate(Wraith_Prefab, Spawnpoint_1.position, Spawnpoint_1.rotation);
    }

    void Update()
    {

    }
}