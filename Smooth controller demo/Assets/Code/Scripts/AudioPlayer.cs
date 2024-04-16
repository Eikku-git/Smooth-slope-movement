using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

    private Dictionary<string, AudioSource> clips;

    private void Awake() {
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        clips = new(sources.Length);
        for (int i = 0; i < sources.Length; i++) {
            clips[sources[i].name] = sources[i];
        }
    }

    public void Play(string clip) {
        clips[clip].Play();
    }
}
