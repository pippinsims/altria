using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;


[System.Serializable]
    public class Entry{ public int X, Y, Team; }

[System.Serializable]
    class Wrapper { public Entry[] items; }


public class LevelManager : MonoBehaviour
{
    public static List<Entry> Load(string path)
    {
        var json = File.ReadAllText(path);
        return new List<Entry>(JsonUtility.FromJson<Wrapper>("{\"items\":" + json + "}").items);
    }

    public List<GameObject> toSpawn;
    public string spawnValues;
    private TeamManager tm;

    public GameObject AIHolder;

    void Start()
    {
        tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
        tm = GameObject.Find("Team Manager").GetComponent<TeamManager>();
        var entries = Load(spawnValues);
        for (int i = 0; i < toSpawn.Count; i++)
        {
            var e = entries[i];
            if(toSpawn[i] != null)
            {
                if(toSpawn[i].GetComponent<AiUnitController>() == null)
                {
                    toSpawn[i].GetComponent<PlayerUnitController>().team = e.Team;
                    toSpawn[i].GetComponent<PlayerUnitController>().spawnLocation = new(e.X, e.Y);
                }
                else
                {
                    toSpawn[i].GetComponent<AiUnitController>().team = e.Team;
                    toSpawn[i].GetComponent<AiUnitController>().spawnLocation = new(e.X, e.Y);
                }
                Instantiate(toSpawn[i], AIHolder.transform);
            }
        }
        tm.GetComponent<TeamManager>().SetTeams();
    }
}