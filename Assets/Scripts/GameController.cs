using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] TMP_Text crystalCountText;
    private int _crystalCount = 0;
    private void Start() {
		crystalCountText.text = _crystalCount.ToString();
	}

    public void AddCrystalCount(int value) {
        _crystalCount += value;
        crystalCountText.text = _crystalCount.ToString();
	}
}
