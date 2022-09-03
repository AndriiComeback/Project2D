using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;                          
	[Range(0, 1)][SerializeField] private float m_CrouchSpeed = .0f;
	[Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;  
	[SerializeField] private bool m_AirControl = true;
	[SerializeField] private LayerMask m_WhatIsGround;
	[SerializeField] private Transform m_GroundCheck;
	[SerializeField] private Transform m_CeilingCheck;
	[SerializeField] private Collider2D m_CrouchDisableCollider;

	[SerializeField] public bool CanClimb = true;
	[SerializeField] private Transform wallCheck;
	[SerializeField] private Transform topWallCheck;
	[SerializeField] private Transform bottomWallCheck;
	private bool isTouchingWall = false;
	private bool isClimbing = false;
	[HideInInspector] public bool IsTopWallEnded = false;

	const float k_GroundedRadius = .1f;
	private bool m_Grounded;
	const float k_CeilingRadius = .2f;
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;
	public UnityEvent OnLeaveGroundEvent;
	public UnityEvent OnClimbEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake() {
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnLeaveGroundEvent == null)
			OnLeaveGroundEvent = new UnityEvent();

		if (OnClimbEvent == null)
			OnClimbEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate() {
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				m_Grounded = true;
				if (!wasGrounded) {
					OnLandEvent.Invoke();
				}
			}
		}
		if (!m_Grounded && wasGrounded) {
			OnLeaveGroundEvent.Invoke();
		}
	}


	public void Move(float horizontalMove, float verticalMove, bool crouch, bool jump) {
		IsTopWallEnded = false;
		// If crouching, check to see if the character can stand up
		if (!crouch) {
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) {
				crouch = true;
			}
		}
		if (m_Grounded && isClimbing) {
			ReleaseWall();
		}
		//only control the player if grounded or airControl is turned on
		if ((m_Grounded || m_AirControl) && !isClimbing) {

			// If crouching
			if (crouch) {
				if (!m_wasCrouching) {
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				horizontalMove *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else {
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching) {
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}
			Vector3 targetVelocity = new Vector2(horizontalMove * 10f, m_Rigidbody2D.velocity.y);
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			if (horizontalMove > 0 && !m_FacingRight) {
				Flip();
			}
			else if (horizontalMove < 0 && m_FacingRight) {
				Flip();
			}
		}
		if (m_Grounded && jump && !isClimbing) {
			m_Grounded = true;
			Jump(0f, m_JumpForce);
		}
		if (CanClimb) {
			isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, k_GroundedRadius, m_WhatIsGround);
			if (!m_Grounded && !isClimbing && isTouchingWall) {
				ClingToWall();
			}
			if (isClimbing && isTouchingWall) {
				IsTopWallEnded = !Physics2D.OverlapCircle(topWallCheck.position, k_GroundedRadius, m_WhatIsGround);
				if (IsTopWallEnded) {
					verticalMove = Mathf.Clamp(verticalMove, float.MinValue, 0f);
				}
				bool isBottomWallEnded = !Physics2D.OverlapCircle(bottomWallCheck.position, k_GroundedRadius, m_WhatIsGround);
				if (isBottomWallEnded) {
					verticalMove = Mathf.Clamp(verticalMove, 0f, float.MaxValue);
				}
				if (verticalMove < 0) {
					verticalMove *= 1.5f;
				}
				Vector3 targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, verticalMove * 10f);
				m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

				if (jump) {
					ReleaseWall(true);
				}
			}
			if (isClimbing && !isTouchingWall) {
				ReleaseWall();
			}
		}
	}
	private void ClingToWall() {
		isClimbing = true;
		m_Rigidbody2D.gravityScale = 0;
		m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
		OnClimbEvent.Invoke();
	}

	private void ReleaseWall(bool jump = false) {
		float forceY = 0f;
		float forceX = 0f;
		if (jump) {
			forceY = m_JumpForce;
			forceX = 400f;
		}
		if (m_FacingRight) {
			forceX *= -1;
		}
		m_Rigidbody2D.gravityScale = 2;
		m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
		Jump(forceX, forceY);
		isClimbing = false;
	}

	private void Jump(float forceX, float forceY) {
		m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
		m_Rigidbody2D.AddForce(new Vector2(forceX, forceY));
	}

	private void Flip() {
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		transform.Rotate(0f, 180f, 0f);
	}
}
