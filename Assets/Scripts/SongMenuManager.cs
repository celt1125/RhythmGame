using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SongMenuManager : MonoBehaviour
{
	public GameObject song_menu_panel;
	public GameObject song_button;
	public GameObject song_menu;
	public GameObject play_panel;
	public GameObject game_panel;
	public Button[] sorting_buttons;
	public Transform scroll;
	public List<string> song_list; // default: sort by name
	private ScrollRect scroll_rect;
	
	private int h = 150;
	private int w = 400;
	private int enlargement_h = 100;
	private int enlargement_w = 100;
	private int space = 500;
	private float scroll_height;
	private float space_ratio;
	private string sorting_state = "name";
	private string selected_song_name;
	private bool is_ascending = true;
	private List<SongProperties> song_properties_list = new List<SongProperties>();
	
	private AudioManager AM;
	private RhythmGame rhythm_game;
	
	void Start(){
		AM = FindObjectOfType<AudioManager>();
		rhythm_game = FindObjectOfType<RhythmGame>();
		
		SetScroll();
		InitializeSongMenu();
	}
	
	private void SetScroll(){
		scroll_height = song_list.Count * h + space * 2 + enlargement_h;
		space_ratio = space / scroll_height;
		scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scroll_height);
		scroll_rect = song_menu.GetComponent<ScrollRect>();
	}
	
	private void InitializeSongMenu(){
		int pos = 0;
		int enlarge_pos = song_list.Count >> 1;
		foreach(string song_name in song_list){
			GameObject new_song_button = Instantiate(song_button, new Vector3(0,0,0), Quaternion.identity);
			new_song_button.transform.SetParent(scroll);
			new_song_button.transform.Find("SongName").GetComponent<TMP_Text>().text = song_name;
			new_song_button.transform.name = song_name;
			//new_song_button.GetComponent<Button>().onClick.AddListener(delegate{ PlaySong(song_name); });
			new_song_button.GetComponent<Button>().onClick.AddListener(OnSongClick);
			
			RectTransform rect_transform = new_song_button.GetComponent<RectTransform>();
			if (pos < enlarge_pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + pos * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
			else if (pos == enlarge_pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + pos * h, h + enlargement_h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w + enlargement_w);
			}
			else{
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + enlargement_h + pos * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
			
			SongProperties song_properties = new SongProperties() {button=new_song_button,
																   name=song_name,
																   difficulty=0,
																   best_score=0,
																   rank="N"};
			song_properties_list.Add(song_properties);
			pos ++;
		}
	}
	
	private void UpdateSongMenu(){
		int pos = 0;
		int enlarge_pos = song_list.Count >> 1;
		foreach(string song_name in song_list){
			GameObject tmp_song_button = song_properties_list[pos].button;
			tmp_song_button.transform.Find("SongName").GetComponent<TMP_Text>().text = song_name;
			tmp_song_button.transform.name = song_name;
			//new_song_button.GetComponent<Button>().onClick.AddListener(delegate{ PlaySong(song_name); });
			
			pos ++;
		}
	}
	
	public void UpdateSongMenuPosition(Vector2 position){
		int pos = Ratio2Position(position.y);
		for (int i = 0; i < song_properties_list.Count; i++){
			RectTransform rect_transform = song_properties_list[i].button.GetComponent<RectTransform>();
			Button button = song_properties_list[i].button.GetComponent<Button>();
			if (i < pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + i * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
			else if (i == pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + i * h, h + enlargement_h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w + enlargement_w);
			}
			else{
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + enlargement_h + i * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
		}
	}
	
	public void OnSongClick(){
		selected_song_name = EventSystem.current.currentSelectedGameObject.name;
		int pos = song_list.FindIndex(s => s == selected_song_name);
		if (pos == -1){
			print("error song name");
			return;
		}
		if (pos == Ratio2Position(scroll_rect.verticalNormalizedPosition)){
			play_panel.SetActive(true);
		}
		else
			StartCoroutine(ScrollToPosition(pos));
	}
	
	public void SortSongMenu(string type){
		//Shuffle(song_list);
		foreach (Button button in sorting_buttons)
			button.interactable = false;
		Sort(type);
		UpdateSongMenu();
		StartCoroutine(ScrollUp( () => {
			foreach (Button button in sorting_buttons)
				button.interactable = true;
		}));
	}
	
	private void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T tmp = list[k];
            list[k] = list[n];
            list[n] = tmp;
        }
    }
	
	private IEnumerator ScrollUp(System.Action callback = null){
		float move_period = 1f;
		for (float time_ratio = 0f; time_ratio < 1f; time_ratio += Time.deltaTime / move_period){
			scroll_rect.verticalNormalizedPosition = time_ratio;
			yield return null;
		}
		scroll_rect.verticalNormalizedPosition = 1f;
		callback?.Invoke();
	}
	
	private IEnumerator ScrollToPosition(int pos){
		float current_ratio = scroll_rect.verticalNormalizedPosition;
		float final_ratio = Position2Ratio(pos);
		float ratio_diff = final_ratio - current_ratio;
		float move_period = 0.1f;
		for (float time_ratio = 0f; time_ratio < 1f; time_ratio += Time.deltaTime / move_period){
			scroll_rect.verticalNormalizedPosition = current_ratio + ratio_diff * time_ratio;
			yield return null;
		}
		scroll_rect.verticalNormalizedPosition = final_ratio;
	}
	
	public void PlaySong(){
		rhythm_game.StartSong(selected_song_name);
		song_menu_panel.SetActive(false);
		play_panel.SetActive(false);
		game_panel.SetActive(true);
	}
	
	private int Ratio2Position(float x){
		// space/scroll_height => 1
		// 1 - space/scroll_height => 0
		x = x < space_ratio ? space_ratio : x;
		x = x > 1 - space_ratio ? 1 - space_ratio : x;
		float y = 1 - (x - space_ratio) / (1 - space_ratio * 2f);
		//print($"{y}, {(int)Mathf.Round((1 - y) * (song_list.Count - 1))}");
		return (int)Mathf.Round(y * (song_list.Count - 1));
	}
	
	private float Position2Ratio(int y){
		float ratio = (y + 0.5f) / song_list.Count;
		return (1 - ratio) * (1 - space_ratio * 2f) + space_ratio;
	}
	
	/*
	private void SwapSongProperties(int i, int j){
		string tmp_name = song_properties_list[i].name;
		song_properties_list[i].name = song_properties_list[j].name;
		song_properties_list[j].name = tmp_name;
		
		song_properties_list[i].difficulty ^= song_properties_list[j].difficulty;
		song_properties_list[i].difficulty ^= song_properties_list[j].difficulty ^= song_properties_list[i].difficulty;
		
		song_properties_list[i].best_score ^= song_properties_list[j].best_score;
		song_properties_list[i].best_score ^= song_properties_list[j].best_score ^= song_properties_list[i].best_score;
		
		string tmp_rank = song_properties_list[i].rank;
		song_properties_list[i].rank = song_properties_list[j].rank;
		song_properties_list[j].rank = tmp_name;
	}*/
	
	private void Sort(string type){ // name, difficulty, score
		if (type == sorting_state)
			is_ascending = !is_ascending;
		
		sorting_state = type;
		int i, j;
		string song_name;
		SongProperties song_properties = new SongProperties();
		for (i = 1; i != song_properties_list.Count; i++){
			song_properties.Assign(song_properties_list[i]);
			song_name = song_list[i];
			j = i - 1;
			while ((j >= 0) && CompareByType(song_properties_list[j], song_properties, type, is_ascending)){
				song_properties_list[j + 1].Assign(song_properties_list[j]);
				song_list[j + 1] = song_list[j];
				j--;
			}
			song_properties_list[j + 1].Assign(song_properties);
			song_list[j + 1] = song_name;
		}
	}
	
	private bool CompareByType(SongProperties a, SongProperties b, string type, bool is_ascending){
		switch (type){
			case "name":
				return is_ascending ? string.Compare(a.name, b.name) >= 0 : string.Compare(a.name, b.name) <= 0;
			
			case "difficulty":
				return is_ascending ? a.difficulty >= b.difficulty : a.difficulty <= b.difficulty;
			
			case "score":
				return is_ascending ? a.best_score >= b.best_score : a.best_score <= b.best_score;
			
			default:
			print("here");
				return true;
		}
	}
}

public class SongProperties{
	public GameObject button;
	public string name;
	public int difficulty;
	public int best_score;
	public string rank;
	
	public void Assign(SongProperties other){
		this.name = other.name;
		this.difficulty = other.difficulty;
		this.best_score = other.difficulty;
		this.rank = other.rank;
	}
	
	public void Swap(SongProperties other){
		string tmp_name = this.name;
		this.name = other.name;
		other.name = tmp_name;
		
		this.difficulty ^= other.difficulty;
		this.difficulty ^= other.difficulty ^= this.difficulty;
		
		this.best_score ^= other.best_score;
		this.best_score ^= other.best_score ^= this.best_score;
		
		string tmp_rank = this.rank;
		this.rank = other.rank;
		other.rank = tmp_name;
	}
	
	public void Print(){
		Debug.Log($"name: {name}, difficulty: {difficulty}, best score: {best_score}, rank: {rank}");
	}
}
