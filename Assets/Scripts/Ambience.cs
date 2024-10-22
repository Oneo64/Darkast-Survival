using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Ambience : MonoBehaviour
{
	public AudioClip[] sounds;
	public AudioClip[] hurtSounds;
	public AudioSource sound;
	public AudioSource hurtSound;
	[Min(1)] public float interval = 5;

	IEnumerator Start() {
		while (sound != null) {
			sound.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);

			yield return new WaitForSeconds(Random.Range(interval, interval * 2));
		}
	}

	public void Hit(string hitName) {
		if (hurtSound != null && hurtSounds.Length > 0) hurtSound.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)]);
	}
}
