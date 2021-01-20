using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterTrialInterval : StateMachineBehaviour
{
    // This state predetermines all of the trial's task parameters,
    // resets task objects, and prepares the cue.
    [Tooltip("Set to true to prevent ITI exit while joystick is touched.")]public bool IsJoystickBlocking = false; 

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ExperimentController expControl = ExperimentController.instance;
        expControl.PrepareTrial();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // On State Update is executed AFTER the main game logic update so the joystick data should be
        // up to date here for the current frame. 
        if (IsJoystickBlocking)
            ExperimentController.instance.JoystickBlock();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
