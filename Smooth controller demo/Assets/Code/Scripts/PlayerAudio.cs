using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour {
    
    [SerializeField] AudioPlayer audioPlayer;
    [SerializeField] string[] footstepClips;
    private int prevFootstep;

    public void PlayFootstep() {
        audioPlayer.Play(footstepClips[prevFootstep = Utility.RandomizeNew((0, footstepClips.Length), prevFootstep)]);
    }
}
