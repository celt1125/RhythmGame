using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ImageManager : MonoBehaviour
{
    public List<PlayerImageList> player_image;
	public List<ImageList> note_image;
	public ImageList score_image;
	public ImageList rank_image;
	public SpriteAtlas atlas;
	
	private string[] skin_name = new string[] {"default", "taichi"};
	private string[] player_type_list = new string[] {"player", "catcher"};
	private string[] note_type_list = new string[] {"main", "ornament", "auxiliary", "osu", "osuring", "cross"};
	private string[] score_type_list = new string[] {"excellent", "good", "bad"};
	private string[] rank_type_list = new string[] {"SS", "S", "A", "B", "C", "D"};
	
	void Start(){
		for (int i = 0; i < player_image.Count; i++){
			for (int j = 0; j < 2; j++)
				player_image[i].image.Add(atlas.GetSprite(skin_name[i] + "_" + player_type_list[j]));
			for (int j = 0; j < 6; j++)
				note_image[i].image.Add(atlas.GetSprite(skin_name[i] + "_" + note_type_list[j]));
		}
		for (int j = 0; j < 3; j++)
			score_image.image.Add(atlas.GetSprite("default" + "_" + score_type_list[j]));
		for (int j = 0; j < 6; j++)
			rank_image.image.Add(atlas.GetSprite(rank_type_list[j]));
	}
}

[System.Serializable]
public class PlayerImageList{
	public List<Sprite> image;
	public float shift;
}

[System.Serializable]
public class ImageList{
	public List<Sprite> image;
}

