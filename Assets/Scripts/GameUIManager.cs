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
	private TMP_Text combo_text;
	
	private Vector2 move_pos;
	
    // Start is called before the first frame update
    void Start()
    {
		combo_text = combo_transform.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void UpdateProgressBar(float val){
		progress_bar_slider.value = val;
	}
	
	public void UpdateScorePercentage(float p){
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
}
