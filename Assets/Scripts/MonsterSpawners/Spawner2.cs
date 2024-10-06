using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Spawner2 : NetworkBehaviour
{
	public Object[] enemies;
	public Transform target;
	public GameObject spawned;
	[Min(1)] public int spawnInterval = 10;

	IEnumerator Start() {
		yield return new WaitUntil(() => isServer);

		spawned = Instantiate(enemies[Random.Range(0, enemies.Length)] as GameObject, target.position, Quaternion.identity);

		NetworkServer.Spawn(spawned);

		while (true) {
			if (spawned == null) {
				yield return new WaitForSeconds(spawnInterval);

				spawned = Instantiate(enemies[Random.Range(0, enemies.Length)] as GameObject, target.position, Quaternion.identity);

				NetworkServer.Spawn(spawned);
			}

			yield return new WaitForSeconds(0.1f);
		}
	}
}
