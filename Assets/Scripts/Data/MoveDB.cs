using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();
        
        //this function will only load the objects that are inside a folder call Resources
        var moveList = Resources.LoadAll<MoveBase>("");
        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError($"There is already a move called {move.Name}!");
                continue;
            }
            
            moves[move.Name] = move;
        }
    }
    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Move with the name {name} not found in database!");
            return null;
        }
        return moves[name];
    }
}
