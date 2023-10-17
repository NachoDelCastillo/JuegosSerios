using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NX
{
    public class ResetAnimatorBool : StateMachineBehaviour
    {
        public string isInteracting = "isInteracting";
        public bool isInteractingStatus = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(isInteracting, isInteractingStatus);
        }
    }
}