using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.AI.Navigation;

using Mirror;

public class MapLoader : MonoBehaviour
{
	public NavMeshSurface[] surfaces;
	public string[] maps;

	string lastMap;

	public void LoadRandomMap() {
		string map = "";

		while (map == "" || map == lastMap) {
			map = maps[Random.Range(0, maps.Length)];
		}

		lastMap = map;
		LoadMap(map);
	}

	public void LoadMap(string n) {
		lastMap = n;
		StartCoroutine(_LoadMap(n));
	}

	public IEnumerator _LoadMap(string n) {
		for (int i = transform.childCount - 1; i >= 0; i--) {
			Destroy(transform.GetChild(i).gameObject);
		}

		Enemy[] enemies = Object.FindObjectsOfType<Enemy>();

		for (int i = 0; i < enemies.Length; i++) {
			Destroy(enemies[i].gameObject);
		}

		PlacedItem[] loot = Object.FindObjectsOfType<PlacedItem>();

		for (int i = 0; i < loot.Length; i++) {
			Destroy(loot[i].gameObject);
		}

		DroppedItem[] loot2 = Object.FindObjectsOfType<DroppedItem>();

		for (int i = 0; i < loot2.Length; i++) {
			Destroy(loot2[i].gameObject);
		}

		yield return new WaitForSeconds(1);

		GameObject map = Instantiate(Resources.Load("Maps/" + n) as GameObject, Vector3.zero, Quaternion.identity, transform);

		NetworkServer.Spawn(map);

		NetworkStartPosition[] spawns = Object.FindObjectsOfType<NetworkStartPosition>();

		foreach (PlayerCore player in Object.FindObjectsOfType<PlayerCore>()) {
			if (player.isDead) player.RpcResurrect();
			
			player.RpcGiveInvincibility(5);
			player.RpcMoveTo(spawns[Random.Range(0, spawns.Length)].transform.position);
		}

		NetworkIdentity[] identities = map.GetComponentsInChildren<NetworkIdentity>();

		for (int i = 0; i < identities.Length; i++) {
			if (identities[i] != map.GetComponent<NetworkIdentity>()) {
				NetworkServer.Spawn(Instantiate(Resources.Load(identities[i].transform.name) as GameObject, Vector3.zero, Quaternion.identity, transform));

				Destroy(identities[i].gameObject);
			}
		}

		yield return new WaitForSeconds(1);

		GameObject[] doorSpawns = GameObject.FindGameObjectsWithTag("DoorCanSpawn");
		Transform levelDoor = GameObject.Find("/MysteriousDoor").transform;
		int retries = 10;

		if (doorSpawns.Length > 0) {
			while (retries == 10 || Physics.Linecast(levelDoor.position + (Vector3.up * 1.5f), levelDoor.position + Vector3.up, LayerMask.GetMask("Default"))) {
				levelDoor.position = doorSpawns[Random.Range(0, doorSpawns.Length)].transform.position + new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
				levelDoor.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

				retries -= 1;

				if (retries <= 0) break;
			}

			if (Physics.Raycast(levelDoor.position + (Vector3.up * 2), Vector3.down, out RaycastHit hit, 10, LayerMask.GetMask("Default"))) {
				levelDoor.position = new Vector3(levelDoor.position.x, hit.point.y, levelDoor.position.z);
			}
		}

		yield return new WaitForSeconds(1);

		foreach (NavMeshSurface surface in surfaces) {
			surface.UpdateNavMesh(surface.navMeshData);
		}
	}
}
