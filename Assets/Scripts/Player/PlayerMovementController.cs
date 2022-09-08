using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationController))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovementController : MonoBehaviour
{
	// Base parameters
	private const float GROUNDED_RADIUS = .1f;
	private const float MOVEMENT_SENSITIVITY = .1f;
	private const float ROLLING_TIMER = 0.23f;
	private const float FALLING_THRESHOLD = -.1f;
	private const float ROLLING_FALLING_THRESHOLD = -8f;

	// Crouch parameters
	private const float CEILING_RADIUS = .2f;

	// Climbing parameters
	private const float CLIMB_JUMP_LOCK_CONTROL_TIME = .3f;

	public bool IsClimbing { get { return _isClimbing; } }
	public bool IsCrouching { private set; get; }
	public float HurtTime { get { return hurtTime; } }

	[Header("======Base parameters======")]
	[SerializeField] private Transform _groundCheck;
	[SerializeField] private LayerMask _whatIsGround;
	[SerializeField] private bool _hasAirControl = true;
	[SerializeField] private float _horizontalMoveSpeed = 500f;
	[SerializeField] private float _jumpForce = 23f;
	[SerializeField] private float _defaultGravityScale = 5f;
	[SerializeField] private float _fallingGravityScale = 2.7f;

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
	[SerializeField] private float _bounceForceX = 39f;
	[SerializeField] private float _bounceForceY = 17f;

	[Header("=====Second Jump=====")]
	[SerializeField] GameObject _secondJump;
	[Header("=====Hurt=====")]
	[SerializeField] float hurtTime = 1f;
	[Header("=====Platforms=====")]
	[SerializeField] private LayerMask _whatIsPlatform;

	// Base parameters
	private AnimationController _animationController;
	private Rigidbody2D _rb;
	private bool _isGrounded, _isFacingRight = true, _jump = false;
	private float _horizontalMove;
	private float _oldVelocityY;
	private bool _hasRolled = false;
	private float _rollingTimer = 0f;

	// Crouch parameters
	private bool _wasCrouching = false;
	private float _verticalMove;

	// Climbing parameters
	private bool _isClimbing = false;
	private bool _isTouchingWall = false;
	private bool _isMoveLeftLocked = false;
	private bool _isMoveRightLocked = false;
	private float _climbJumpControlLockTimer = 0f;

	// Double jump
	private bool _isSecondJumpAvailable = false;

	// Hurt
	private bool _isHurt = false;
	private float _hurtTimer = 0f;
	private SpriteRenderer _spriteRenderer;

	private void Awake() {
		_rb = GetComponent<Rigidbody2D>();
		_rb.gravityScale = _defaultGravityScale;
		_rb.freezeRotation = true;
		_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		_animationController = GetComponent<AnimationController>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void FixedUpdate() {
		if (_isHurt) {
			_hurtTimer += Time.fixedDeltaTime;
			if (_hurtTimer > hurtTime) {
				_isHurt = false;
				_hurtTimer = 0f;
				_animationController.SetAnimationParameter("IsHurt", false);
				Physics2D.IgnoreLayerCollision(7, 8, false);
				_rb.gravityScale = _defaultGravityScale;
			}
			return;
		}

		bool wasGrounded = _isGrounded;
		_isGrounded = GetIfGrounded();
		_animationController.SetAnimationParameter("IsInAir", !_isGrounded);

		if (!_isGrounded) {
			// rolling
			if (!_hasRolled && _oldVelocityY > 0) {
				_rollingTimer += Time.fixedDeltaTime;
				if (_rollingTimer > ROLLING_TIMER) {
					_hasRolled = true;
					_rollingTimer = 0;
					_animationController.SetAnimationParameter("Roll");
				}
			}
			if (!_isClimbing) {
				// falling
				if (!_hasRolled && _oldVelocityY <= FALLING_THRESHOLD) {
					_rb.gravityScale = _fallingGravityScale;
					_animationController.SetAnimationParameter("Fall");
				// falling after rolling
				} else if (_hasRolled && _oldVelocityY <= ROLLING_FALLING_THRESHOLD) {
					_rb.gravityScale = _fallingGravityScale;
					_animationController.SetAnimationParameter("Fall");
				}
			}
		} else {
			// landing
			if (!wasGrounded) {
				_rb.gravityScale = _defaultGravityScale;
				_hasRolled = false;
				_animationController.ResetAllTriggers();
			}
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
		if (_isGrounded) {
			_isSecondJumpAvailable = false;
			_secondJump.SetActive(false);
			if (_isClimbing) {
				ReleaseWall();
			}
			jump = HandlePlatform(jump);
			if (jump && !_isClimbing) {
				_animationController.SetAnimationParameter("Jump");
				Jump(new Vector2(0f, _jumpForce));
			}
		} else {
			if (_isSecondJumpAvailable && jump) {
				_isSecondJumpAvailable = false;
				_secondJump.SetActive(false);
				_animationController.SetAnimationParameter("Jump");
				Jump(new Vector2(0f, _jumpForce));
			}
		}
		if (_isGrounded || _hasAirControl) {
			if (crouch && !_isClimbing) {
				if (!_wasCrouching) {
					_wasCrouching = true;
				}
				horizontalMove *= _crouchSpeedMultiplier;
				if (_crouchDisableCollider != null) {
					_crouchDisableCollider.enabled = false;
					IsCrouching = true;
				}
			} else {
				if (_crouchDisableCollider != null) {
					_crouchDisableCollider.enabled = true;
					IsCrouching = false;
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
		if (_canClimb) {
			_isTouchingWall = GetIfTouchingWall();
			if (!_isClimbing && !_isGrounded && _isTouchingWall) {
				ClingToWall();
			}
			if (_isClimbing) {
				if (_isTouchingWall) {
					bool dropWallWithJump = verticalMove >= 0;
					if (verticalMove < 0) {
						verticalMove *= 1.75f;
					}
					bool isWallTopWallReached = !Physics2D.OverlapCircle(_topWallCheck.position, GROUNDED_RADIUS, _whatIsGround);
					if (isWallTopWallReached || isCeilingCheck) {
						verticalMove = verticalMove > 0 ? 0 : verticalMove;
					}
					bool isBottomWallEnded = !Physics2D.OverlapCircle(_bottomWallCheck.position, GROUNDED_RADIUS, _whatIsGround);
					if (isBottomWallEnded) {
						dropWallWithJump = (verticalMove < 0 && jump) ? false : dropWallWithJump;
						verticalMove = verticalMove < 0 ? 0 : verticalMove;
					}
					_rb.velocity = new Vector2(0, verticalMove);
					if (jump) {
						ReleaseWall(dropWallWithJump);
					}
					_animationController.SetAnimationParameter("VerticalSpeed", Mathf.Abs(verticalMove));
				} else {
					ReleaseWall();
				}
			}
		}
	}
	private void Flip() {
		_isFacingRight = !_isFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}
	private void Jump(Vector2 forceVector, bool zeroX = false) {
		_rb.velocity = new Vector2(zeroX ? 0 : _rb.velocity.x, 0f);
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
		Vector2 forceVector = new Vector2(_bounceForceX, _bounceForceY);
		if (_isFacingRight) {
			forceVector.x *= -1;
		}
		if (jump) {
			_isMoveRightLocked = _isFacingRight;
			_isMoveLeftLocked = !_isFacingRight;
		}
		_rb.gravityScale = _defaultGravityScale;
		_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		_isClimbing = false;
		_animationController.SetAnimationParameter("IsClimbing", false);
		_animationController.SetAnimationParameter(jump ? "Jump" : "Fall");
		if (jump) {
			Jump(forceVector);
		} else {
			transform.Translate(-GROUNDED_RADIUS, 0, 0);
		}
	}
	private bool GetIfTouchingWall() {
		bool result = true;
		foreach (Transform wallcheck in _wallChecks) {
			result = Physics2D.OverlapCircle(wallcheck.position, GROUNDED_RADIUS, _whatIsGround);
			if (!result) {
				return result;
			}
		}
		return result;
	}
	private bool GetIfGrounded() {
		bool isGrounded = false;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, GROUNDED_RADIUS, _whatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				isGrounded = true;
			}
		}
		return isGrounded;
	}
	private bool HandlePlatform(bool jump) {
		Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, GROUNDED_RADIUS, _whatIsPlatform);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				if (_verticalMove < 0 && jump) {
					StartCoroutine(IgnorePlatforms(colliders[i]));
					return false;
				}
			}
		}
		return jump;
	}

	IEnumerator IgnorePlatforms(Collider2D collider) {
		collider.enabled = false;
		yield return new WaitForSeconds(0.5f);
		collider.enabled = true;
	}

	public void Hurt() {
		_animationController.SetAnimationParameter("IsHurt", true);
		_isHurt = true;
		Physics2D.IgnoreLayerCollision(7, 8);
		//Vector2 forceVector = new Vector2(_bounceForceX, _bounceForceY);
		Vector2 forceVector = new Vector2(5, 5);
		if (_isFacingRight) {
			forceVector.x *= -1;
		}
		_rb.gravityScale = 0.5f;
		Jump(forceVector, true);
		StartCoroutine(BlinkSprite(hurtTime, 10f));
	}

	IEnumerator BlinkSprite(float time, float frequency) {
		Color tmp = _spriteRenderer.color;
		bool swich = false;
		for (float i = 0; i < time; i += time / frequency) {
			tmp.a = swich? 0: 1;
			swich = !swich;
			_spriteRenderer.color = tmp;
			yield return new WaitForSeconds(time / frequency);
		}
		tmp.a = 1;
		_spriteRenderer.color = tmp;
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

	public void SetJump() {
		if (!_isGrounded && !_isClimbing) {
			_isSecondJumpAvailable = true;
			_secondJump.SetActive(true);
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
