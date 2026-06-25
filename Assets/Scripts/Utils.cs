using UnityEngine;

public static class Utils
{
    public static float ManhattanDistance(Transform t1, Transform t2)
    {
        return Mathf.Abs(t1.position.x - t2.position.x) + Mathf.Abs(t1.position.y - t2.position.y);
    }

    public static bool WithinPercent(int chance)
    {
        return Random.Range(0,101) <= chance;
    }
}