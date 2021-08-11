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
	private float actual_score;
	private float full_score;
	private int combo;
	private int length;
	private int ptr;
	private float limit;
	private bool is_music_play = false;
	private string mode;
	private float song_length;
	private bool is_playing = false;
	
	private AudioManager AM;
	private GameUIManager GUIM;
	private ObjectPooler object_pooler;
	
	public Transform note_pool;
	public Transform catcher;
	public Transform black_fade;
	private SpriteRenderer black_fade_sprite;
	
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
		GUIM = FindObjectOfType<GameUIManager>();
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
			if (time > song_length){
				EndGame();
			}
			
			GUIM.UpdateProgressBar(time > 0f ? time / song_length : 0f);
			GUIM.UpdateScorePercentage(full_score == 0 ? 100 : actual_score / full_score * 100f);
			GUIM.UpdateCombo(catcher.position + new Vector3(0, 1f, 0), combo);
			
			time += Time.deltaTime;
		}
    }
	
	public void StartSong(string song_name){
		song = DataIO.LoadSongData(song_name);
		speed = song.speed;
		length = song.notes.Length;
		song_length = speed + 1f;
		time = -1f;
		score = 0;
		full_score = 0;
		actual_score = 0;
		combo = 0;
		ptr = 0;
		is_playing = true;
		
		player.SetActive(true);
	}
	
	private void PrepareForGame(){
		black_fade_sprite.color = new Color(0, 0, 0, time < 0 ? time + 1f : 1f);
	}
	 
	private void DropNote(Note note){
		//print($"{limit}, {-limit + 2*limit*note.position/22}");
		
		GameObject new_note = object_pooler.SpawnFromPool(note.note_type, new Vector3(-limit + 2*limit*note.position/22,10,0),
													Quaternion.identity, note_pool.transform);
		new_note.GetComponent<DropNote>().clear = note.clear;
	}
	
	public void CatchNote(bool is_osu, bool success, int status){
		// status: (is_osu == true)  0 for failed, 1 for bad, 2 for good, 3 for excellent
		//         (is_osu == false) 1 for auxiliary, 2 for ornament, 3 for main
		if (is_osu){
			full_score += 300;
			switch (status){
				case 0:
					combo = 0;
					break;
				case 1:
					combo = 0;
					score += 50;
					actual_score += 50;
					break;
				case 2:
					combo++;
					score += combo * 10;
					score += 150;
					actual_score += 150;
					break;
				case 3:
					combo++;
					score += combo * 10;
					score += 300;
					actual_score += 300;
					break;
				default:
					break;
			}
		}
		else{
			if (!success)
				combo = 0;
			switch (status){
				case 1:
					full_score += 50;
					if (success){
						score += 50;
						actual_score += 50;
					}
					break;
				case 2:
					full_score += 150;
					if (success){
						combo++;
						score += combo * 10;
						score += 150;
						actual_score += 150;
					}
					break;
				case 3:
					full_score += 300;
					if (success){
						combo++;
						score += combo * 10;
						score += 300;
						actual_score += 300;
					}
					break;
				default:
					break;
			}
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
		is_playing = false;
		player.SetActive(false);
		//print("END");
	}
}
