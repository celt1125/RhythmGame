using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropNote : MonoBehaviour
{
	public Sprite[] note_img;
	private SpriteRenderer sprite_renderer;
	private float speed;
	private float drop_height;
	private float bottom;
	private float time;
	
	private RhythmGame rhythm_game;
	
    // Start is called before the first frame update
    void Start()
    {
		rhythm_game = FindObjectOfType<RhythmGame>();
		
        sprite_renderer = GetComponent<SpriteRenderer>();
		speed = rhythm_game.speed;
		drop_height = -2 * Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 1.2f;
		bottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 0.5f;
		time = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        switch (transform.name){
			case "big":
				sprite_renderer.sprite = note_img[0];
				break;
			case "small":
				sprite_renderer.sprite = note_img[1];
				break;
			case "osu":
				sprite_renderer.sprite = note_img[2];
				break;
			default:
				sprite_renderer.sprite = note_img[0];
				break;
		}
		float y_pos = drop_height - (drop_height - bottom) * time / speed;
		if (y_pos > bottom)
			transform.position = new Vector3(transform.position.x, y_pos, 0);
		else{
			transform.position = new Vector3(transform.position.x, bottom, 0);
		}
		time += Time.deltaTime;
    }
}
