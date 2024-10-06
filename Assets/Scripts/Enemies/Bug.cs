using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Mirror;

public class Bug : Enemy
{
	public int minDamage = 5;
	public int maxDamage = 10;
	public int flingSpeed = 100;

	public AudioClip[] flySounds;

	float originalSpeed;

	float stopWait;

	public void Initialize() {
		originalSpeed = agent.speed;
	}

	public void UpdateLoop() {
		if (isServer) {
			agent.speed = stopWait > Time.time + 0.1f ? originalSpeed : 0;

			if (stopWait < Time.time) stopWait = Time.time + Random.Range(0.8f, 1f);
		}
	}

	public void Attack() {
		if (target != null && Vector3.Distance(transform.position, target.position) <= reach) {
			target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage, maxDamage + 1), transform.forward * 3);
		}
	}

	void Movement() {
		if (target != null) {
			SetAgentDestination(target.position + GetRandomPosition(0, 1f));
		} else {
			Vector3 pos = transform.position + (isLeader ? GetRandomPosition(2, wanderDistance) : GetRandomPosition(2f, 4f));

			SetAgentDestination(pos);

			walkWait = Time.time + (Vector3.Distance(transform.position, pos) / agent.speed) + Random.Range(3f, 6f);
		}
	}

	public IEnumerator SpecialAbility() {
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<NavMeshAgent>().enabled = false;
		GetComponent<Rigidbody>().AddForce(((target.position - transform.position).normalized + (Vector3.up * 0.6f)) * flingSpeed);
		animator.SetBool("Walking", false);

		transform.Find("Fly").GetComponent<AudioSource>().PlayOneShot(flySounds[Random.Range(0, flySounds.Length)]);

		bool damaged = false;

		for (int i = 0; i < 10; i++) {
			yield return new WaitForSeconds(0.1f);

			if (target != null && GetComponent<Rigidbody>().velocity.magnitude > 1 && Vector3.Distance(transform.position, target.position) <= reach && !damaged) {
				target.GetComponent<PlayerCore>().RpcDamage(Random.Range(minDamage * 4, (maxDamage * 4) + 1), transform.forward * 3);
				damaged = true;
			}
		}

		yield return new WaitForSeconds(1.5f);

		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<NavMeshAgent>().enabled = true;
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
