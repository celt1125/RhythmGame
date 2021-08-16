using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongData : MonoBehaviour
{
	private Dictionary<string, Song> song_data;
	private List<string> song_list;
	
    // Start is called before the first frame update
    void Start()
    {
		song_data = new Dictionary<string, Song>();
        song_list = DataIO.LoadSongList();
		foreach (string song in song_list){
			song_data.Add(song, DataIO.LoadSongData(song));
		}
    }
	
	public Song LoadSong(string song){
		return song_data[song];
	}
}
