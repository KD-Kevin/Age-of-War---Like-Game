using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSprite : MonoBehaviour
{
    public Sprite SpriteForArmor;
    public ArmorSlot Slot;
    public RaceArmor ArmorForRace;
}

public enum ArmorSlot
{
    Helmet,
    Torso,
    Legs,
    Shoes,
    Number,
}

public enum RaceArmor
{
    Human,
    Elven,
    Goblin,
    Frogs,
    Turkey,
    Number,
}
