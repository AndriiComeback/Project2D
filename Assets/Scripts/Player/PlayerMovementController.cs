using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationController))]
public class PlayerMovementController : MonoBehaviour
{
	// Base parameters
	private const float GROUNDED_RADIUS = .2f;
	private const float MOVEMENT_SENSITIVITY = .1f;
	private const float ROLLING_THRESHOLD = 4f;
	private const float FALLING_THRESHOLD = -1f;

	// Crouch parameters
	private const float CEILING_RADIUS = .2f;

	// Climbing parameters
	private const float CLIMB_JUMP_LOCK_CONTROL_TIME = .3f;

	[Header("======Base parameters======")]
	[SerializeField] private Transform _groundCheck;
	[SerializeField] private LayerMask _whatIsGround;
	[SerializeField] private bool _hasAirControl = true;
	[SerializeField] private float _horizontalMoveSpeed = 500f;
	[SerializeField] private float _jumpForce = 15f;

	[Header("======Crouch parameters======")]
	[SerializeField] private Transform _ceilingCheck;
	[SerializeField] private Collider2D _crouchDisableCollider;
	[SerializeField, Range(0, 1)] private float _crouchSpeedMultiplier = 0;

	[Header("=====Climbing=====")]
	[SerializeField] private bool _canClimb = false;
	[SerializeField] private float _verticalMoveSpeed = 250f;
	[SerializeField] private Transform[] _wallChecks;
	[SerializeField] private Transform _topWallCheck;
	[SerializeField] private Transform _bottomWallCheck;
	[SerializeField] private float _bounceForceX = 30f;
	[SerializeField] private float _bounceForceY = 15f;

	// Base parameters
	private AnimationController _animationController;
	private Rigidbody2D _rb;
	private bool _isGrounded, _isFacingRight = true, _jump = false;
	private float _horizontalMove;
	private float _oldVelocityY;
	private bool _hasRolled = false;

	// Crouch parameters
	private bool _wasCrouching = false;
	private float _verticalMove;

	// Climbing parameters
	private bool _isClimbing = false;
	private bool _isTouchingWall = false;
	private bool _isMoveLeftLocked = false;
	private bool _isMoveRightLocked = false;
	private float _climbJumpControlLockTimer = 0f;

	private void Awake() {
		_rb = GetComponent<Rigidbody2D>();
		_rb.gravityScale = 2;
		_rb.freezeRotation = true;
		_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_animationController = GetComponent<AnimationController>();
	}

	private void FixedUpdate() {
		bool wasGrounded = _isGrounded;
		_isGrounded = false;


		Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, GROUNDED_RADIUS, _whatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				_isGrounded = true;
			}
		}
		_animationController.SetAnimationParameter("IsInAir", !_isGrounded);
		if (!_isGrounded && _rb.velocity.y < ROLLING_THRESHOLD && _rb.velocity.y < _oldVelocityY && !_hasRolled && _oldVelocityY > 0) {
			_hasRolled = true;
			_animationController.SetAnimationParameter("Roll");
		}
		if (!_isGrounded && _oldVelocityY <= FALLING_THRESHOLD && !_isClimbing) {
			_animationController.SetAnimationParameter("Fall");
		}
		if (_isGrounded && !wasGrounded) {
			_hasRolled = false;
			_animationController.ResetAllTriggers();
		}

		_horizontalMove = NormalizeMovementSpeed(_horizontalMove, _horizontalMoveSpeed, MOVEMENT_SENSITIVITY);
		_animationController.SetAnimationParameter("HorizontalSpeed", Mathf.Abs(_horizontalMove));
		_verticalMove = NormalizeMovementSpeed(_verticalMove, _verticalMoveSpeed, MOVEMENT_SENSITIVITY);
		bool crouch = _verticalMove < -.1f;

		if (!_isMoveLeftLocked && !_isMoveRightLocked) {
			HandleMove(_horizontalMove * Time.fixedDeltaTime, _verticalMove * Time.fixedDeltaTime, crouch, _jump);
		} else {
			_climbJumpControlLockTimer += Time.fixedDeltaTime;
			if (_climbJumpControlLockTimer > CLIMB_JUMP_LOCK_CONTROL_TIME) {
				_climbJumpControlLockTimer = 0;
				_isMoveLeftLocked = false;
				_isMoveRightLocked = false;
			}
			if ((_horizontalMove < 0 && _isMoveLeftLocked) || (_horizontalMove > 0 && _isMoveRightLocked)) {
				HandleMove(0, _verticalMove * Time.fixedDeltaTime, false, _jump);
			} else {
				HandleMove(_horizontalMove * Time.fixedDeltaTime, _verticalMove * Time.fixedDeltaTime, crouch, _jump);
			}
		}

		_jump = false;
		_oldVelocityY = _rb.velocity.y;
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
		bool isCeilingCheck = Physics2D.OverlapCircle(_ceilingCheck.position, CEILING_RADIUS, _whatIsGround);
		if (!crouch) {
			if (isCeilingCheck) {
				crouch = true;
			}
		}
		_animationController.SetAnimationParameter("IsCrouching", crouch);
		if (_isClimbing && _isGrounded) {
			ReleaseWall();
		}
		if (_isGrounded || _hasAirControl) {
			if (crouch && !_isClimbing) {
				if (!_wasCrouching) {
					_wasCrouching = true;
				}
				horizontalMove *= _crouchSpeedMultiplier;
				if (_crouchDisableCollider != null) {
					_crouchDisableCollider.enabled = false;
				}
			} else {
				if (_crouchDisableCollider != null) {
					_crouchDisableCollider.enabled = true;
				}
				if (_wasCrouching) {
					_wasCrouching = false;
				}
			}
			if (!_isClimbing) {
				if ((horizontalMove > 0 && !_isFacingRight) || (horizontalMove < 0 && _isFacingRight)) {
					Flip();
				}
				Vector2 playerVelocity = new Vector2(horizontalMove, _rb.velocity.y);
				_rb.velocity = playerVelocity;
			}
		}
		if (_isGrounded && jump && !_isClimbing) {
			_animationController.SetAnimationParameter("Jump");
			Jump(new Vector2(0f, _jumpForce));
		}
		if (_canClimb) {
			foreach (Transform wallcheck in _wallChecks) {
				_isTouchingWall = Physics2D.OverlapCircle(wallcheck.position, GROUNDED_RADIUS, _whatIsGround);
				if (!_isTouchingWall) {
					break;
				}
			}
			if (!_isGrounded && !_isClimbing && _isTouchingWall) {
				ClingToWall();
			}
			if (_isClimbing && _isTouchingWall) {
				bool isWallTopWallReached = !Physics2D.OverlapCircle(_topWallCheck.position, GROUNDED_RADIUS, _whatIsGround);
				if (isWallTopWallReached || isCeilingCheck) {
					if (verticalMove > 0) {
						verticalMove = 0;
					}
				}
				bool isBottomWallEnded = !Physics2D.OverlapCircle(_bottomWallCheck.position, GROUNDED_RADIUS, _whatIsGround);
				if (isBottomWallEnded) {
					if (verticalMove < 0) {
						verticalMove = 0;
					}
				}
				if (verticalMove < 0) {
					verticalMove *= 1.75f;
				}
				Vector2 playerVelocity = new Vector2(0, verticalMove);
				_rb.velocity = playerVelocity;
				if (jump) {
					ReleaseWall(verticalMove >= 0);
				}
				_animationController.SetAnimationParameter("VerticalSpeed", Mathf.Abs(verticalMove));
			}
			if (_isClimbing && !_isTouchingWall) {
				ReleaseWall();
			}
		}
	}
	private void Flip() {
		_isFacingRight = !_isFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
	private void Jump(Vector2 forceVector) {
		_rb.velocity = new Vector2(_rb.velocity.x, 0f);
		_rb.AddForce(forceVector, ForceMode2D.Impulse);
	}

	private void ClingToWall() {
		_isClimbing = true;
		_rb.gravityScale = 0;
		_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
		_hasRolled = false;
		_animationController.ResetAllTriggers();
		_animationController.SetAnimationParameter("IsClimbing", true);
		_animationController.SetAnimationParameter("Climb");
	}

	private void ReleaseWall(bool jump = false) {
		Debug.Log(jump);
		Vector2 forceVector = new Vector2(_bounceForceX / 3f, 0f);
		if (jump) {
			forceVector = new Vector2(_bounceForceX, _bounceForceY);
			_isMoveRightLocked = _isFacingRight;
			_isMoveLeftLocked = !_isFacingRight;
		}
		if (_isFacingRight) {
			forceVector.x *= -1;
		}
		_rb.gravityScale = 2;
		_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		Jump(forceVector);
		_isClimbing = false;
		_animationController.SetAnimationParameter("IsClimbing", false);
		_animationController.SetAnimationParameter(jump? "Jump" : "Fall");
	}

	#region Input public methods

	public void OnHorizontalMove(InputAction.CallbackContext context) {
		_horizontalMove = context.ReadValue<float>();
	}

	public void OnVerticalMove(InputAction.CallbackContext context) {
		_verticalMove = context.ReadValue<float>();
	}

	public void OnJump(InputAction.CallbackContext context) {
		if (context.started) {
			_jump = true;
		}
	}

	#endregion
	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(_bottomWallCheck.position, GROUNDED_RADIUS);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(_topWallCheck.position, GROUNDED_RADIUS);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_wallChecks[0].position, GROUNDED_RADIUS);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_wallChecks[1].position, GROUNDED_RADIUS);
	}
}
