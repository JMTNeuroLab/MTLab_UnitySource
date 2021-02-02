using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public enum StateNames
    {
        ITI,
        StartOfTrial,
        Delay_1,
        Cue,
        Delay_2,
        Distractor,
        Delay_3,
        ForceFOV,
        Target,
        Response,
        EndOfTrial,
        Feedback,
        Pause,
        Resume,
        Setup,
        QuarterReward,
        HalfReward,
        ThreeQuaurterReward,
        FullReward,
        NoReward,
        Null
    }

    public enum MultipleRewardMultiplier
    {
        Quarter,
        Half, 
        ThreeQuarter,
        Full, 
        None
    }

    public class Misc
    {
    }
}
