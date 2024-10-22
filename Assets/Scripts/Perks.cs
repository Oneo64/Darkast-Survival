using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Perks : MonoBehaviour
{
	public Perk perk;

	public void SetPerk(int p) {
		transform.GetChild((int) perk).GetComponent<Image>().color = Color.white;

		perk = (Perk) p;
		//gameObject.SetActive(false);

		transform.GetChild(p).GetComponent<Image>().color = new Color(0.8f, 1f, 0.8f);
	}
}
