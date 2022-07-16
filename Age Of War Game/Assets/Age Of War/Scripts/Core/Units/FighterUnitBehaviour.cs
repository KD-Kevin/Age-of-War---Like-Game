using UnityEngine;

namespace AgeOfWar.Core.Units
{
    public class FighterUnitBehaviour : BaseUnitBehaviour
    {
        // Parameters
        [SerializeField]
        protected int MeleeDamage = 25;
        [SerializeField]
        protected int MagicMeleeDamage = 0;
        [SerializeField]
        protected int TrueMeleeDamage = 0;

        // Objects
        [SerializeField]
        protected MeleeBox MeleeAttackObject;

        public override int GetAttackDamage()
        {
            int AttackDamage = MeleeDamage;

            // Modifiers here

            return AttackDamage;
        }

        public override int GetMagicAttackDamage()
        {
            int AttackDamage = MagicMeleeDamage;

            // Modifiers here

            return AttackDamage;
        }

        public override int GetTrueAttackDamage()
        {
            int AttackDamage = TrueMeleeDamage;

            // Modifiers here

            return AttackDamage;
        }
    }
}
