using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Write Anything here, Generally for testing that doesn't require a dedicated script
/// </summary>
public class TemporaryBehaviour : MonoBehaviour
{
    public int Damage = 0;
    public int Turn = 0;

    // Update is called once per frame
    void Update()
    {
        if (Damage < 1000000)
        {
            Turn++;
            Damage += Turn;
        }
        else
        {
            Debug.Log($"Turns {Turn} -> Damage {Damage}");
        }
    }
}
