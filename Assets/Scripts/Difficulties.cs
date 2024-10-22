using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Difficulties : MonoBehaviour
{
	public Difficulty difficulty;

	public void SetDifficulty(int d) {
		transform.GetChild((int) difficulty).GetComponent<Image>().color = Color.white;

		difficulty = (Difficulty) d;
		//gameObject.SetActive(false);

		transform.GetChild(d).GetComponent<Image>().color = new Color(0.8f, 1f, 0.8f);
	}
}
