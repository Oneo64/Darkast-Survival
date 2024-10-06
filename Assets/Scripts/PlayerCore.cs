using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.UI;

using Mirror;

public class PlayerCore : NetworkBehaviour
{
	public Camera camera;
	public Transform neck;
	public Transform tool;
	[SyncVar] public bool isDead;
	[SyncVar] public bool sneaking;
	[SyncVar(hook="UpdateScore")] public int score;

	Volume bloodEffect;
	int maxHealth = 100;
	int health = 100;

	float shootWait;

	Transform canvas;

	PlayerInventory inventory;

	[HideInInspector] public Animator animator;

	bool reloading;

	float invincibilityTime = 0;

	[HideInInspector] public Perk perk;

	void Start() {
		Cursor.lockState = CursorLockMode.Locked;
		bloodEffect = GameObject.Find("BloodVolume").GetComponent<Volume>();

		canvas = GameObject.Find("/Canvas").transform;
		animator = GetComponent<Animator>();
		inventory = GetComponent<PlayerInventory>();

		Application.targetFrameRate = 70;

		perk = canvas.Find("Perks").GetComponent<Perks>().perk;
		canvas.Find("Perks").gameObject.SetActive(false);

		switch (perk) {
			case Perk.Athlete:
				maxHealth = 150;
				break;

			case Perk.Engineer:
				inventory.AddItem("wood", 5);
				inventory.AddItem("metal", 5);
				inventory.AddItem("spring", 5);
				inventory.AddItem("battery", 1);
				break;

			case Perk.ExplosionGuy:
				inventory.AddItem("grenade", 1);
				inventory.AddItem("paper", 5);
				inventory.AddItem("metal", 8);
				inventory.AddItem("gunpowder", 8);
				break;

			case Perk.Monkey:
				maxHealth = 10;
				inventory.AddItem("canned_soup", 1);
				inventory.AddItem("rope", 1);
				break;

			case Perk.Survivior:
				inventory.AddItem("flare", 2);
				inventory.AddItem("colt_navy", 1);
				inventory.AddItem(".38_rimfire_box", 2);
				break;
		}

		health = maxHealth;
	}

	void Update() {
		if (isLocalPlayer) {
			if (invincibilityTime > 0) invincibilityTime -= Time.deltaTime;

			if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, 2.5f, LayerMask.GetMask(new string[] {"Default", "Item"}))) {
				if (hit.transform.tag == "Interactable" || hit.transform.tag == "Item") {
					canvas.Find("Interact").gameObject.SetActive(true);

					if (hit.transform.GetComponent<PlacedItem>()) {
						canvas.Find("Interact").GetComponent<Text>().text = "E to take " + Database.items[hit.transform.GetComponent<PlacedItem>().item].name;
					} else if (hit.transform.GetComponent<DroppedItem>()) {
						canvas.Find("Interact").GetComponent<Text>().text = "E to take " + Database.items[hit.transform.GetComponent<DroppedItem>().item.id].name;
					} else {
						canvas.Find("Interact").GetComponent<Text>().text = "E to interact";
					}

					if (PlayerControls.GetInput("interact")) {
						if (hit.transform.tag == "Interactable") {
							hit.transform.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
						} else {
							if (hit.transform.GetComponent<PlacedItem>()) {
								if (inventory.HasSpaceFor(hit.transform.GetComponent<PlacedItem>().item)) {
									inventory.AddItem(hit.transform.GetComponent<PlacedItem>().item);

									hit.transform.GetComponent<PlacedItem>().CmdDestroy();

									PlayLocalSound("Take");

									animator.CrossFade("Take", 0.2f);
								}
							} else {
								if (inventory.HasSpaceFor(hit.transform.GetComponent<DroppedItem>().item.id)) {
									inventory.AddItem(hit.transform.GetComponent<DroppedItem>().item);

									hit.transform.GetComponent<DroppedItem>().CmdDestroy();

									PlayLocalSound("Take");

									animator.CrossFade("Take", 0.2f);
								}
							}

							inventory.UpdateInventory();
							inventory.CheckAnimations();
						}
					}
				} else {
					canvas.Find("Interact").gameObject.SetActive(false);
				}
			} else {
				canvas.Find("Interact").gameObject.SetActive(false);
			}

			if (inventory.selectedItem.id != "" && !inventory.crafting) {
				if (PlayerControls.GetInput("drop")) {
					inventory.selectedItem.amount -= 1;
					
					Item item = new Item(inventory.selectedItem.id, 1);
					item.externalData = inventory.selectedItem.externalData;

					if (inventory.selectedItem.amount <= 0) inventory.selectedItem.id = "";

					CmdDrop(item, camera.transform.position, camera.transform.forward, 1);

					inventory.UpdateInventory();
					inventory.CheckAnimations();

					animator.CrossFade("Take", 0.2f);
				} else if (PlayerControls.GetInput("throw")) {
					animator.CrossFade("Throw", 0.2f);
				} else if (PlayerControls.GetInput("use") && inventory.selectedItem.GetData() is Food) {
					Item item = inventory.inventory[inventory.selected];
					item.amount -= 1;

					health += ((Food) item.GetData()).heal;

					if (health > maxHealth) health = maxHealth;

					if (item.amount <= 0) inventory.inventory[inventory.selected].id = "";

					inventory.UpdateInventory();
					inventory.CheckAnimations();

					CmdPlaySound("Eat");
				} else if (inventory.selectedItem.GetData() is Gun) {
					Gun gun = (Gun) inventory.selectedItem.GetData();
					bool fire = PlayerControls.GetInput("use") || (PlayerControls.GetInput("use_automatic") && gun.isAutomatic);

					if (fire && !reloading) {
						if (inventory.selectedItem.externalData > 0) {
							if (shootWait < Time.time) {
								Vector3[] directions = new Vector3[gun.shots];

								for (int i = 0; i < gun.shots; i++) {
									directions[i] = camera.transform.eulerAngles + new Vector3(
										Random.Range(-gun.spread, gun.spread),
										Random.Range(-gun.spread, gun.spread),
										Random.Range(-gun.spread, gun.spread)
									);
								}

								CmdFireGun(tool.position + (Vector3.up * 0.02f) - (camera.transform.forward * 0.2f), directions, gun);
								FireGun(tool.position + (Vector3.up * 0.02f) - (camera.transform.forward * 0.2f), directions, gun);
								shootWait = Time.time + gun.fireRate;

								Vector3 recoil = new Vector3(-Random.Range(5f, 10f), Random.Range(-1f, 1f)) * (gun.muzzleVelocity / 100f) * (gun.maxDamage / 50f) * gun.shots;

								GetComponent<PlayerController>().AddRecoil(new Vector3(-Random.Range(25f, 30f), Random.Range(-5f, 5f)));

								inventory.selectedItem.externalData -= 1;
							}
						} else if (PlayerControls.GetInput("use")) {
							CmdPlaySound("GunEmpty");
						}
					}

					if (PlayerControls.GetInput("reload") && inventory.HasItem(gun.model[2]) && inventory.selectedItem.externalData <= 0) StartCoroutine(ReloadGun());

					animator.SetBool("Aiming", PlayerControls.GetInput("aim"));
					canvas.Find("Dot").gameObject.SetActive(!PlayerControls.GetInput("aim"));
				} else if (inventory.selectedItem.GetData() is Throwable) {
					if (PlayerControls.GetInput("throw")) {
						animator.CrossFade("Throw", 0.2f);
					}
				} else {
					animator.SetBool("Aiming", false);
					if (!canvas.Find("Dot").gameObject.activeInHierarchy) canvas.Find("Dot").gameObject.SetActive(true);
				}
			} else {
				animator.SetBool("Aiming", false);
				if (!canvas.Find("Dot").gameObject.activeInHierarchy) canvas.Find("Dot").gameObject.SetActive(true);
			}

			Color healthColor = Color.HSVToRGB(Mathf.Clamp01(Mathf.Lerp(-0.5f, 1.5f, (float) health / maxHealth)) / 3, 1, 1);

			canvas.Find("Health").Find("Bar").GetComponent<Image>().color = healthColor;
			canvas.Find("Health").Find("Bar").GetComponent<Image>().fillAmount = (float) health / maxHealth;

			if (health <= 50) {
				bloodEffect.weight = 1 - Mathf.Clamp(health / 50f, 0, 1);
			} else {
				bloodEffect.weight = 0;
			}
		}
	}

	public void DoThrow() {
		if (inventory.inventory[inventory.selected] != null && inventory.inventory[inventory.selected].id != "") {
			if (inventory.inventory[inventory.selected].GetData() is Throwable) {
				CmdThrow(
					((Throwable) inventory.selectedItem.GetData()).model,
					tool.position + (camera.transform.forward * 0.2f), camera.transform.forward, 30
				);

				Item item = inventory.inventory[inventory.selected];
				item.amount -= 1;

				if (item.amount <= 0) inventory.inventory[inventory.selected].id = "";

				inventory.UpdateInventory();
				inventory.CheckAnimations();
			} else {
				inventory.selectedItem.amount -= 1;
					
				Item item = new Item(inventory.selectedItem.id, 1);
				item.externalData = inventory.selectedItem.externalData;

				if (inventory.selectedItem.amount <= 0) inventory.selectedItem.id = "";

				CmdDrop(item, camera.transform.position, camera.transform.forward, 30);

				inventory.UpdateInventory();
				inventory.CheckAnimations();
			}
		}
	}

	public IEnumerator ReloadGun() {
		animator.CrossFade(((Gun) inventory.selectedItem.GetData()).model[1] == "HoldRifle" ? "ReloadRifle" : "ReloadPistol", 0.2f);

		reloading = true;

		yield return new WaitForSeconds(2.6f);

		reloading = false;

		if (inventory.selectedItem.id != "" && inventory.selectedItem.GetData() is Gun) {
			Gun gun = (Gun) inventory.selectedItem.GetData();

			inventory.selectedItem.externalData = gun.maxAmmunition;
			inventory.RemoveItem(gun.model[2]);
		}
	}

	[TargetRpc]
	public void RpcMoveTo(Vector3 pos) {
		transform.position = pos;
	}

	[TargetRpc]
	public void RpcGiveInvincibility(float i) {
		invincibilityTime = i;
	}

	[TargetRpc]
	public void RpcDamage(int damage, Vector3 force) {
		Damage(damage, force, "", Vector3.zero);
	}

	[TargetRpc]
	public void RpcDamage(int damage, Vector3 force, string hit, Vector3 hitPos) {
		Damage(damage, force, hit, hitPos);
	}

	public void Damage(int damage, Vector3 force, string hit, Vector3 hitPos) {
		if (health <= 0 || invincibilityTime > 0) return;
		
		health -= damage;

		if (health <= 0) {
			canvas.Find("Dead").gameObject.SetActive(true);

			CmdCreateRagdoll(
				"HumanoidRagdoll",
				transform.position,
				transform.eulerAngles,
				Database.GetLimbs(transform.Find("Armature"), GetComponent<Rigidbody>().velocity + force, hit, hitPos),
				Color.white,
				Color.white
			);

			GetComponent<Rigidbody>().isKinematic = true;
			transform.position = Vector3.down * 500;

			CmdSetDead(true);
		}
	}

	[Command]
	public void CmdSneak(bool b) {
		sneaking = b;
	}

	[Command]
	public void CmdCreateRagdoll(string n, Vector3 position, Vector3 rotation, Limb[] limbs, Color shirt, Color pants) {
		RpcCreateRagdoll(n, position, rotation, limbs, shirt, pants);
	}

	[ClientRpc]
	public void RpcCreateRagdoll(string n, Vector3 position, Vector3 rotation, Limb[] limbs, Color shirt, Color pants) {
		GameObject ragdollObject = Instantiate(Resources.Load("Ragdolls/" + n) as GameObject, position, Quaternion.Euler(rotation));
		Transform ragdoll = ragdollObject.transform;

		if (ragdoll.Find("Armature") != null) {
			ragdoll.Find("Armature").gameObject.SetActive(false);

			foreach (Transform t in ragdoll.Find("Armature").GetComponentsInChildren<Transform>()) {
				foreach (Limb limb in limbs) {
					if (t.transform.name == limb.name) {
						t.transform.localPosition = limb.position;
						t.transform.localEulerAngles = limb.rotation;

						print(limb.name + " " + limb.force.magnitude);

						if (t.GetComponent<Rigidbody>() != null && limb.force.magnitude > 0.1f) {
							if (limb.forcePos != Vector3.zero) t.GetComponent<Rigidbody>().AddForceAtPosition(limb.force, limb.forcePos); else t.GetComponent<Rigidbody>().AddForce(limb.force);
						}
					}
				}
			}

			ragdoll.Find("Armature").gameObject.SetActive(true);
			//ragdoll.Find("Armature").GetChild(0).GetComponent<Rigidbody>().AddForce(force);
		}

		if (ragdoll.transform.Find("Humanoid") != null) {
			Material shirtMat = ragdoll.transform.Find("Humanoid").GetComponent<SkinnedMeshRenderer>().materials[2];
			Material pantsMat = ragdoll.transform.Find("Humanoid").GetComponent<SkinnedMeshRenderer>().materials[0];

			shirtMat.SetColor("_BaseColor", shirt);
			pantsMat.SetColor("_BaseColor", pants);
		}

		Destroy(ragdollObject, 60);
	}

	[Command]
	private void CmdFireGun(Vector3 pos, Vector3[] dir, Gun gun) {
		RpcFireGun(pos, dir, gun);
	}

	[ClientRpc(includeOwner = false)]
	private void RpcFireGun(Vector3 pos, Vector3[] dir, Gun gun) {
		FireGun(pos, dir, gun);
	}

	private void FireGun(Vector3 pos, Vector3[] dir, Gun gun) {
		if (gun.muzzleVelocity > 0) {
			for (int i = 0; i < dir.Length; i++) {
				GameObject g = Instantiate(Resources.Load(gun.bullet) as GameObject, pos, Quaternion.Euler(dir[i]));

				g.GetComponent<StaticProjectile>().minDamage = gun.minDamage;
				g.GetComponent<StaticProjectile>().maxDamage = gun.maxDamage;
				g.GetComponent<StaticProjectile>().speed = gun.muzzleVelocity;
				g.GetComponent<StaticProjectile>().owner = transform;
			}
		}

		if (tool.childCount > 0 && tool.GetChild(0) != null && tool.GetChild(0).Find("ShootNear") != null) {
			foreach (Transform t in tool.GetChild(0)) {
				if (t.GetComponent<ParticleSystem>()) t.GetComponent<ParticleSystem>().Play();
			}
		
			AudioSource audio1 = tool.GetChild(0).Find("ShootNear").GetComponent<AudioSource>();
			AudioSource audio2 = tool.GetChild(0).Find("ShootFar").GetComponent<AudioSource>();

			audio1.pitch = Random.Range(0.95f, 1.05f);
			audio2.pitch = Random.Range(0.95f, 1.05f);

			audio1.PlayOneShot(audio1.clip);
			audio2.PlayOneShot(audio2.clip);
		}

		CmdScore(1);
	}

	[Command]
	private void CmdThrow(string obj, Vector3 pos, Vector3 dir, float force) {
		GameObject obj2 = Instantiate(Resources.Load(obj) as GameObject, pos, Quaternion.LookRotation(dir));

		obj2.GetComponent<Rigidbody>().AddForce(dir * force, ForceMode.VelocityChange);

		Physics.IgnoreCollision(obj2.GetComponent<Collider>(), GetComponent<Collider>());

		if (obj2.GetComponent<Grenade>()) obj2.GetComponent<Grenade>().owner = transform;
		if (obj2.GetComponent<DroppedItem>()) obj2.GetComponent<DroppedItem>().owner = transform;

		NetworkServer.Spawn(obj2);
	}

	[Command]
	private void CmdSetDead(bool d) {
		isDead = d;
	}

	[Command]
	private void CmdDrop(Item item, Vector3 pos, Vector3 dir, float force) {
		GameObject item2 = Instantiate(Resources.Load("DroppedItem") as GameObject, pos, Quaternion.LookRotation(dir));

		item2.GetComponent<DroppedItem>().owner = transform;
		item2.GetComponent<DroppedItem>().item = item;

		item2.GetComponent<Rigidbody>().mass = item.GetData().weight;

		float forceDiv = Mathf.Max(item.GetData().weight, 1);

		if (item.GetData().weight < 0.1) {
			forceDiv += (0.1f - item.GetData().weight) * 10;
		}

		item2.GetComponent<Rigidbody>().AddForce(dir * (force / forceDiv), ForceMode.VelocityChange);

		NetworkServer.Spawn(item2);
	}

	[Command]
	private void CmdScore(int s) {
		score += s;
	}

	[Command]
	private void CmdPlaySound(string name) {
		RpcPlaySound(name);
	}

	[ClientRpc]
	private void RpcPlaySound(string name) {
		transform.Find(name).GetComponent<AudioSource>().Play();
	}

	public void PlayLocalSound(string n) {
		GameObject.Find("/LocalSounds/" + n).GetComponent<AudioSource>().Play();
	}

	void OnParticleCollision(GameObject obj) {
		if (isLocalPlayer && obj.transform.name == "Attack") {
			Enemy e = obj.GetComponentInParent<Enemy>();

			if (e) {
				if (e is BallMonster) {
					int minDamage = ((BallMonster) e).minDamage;
					int maxDamage = ((BallMonster) e).maxDamage;

					Damage(Random.Range(minDamage, maxDamage + 1), obj.transform.forward * 1000, "", Vector3.zero);
				}
			}
		}
	}

	void UpdateScore(int old, int _new) {
		if (isLocalPlayer) canvas.Find("Score").GetComponent<Text>().text = "Score is " + score;
	}
}
