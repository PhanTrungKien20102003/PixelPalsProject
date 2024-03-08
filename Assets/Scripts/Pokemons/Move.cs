using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
    public MoveBase Base { get; set; } /* I don't use the short way like this in PokemonBase.cs because I want the variables 
                                          to be shown in the inspector */
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.Name,
            pp = PP
        };
        return saveData;
    }
}

[Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
