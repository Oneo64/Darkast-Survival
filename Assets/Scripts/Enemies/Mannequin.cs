using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Mannequin : Enemy
{
	public int speed = 10;

	public SkinnedMeshRenderer renderer;
	public int minDamage = 20;
	public int maxDamage = 40;

	public Transform neck;

	public int stalkingMinRange = 40;
	public int stalkingMaxRange = 80;

	float alert;
	float teleportTime;
	float stalkWait;
	bool stalking;

	int maxHealth;

	bool tryingToTeleport;

	void Initialize() {
		maxHealth = health;

		/*LookForEnemies(true);
		searchWait = Time.time + 5;

		if (target != null && FindStalkPoint(out Vector3 pos)) {
			SetAgentDestination(pos);
			stalking = true;

			stalkWait = Time.time + Random.Range(40, 60);
		} else {
			target = null;
			stalkWait = Time.time + Random.Range(5, 15);
		}*/
	}

	public void UpdateLoop() {
		if (isServer && !isDead) {
			if (target != null && !CanBeSeen(transform.position)) {
				neck.rotation = Quaternion.LookRotation((target.position - transform.position).normalized);
				animator.SetBool("Walking", agent.velocity.magnitude >= 0.2f);
			}

			/*if (stalkWait < Time.time) {
				LookForEnemies(true);
				searchWait = Time.time + 5;

				stalking = false;

				if (target != null && FindStalkPoint(out Vector3 pos)) {
					SetAgentDestination(pos);
					stalking = true;

					stalkWait = Time.time + Random.Range(40, 60);
				} else {
					stalkWait = Time.time + Random.Range(10, 15);
				}
			}*/

			/*if (teleportTime < Time.time) {
				if (tryingToTeleport && !CanBeSeen(transform.position)) {
					Teleport();
				} else if (alert > Time.time && (target != null && Vector3.Distance(target.position, transform.position) > 50)) {
					tryingToTeleport = true;
					MoveToHide();
				}

				teleportTime = Time.time + Random.Range(0.5f, 1f);
			}*/
		}
	}

	public void Teleport() {
		for (int i = 0; i < 10; i++) {
			Vector3 pos = transform.position + (Random.insideUnitSphere * 50);

			if (!CanBeSeen(pos) && UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit hit, 20, UnityEngine.AI.NavMesh.AllAreas)) {
				agent.enabled = false;
				transform.position = hit.position;
				agent.enabled = true;
				tryingToTeleport = false;
			}
		}
	}

	public IEnumerator Attack() {
		if (!stalking || alert > Time.time) {
			animator.CrossFade("Attack", 0.2f);

			alert = Time.time + 1f;

			yield return new WaitForSeconds(0.4f);

			if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
				target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
			}
		} else {
			yield return null;
		}
	}

	public void Hit() {
		alert = Time.time + 20f;
		stalking = false;
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

	public bool FindStalkPoint(out Vector3 pos) {
		pos = transform.position;

		Vector3 lastPos = pos;
		int lastScore = 0;

		for (int i = 0; i < 20; i++) {
			Vector3 pos2 = target.position + ((transform.position - target.position).normalized * Random.Range(stalkingMinRange, stalkingMaxRange)) + (new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y) * 20);
			int score = 0;

			if (UnityEngine.AI.NavMesh.SamplePosition(pos2, out UnityEngine.AI.NavMeshHit hit, 20, UnityEngine.AI.NavMesh.AllAreas)) pos2 = hit.position;

			bool b = !Physics.Linecast(pos2 + (Vector3.up * 1.6f), target.position + (Vector3.up * 1.8f), LayerMask.GetMask("Default")) ||
				!Physics.Linecast(pos2 + (Vector3.up * 1.6f), target.position + (Vector3.up * 1), LayerMask.GetMask("Default")) ||
				!Physics.Linecast(pos2 + (Vector3.up * 1.6f), target.position + (Vector3.up * 0.2f), LayerMask.GetMask("Default"));

			if (b) {
				UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

				if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, pos2, 1, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) score += Mathf.FloorToInt(Vector3.Distance(target.position, pos2));
			}

			if (score > lastScore) {
				lastScore = score;
				lastPos = pos2;
			}
		}

		pos = lastPos;

		return lastScore > 0;
	}

	public void MoveToHide() {
		for (int i = 0; i < 20; i++) {
			Vector3 pos = transform.position + (Random.insideUnitSphere * 20);

			if (UnityEngine.AI.NavMesh.SamplePosition(pos, out UnityEngine.AI.NavMeshHit hit, 20, UnityEngine.AI.NavMesh.AllAreas)) pos = hit.position;

			if (!CanBeSeen(pos)) {
				UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

				if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, pos, 1, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) SetAgentDestination(pos);
			}
		}
	}

	void Movement() {
		if (alert < Time.time && CanBeSeen(transform.position)) {
			agent.speed = 0;
			animator.SetFloat("WalkAnimSpeed", 0.0001f);
			return;
		} else {
			agent.speed = speed;
			animator.SetFloat("WalkAnimSpeed", 1);
		}

		if (target != null) {
			SetAgentDestination(target.position);

			walkWait = Time.time + Random.Range(0.2f, 0.3f);
		} else {
			Vector3 pos = transform.position + GetRandomPosition(5, 21);

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
