using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class DroppedItem : NetworkBehaviour
{
	[SyncVar(hook="UpdateModel")] public Item item;
	[SyncVar] public Transform owner;

	public AudioClip[] impactNoises;
	public AudioSource impactNoise;

	public PhysicMaterial mat;

	void Start() {
		if (!isServer) GetComponent<Rigidbody>().isKinematic = true;
	}

	[Command(requiresAuthority = false)]
	public void CmdDestroy() {
		Destroy(gameObject);
	}

	private void UpdateModel(Item oldValue, Item newValue) {
		//if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);

		if (newValue != null) {
			string modelName = "";

			if (Database.items[newValue.id] is Gun) {
				modelName = ((Gun) Database.items[newValue.id]).model[0];
			} else if (Database.items[newValue.id] is Tool) {
				modelName = ((Tool) Database.items[newValue.id]).model;
			} else if (Database.items[newValue.id] is Food) {
				modelName = ((Food) Database.items[newValue.id]).model;
			} else if (Database.items[newValue.id] is Throwable) {
				modelName = ((Throwable) Database.items[newValue.id]).model;
			}

			if (Resources.Load("ItemModels/" + modelName)) {
				GameObject model = Instantiate(Resources.Load("ItemModels/" + modelName) as GameObject, transform);
				Mesh m = model.GetComponent<MeshFilter>().sharedMesh;

				model.transform.localScale = Vector3.one;
				model.transform.localPosition = Vector3.zero;
				model.transform.localEulerAngles = Vector3.zero;

				BoxCollider collider = gameObject.AddComponent<BoxCollider>();

				collider.material = mat;

				collider.center = m.bounds.center;
				collider.size = m.bounds.size;

				Physics.IgnoreCollision(collider, owner.GetComponent<Collider>());
			}
		}
	}

	void OnCollisionEnter(Collision c) {
		if (isServer && owner != null && c.transform != owner) {
			int dmg = Mathf.FloorToInt((c.relativeVelocity.magnitude * Mathf.Pow(GetComponent<Rigidbody>().mass, 2)) / 2f);

			if (dmg > 5) {
				if (c.transform.root.GetComponentInParent<Enemy>()) {
					c.transform.root.GetComponentInParent<Enemy>().CmdDamage(dmg, GetComponent<Rigidbody>().velocity * GetComponent<Rigidbody>().mass * 0.5f, null, c.transform.name, GetComponent<Rigidbody>().velocity);
				}

				if (c.transform.root.GetComponentInParent<PlayerCore>()) {
					c.transform.root.GetComponentInParent<PlayerCore>().RpcDamage(dmg, GetComponent<Rigidbody>().velocity * GetComponent<Rigidbody>().mass * 0.5f, c.transform.name, GetComponent<Rigidbody>().velocity);
				}
			}
		}

		if (impactNoise != null && c.relativeVelocity.magnitude > 0.5f) {
			impactNoise.clip = impactNoises[Random.Range(0, impactNoises.Length)];
			impactNoise.maxDistance = 1 + c.relativeVelocity.magnitude / 2f;

			impactNoise.Play();
		}
	}
}
