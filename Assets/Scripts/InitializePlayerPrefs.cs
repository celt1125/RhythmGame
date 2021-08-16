using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializePlayerPrefs : MonoBehaviour
{
	private List<string> song_list;
	
    void Start()
    {
		song_list = DataIO.LoadSongList();
		//print(PlayerPrefs.GetInt("is_initialized", 0));
		foreach (string song in song_list){
			if (PlayerPrefs.GetInt(song + "_initialized", 0) == 0){
				PlayerPrefs.SetInt(song + "_initialized", 1);
				PlayerPrefs.SetInt(song + "bestscore", 0);
				PlayerPrefs.SetInt(song + "accuracy", 0);
				PlayerPrefs.SetInt(song + "rank", -1);
			}
		}
		if (PlayerPrefs.GetInt("is_initialized", 0) == 0){
			PlayerPrefs.SetInt("is_initialized", 1);
			PlayerPrefs.SetInt("skin", 1);
		}
    }
}
