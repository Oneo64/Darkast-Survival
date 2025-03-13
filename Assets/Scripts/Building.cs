using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Building : NetworkBehaviour
{
	public int maxHealth = 100;
	public float blastVulnerability = 1;

	public int health = 100;

	public Object debris;

	void Start() {
		health = maxHealth;
	}

	[Command(requiresAuthority = false)]
	public void CmdDamage(int damage, Vector3 force) {
		health -= damage;

		if (health <= 0) Die(force);
	}

	public void Die(Vector3 force) {
		RpcSpawnDebris(force);
		Destroy(gameObject, 0.2f);
	}

	[ClientRpc]
	public void RpcSpawnDebris(Vector3 force) {
		gameObject.SetActive(false);

		GameObject spawnedDebris = Instantiate(debris, transform.position, transform.rotation) as GameObject;

		foreach (Transform t in spawnedDebris.transform) {
			t.GetComponent<Rigidbody>().AddForce(force + (Random.insideUnitSphere.normalized * 100));

			Destroy(t.gameObject, Random.Range(8f, 12f));
		}

		Destroy(spawnedDebris, 12);
	}
}
