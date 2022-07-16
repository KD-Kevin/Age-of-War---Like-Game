using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core
{
    public abstract class RacePassive : MonoBehaviour
    {
        public abstract void OnStartEffects();

        public abstract void OnUpdateEffects();

        public abstract void OnPlayerBaseDamagedEffects();

        public abstract void OnOpponentBaseDamagedEffects();

        public abstract void OnPlayerUnitKilledEffects();

        public abstract void OnPlayerUnitSpawnEffects();

        public abstract void OnOpponentUnitKilledEffects();

        public abstract void OnOpponentUnitSpawnEffects();

        public abstract void OnPlayerUnitFinishingBlowEffects();

        public abstract void OnOpponentUnitFinishingBlowEffects();

        public abstract void OnPlayerUnitCommenceAttackEffects();

        public abstract void OnOpponentUnitCommenceAttackEffects();

        public abstract void OnPlayerUnitMovementEffects();

        public abstract void OnOpponentUnitMovementEffects();

        public abstract void OnPlayerTowerWeaponCreatedEffects();

        public abstract void OnOpponentTowerWeaponCreatedEffects();

        public abstract void OnPlayerBackgroundWeaponCreatedEffects();

        public abstract void OnOpponentFloorWeaponCreatedEffects();

        public abstract void OnPlayerFloorWeaponCreatedEffects();

        public abstract void OnOpponentBackgroundWeaponCreatedEffects();

        public abstract void OnPlayerTowerWeaponFinishingBlowEffects();

        public abstract void OnOpponentTowerWeaponFinishingBlowEffects();

        public abstract void OnPlayerBackgroundWeaponFinishingBlowEffects();

        public abstract void OnOpponentBackgroundWeaponFinishingBlowEffects();

        public abstract void OnOpponentFloorWeaponFinishingBlowEffects();

        public abstract void OnPlayerFloorWeaponFinishingBlowEffects();

        public abstract void OnOpponentFloorWeaponDestroyedEffects();

        public abstract void OnPlayerFloorWeaponDestroyedEffects();
    }
}
