using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A very easy ai, will only choose to buy unit's it can afford randomly or upgrade them randomly. 
/// 
/// ----------------------------------- Decision Tree ----------------------------------------------------
/// 1. The ai will only make a decision every 'x' times it is asked for a decision. So if it's not, go idle
/// 2. Sees if it can upgrade any, if multiple choose at random. 
/// 3. if no upgrade was chosen then see if it can buy a unit, if multiple choose randomly. 
/// 4. if no units were bought then go idle
/// ------------------------------------------------------------------------------------------------------
/// 
/// </summary>
public class GenericVeryEasyAi : GameAi
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
