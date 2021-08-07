using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGame : MonoBehaviour
{
	public GameObject note_prefab;
	
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
	
	private AudioManager AM;
	public Transform note_pool;
	public Transform catcher;
	
	private ObjectPooler object_pooler;
	
	void Awake(){
		song = DataIO.LoadSongData("Testing");
        //song = song_list[0];
		/*
		if (PlayerPrefs.GetInt("skin") == null)
			PlayerPrefs.SetInt("skin", 0);
		*/
	}
	
    // Start is called before the first frame update
    void Start()
    {
		speed = song.speed;
		time = -0.5f;
		score = 0;
		combo = 0;
		length = song.notes.Length;
		ptr = 0;
		limit = -Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - 0.5f;
		
		AM = FindObjectOfType<AudioManager>();
		object_pooler = ObjectPooler.instance;
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
		if (!is_music_play){
			if (speed < time){
				song_length = AM.Play(song.name);
				song_length += 0.5f;
				is_music_play = true;
			}
		}
		else
			if (time > song_length)
				EndGame();
		time += Time.deltaTime;
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
		Time.timeScale = 0;
		//print("END");
	}
}
