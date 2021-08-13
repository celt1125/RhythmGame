using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializePlayerPrefs : MonoBehaviour
{
	
    public List<SongDifficulty> song_list;
	
    void Start()
    {
		print(PlayerPrefs.GetInt("is_initialized", 0));
		if (PlayerPrefs.GetInt("is_initialized", 0) == 0){
			PlayerPrefs.SetInt("is_initialized", 1);
			foreach (SongDifficulty song in song_list){
				PlayerPrefs.SetInt(song.song_name + "bestscore", 0);
				PlayerPrefs.SetInt(song.song_name + "rank", -1);
				PlayerPrefs.SetInt(song.song_name + "difficulty", song.difficulty);
			}
			PlayerPrefs.SetInt("skin", 1);
		}
        
    }
}

[System.Serializable]
public class SongDifficulty{
	public string song_name;
	public int difficulty;
}
