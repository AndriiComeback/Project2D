using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static GameController;

public class InventoryUIButton : MonoBehaviour
{
    public InventoryItem itemData;
	public InventoryItem ItemData { get { return itemData; } set { itemData = value; } }
	[SerializeField] Image image;
	[SerializeField] TMP_Text label;
	[SerializeField] TMP_Text count;
	[SerializeField] private List<Sprite> sprites;
	private InventoryUsedCallback callback;
	public InventoryUsedCallback Callback { get { return callback; } set { callback = value; } }
	private void Start() {
		string spriteNameToSearch = ItemData.crystallType.ToString().ToLower();
		image.sprite = sprites.Find(x => x.name.Contains(spriteNameToSearch));
		label.text = spriteNameToSearch;
		count.text = ItemData.quantity.ToString();
		gameObject.GetComponent<Button>().onClick.AddListener(() => callback(this));
	}

}

