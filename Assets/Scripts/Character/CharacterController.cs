using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationController))]
public class CharacterController : MonoBehaviour, IDestructable {
	private const float GROUNDED_RADIUS = .05f;

	public CharacterStateMachine movementSM;
	public CharacterGroundedState grounded;
	public CharacterJumpingState jumping;
	public CharacterFallingState falling;

	protected Rigidbody2D rb;
	[HideInInspector] public AnimationController anim;
	[SerializeField] protected bool isFacingRight = true;
	[SerializeField] protected CharacterControllerData data;
	[SerializeField] protected Transform leftGroundCheck;
	[SerializeField] protected Transform rightGroundCheck;
	[SerializeField] private LayerMask whatIsGround;
	[SerializeField] private LayerMask whatIsPlatform;

	[SerializeField] private GameObject shot;

	[HideInInspector] public PlayerControls playerControls;

	private int health = 5;

	public int Health { get { return health; } }

	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = data.gravityScale;
		anim = GetComponent<AnimationController>();
		playerControls = new PlayerControls();

	}
	private void Start() {
		movementSM = new CharacterStateMachine();

		grounded = new CharacterGroundedState(this, movementSM);
		jumping = new CharacterJumpingState(this, movementSM);
		falling = new CharacterFallingState(this, movementSM);

		movementSM.Initialize(grounded);
	}

	private void Update() {
		movementSM.CurrentState.HandleInput();

		movementSM.CurrentState.LogicUpdate();
	}

	private void FixedUpdate() {
		movementSM.CurrentState.PhysicsUpdate();
	}
	public void Move(float horizontalMove) {
		if ((horizontalMove > 0 && !isFacingRight) || (horizontalMove < 0 && isFacingRight)) {
			Flip();
		}
		rb.velocity = new Vector2(horizontalMove * data.horizontalSpeed, rb.velocity.y);
	}
	public bool GetIfGrounded(bool ignorePlatforms = false) {
		LayerMask mask = whatIsGround;
		if (!ignorePlatforms) {
			mask = whatIsGround | whatIsPlatform;
		}
		return GetIfPointIsGrounded(leftGroundCheck, mask) || GetIfPointIsGrounded(rightGroundCheck, mask);
	}
	protected bool GetIfPointIsGrounded(Transform point, LayerMask whatIsGround) {
		bool isGrounded = false;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(point.position, GROUNDED_RADIUS, whatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				isGrounded = true;
			}
		}
		return isGrounded;
	}
	public void Jump(bool zeroX = false) {
		rb.transform.Translate(Vector2.up * (GROUNDED_RADIUS + 0.1f));
		Vector2 forceVector = new Vector2(0f, data.jumpForce);
		rb.velocity = new Vector2(zeroX ? 0 : rb.velocity.x, 0f);
		rb.AddForce(forceVector, ForceMode2D.Impulse);
	}
	public void OnGroundMovementReset() {
		rb.velocity = new Vector2(rb.velocity.x, 0);
	}
	public bool GetIfIsFalling() {
		bool isGrounded = GetIfGrounded();
		return !isGrounded && rb.velocity.y < 0;
	}
	public void SetGravityScale(bool falling = false) {
		rb.gravityScale = falling ? 6 : data.gravityScale;
	}
	public void Shoot() {
		StartCoroutine(ActivateShot());
	}
	private IEnumerator ActivateShot() {
		shot.SetActive(true);
		yield return new WaitForSeconds(0.22f);
		shot.SetActive(false);
	}
	protected void Flip() {
		isFacingRight = !isFacingRight;
		transform.Rotate(0f, 180f, 0f);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(leftGroundCheck.position, GROUNDED_RADIUS);
		Gizmos.DrawSphere(rightGroundCheck.position, GROUNDED_RADIUS);
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.layer == 8) { // enemy
			//Hit(1);
		}
	}
	public void Hit(int damage) {
		health -= damage;
		GameController.Instance.Hit(this);
		if (health <= 0) {
			gameObject.SetActive(false);
		}
	}
}
