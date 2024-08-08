using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeDown : StateMachineBehaviour
{
    public AnimationCurve curve;
    private float elapsedTime = 0;
    private Vector3 originalPosition;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        curve = new AnimationCurve();
        curve.AddKey(0f, 0f);
        curve.AddKey(0.05f, -0.2f);
        curve.AddKey(0.15f, -0.3f);
        curve.AddKey(0.35f, -0.2f);
        curve.AddKey(0.5f, 0f);
        originalPosition = animator.transform.position;
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Time.timeScale != 0)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / 1);
            float yOffset = strength * 2 * (1 - stateInfo.normalizedTime);
            float maxYOffset = 0.5f;
            yOffset = Mathf.Clamp(yOffset, -maxYOffset, maxYOffset);
            Vector3 shakeOffset = new Vector3(0, yOffset, 0);
            animator.gameObject.transform.position = originalPosition + shakeOffset;
        }
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        elapsedTime = 0;
        animator.gameObject.transform.position = originalPosition;
    }
}
