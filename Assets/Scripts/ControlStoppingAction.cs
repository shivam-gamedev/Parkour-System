using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlStoppingAction : StateMachineBehaviour
{
    PlayerMovement playercontroller;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playercontroller == null)
            playercontroller = animator.GetComponent<PlayerMovement>();

        playercontroller._HasControl = false;
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playercontroller._HasControl = true;
          
        animator.SetBool("isHighJump", false);
    }
}
