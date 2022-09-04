using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private void Awake() {
        animator = GetComponent<Animator>();
    }
    public void SetAnimationParameter(string animationName, bool state) {
        animator.SetBool(animationName, state);
    }
	public void SetAnimationParameter(string animationName, float value) {
		animator.SetFloat(animationName, value);
	}
	public void SetAnimationParameter(string animationName) {
		animator.SetTrigger(animationName);
	}
}
