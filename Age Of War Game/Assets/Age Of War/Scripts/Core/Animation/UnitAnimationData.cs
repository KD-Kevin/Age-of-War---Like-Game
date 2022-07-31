using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Animation
{
    [CreateAssetMenu(fileName = "Unit Animation Data", menuName = "AgeOfWar/Data/UnitAnimationData")]
    public class UnitAnimationData : ScriptableObject
    {
        public UnitAnimationNode[] UnitAnimations;

        public string GetAnimationForKey(UnitAnimationKey animationName)
        {
            for (int i = 0; i < UnitAnimations.Length; i++)
            {
                if (UnitAnimations[i].AnimationKey == animationName)
                {
                    return UnitAnimations[i].AnimationValue;
                }
            }
            return UnitAnimations[0].AnimationValue;
        }
    }

    [Serializable]
    public class UnitAnimationNode
    {
        public UnitAnimationKey AnimationKey;
        public string AnimationValue;
    }

    public enum UnitAnimationKey
    {
        Idle,
        Walk,
        Death1,
        Death2,
        Death3,
        PrepareToAttack,
        Attack1,
        Attack2,
        Attack3,
        Jump,
        InAir,
        Land,
        Run,
        Win,
        Lose, 
        Draw
    }
}
