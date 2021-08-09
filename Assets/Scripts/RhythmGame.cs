using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGame : MonoBehaviour
{
	public GameObject note_prefab;
	public GameObject player;
	
	private Song song;
	public float speed;
	private float time;
	public float score;
	private int combo;
	private int length;
	private int ptr;
	private float limit;
	private bool is_music_play = false;
	private string mode;
	private float song_length;
	private bool is_playing = false;
	
	private AudioManager AM;
	public Transform note_pool;
	public Transform catcher;
	public Transform black_fade;
	private SpriteRenderer black_fade_sprite;
	
	private ObjectPooler object_pooler;
	
	void Awake(){
        //song = song_list[0];
		/*
		if (PlayerPrefs.GetInt("skin") == null)
			PlayerPrefs.SetInt("skin", 0);
		*/
	}
	
    // Start is called before the first frame update
    void Start()
    {
		limit = -Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - 0.5f;
		AM = FindObjectOfType<AudioManager>();
		object_pooler = ObjectPooler.instance;
		black_fade_sprite = black_fade.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
		if (is_playing){
			if (time <= 0.1f)
				PrepareForGame();
			
			while (ptr < length){
				if (song.notes[ptr].timing - time < Time.deltaTime){
					DropNote(song.notes[ptr]);
					ptr++;
				}
				else
					break;
			}
			if (!is_music_play){
				if (speed < time){
					song_length = AM.Play(song.name);
					song_length += 0.5f;
					is_music_play = true;
				}
			}
			
			if (time > song_length)
				EndGame();
			time += Time.deltaTime;
		}
    }
	
	private void InitializeSong(string song_name){
		song = DataIO.LoadSongData(song_name);
		speed = song.speed;
		length = song.notes.Length;
		time = -0.5f;
		score = 0;
		combo = 0;
		ptr = 0;
		
		player.SetActive(true);
	}
	
	private void PrepareForGame(){
		black_fade_sprite.color = new Color(0, 0, 0, time < 0 ? time + 1f : 1f);
	}
	 
	private void DropNote(Note note){
		GameObject new_note = object_pooler.SpawnFromPool(note.note_type, new Vector3(-limit + 2*limit*note.position/22,10,0),
													Quaternion.identity, note_pool.transform);
		new_note.GetComponent<DropNote>().clear = note.clear;
	}
	
	public void CatchNote(bool success, int status){ // status: 0 for bad, 1 for good, 2 for excellent
		if (success){
			if (status == 0){
				combo = 0;
				score += 50;
			}
			else if (status == 1){
				combo++;
				score += combo * 10;
				score += 150;
			}
			else if (status == 2){
				combo++;
				score += combo * 10;
				score += 300;
			}
			else if (status == 3){
				score += 50;
			}
		}
		else{
			combo = 0;
		}
		//print($"{combo}, {score}, {status}");
	}
	
	private IEnumerator Wait(float wait_time, System.Action callback = null){
		yield return new WaitForSeconds(wait_time);
		callback?.Invoke();
	}
	
	public void CleanCatcher(){
		List<Transform> clean_list = new List<Transform>();
		foreach(Transform child in catcher)
			clean_list.Add(child);
		foreach(Transform child in clean_list)
			child.GetComponent<DropNote>().CleanOut();
	}
	
	private void EndGame(){
		player.SetActive(false);
		//print("END");
	}
}
