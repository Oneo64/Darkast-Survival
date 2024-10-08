using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Grenade : NetworkBehaviour
{
	public bool mustBeLit;
	public float fuse = 4;
	[HideInInspector] public bool hasExploded;

	[Header("Damage")]
	public int maxDamage = 40;
	public int minDamage = 20;
	public float explosionRadiusMin;
	public float explosionRadiusMax;

	[Header("Cosmetics")]
	public int force = 2000;
	public float destroyTime;

	public Transform owner;

	IEnumerator Start() {
		if (!isServer) GetComponent<Rigidbody>().isKinematic = true;

		yield return new WaitForSeconds(fuse);
		if (!mustBeLit) Check();
	}

	public void Check() {
		if (hasExploded) return;

		hasExploded = true;

		if (NetworkClient.localPlayer.isServer) {
			Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadiusMax);

			foreach (Collider c in colliders) {
				Vector3 forceDir = (c.transform.position - transform.position).normalized;

				int dmg = Random.Range(minDamage, maxDamage + 1);

				if (Vector3.Distance(transform.position, c.transform.position) > explosionRadiusMin) {
					dmg = Mathf.FloorToInt(dmg * (1 - ((Vector3.Distance(transform.position, c.transform.position) + explosionRadiusMin) / explosionRadiusMax)));
				}

				float dmg2 = dmg / 100f;
				int newForce = (int) Mathf.Round(force * (dmg2 * dmg2));

				bool a = !Physics.Linecast(transform.position, c.transform.position, LayerMask.GetMask("Default"));

				if (!a) dmg /= 2;

				if (a || Vector3.Distance(transform.position, c.transform.position) < explosionRadiusMin) {
					if (c.transform.GetComponentInParent<Enemy>()) c.transform.GetComponentInParent<Enemy>().CmdDamage(dmg, forceDir * newForce, owner.GetComponent<PlayerCore>(), "", Vector3.zero);
					if (c.transform.GetComponentInParent<PlayerCore>()) c.transform.GetComponentInParent<PlayerCore>().RpcDamage(dmg, forceDir * newForce, "", Vector3.zero);

					if (c.transform.GetComponentInParent<HasBlood>()) Blood(c.transform.position, forceDir, forceDir);

					if (c.transform.GetComponent<Rigidbody>() && c.transform.name == "LowerSpine") {
						c.transform.GetComponent<Rigidbody>().AddForceAtPosition(forceDir * newForce, transform.position);
					}

					// Big kaboom
					if (c.transform.GetComponent<Grenade>() && c.transform != transform && c.transform.GetComponent<Grenade>().mustBeLit) {
						c.transform.GetComponent<Grenade>().Check();
					}
				}
			}

			GetComponent<MeshRenderer>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<MeshCollider>().enabled = false;

			transform.eulerAngles = Vector3.zero;

			RpcExplode();

			Destroy(gameObject, destroyTime);
		}
	}

	[ClientRpc]
	private void RpcExplode() {
		transform.eulerAngles = Vector3.zero;

		if (transform.Find("Sound")) transform.Find("Sound").GetComponent<AudioSource>().Play();
		if (transform.Find("Sound2")) transform.Find("Sound2").GetComponent<AudioSource>().Play();

		foreach (Transform t in transform) {
			if (t.GetComponent<ParticleSystem>()) t.GetComponent<ParticleSystem>().Play();
		}
	}

	private void Blood(Vector3 pos, Vector3 dir, Vector3 normal) {
		Destroy(
			Instantiate(Resources.Load("Blood") as GameObject, pos, Quaternion.LookRotation(normal)),
			3
		);

		int amount = Random.Range(1, 3);

		for (int i = 0; i < amount; i++) {
			if (Physics.Raycast(pos, dir + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)), out RaycastHit hit, 5, LayerMask.GetMask("Default"))) {
				Transform bloodParent = null;

				if (hit.transform.parent && hit.transform.parent.tag == "Door") bloodParent = hit.transform;

				Vector3 rot = Quaternion.LookRotation(-hit.normal).eulerAngles;

				Destroy(
					Instantiate(
						Resources.Load("Blood") as GameObject,
						hit.point,
						Quaternion.Euler(rot + (Vector3.forward * Random.Range(0, 360))),
						bloodParent
					),
					30
				);
			}
		}
	}
}
