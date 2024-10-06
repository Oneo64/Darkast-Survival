using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class PlayerController : NetworkBehaviour
{
	public float bobSpeed = 2;
	public float bobIntensity = 0.1f;
	public Transform leftShoulder;
	public Transform rightShoulder;

	bool falling;
	float topYPos;
	float jumpWait;
	float hitWait;

	PlayerCore core;
	PlayerInventory inventory;
	Rigidbody controller;

	Vector2 rotation;
	Vector2 recoil;

	Vector3 rotationCorrection;

	Vector3 camBobPos;

	float recoilTime;
	float bobTime;
	float bobMult;

	void Start() {
		core = GetComponent<PlayerCore>();
		inventory = GetComponent<PlayerInventory>();
		controller = GetComponent<Rigidbody>();

		if (!isLocalPlayer) {
			core.camera.gameObject.SetActive(false);
			controller.isKinematic = false;
		}

		camBobPos = core.camera.transform.localPosition;
	}

	void Update() {
		if (isLocalPlayer) {
			CheckMovement(!core.isDead);

			rotation += new Vector2(-PlayerControls.GetInputAxis("look_y"), PlayerControls.GetInputAxis("look_x"));

			if (!PlayerControls.isKeyboardInput) {
				foreach (GameObject monster in GameObject.FindGameObjectsWithTag("Enemy")) {
					Vector3 look = Quaternion.Euler(rotation) * Vector3.forward;
					Vector3 monsterPos = monster.transform.position;

					if (monster.GetComponent<BallMonster>() && monster.GetComponent<BallMonster>().body != null) monsterPos = monster.GetComponent<BallMonster>().body.position;
					if (monster.transform.Find("Armature") && monster.transform.Find("Armature").Find("Hip")) monsterPos = monster.transform.Find("Armature").Find("Hip").position;

					Vector3 dest = (monsterPos - core.camera.transform.position).normalized;

					if (Vector3.Dot(look, dest) > 0.99f && !Physics.Linecast(core.camera.transform.position, monsterPos, LayerMask.GetMask("Default"))) {
						Vector3 lerp = Vector3.Lerp(look, dest, Time.deltaTime * 2);
						Vector3 rot = Quaternion.LookRotation(lerp).eulerAngles;

						transform.eulerAngles = Vector3.up * rot.y;
						core.neck.forward = Quaternion.Euler(rotation) * Vector3.forward;

						float a = rot.x;

						if (a > 180) a -= 360;

						rotation = new Vector2(a, rot.y);

						break;
					}
				}
			}

			rotation.x = Mathf.Clamp(rotation.x, -80, 80);

			if (recoil != Vector2.zero) {
				rotation += recoil * Time.deltaTime;

				recoil = Vector2.Lerp(recoil, Vector2.zero, (Time.time - recoilTime) * 2);

				if (core.tool.childCount > 0) core.tool.GetChild(0).localEulerAngles = recoil * 0.2f;
			} else {
				if (core.tool.childCount > 0) core.tool.GetChild(0).localEulerAngles = Vector3.zero;
			}

			if (rotation.y > 360) {
				rotation.y -= 360;
			}

			if (rotation.y < 0) {
				rotation.y += 360;
			}

			if (!core.isDead && transform.position.y < -200) {
				core.Damage(100, Vector3.zero, "", Vector3.zero);
			}

			Vector3 dir = controller.velocity;

			dir.y = 0;

			float bobX = Mathf.Sin(bobTime * bobSpeed * 0.5f) * bobMult * bobIntensity * 0.01f;
			float bobY = Mathf.Sin((bobTime + 0.5f) * bobSpeed) * bobMult * bobIntensity * 0.01f;

			bobTime += Time.deltaTime * Mathf.Max(dir.magnitude, 0.5f);

			if (dir.magnitude > 0.1f) bobMult += Time.deltaTime; else bobMult -= Time.deltaTime * 2;

			bobMult = Mathf.Clamp(bobMult, 0.05f, 1);

			if (recoil != Vector2.zero) {
				core.camera.transform.localPosition = camBobPos + (Vector3.right * bobX) + (Vector3.up * bobY) + ((Random.insideUnitSphere * recoil.magnitude) / 500000f);
				core.camera.transform.localEulerAngles = recoil * 0.1f;
			} else {
				core.camera.transform.localPosition = camBobPos + (Vector3.right * bobX) + (Vector3.up * bobY);
				core.camera.transform.localEulerAngles = Vector3.zero;
			}

			transform.localEulerAngles = Vector3.up * rotation.y;
			core.neck.forward = Vector3.Lerp(core.neck.forward, Quaternion.Euler(rotation) * Vector3.forward, Time.deltaTime * 50);

			if (inventory.selectedItem != null && inventory.selectedItem.id != "" && (inventory.selectedItem.GetData() is Gun || inventory.selectedItem.GetData() is Throwable || inventory.selectedItem.id == "flashlight")) {
				leftShoulder.localEulerAngles = new Vector3(rotation.x, 0, 102.339f);
				rightShoulder.localEulerAngles = new Vector3(rotation.x, 0, -102.339f);
			} else {
				leftShoulder.localEulerAngles = new Vector3(0, 0, 102.339f);
				rightShoulder.localEulerAngles = new Vector3(0, 0, -102.339f);
			}
		}

		GetComponent<Footsteps>().canPlay = Physics.CheckSphere(transform.position, 0.22f, LayerMask.GetMask(new string[] {"Default", "Ragdoll"}));
		GetComponent<Footsteps>().running = PlayerControls.GetInput("run");
	}

	private void CheckMovement(bool canMove = true) {
		Vector3 way = Vector3.zero;
		float speed = 1;

		bool isGrounded = Physics.CheckSphere(transform.position, 0.22f, LayerMask.GetMask(new string[] {"Default", "Ragdoll"}));
		bool isUnderwater = transform.position.y < -0.5f;

		if (canMove) {
			if (PlayerControls.GetInput("move_forwards")) way.z += 1f;
			if (PlayerControls.GetInput("move_backwards")) {
				way.z -= 1f;
				speed = 0.5f;
			}
			if (PlayerControls.GetInput("move_left")) way.x -= 1f;
			if (PlayerControls.GetInput("move_right")) way.x += 1f;
		}

		if (PlayerControls.GetInput("run")) {
			int runSpeed = 7;

			if (core.perk == Perk.Athlete) runSpeed = 8; else if (core.perk == Perk.Monkey) runSpeed = 9;

			way = way.normalized * runSpeed * speed;

			if (core.animator.GetBool("Crouching")) {
				core.animator.SetBool("Crouching", false);
				core.CmdSneak(false);
			}			
		} else {
			way = way.normalized * 3 * speed;
		}

		if (!falling && !isGrounded) falling = true;
		if (falling && isGrounded && topYPos - transform.position.y >= 1) falling = false;

		Vector3 velocity = (transform.right * way.x) + (transform.forward * way.z);

		controller.velocity = new Vector3(velocity.x, controller.velocity.y, velocity.z);

		if (isGrounded && canMove) {
			if (PlayerControls.GetInput("jump") && jumpWait < Time.time) {
				controller.AddForce(Vector3.up * 6, ForceMode.VelocityChange);
				jumpWait = Time.time + 0.25f;
			}
		}

		if (!isGrounded && transform.position.y > topYPos) topYPos = transform.position.y;

		if (isGrounded && topYPos != -1000) {
			float diff = topYPos - transform.position.y;

			if (diff >= 3 && hitWait < Time.time) {
				core.Damage((int) Mathf.Ceil((diff - 3) * 10), controller.velocity, "", Vector3.zero);

				hitWait = Time.time + 0.1f;
			}

			topYPos = -1000;
		}

		core.animator.SetBool("Walking", way.magnitude > 0.5f && way.magnitude < 5f);
		core.animator.SetBool("Running", way.magnitude >= 5f);

		if (Input.GetKeyDown(KeyCode.LeftControl)) {
			core.animator.SetBool("Crouching", !core.animator.GetBool("Crouching"));
			core.CmdSneak(core.animator.GetBool("Crouching"));
		}
	}

	public void AddRecoil(Vector2 r, bool additive = false) {
		if (additive) recoil += r; else recoil = r;
		recoilTime = Time.time;
	}
}
