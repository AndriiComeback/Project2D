using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
	[SerializeField] private float jumpForce = 15f;                          
	//[Range(0, 1)][SerializeField] private float m_CrouchSpeed = .0f;
	[Range(0, .3f)][SerializeField] private float movementSmoothing = .05f;  
	[SerializeField] private bool airControl = true;
	[SerializeField] private LayerMask whatIsGround;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private Transform ceilingCheck;
	//[SerializeField] private Collider2D crouchDisableCollider;

	[Header("=====Climbing=====")]
	[SerializeField] private bool canClimb = false;
	[SerializeField] private Transform wallCheck1;
	[SerializeField] private Transform wallCheck2;
	[SerializeField] private Transform topWallCheck;
	[SerializeField] private Transform bottomWallCheck;
	private bool isTouchingWall = false;
	private bool isClimbing = false;
	[HideInInspector] public bool IsTopWallEnded = false;
	[HideInInspector] public bool IsCeilingCheck = false;

	const float k_GroundedRadius = .1f;
	private bool m_Grounded;
	const float k_CeilingRadius = .2f;
	private Rigidbody2D rb;
	private bool m_FacingRight = true;
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;
	public UnityEvent OnLeaveGroundEvent;
	public UnityEvent OnClimbEvent;

	//[System.Serializable]
	//public class BoolEvent : UnityEvent<bool> { }

	//public BoolEvent OnCrouchEvent;
	//private bool m_wasCrouching = false;

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = 2;
		rb.freezeRotation = true;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnLeaveGroundEvent == null)
			OnLeaveGroundEvent = new UnityEvent();

		if (OnClimbEvent == null)
			OnClimbEvent = new UnityEvent();

		//if (OnCrouchEvent == null)
			//OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate() {
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, k_GroundedRadius, whatIsGround);
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
		IsCeilingCheck = Physics2D.OverlapCircle(ceilingCheck.position, k_CeilingRadius, whatIsGround);
		if (!crouch) {
			if (IsCeilingCheck) {
				crouch = true;
			}
		}
		if (isClimbing && m_Grounded) {
			ReleaseWall();
		}
		//only control the player if grounded or airControl is turned on
		if ((m_Grounded || airControl) && !isClimbing) {

			// If crouching
			if (crouch) {
				//if (!m_wasCrouching) {
					//m_wasCrouching = true;
					//OnCrouchEvent.Invoke(true);
				//}

				//horizontalMove *= m_CrouchSpeed;

				//if (crouchDisableCollider != null)
					//crouchDisableCollider.enabled = false;
			} else {
				//if (m_CrouchDisableCollider != null)
					//m_CrouchDisableCollider.enabled = true;

				//if (m_wasCrouching) {
					//m_wasCrouching = false;
					//OnCrouchEvent.Invoke(false);
				//}
			}
			Vector3 targetVelocity = new Vector2(horizontalMove * 10f, rb.velocity.y);
			rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

			if (horizontalMove > 0 && !m_FacingRight) {
				Flip();
			}
			else if (horizontalMove < 0 && m_FacingRight) {
				Flip();
			}
		}
		if (m_Grounded && jump && !isClimbing) {
			m_Grounded = true;
			Jump(0f, jumpForce);
		}
		if (canClimb) {
			isTouchingWall = Physics2D.OverlapCircle(wallCheck1.position, k_GroundedRadius, whatIsGround) &&
				Physics2D.OverlapCircle(wallCheck2.position, k_GroundedRadius, whatIsGround);
			if (!m_Grounded && !isClimbing && isTouchingWall) {
				ClingToWall();
			}
			if (isClimbing && isTouchingWall) {
				IsTopWallEnded = !Physics2D.OverlapCircle(topWallCheck.position, k_GroundedRadius, whatIsGround);
				if (IsTopWallEnded) {
					verticalMove = Mathf.Clamp(verticalMove, float.MinValue, 0f);
				}
				bool isBottomWallEnded = !Physics2D.OverlapCircle(bottomWallCheck.position, k_GroundedRadius, whatIsGround);
				if (isBottomWallEnded) {
					verticalMove = Mathf.Clamp(verticalMove, 0f, float.MaxValue);
				}
				if (verticalMove < 0) {
					verticalMove *= 1.5f;
				}
				Vector3 targetVelocity = new Vector2(rb.velocity.x, verticalMove * 10f);
				rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

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
		rb.gravityScale = 0;
		rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
		OnClimbEvent.Invoke();
	}

	private void ReleaseWall(bool jump = false) {
		float forceY = 0f;
		float forceX = 0f;
		if (jump) {
			forceY = jumpForce;
			forceX = jumpForce;
		}
		if (m_FacingRight) {
			forceX *= -1;
		}
		rb.gravityScale = 2;
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		Jump(forceX, forceY);
		isClimbing = false;
	}

	private void Jump(float forceX, float forceY) {
		rb.velocity = new Vector2(rb.velocity.x, 0f);
		rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
	}

	private void Flip() {
		m_FacingRight = !m_FacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
}
