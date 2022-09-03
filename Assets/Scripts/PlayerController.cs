using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public enum JumpState { None, Accelerating, Fullspeed, Rolling, Falling, Climbing }
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
	[Header("-----Shooting-----")]
	[SerializeField] private float shootPower = 10f;
	[SerializeField] private GameObject shotPrefab;
	[SerializeField] private Transform gunPoint;
	[SerializeField] private Transform backGunPoint;
	[SerializeField] private float m_shotAnimationTime = .2f;
	[SerializeField] private float m_ShotsPerSecond;
	private float shootTime = 0f;
	private bool shotOnCooldown = false;

	private float verticalMove;
	[SerializeField] private float m_verticalMoveSpeed = 40f;

	private void Start() {
		characterController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		animator.SetFloat("Speed", Mathf.Abs(horizontalMove * m_horizontalMoveSpeed));
		HandleShooting();
	}

	private void FixedUpdate() {
		if (Mathf.Abs(rb.velocity.y) < .01f && characterController.IsTopWallEnded) {
			verticalMove = Mathf.Clamp(verticalMove, float.MinValue, 0f);
		}
		animator.SetFloat("ClimbSpeed", verticalMove);

		characterController.Move(horizontalMove * m_horizontalMoveSpeed * Time.fixedDeltaTime,
			verticalMove * m_verticalMoveSpeed * Time.fixedDeltaTime,
			false, jump);
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

	private void HandleShooting() {
		if (shotOnCooldown) {
			shootTime += Time.deltaTime;
			if (shootTime > 1f / m_ShotsPerSecond) {
				shotOnCooldown = false;
				shootTime = 0f;
			}
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

	public void OnClimb() {
		jumpState = JumpState.Climbing;
		animator.SetTrigger("Climb");
	}

	#region Input Methods: public

	public void OnHorizontalMove(InputAction.CallbackContext context) {
		horizontalMove = context.ReadValue<float>();
	}
	public void OnVerticalMove(InputAction.CallbackContext context) {
		verticalMove = context.ReadValue<float>();
	}
	public void OnJump(InputAction.CallbackContext context) {
		if (context.started) {
			if (jumpState == JumpState.None || jumpState == JumpState.Climbing) {
				jump = true;
				jumpState = JumpState.Accelerating;
				animator.SetTrigger("Jump");
			}
		}
	}

	public void OnShoot(InputAction.CallbackContext context) {
		if (context.started) {
			if (shotOnCooldown) {
				return;
			}
			animator.SetTrigger("Shoot");
			Transform point = gunPoint;
			if (jumpState == JumpState.Climbing) {
				point = backGunPoint;
			}
			GameObject newBullet = Instantiate(shotPrefab, point.position, point.rotation) as GameObject;
			//newBullet.transform.SetParent(gameObject.transform);
			Destroy(newBullet, m_shotAnimationTime);
			shotOnCooldown = true;
		}
	}

	#endregion
}
