using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door2 : MonoBehaviour
{
	public int detectRadius = 10;
	public bool canOpen = false;

	IEnumerator Start() {
		while (true) {
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

			canOpen = true;

			foreach (GameObject player in players) {
				if (Vector3.Distance(player.transform.position, transform.position) > detectRadius && player.transform.position.y > 490) canOpen = false;
			}

			yield return new WaitForSeconds(2);
		}
	}
}
