using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
	public Canvas canvas;
	
	public Slider progress_bar_slider;
	public TMP_Text score_percentage_text;
	public Transform combo_transform;
	public GameObject game_panel;
	public GameObject pause_panel;
	public Image black_fade;
	public GameObject score_board;
	private TMP_Text combo_text;
	
	/* Scoreboard */
	public TMP_Text song_name_text;
	public TMP_Text score_text;
	public TMP_Text new_record_text;
	public TMP_Text max_combo_text;
	public TMP_Text accuracy_text;
	public Transform rank_img;
	
	
	
	private Vector2 move_pos;
	
	private RhythmGame rhythm_game;
	private SongMenuManager SMM;
	private AudioManager AM;
	private ImageManager IM;
	
    // Start is called before the first frame update
    void Start()
    {
		combo_text = combo_transform.GetComponent<TMP_Text>();
		rhythm_game = FindObjectOfType<RhythmGame>();
		SMM = FindObjectOfType<SongMenuManager>();
		AM = FindObjectOfType<AudioManager>();
		IM = FindObjectOfType<ImageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void UpdateProgressBar(float val){
		progress_bar_slider.value = val;
	}
	
	public void UpdateAccuracy(float p){
		if (p == 100f)
			score_percentage_text.text = "100%";
		else
			score_percentage_text.text = p.ToString("F1") + "%";
	}
	
	public void UpdateCombo(Vector3 pos, int combo){
		if (combo == 0)
			combo_text.text = "";
		else
			combo_text.text = combo.ToString();
		
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvas.transform as RectTransform,
			Camera.main.WorldToScreenPoint(pos),
			canvas.worldCamera,
			out move_pos);
		
		combo_transform.position = canvas.transform.TransformPoint(move_pos);
	}
	
	public void ShowScoreboard(string song_name, int score, int max_combo, float accuracy){
		ResetScoreboard();
		StartCoroutine(FadeIn(() => {
			game_panel.SetActive(false);
			rhythm_game.player.SetActive(false);
			score_board.SetActive(true);
		}));
		song_name_text.text = song_name;
		StartCoroutine(SetScore(song_name, score, accuracy));
		max_combo_text.text = max_combo.ToString();
		if (accuracy > 99.99f)
			accuracy_text.text = "100%";
		else
			accuracy_text.text = accuracy.ToString("F1") + "%";
	}
	
	private IEnumerator SetScore(string song_name, int score, float accuracy){
		for (float time = 0f; time < 1f; time += Time.deltaTime)
			yield return null;
		for (float time = 0f; time < 1f; time += Time.deltaTime * 0.6f){
			score_text.text = ((int)(score * time)).ToString();
			yield return null;
		}
		score_text.text = score.ToString();
		
		bool is_best_score = false;
		if (score > PlayerPrefs.GetInt(song_name + "bestscore")){
			new_record_text.text = "NEW RECORD";
			for (float time = 0f; time < 0.5f; time += Time.deltaTime)
				new_record_text.fontSize = 36 - 24 * time;
			PlayerPrefs.SetInt(song_name + "bestscore", score);
			is_best_score = true;
		}
		
		int rank = 0;
		if (accuracy > 99f)
			rank = 0;
		else if (accuracy > 96f)
			rank = 1;
		else if (accuracy > 90f)
			rank = 2;
		else if (accuracy > 80f)
			rank = 3;
		else if (accuracy > 70f)
			rank = 4;
		else
			rank = 5;
		
		if (is_best_score)
			PlayerPrefs.SetInt(song_name + "rank", rank);
		
		rank_img.GetComponent<Image>().sprite = IM.rank_image.image[rank];
		rank_img.GetComponent<Image>().color = new Color(1, 1, 1, 1);
		
		Rect rank_rect = IM.rank_image.image[rank].rect;
		RectTransform rank_recttransform = rank_img.GetComponent<RectTransform>();
		for (float time = 0f; time < 1f; time += Time.deltaTime * 1.5f){
			rank_recttransform.sizeDelta = new Vector2(rank_rect.width * (1 + 0.3f * time), rank_rect.height * (1 + 0.5f * time));
			yield return null;
		}
		rank_recttransform.sizeDelta = new Vector2(rank_rect.width, rank_rect.height);
	}
	
	private void ResetScoreboard(){
		song_name_text.text = "";
		score_text.text = "0";
		new_record_text.text = "";
		max_combo_text.text = "0";
		accuracy_text.text = "0%";
		rank_img.GetComponent<Image>().sprite = null;
		rank_img.GetComponent<Image>().color = new Color(1, 1, 1, 0);
	}
	
	public void ShowPausePanel(){
		Time.timeScale = 0f;
		AM.Pause(rhythm_game.song.name);
		pause_panel.SetActive(true);
	}
	
	public void PausePanelToGame(){
		Time.timeScale = 1f;
		AM.Play(rhythm_game.song.name);
		pause_panel.SetActive(false);
	}
	
	public void PausePanelToMenu(){
		AM.Stop(rhythm_game.song.name);
		Time.timeScale = 1f;
		StartCoroutine(FadeOut(() => {
			pause_panel.SetActive(false);
			game_panel.SetActive(false);
			rhythm_game.ClearAllNotes();
			rhythm_game.EndGame(false);
			rhythm_game.black_fade_sprite.color = new Color(0, 0, 0, 0);
			SMM.ShowSongMenu();
		}));
	}
	
	public void ScoreboardToGame(){
		StartCoroutine(FadeOut(() => {
			score_board.SetActive(false);
			game_panel.SetActive(true);
		}));
	}
	
	public void ScoreboardToMenu(){
		StartCoroutine(FadeOut(() => {
			score_board.SetActive(false);
			rhythm_game.black_fade_sprite.color = new Color(0, 0, 0, 0);
			SMM.ShowSongMenu();
		}));
	}
	
	private IEnumerator FadeOut(System.Action callback = null){
		black_fade.enabled = true;
		for (float time = 0f; time < 1f; time += Time.deltaTime * 1.25f){
			black_fade.color = new Color(0, 0, 0, time);
			yield return null;
		}
		black_fade.color = new Color(0, 0, 0, 1f);
		
		callback?.Invoke();
		
		for (float time = 1f; time > 0f; time -= Time.deltaTime * 1.25f){
			black_fade.color = new Color(0, 0, 0, time);
			yield return null;
		}
		black_fade.color = new Color(0, 0, 0, 0);
		black_fade.enabled = false;
	}
	
	private IEnumerator FadeIn(System.Action callback = null){
		black_fade.enabled = true;
		for (float time = 0f; time < 1f; time += Time.deltaTime * 1.25f){
			black_fade.color = new Color(0, 0, 0, time);
			yield return null;
		}
		black_fade.color = new Color(0, 0, 0, 1f);
		
		callback?.Invoke();
		
		for (float time = 0f; time < 0.3f; time += Time.deltaTime){
			yield return null;
		}
		for (float time = 1f; time > 0f; time -= Time.deltaTime * 1.25f){
			black_fade.color = new Color(0, 0, 0, time);
			yield return null;
		}
		black_fade.color = new Color(0, 0, 0, 0);
		black_fade.enabled = false;
	}
	
}
