using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TBD
/// 
/// ----------------------------------- Decision Tree ----------------------------------------------------
/// 1. TBD
/// ------------------------------------------------------------------------------------------------------
/// 
/// </summary>
public class GenericHardAi : GameAi
{
    [SerializeField]
    protected int MakeDecisionEveryXTurns = 3;

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
