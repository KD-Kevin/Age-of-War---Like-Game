using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeam
{
    public int Team { get; set; }

    public abstract void SetTeam(int TeamID);
}
