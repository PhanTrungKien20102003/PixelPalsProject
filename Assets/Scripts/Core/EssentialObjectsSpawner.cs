using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this script will be responsible for spawning the essential objects
public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    //spawn the prefab if the essentialObjects doesn't exist in the scene
    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0) //this means the essentialObjects doesn't exist in the scene => spawn the prefab using Instantiate
        {
            Instantiate(essentialObjectsPrefab, new Vector3(0,0,0), Quaternion.identity);
        }
    }
}
