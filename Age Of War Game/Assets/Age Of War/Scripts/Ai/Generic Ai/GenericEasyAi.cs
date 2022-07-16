using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.AI
{
    /// <summary>
    /// TBD
    /// 
    /// ----------------------------------- Decision Tree ----------------------------------------------------
    /// 1. TBD
    /// ------------------------------------------------------------------------------------------------------
    /// 
    /// </summary>
    public class GenericEasyAi : GameAi
    {
        [SerializeField]
        protected int MakeDecisionEveryXTurns = 4;

        protected int DecisionCounter = 0;

        public override AiDecisionTypes MakeDecision()
        {
            AiDecisionTypes Decision = AiDecisionTypes.NoDecision;

            DecisionCounter++;
            if (DecisionCounter == MakeDecisionEveryXTurns)
            {
                DecisionCounter = 0;
                // Go down decision tree

            }
            else
            {
                Decision = AiDecisionTypes.Idle;
            }

            lastDecisionType = Decision;
            return Decision;
        }
    }
}
