using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FootstepSound {
	public string keyword;
	public AudioClip[] walking;
	public AudioClip[] running;

	public FootstepSound(string kw, AudioClip[] w, AudioClip[] r) {
		keyword = kw;
		walking = w;
		running = r;
	}
}

public class Footsteps : MonoBehaviour
{
	public AudioSource source;
	public bool canPlay;

	public bool running;

	public FootstepSound[] sounds;

	void PlaySound() {
		source.clip = running ? sounds[0].running[Random.Range(0, sounds[0].running.Length)] : sounds[0].walking[Random.Range(0, sounds[0].walking.Length)];

		if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 1.5f, LayerMask.GetMask("Default"))) {
			if (hit.transform.GetComponent<Renderer>()) {
				foreach (FootstepSound sound in sounds) {
					if (hit.transform.GetComponent<Renderer>().materials[0].name.ToLower().Contains(sound.keyword)) {
						source.clip = running ? sound.running[Random.Range(0, sound.running.Length)] : sound.walking[Random.Range(0, sound.walking.Length)];
						break;
					}
				}
			}
		}

		if (canPlay) source.PlayOneShot(source.clip);
	}
}
