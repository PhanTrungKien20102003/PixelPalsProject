using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generic class
public class ScriptableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{ 
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();

        //this function will only load the objects that are inside a folder call Resources
        var objectArray = Resources.LoadAll<T>("");
        foreach (var obj in objectArray)
        {
            if (objects.ContainsKey(obj.name))
            {
                Debug.LogError($"There is already an object called {obj.name}!");
                continue;
            }

            objects[obj.name] = obj;
        }
    }
    
    public static T GetObjectByName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"Object with the name {name} not found in database!");
            return null;
        }
        return objects[name];
    }
}
