using UnityEngine;

public class Encounters : MonoBehaviour
{
    [SerializeField]
    private GameObject[] Dungeons;

    public Transform Spawnpoint_1;

    void Start()
    {
        // Pick a random number between 0 and the length of your list
        int randomIndex = Random.Range(0, Dungeons.Length);

        // Pick that specific monster from the array using [randomIndex]
        GameObject selectedMonster = Dungeons[randomIndex];

        // Spawn only that one selected monster
        Instantiate(selectedMonster, Spawnpoint_1.position, Spawnpoint_1.rotation);
    }
}