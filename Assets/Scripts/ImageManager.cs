using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ImageManager : MonoBehaviour
{
    public List<ImageList> player_image;
	public List<ImageList> note_image;
	public SpriteAtlas atlas;
	
	private string[] skin_name = new string[] {"default", "taichi"};
	private string[] player_type_list = new string[] {"player", "catcher"};
	private string[] note_type_list = new string[] {"main", "ornament", "auxiliary", "osu", "osuring", "cross"};
	
	void Start(){
		for (int i = 0; i < player_image.Count; i++){
			for (int j = 0; j < 2; j++)
				player_image[i].image.Add(atlas.GetSprite(skin_name[i] + "_" + player_type_list[j]));
			for (int j = 0; j < 6; j++)
				note_image[i].image.Add(atlas.GetSprite(skin_name[i] + "_" + note_type_list[j]));
		}
	}
}

[System.Serializable]
public class ImageList{
	public List<Sprite> image;
	public float shift;
	public float boundary_endurance;
}
