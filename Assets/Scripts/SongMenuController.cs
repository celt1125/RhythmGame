using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongMenuController : MonoBehaviour
{
	public GameObject song_button;
	public Transform scroll;
	public string[] song_list;
	
	private int h = 150;
	private int w = 400;
	private List<SongProperties> song_properties_list = new List<SongProperties>();
	
	void Start(){
		InitializeSongMenu();
		int a = 3;
		print(a>>1);
	}
	
	private void InitializeSongMenu(){
		scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, song_list.Length * h + 200);
		int pos = 0;
		int enlarge_pos = song_list.Length >> 1;
		foreach(string song_name in song_list){
			GameObject new_song_button = Instantiate(song_button, new Vector3(0,0,0), Quaternion.identity);
			new_song_button.transform.SetParent(scroll);
			new_song_button.transform.Find("SongName").GetComponent<TMP_Text>().text = song_name;
			new_song_button.transform.name = song_name;
			RectTransform rect_transform = new_song_button.GetComponent<RectTransform>();
			
			rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			if (pos < enlarge_pos)
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 50 + pos * h, h);
			else if (pos == enlarge_pos)
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 50 + pos * h, h + 100);
			else
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 150 + pos * h, h);
			
			SongProperties song_properties = new SongProperties() {button=new_song_button,
																   name=song_name,
																   difficulty=0,
																   best_score=0,
																   rank="N"};
			song_properties_list.Add(song_properties);
			pos ++;
		}
	}
	
	public void UpdateSongMenu(Vector2 position){
		int pos = (int)Mathf.Round((1 - position.y) * (song_list.Length - 1));
		pos = pos >= song_properties_list.Count ? song_properties_list.Count - 1 : pos;
		pos = pos < 0 ? 0 : pos;
		
		for (int i = 0; i < song_properties_list.Count; i++){
			RectTransform rect_transform = song_properties_list[i].button.GetComponent<RectTransform>();
			if (i < pos)
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 50 + i * h, h);
			else if (i == pos)
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 50 + i * h, h + 100);
			else
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 150 + i * h, h);
		}
	}
	
    public void PrintValue(Vector2 position){
		//print($"{position.x}, {position.y}");
	}
}

public class SongProperties{
	public GameObject button;
	public string name;
	public int difficulty;
	public int best_score;
	public string rank;
}
