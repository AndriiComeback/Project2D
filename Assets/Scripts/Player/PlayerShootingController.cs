using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerShootingController : MonoBehaviour
{
	[SerializeField] private GameObject shotPrefab;
	[SerializeField] private Transform gunPoint;
	[SerializeField] private Transform backGunPoint;
	[SerializeField] private Transform crouchGunPoint;
	[SerializeField] private float m_shotAnimationTime = .2f;
	[SerializeField] private float m_ShotsPerSecond;
	private float shootTime = 0f;
	private bool shotOnCooldown = false;

	private AnimationController _animationController;
	private PlayerMovementController playerMovementController;

	public bool isShootingLocked = false;
	private float shootLockTime = 0f;

	private void Awake() {
		_animationController = GetComponent<AnimationController>();
		playerMovementController = GetComponent<PlayerMovementController>();
	}

	private void Update() {
		HandleShooting();
	}

	private void HandleShooting() {
		if (shotOnCooldown) {
			shootTime += Time.deltaTime;
			if (shootTime > 1f / m_ShotsPerSecond) {
				shotOnCooldown = false;
				shootTime = 0f;
			}
		}
		if (isShootingLocked) {
			shootLockTime += Time.deltaTime;
			if (shootLockTime > playerMovementController.HurtTime) {
				isShootingLocked = false;
				shootLockTime = 0f;
			}
		}
	}
	public void OnShoot(InputAction.CallbackContext context) {
		if (context.started) {
			if (shotOnCooldown || isShootingLocked) {
				return;
			}
			_animationController.SetAnimationParameter("Shoot");
			Transform point = gunPoint;
			if (playerMovementController.IsClimbing) {
				point = backGunPoint;
			} else if (playerMovementController.IsCrouching) {
				point = crouchGunPoint;
			}
			GameObject newBullet = Instantiate(shotPrefab, point.position, point.rotation) as GameObject;
			newBullet.transform.SetParent(gameObject.transform);
			Destroy(newBullet, m_shotAnimationTime);
			shotOnCooldown = true;
		}
	}

	public void LockShoot() {
		isShootingLocked = true;
	}
}
