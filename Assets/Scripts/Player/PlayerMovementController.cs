using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationController))]
public class PlayerMovementController : MonoBehaviour
{
	// Nested classes, enums, delegates and events.
	// Static, const and readonly fields.
	// Fields and properties.
	// Constructors and finalizers.
	// Methods.

	// Public.
	// Internal.
	// Protected internal.
	// Protected.
	// Private.
	private const float GROUNDED_RADIUS = .1f;
	private const float MOVEMENT_SENSITIVITY = .1f;

	[SerializeField] private Transform _groundCheck;
	[SerializeField] private LayerMask _whatIsGround;
	[SerializeField] private bool _hasAirControl = true;
	[SerializeField] private float _horizontalMoveSpeed = 500f;
	[SerializeField] private float _jumpForce = 15f;

	private AnimationController _animationController;
	private Rigidbody2D _rb;
	private bool _isGrounded, _isFacingRight = true, _jump = false;
	private float _horizontalMove;

	private void Awake() {
		_rb = GetComponent<Rigidbody2D>();
		_rb.gravityScale = 2;
		_rb.freezeRotation = true;
		_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_animationController = GetComponent<AnimationController>();
	}

	private void FixedUpdate() {
		Debug.Log(Mathf.Abs(_rb.velocity.x));
		bool wasGrounded = _isGrounded;
		_isGrounded = false;

		Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, GROUNDED_RADIUS, _whatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				_isGrounded = true;
				if (!wasGrounded) {
					//OnLandEvent.Invoke();
				}
			}
		}
		if (!_isGrounded && wasGrounded) {
			//OnLeaveGroundEvent.Invoke();
		}

		_horizontalMove = NormalizeMovementSpeed(_horizontalMove, _horizontalMoveSpeed, MOVEMENT_SENSITIVITY);
		_animationController.SetAnimationParameter("HorizontalSpeed", Mathf.Abs(_horizontalMove));
		HandleMove(_horizontalMove * Time.fixedDeltaTime, 0 * Time.fixedDeltaTime, false, _jump);
		_jump = false;
	}

	private float NormalizeMovementSpeed(float speed, float maxSpeed, float sensitivity) {
		if (speed > sensitivity) {
			speed = maxSpeed;
		} else if (speed < -sensitivity) {
			speed = -maxSpeed;
		}
		return speed;
	}

	private void HandleMove(float horizontalMove, float verticalMove, bool crouch, bool jump) {
		//IsTopWallEnded = false;
		//IsCeilingCheck = Physics2D.OverlapCircle(ceilingCheck.position, k_CeilingRadius, whatIsGround);
		//if (!crouch) {
		//if (IsCeilingCheck) {
		//crouch = true;
		//}
		//}
		//if (isClimbing && m_Grounded) {
		//ReleaseWall();
		//}
		//only control the player if grounded or airControl is turned on
		if ((_isGrounded || _hasAirControl)/* && !isClimbing*/) {

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
			/*horizontalMove = Mathf.Clamp(horizontalMove, float.MinValue, _maxHorizontalMoveSpeed);
			Vector3 targetVelocity = new Vector2(horizontalMove, _rb.velocity.y);
			_rb.velocity = Vector3.SmoothDamp(_rb.velocity, targetVelocity, ref _velocity, _movementSmoothing);*/
			Vector2 playerVelocity = new Vector2(horizontalMove, _rb.velocity.y);
			_rb.velocity = playerVelocity;

			if (horizontalMove > 0 && !_isFacingRight) {
				Flip();
			} else if (horizontalMove < 0 && _isFacingRight) {
				Flip();
			}
		}
		if (_isGrounded && jump /*&& !isClimbing*/) {
			//m_Grounded = true;
			_animationController.SetAnimationParameter("Jump");
			Jump(0f, _jumpForce);
		}
		/*if (canClimb) {
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
		}*/
	}
	private void Flip() {
		_isFacingRight = !_isFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
	private void Jump(float forceX, float forceY) {
		_rb.velocity = new Vector2(_rb.velocity.x, 0f);
		_rb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
	}

	#region Input public methods

	public void OnHorizontalMove(InputAction.CallbackContext context) {
		_horizontalMove = context.ReadValue<float>();
	}

	public void OnJump(InputAction.CallbackContext context) {
		if (context.started) {
			//if (jumpState == JumpState.None || jumpState == JumpState.Climbing) {
			_jump = true;
			//jumpState = JumpState.Accelerating;
			//animator.SetTrigger("Jump");
			//}
		}
	}

	#endregion
}
