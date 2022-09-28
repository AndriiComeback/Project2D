using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static GameController;

public class InventoryItem : MonoBehaviour
{
    public CrystallType crystallType;
    public float quantity;
	public CrystallType CrystallType { get { return crystallType; } set { crystallType = value; } }
	public float Quantity { get { return quantity; } set { quantity = value; } }
	[SerializeField] Image image;
	[SerializeField] TMP_Text label;
	[SerializeField] TMP_Text count;
	[SerializeField] private List<Sprite> sprites;
	private InventoryUsedCallback callback;
	public InventoryUsedCallback Callback { get { return callback; } set { callback = value; } }
	private void Start() {
		string spriteNameToSearch = crystallType.ToString().ToLower();
		image.sprite = sprites.Find(x => x.name.Contains(spriteNameToSearch));
		label.text = spriteNameToSearch;
		count.text = Quantity.ToString();
		gameObject.GetComponent<Button>().onClick.AddListener(() => callback(this));
	}

}

