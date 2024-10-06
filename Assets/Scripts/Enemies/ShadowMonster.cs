using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class ShadowMonster : Enemy
{
	public int minDamage = 20;
	public int maxDamage = 40;

	void Initialize() {
		if (isServer) {
		}
	}

	public IEnumerator Attack() {
		animator.CrossFade("Attack", 0.2f);

		yield return new WaitForSeconds(0.4f);

		if (target != null && Vector3.Distance(transform.position, target.position) <= reach && Vector3.Dot((target.position - transform.position).normalized, transform.forward) > 0.5f) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
	}

	public IEnumerator SpecialAbility() {
		agent.enabled = false;
		RpcTeleportSmoke();
		yield return new WaitForSeconds(0.5f);
		Teleport();
		agent.enabled = true;

		RpcTeleportSmoke();
	}

	[ClientRpc]
	public void RpcTeleportSmoke() {
		transform.Find("Weirdo").Find("Teleport").GetComponent<ParticleSystem>().Play();
	}

	public bool CanBeSeen(Vector3 pos) {
		foreach (Camera cam in Object.FindObjectsOfType<Camera>(true)) {
			Vector3 dir = (pos - cam.transform.position).normalized;

			if (Vector3.Dot(cam.transform.forward, dir) > 0.55f) {
				if (!Physics.Linecast(pos + (Vector3.up * 1.8f) + (Vector3.forward * 0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 1.8f) + (Vector3.forward * -0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 1.8f) + (Vector3.right * 0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 1.8f) + (Vector3.right * -0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;

				if (!Physics.Linecast(pos + (Vector3.up * 0.2f) + (Vector3.forward * 0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 0.2f) + (Vector3.forward * -0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 0.2f) + (Vector3.right * 0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
				if (!Physics.Linecast(pos + (Vector3.up * 0.2f) + (Vector3.right * -0.4f), cam.transform.position, LayerMask.GetMask("Default"))) return true;
			}
		}

		return false;
	}

	public void Teleport() {
		for (int i = 0; i < 10; i++) {
			Vector3 pos = transform.position + (Random.insideUnitSphere * 50);

			if (!CanBeSeen(pos) && UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit hit, 20, UnityEngine.AI.NavMesh.AllAreas)) {
				agent.enabled = false;
				transform.position = hit.position;
				agent.enabled = true;
				return;
			}
		}
	}

	void Movement() {
		if (target != null) {
			SetAgentDestination(target.position);
		} else {
			Vector3 pos = transform.position + GetRandomPosition(2, wanderDistance);

			SetAgentDestination(pos);

			walkWait = Time.time + (Vector3.Distance(transform.position, pos) / agent.speed) + Random.Range(10, 21);
		}
	}

	void Die(DeathParameters deathParams) {
		NetworkClient.localPlayer.GetComponent<PlayerCore>().CmdCreateRagdoll(
			ragdollname,
			transform.position,
			transform.eulerAngles,
			Database.GetLimbs(transform.Find("Armature"), agent.velocity + deathParams.force, deathParams.hitName, deathParams.hitPos),
			Color.white, Color.white
		);

		Destroy(gameObject, 0);
	}
}
