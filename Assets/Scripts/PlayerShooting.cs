using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerShooting : MonoBehaviour
{
	[SerializeField] private float shootPower = 10f;
	[SerializeField] private GameObject shotPrefab;
	[SerializeField] private Transform gunPoint;
	[SerializeField] private float m_shotAnimationTime = .2f;
	private Animator animator;
	private void Start() {
		animator = GetComponent<Animator>();
	}
	public void OnShoot(InputAction.CallbackContext context) {
		if (context.started) {
			animator.SetTrigger("Shoot");
			GameObject newBullet = Instantiate(shotPrefab, gunPoint.position, gunPoint.rotation) as GameObject;
			newBullet.transform.SetParent(gameObject.transform);
			//newBullet.GetComponent<Rigidbody2D>().AddForce(gunPoint.forward * shootPower);
			Destroy(newBullet, m_shotAnimationTime);
		}
	}
}
