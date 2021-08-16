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
	public TMP_Text song_name_text;
	public TMP_Text best_score_text;
	public TMP_Text accuracy_text;
	public Image rank_img;
	public Transform scroll;
	private List<string> song_list; // default: sort by name
	private ScrollRect scroll_rect;
	
	private int h = 100;
	private int w = 400;
	private int enlargement_h = 100;
	private int enlargement_w = 100;
	private int space = 500;
	private float scroll_height;
	private float space_ratio;
	private string sorting_state = "name";
	private string selected_song_name;
	private string current_song;
	private string previous_song;
	private bool is_ascending = true;
	private bool is_sorting = false;
	private List<SongProperties> song_properties_list = new List<SongProperties>();
	
	private AudioManager AM;
	private ImageManager IM;
	private RhythmGame rhythm_game;
	private SongData song_data;
	
	void Start(){
		song_list = DataIO.LoadSongList();
		
		AM = FindObjectOfType<AudioManager>();
		IM = FindObjectOfType<ImageManager>();
		rhythm_game = FindObjectOfType<RhythmGame>();
		song_data = FindObjectOfType<SongData>();
		
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
			new_song_button.transform.Find("SongName").GetComponent<TMP_Text>().text = "   " + song_name;
			new_song_button.transform.Find("SongName").GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, h);
			new_song_button.transform.Find("Status").GetComponent<RectTransform>().
											SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, h, enlargement_h);
			new_song_button.transform.name = song_name;
			//new_song_button.GetComponent<Button>().onClick.AddListener(delegate{ PlaySong(song_name); });
			new_song_button.GetComponent<Button>().onClick.AddListener(OnSongClick);
			
			Song s = song_data.LoadSong(song_name);
			
			RectTransform rect_transform = new_song_button.GetComponent<RectTransform>();
			if (pos < enlarge_pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + pos * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
			else if (pos == enlarge_pos){
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + pos * h, h + enlargement_h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w + enlargement_w);
				current_song = song_name;
				AM.Play(current_song);
				
				new_song_button.transform.Find("Status").GetComponent<TMP_Text>().text = $"    Difficulty: {s.difficulty}    BPM: {s.BPM}";
				song_name_text.text = song_name;
				best_score_text.text = PlayerPrefs.GetInt(song_name + "bestscore").ToString();
				accuracy_text.text = PlayerPrefs.GetFloat(song_name + "accuracy").ToString("F1") + "%";
				if (PlayerPrefs.GetInt(song_name + "rank") != -1){
					rank_img.color = new Color(1, 1, 1, 1);
					rank_img.sprite = IM.rank_image.image[PlayerPrefs.GetInt(song_name + "rank")];
				}
				else
					rank_img.color = new Color(1, 1, 1, 0);
			}
			else{
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, space + enlargement_h + pos * h, h);
				rect_transform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, w);
			}
			
			SongProperties song_properties = new SongProperties() {button = new_song_button,
																   name = song_name,
																   difficulty = s.difficulty,
																   BPM = s.BPM,
																   best_score = PlayerPrefs.GetInt(song_name + "bestscore"),
																   accuracy = PlayerPrefs.GetFloat(song_name + "accuracy"),
																   rank = PlayerPrefs.GetInt(song_name + "rank")};
			
			
			Transform rank_transform = new_song_button.transform.Find("Rank");
			
			if (song_properties.rank != -1){
				Rect rank_rect = IM.rank_image.image[song_properties.rank].rect;
				rank_transform.GetComponent<Image>().sprite = IM.rank_image.image[song_properties.rank];
				rank_transform.GetComponent<Image>().color = new Color(1, 1, 1, 1);
				if (pos == enlarge_pos){
					rank_transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + enlargement_w + 20, 0);
					rank_transform.GetComponent<RectTransform>().sizeDelta = 
											new Vector2(rank_rect.width * (h + enlargement_h) * 0.6f / rank_rect.height, (h + enlargement_h) * 0.6f);
				}
				else{
					rank_transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + 20, 0);
					rank_transform.GetComponent<RectTransform>().sizeDelta = 
											new Vector2(rank_rect.width * h * 0.6f / rank_rect.height, h * 0.6f);
				}
			}
			else
				rank_transform.GetComponent<Image>().color = new Color(1, 1, 1, 0);
			
			song_properties_list.Add(song_properties);
			pos ++;
		}
	}
	
	private void UpdateSongMenu(){
		int pos = 0;
		int enlarge_pos = song_list.Count >> 1;
		foreach(string song_name in song_list){
			GameObject tmp_song_button = song_properties_list[pos].button;
			tmp_song_button.transform.Find("SongName").GetComponent<TMP_Text>().text = "   " + song_name;
			tmp_song_button.transform.name = song_name;
			//new_song_button.GetComponent<Button>().onClick.AddListener(delegate{ PlaySong(song_name); });
			
			pos ++;
		}
	}
	
	private void UpdateSongRank(){
		foreach (var s in song_properties_list){
			if (s.rank != -1){
				Transform rank_transform = s.button.transform.Find("Rank");
				rank_transform.GetComponent<Image>().sprite = IM.rank_image.image[PlayerPrefs.GetInt(s.name + "rank")];
				rank_transform.GetComponent<Image>().color = new Color(1, 1, 1, 1);
				Rect rank_rect = rank_transform.GetComponent<Image>().sprite.rect;
				
				if (s.name == current_song){
					rank_transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + enlargement_w + 20, 0);
					rank_transform.GetComponent<RectTransform>().sizeDelta =
									new Vector2(rank_rect.width * (h + enlargement_w) * 0.6f / rank_rect.height, (h + enlargement_w) * 0.6f);
				}
				else{
					rank_transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + 20, 0);
					rank_transform.GetComponent<RectTransform>().sizeDelta =
									new Vector2(rank_rect.width * h * 0.6f / rank_rect.height, h * 0.6f);
				}
			}
			else
				s.button.transform.Find("Rank").GetComponent<Image>().color = new Color(1, 1, 1, 0);
		}
	}
	
	public void UpdateSongStatus(string song_name){
		int pos = song_list.FindIndex(s => s == song_name);
		best_score_text.text = PlayerPrefs.GetInt(song_name + "bestscore").ToString();
		accuracy_text.text = PlayerPrefs.GetFloat(song_name + "accuracy").ToString("F1") + "%";
		rank_img.color = new Color(1, 1, 1, 1);
		rank_img.sprite = IM.rank_image.image[PlayerPrefs.GetInt(song_name + "rank")];
		
		Transform rank_transform = song_properties_list[pos].button.transform.Find("Rank");
		rank_transform.GetComponent<Image>().sprite = IM.rank_image.image[PlayerPrefs.GetInt(song_name + "rank")];
		rank_transform.GetComponent<Image>().color = new Color(1, 1, 1, 1);
		Rect rank_rect = rank_transform.GetComponent<Image>().sprite.rect;
		rank_transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + enlargement_w + 20, 0);
		rank_transform.GetComponent<RectTransform>().sizeDelta =
						new Vector2(rank_rect.width * (h + enlargement_h) * 0.6f / rank_rect.height, (h + enlargement_h) * 0.6f);
	}
	
	private void OnSongChanged(){
		if (!is_sorting){
			AM.Stop(previous_song);
			AM.Play(current_song);
		}
		SongProperties s_previous = song_properties_list.Find(s => s.name == previous_song);
		s_previous.button.transform.Find("Status").GetComponent<TMP_Text>().text = "";
		if (s_previous.rank != -1){
			Transform rank_transform1 = s_previous.button.transform.Find("Rank");
			//rank_transform1.GetComponent<Image>().sprite = IM.rank_image.image[PlayerPrefs.GetInt(previous_song + "rank")];
			//rank_transform1.GetComponent<Image>().color = new Color(1, 1, 1, 1);
			Rect rank_rect1 = rank_transform1.GetComponent<Image>().sprite.rect;
			rank_transform1.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + 20, 0);
			rank_transform1.GetComponent<RectTransform>().sizeDelta =
							new Vector2(rank_rect1.width * h * 0.6f / rank_rect1.height, h * 0.6f);
		}
		else
			s_previous.button.transform.Find("Rank").GetComponent<Image>().color = new Color(1, 1, 1, 0);
		
		SongProperties s_current = song_properties_list.Find(s => s.name == current_song);
		s_current.button.transform.Find("Status").GetComponent<TMP_Text>().text = $"    Difficulty: {s_current.difficulty}    BPM: {s_current.BPM}";
		if (s_current.rank != -1){
			rank_img.color = new Color(1, 1, 1, 1);
			rank_img.sprite = IM.rank_image.image[PlayerPrefs.GetInt(current_song + "rank")];
			Transform rank_transform2 = s_current.button.transform.Find("Rank");
			//rank_transform2.GetComponent<Image>().sprite = IM.rank_image.image[PlayerPrefs.GetInt(current_song + "rank")];
			//rank_transform2.GetComponent<Image>().color = new Color(1, 1, 1, 1);
			Rect rank_rect2 = rank_transform2.GetComponent<Image>().sprite.rect;
			rank_transform2.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, w + enlargement_w + 20, 0);
			rank_transform2.GetComponent<RectTransform>().sizeDelta =
							new Vector2(rank_rect2.width * (h + enlargement_h) * 0.6f / rank_rect2.height, (h + enlargement_h) * 0.6f);
		}
		else{
			rank_img.color = new Color(1, 1, 1, 0);
			s_current.button.transform.Find("Rank").GetComponent<Image>().color = new Color(1, 1, 1, 0);
		}
		
		song_name_text.text = current_song;
		best_score_text.text = PlayerPrefs.GetInt(current_song + "bestscore").ToString();
		accuracy_text.text = PlayerPrefs.GetFloat(current_song + "accuracy").ToString("F1") + "%";
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
				previous_song = current_song;
				current_song = song_list[i];
				if (previous_song != current_song)
					OnSongChanged();
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
		is_sorting = true;
		AM.Stop(current_song);
		foreach (Button button in sorting_buttons)
			button.interactable = false;
		Sort(type);
		UpdateSongMenu();
		UpdateSongRank();
		StartCoroutine(ScrollUp( () => {
			foreach (Button button in sorting_buttons)
				button.interactable = true;
			is_sorting = false;
			AM.Play(current_song);
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
		AM.Stop(current_song);
		song_menu_panel.SetActive(false);
		play_panel.SetActive(false);
		game_panel.SetActive(true);
		rhythm_game.StartSong(selected_song_name);
	}
	
	public void ShowSongMenu(){
		song_menu_panel.SetActive(true);
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
				return is_ascending ? a.accuracy >= b.accuracy : a.accuracy <= b.accuracy;
			
			default:
				return true;
		}
	}
}

public class SongProperties{
	public GameObject button;
	public string name;
	public int difficulty;
	public int BPM;
	public int best_score;
	public float accuracy;
	public int rank;
	
	public void Assign(SongProperties other){
		this.name = other.name;
		this.difficulty = other.difficulty;
		this.BPM = other.BPM;
		this.best_score = other.best_score;
		this.accuracy = other.accuracy;
		this.rank = other.rank;
	}
	
	public void Swap(SongProperties other){
		string tmp_name = this.name;
		this.name = other.name;
		other.name = tmp_name;
		
		this.difficulty ^= other.difficulty;
		this.difficulty ^= other.difficulty ^= this.difficulty;
		
		this.BPM ^= other.BPM;
		this.BPM ^= other.BPM ^= this.BPM;
		
		this.best_score ^= other.best_score;
		this.best_score ^= other.best_score ^= this.best_score;
		
		float tmp_accuracy = this.accuracy;
		this.accuracy = other.accuracy;
		other.accuracy = tmp_accuracy;
		
		this.rank ^= other.rank;
		this.rank ^= other.rank ^= this.rank;
	}
	
	public void Print(){
		Debug.Log($"name: {name}, difficulty: {difficulty}, BPM: {BPM}, best score: {best_score}, accuracy: {accuracy}, rank: {rank}");
	}
}
