using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core
{
    public class BackgroundWeapon : MonoBehaviour, ITeam
    {
        public int Team { get => TeamID; set => SetTeam(value); }
        protected int TeamID = -1;

        public void SetTeam(int ID)
        {
            TeamID = ID;
        }
    }
}
