using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds){
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.outputAudioMixerGroup = s.output;
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
		}
    }

    public float Play (string name){
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null){
			print("no such song");
			return 0;
		}
		s.source.Play();
		return s.source.clip.length;
	}
	
	public void Pause (string name){
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null){
			print("no such song");
			return;
		}
		s.source.Pause();
	}
	
	public void Stop (string name){
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null){
			print("no such song");
			return;
		}
		s.source.Stop();
	}
}
