using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer; //variable for the solid object that can't go through

    [SerializeField] LayerMask grassLayer;        //variable for grass layer to get Encounter battle

    [SerializeField] LayerMask interactableLayer; //variable to talk with NPCs

    [SerializeField] LayerMask playerLayer; //variable for NPCs to not walk through player

    //use singleton pattern so that this script can easily be accessed from any other script
    public static GameLayers Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }
    public LayerMask SolidLayer {
        get => solidObjectsLayer;
    }
    public LayerMask GrassLayer
    {
        get => grassLayer;
    }
    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }
}
