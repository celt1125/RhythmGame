using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGame : MonoBehaviour
{
	public GameObject note_prefab;
	
	public Song[] song_list;
	private Song song;
	public float speed;
	private float time;
	public float score;
	private int length;
	private int ptr;
	private float limit;
	
	private AudioManager AM;
	
	
	void Awake(){
		print("awake");
	}
	
    // Start is called before the first frame update
    void Start()
    {
        song = song_list[0];
		
		speed = song.speed;
		time = 0;
		score = 0;
		length = song.notes.Length;
		ptr = 0;
		limit = -Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - 0.5f;
		
		AM = FindObjectOfType<AudioManager>();
		//AM.Play(song.name);
    }

    // Update is called once per frame
    void Update()
    {
        while (ptr < length){
			if (song.notes[ptr].timing - time < Time.deltaTime){
				DropNote(song.notes[ptr]);
				ptr++;
			}
			else
				break;
		}
		time += Time.deltaTime;
    }
	
	private void DropNote(Note note){
		GameObject new_note = Instantiate(note_prefab, new Vector3(-limit + 2*limit*note.position*0.05f,10,0), Quaternion.identity);
		new_note.name = note.note_type;
	}
}
