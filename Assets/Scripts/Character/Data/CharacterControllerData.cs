using UnityEngine;

[CreateAssetMenu(fileName = "CharacterControllerData", menuName = "Game Data/Character Controller Data")]
public class CharacterControllerData : ScriptableObject {
	public float horizontalSpeed = 10f;
	public float jumpForce = 13f;
	public float gravityScale = 2f;
}