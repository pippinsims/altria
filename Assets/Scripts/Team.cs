using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public bool isAi;
    public List<Unit> members = new();

    public Team(bool ai = false)
    {
        isAi = ai;
    }
}
