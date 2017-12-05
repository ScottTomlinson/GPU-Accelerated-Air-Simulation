using UnityEngine;
using System.Collections;

public class RNG : MonoBehaviour {

    public static float Floats(float min, float max)
    {
        float r;
        r = Random.Range(min, max);
        return r;
    }

    public static int Ints(int min, int max)
    {
        int i;
        i = Random.Range(min, max);
        return i;
    }
}
