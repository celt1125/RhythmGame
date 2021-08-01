using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataIO : MonoBehaviour
{
    public static Song LoadSongData(string file_name){
		Song song;
		string tmp_path = "Songs/" + file_name;
		TextAsset json = Resources.Load<TextAsset>(tmp_path);
		if (json == null){
			print("The song does not exist.");
			song = new Song();
			return song;
		}
		else{
			song = JsonUtility.FromJson<Song>(json.text);
			return song;
		}
	}
	
	public void WriteSongData(Song song, string file_name){
		string tmp_path = Path.Combine(Application.persistentDataPath, file_name + ".json");
		print(tmp_path);
		using (StreamWriter stream = new StreamWriter(tmp_path)){
			string json = JsonUtility.ToJson(song);
			stream.Write(json);
		}
	}
}
