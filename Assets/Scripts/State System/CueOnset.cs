﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueOnset : StateMachineBehaviour
{
    // This state predetermines all of the trial's task parameters,
    // resets task objects, and prepares the cue.

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ExperimentController expControl = ExperimentController.instance;
        expControl.ShowCues();
        
        // AS an example we will disable movement during the cue epoch
        expControl.FreezePlayer(true);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // As an example here we have the cues disappearing
        ExperimentController.instance.HideCues();
        ExperimentController.instance.FreezePlayer(false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}