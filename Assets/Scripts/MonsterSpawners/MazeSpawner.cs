using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class MazeSpawner : NetworkBehaviour
{
	public Object[] specialEnemies;
	public GameObject specialEnemy;

	public Object[] rareEnemies;
	public Object[] commonEnemies;

	public int maxEnemies = 50;
	public int maxSpawnRange = 60;
	public int spawnRange = 20;
	[Min(1)] public int spawnInterval = 10;

	IEnumerator Start() {
		if (GetComponent<Maze>() || GetComponent<DynamicMaze>()) {
			yield return new WaitUntil(() => isServer && ((GetComponent<Maze>() && GetComponent<Maze>().spawnedTiles.Count > 0) || (GetComponent<DynamicMaze>() && GetComponent<DynamicMaze>().spawnedTiles.Count > 0)));
		}
		
		Transform[] tiles = {};

		if (GetComponent<Maze>()) tiles = GetComponent<Maze>().spawnedTiles.ToArray();
		if (GetComponent<DynamicMaze>()) tiles = GetComponent<DynamicMaze>().spawnedTiles.ToArray();

		if (tiles.Length == 0) {
			List<Transform> l = new List<Transform>() {};

			foreach (GameObject g in GameObject.FindGameObjectsWithTag("Spawner")) {
				l.Add(g.transform);
			}

			tiles = l.ToArray();
		}

		while (true) {
			if (GetEnemyCount() < maxEnemies) {
				Transform tile = tiles[Random.Range(0, tiles.Length)];

				bool canSpawn = true;

				foreach (PlayerCore player in Object.FindObjectsOfType<PlayerCore>()) {
					if (Vector3.Distance(player.transform.position, tile.position) < spawnRange || Vector3.Distance(player.transform.position, tile.position) > maxSpawnRange) canSpawn = false;
				}

				if (canSpawn) {
					if (Random.Range(1, 5) == 1) {
						NetworkServer.Spawn(Instantiate(rareEnemies[Random.Range(0, rareEnemies.Length)] as GameObject, tile.position, Quaternion.identity));
					} else {
						NetworkServer.Spawn(Instantiate(commonEnemies[Random.Range(0, commonEnemies.Length)] as GameObject, tile.position, Quaternion.identity));
					}
				}
			}

			if (specialEnemy == null && specialEnemies.Length > 0) {
				Transform tile = tiles[Random.Range(0, tiles.Length)];

				bool canSpawn = true;

				foreach (PlayerCore player in Object.FindObjectsOfType<PlayerCore>()) {
					if (Vector3.Distance(player.transform.position, tile.position) < spawnRange || Vector3.Distance(player.transform.position, tile.position) > maxSpawnRange) canSpawn = false;
				}

				if (canSpawn) {
					GameObject g = Instantiate(specialEnemies[Random.Range(0, specialEnemies.Length)] as GameObject, tile.position, Quaternion.identity);

					specialEnemy = g;

					NetworkServer.Spawn(g);
				}
			}

			yield return new WaitForSeconds(spawnInterval);
		}
	}

	private int GetEnemyCount() {
		int count = 0;

		foreach (Enemy e in Object.FindObjectsOfType<Enemy>()) {
			if (!e.follow) count++;
		}

		return count;
	}
}
