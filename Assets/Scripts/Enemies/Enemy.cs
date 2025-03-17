using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Mirror;

public class Enemy : NetworkBehaviour
{
	public int detectionRange = 50;
	public float audioDetectionImpairment = 2;
	public int health = 100;
	public int score = 10;
	public float reach;
	public bool isDead;

	public float attackInterval = 1;
	public float specialAbilityInterval = 3;
	public SpecialAbilityTriggerType specialAbilityTrigger = SpecialAbilityTriggerType.NoConditions;
	public float moveInterval = 0.5f;

	public string ragdollname = "HumanoidRagdoll";

	public Transform follow;
	public bool isLeader;
	public int leaderChance = 100;
	public string minionId;
	public int minionCount = 2;
	public int minionAttackDistance = 20;

	public int wanderDistance = 20;

	public bool slowReaction;

	[HideInInspector] public NavMeshAgent agent;
	[HideInInspector] public Animator animator;

	[HideInInspector] public float searchWait;
	[HideInInspector] public float walkWait;
	[HideInInspector] public float attackWait;
	[HideInInspector] public float specialWait;

	Vector3 lastSeen;
	
	float doorWait;
	float forgetWait;
	float listenWait;
	float listenValue; // if higher than 1 then will react to sound
	float seeWait;

	bool canSee;

	[HideInInspector] public Transform target;

	Difficulty difficulty = Difficulty.Normal;

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();

		if (isServer && !agent.isOnNavMesh) TryFixAgent();

		if (isServer && isLeader) {
			if (Random.Range(1, 101) <= leaderChance) {
				for (int i = 0; i < Random.Range(minionCount, (minionCount * 2) + 1); i++) {
					GameObject minion = Instantiate(Resources.Load("Enemies/" + minionId) as GameObject, transform.position, Quaternion.identity);

					minion.GetComponent<Enemy>().follow = transform;
					minion.GetComponent<Enemy>().isLeader = false;

					NetworkServer.Spawn(minion);
				}
			}
		}

		if (isServer) {
			difficulty = GameObject.Find("/Canvas").transform.Find("Difficulty").GetComponent<Difficulties>().difficulty;

			if (difficulty == Difficulty.ReallyEasy || difficulty == Difficulty.Easy) {
				agent.speed *= 0.75f;

				if (this is BallMonster) {
					((BallMonster) this).normalRoll = Mathf.CeilToInt(((BallMonster) this).normalRoll * 0.75f);
					((BallMonster) this).fastRoll = Mathf.CeilToInt(((BallMonster) this).fastRoll * 0.75f);
				}
			}

			if (difficulty == Difficulty.Insanity) {
				agent.speed *= 10;

				if (this is BallMonster) {
					((BallMonster) this).normalRoll = Mathf.CeilToInt(((BallMonster) this).normalRoll * 10);
					((BallMonster) this).fastRoll = Mathf.CeilToInt(((BallMonster) this).fastRoll * 10);
				}
			}
		}

		SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);
	}

	void Update() {
		if (isServer && !isDead) {
			if (searchWait < Time.time) {
				LookForEnemies();

				searchWait = Time.time + Random.Range(0.3f, 0.6f);
			}

			if (specialWait < Time.time && (int) specialAbilityTrigger <= 3) {
				specialWait = Time.time + Random.Range(specialAbilityInterval * 0.8f, specialAbilityInterval * 1.2f);

				if (specialAbilityTrigger == SpecialAbilityTriggerType.WhenIdle && target == null) {
					SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
				} else if (specialAbilityTrigger == SpecialAbilityTriggerType.WhenChasing && target != null) {
					SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
				} else if (specialAbilityTrigger == SpecialAbilityTriggerType.NoConditions) {
					SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
				}
			}

			if (target != null) {
				Vector3 dir = (target.position - transform.position).normalized;

				dir.y = 0;
				dir = dir.normalized;

				if (canSee) {
					if (attackWait < Time.time && Vector3.Angle(transform.forward, dir) <= 45 && Vector3.Distance(transform.position, target.position) <= reach) {
						attackWait = Time.time + Random.Range(attackInterval * 0.8f, attackInterval * 1.2f);

						SendMessage("Attack");
					}

					lastSeen = target.position;

					forgetWait = Time.time + Random.Range(10, 21);
				}

				if (seeWait < Time.time) {
					canSee = !Physics.Linecast(
						transform.position + (Vector3.up * 1.5f), target.position + (Vector3.up * 1.5f), LayerMask.GetMask("Default")
					);

					seeWait = Time.time + (Random.Range(0.5f, 1f) * (slowReaction ? 2 : 1));
				}
			}

			if (listenWait < Time.time && target == null && difficulty == Difficulty.Normal) {
				AudioSource audio = LookForAudios();

				if (audio != null) {
					listenValue += (200 - Mathf.Min(Vector3.Distance(transform.position, audio.transform.position), 195)) / 150f;

					print(listenValue);

					if (listenValue >= 1) {
						SetAgentDestination(audio.transform.position + GetRandomPosition(1f, 20f));
						print("hear");
					}

					walkWait = Time.time + (Vector3.Distance(transform.position, audio.transform.position) / agent.speed) + Random.Range(10, 21);
				}

				listenWait = Time.time + Random.Range(1f, 2f);
			}

			listenValue = Mathf.Max(listenValue - (Time.deltaTime * 0.1f), 0);

			if (walkWait < Time.time && moveInterval > 0) {
				walkWait = Time.time + Random.Range(moveInterval, moveInterval * 2);

				if (follow != null && follow.GetComponent<Enemy>().target != null && Vector3.Distance(follow.position, follow.GetComponent<Enemy>().target.position) <= follow.GetComponent<Enemy>().minionAttackDistance) {
					SetAgentDestination(follow.GetComponent<Enemy>().target.position + GetRandomPosition(1f, 2f));
				} else if (follow != null && Random.Range(1, 5) != 1) {
					SetAgentDestination(follow.position + GetRandomPosition(2f, 4f));
				} else if (target == null && forgetWait > Time.time) {
					SetAgentDestination(lastSeen + GetRandomPosition(1f, 8f));
					walkWait = Time.time + Random.Range(5f, 10f);
				} else {
					SendMessage("Movement", SendMessageOptions.DontRequireReceiver);
				}
			}

			if (follow != null && follow.GetComponent<Enemy>().target != null && walkWait > Time.time + (moveInterval * 3f)) walkWait = 0;

			if (doorWait < Time.time) {
				foreach (Door door in Object.FindObjectsOfType<Door>()) {
					if (Vector3.Distance(door.transform.position, transform.position) < 2) {
						door.SendMessage("Interact");
						break;
					}
				}

				doorWait = Time.time + Random.Range(4f, 6f);
			}

			if (this is not Mannequin && agent.enabled) animator.SetBool("Walking", agent.velocity.magnitude >= 0.2f);
		}

		SendMessage("UpdateLoop", SendMessageOptions.DontRequireReceiver);
	}

	List<PlayerCore> players = new List<PlayerCore>();
	List<Building> buildings = new List<Building>();
	float playerGetWait = 0;

	public void LookForEnemies(bool esp = false) {
		Transform newTarget = null;
		float dist = detectionRange;

		if (playerGetWait < Time.time) {
			players = new List<PlayerCore>(Object.FindObjectsOfType<PlayerCore>());
			buildings = new List<Building>(Object.FindObjectsOfType<Building>());
			playerGetWait = Time.time + Random.Range(4f, 6f);
		}

		foreach (PlayerCore player in players) {
			if (player != null && player.transform != null) {
				Transform targ = player.transform;

				if (!Physics.Linecast(transform.position + (Vector3.up * 1.5f), targ.position + (Vector3.up * 1.5f), LayerMask.GetMask("Default")) || esp) {
					float dist2 = Vector3.Distance(transform.position, targ.position);

					if (player.sneaking) {
						if (dist2 > detectionRange / 2f || Vector3.Dot(transform.forward, (targ.position - transform.position).normalized) < 0.5f) {
							continue;
						}
					}

					if (dist2 < dist && Vector3.Dot(transform.forward, (targ.position - transform.position).normalized) > 0) {
						newTarget = targ;
						dist = dist2;
					}
				}
			}
		}

		foreach (Building building in buildings) {
			if (building != null && building.transform != null) {
				Transform targ = building.transform;

				bool b = Physics.Linecast(transform.position + (Vector3.up * 1.5f), targ.position + (Vector3.up * 1.5f), out RaycastHit hit, LayerMask.GetMask("Default"));

				if (!b || esp) {
					float dist2 = Vector3.Distance(transform.position, targ.position);

					if (dist2 < dist && dist < 10 && (Vector3.Dot(transform.forward, (targ.position - transform.position).normalized) > 0 || dist < 2)) {
						newTarget = targ;
						dist = dist2;
					}
				}
			}
		}

		if (target != newTarget || (target == null && newTarget != null)) {
			walkWait = 0;

			if (specialAbilityTrigger == SpecialAbilityTriggerType.OnAggro) SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
		}

		target = newTarget;
	}

	public AudioSource LookForAudios() {
		List<AudioSource> audios = new List<AudioSource>(Object.FindObjectsOfType<AudioSource>());
		AudioSource newAudio = null;
		float lastDist = 1000;

		foreach (AudioSource audio in audios) {
			float dist = Vector3.Distance(audio.transform.position, transform.position);
			bool canBeHeard = (dist < audio.maxDistance / audioDetectionImpairment) || (dist < audio.maxDistance && audio.maxDistance < 5);

			if (audio.isPlaying && audio.spatialBlend > 0.2f && canBeHeard && audio.transform.parent && !audio.transform.parent.GetComponent<Enemy>()) {
				if (dist < lastDist) {
					newAudio = audio;
					lastDist = dist;
				}
			}
		}

		return newAudio;
	}

	[Command(requiresAuthority = false)]
	public void CmdDamage(int damage, Vector3 force, PlayerCore source, string hitName, Vector3 hitPos) {
		if (isDead) return;

		health -= damage;

		if (source && target == null) {
			target = source.transform;
			forgetWait = Time.time + 5;
		}

		if (health <= 0) {
			isDead = true;
			agent.enabled = false;

			animator.SetBool("Walking", false);
			if (GetComponent<Ambience>()) RpcStopAmbience();

			if (source) source.score += score;

			if (specialAbilityTrigger == SpecialAbilityTriggerType.OnDeath && specialAbilityInterval < Time.time) {
				specialWait = Time.time + specialAbilityInterval;

				SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
			}

			SendMessage("Die", new DeathParameters(force, hitName, hitPos), SendMessageOptions.DontRequireReceiver);
		} else {
			SendMessage("Hit", hitName, SendMessageOptions.DontRequireReceiver);

			if (specialAbilityTrigger == SpecialAbilityTriggerType.OnDamaged && specialAbilityInterval < Time.time) {
				specialWait = Time.time + specialAbilityInterval;

				SendMessage("SpecialAbility", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void SetAgentDestination(Vector3 pos) {
		if (agent && agent.isOnNavMesh) {
			NavMeshPath path0 = new NavMeshPath();

			if (NavMesh.CalculatePath(transform.position, pos, 1, path0)) {
				agent.SetPath(path0);
			} else {
				agent.SetDestination(pos);
			}
		}
	}

	public Vector3 GetRandomPosition(float min, float max) {
		Vector2 pos = Random.insideUnitCircle * Random.Range(min, max);

		return new Vector3(pos.x, 0, pos.y);
	}

	public void TryFixAgent() {
		if (!agent.isOnNavMesh) {
			agent.enabled = false;

			if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 20, NavMesh.AllAreas)) {
				transform.position = hit.position;
			}

			agent.enabled = true;
		}
	}

	[ClientRpc]
	public void RpcStopAmbience() {
		GetComponent<Ambience>().enabled = false;
	}

	void OnParticleCollision(GameObject particle) {
		if (isServer) {
			if (particle.transform.name == "Fire") {
				CmdDamage(1, particle.transform.forward * 5, particle.GetComponentInParent<PlayerCore>(), "", Vector3.zero);
			}
		}
	}
}

public enum SpecialAbilityTriggerType {
	NoConditions,
	OnAggro,
	WhenChasing,
	WhenIdle,
	OnDamaged,
	OnDeath
}