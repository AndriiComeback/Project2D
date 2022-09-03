using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public enum JumpState { None, Accelerating, Fullspeed, Rolling, Falling }
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_horizontalMoveSpeed = 80f;
	[SerializeField] private float m_stopAcceleratingSpeedCap = 5f;
	[SerializeField] private float m_startRollingSpeedCap = 2f;
	[SerializeField] private float m_startFallingSpeedCap = -5f;
	private CharacterController characterController;
	private Animator animator;
	private Rigidbody2D rb;
	private float horizontalMove;
	private bool jump = false;
	private JumpState jumpState = JumpState.None;

	private void Start() {
		characterController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove * m_horizontalMoveSpeed));
	}

	private void FixedUpdate() {
		characterController.Move(horizontalMove * m_horizontalMoveSpeed * Time.fixedDeltaTime, false, jump);
		jump = false;

		if (jumpState == JumpState.Accelerating && rb.velocity.y > m_stopAcceleratingSpeedCap) {
			jumpState = JumpState.Fullspeed;
		}
		if (jumpState == JumpState.Fullspeed && rb.velocity.y < m_startRollingSpeedCap) {
			animator.SetTrigger("Roll");
			jumpState = JumpState.Rolling;
		}
		if (jumpState == JumpState.Rolling && rb.velocity.y < m_startFallingSpeedCap) {
			animator.SetTrigger("Fall");
			jumpState = JumpState.Falling;
		}
	}

	public void OnLanding() {
		if (jumpState != JumpState.None) {
			animator.SetTrigger("Land");
		}
		jumpState = JumpState.None;
	}

	public void OnLeaveGround() {
		if (jumpState == JumpState.None) {
			animator.SetTrigger("Fall");
			jumpState = JumpState.Falling;
		}
	}

	#region Input Methods: public

	public void OnMove(InputAction.CallbackContext context) {
		horizontalMove = context.ReadValue<float>();
	}
	public void OnJump(InputAction.CallbackContext context) {
		if (context.started) {
			if (jumpState == JumpState.None) {
				jump = true;
				jumpState = JumpState.Accelerating;
				animator.SetTrigger("Jump");
			}
		}
	}

	#endregion
}
