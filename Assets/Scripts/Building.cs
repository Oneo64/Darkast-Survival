using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Building : NetworkBehaviour
{
	public int maxHealth = 100;
	public float blastVulnerability = 1;

	public int health = 100;

	void Start() {
		health = maxHealth;
	}

	[Command(requiresAuthority = false)]
	public void CmdDamage(int damage) {
		health -= damage;
	}
}
