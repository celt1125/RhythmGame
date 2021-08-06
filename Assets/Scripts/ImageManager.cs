using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
    public List<ImageList> player_image;
	public List<ImageList> note_image;
	
	void Start(){
		/*
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.sprite = player_image[1].image[0];
		print(player_image[1].image[0].rect);*/
		PlayerPrefs.SetInt("skin", 1);
	}
}

[System.Serializable]
public class ImageList{
	public List<Sprite> image;
	public float shift;
}
