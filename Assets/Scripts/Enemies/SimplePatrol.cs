using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationController))]
public class SimplePatrol : MonoBehaviour
{
	private const float GROUNDED_RADIUS = .2f;

	[SerializeField] private Transform groundCheck;
	[SerializeField] private Transform platformEndCheck;
	[SerializeField] private Transform wallCheck;
	[SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private float speed = 2f;
	[SerializeField] private bool isFacingRight = true;

	private Rigidbody2D rb;
	private AnimationController animationController;
	private bool isGrounded;
	private bool isPlatformEnded = false;
	private void Awake() {
		rb = GetComponent<Rigidbody2D>();
		rb.freezeRotation = true;
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		animationController = GetComponent<AnimationController>();
	}
	private void Start() {
		Move(speed);
	}

	private void FixedUpdate() {
		bool wasGrounded = isGrounded;
		isGrounded = false;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, GROUNDED_RADIUS, _whatIsGround);
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders[i].gameObject != gameObject) {
				isGrounded = true;
			}
		}
		if (!isGrounded && wasGrounded) {
			Move(0);
		}
		if (isGrounded) {
			isPlatformEnded = !Physics2D.OverlapCircle(platformEndCheck.position, GROUNDED_RADIUS, _whatIsGround);
			if (!isPlatformEnded) {
				isPlatformEnded = Physics2D.OverlapCircle(wallCheck.position, GROUNDED_RADIUS, _whatIsGround);
			}
			if (isPlatformEnded) {
				Flip();
			}
		}
	}

	public void Flip() {
		isFacingRight = !isFacingRight;
		transform.Rotate(0f, 180f, 0f);
		Move(speed);
	}
	private void Move(float speed) {
		rb.velocity = new Vector2(speed * (isFacingRight ? 1 : -1), rb.velocity.y);
	}
	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(groundCheck.position, GROUNDED_RADIUS);
		Gizmos.DrawSphere(wallCheck.position, GROUNDED_RADIUS);
	}
}
