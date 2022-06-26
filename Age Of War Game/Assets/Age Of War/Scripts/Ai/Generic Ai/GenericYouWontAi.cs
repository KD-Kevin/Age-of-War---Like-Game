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
public class GenericYouWontAi : GameAi
{
    public override AiDecisionTypes MakeDecision()
    {
        AiDecisionTypes Decision = AiDecisionTypes.NoDecision;

        lastDecisionType = Decision;
        return Decision;
    }
}
