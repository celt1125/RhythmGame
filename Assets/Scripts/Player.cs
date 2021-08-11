using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	private Vector3 origin;
	private SpriteRenderer sprite_renderer;
	private float player_height;
	private float player_width;
	private ImageManager IM;
	
	private float boundary_endurance = 0.2f;
	
    // Start is called before the first frame update
    void Start()
    {
		IM = FindObjectOfType<ImageManager>();
		SetPlayerSkin();
		
		float y_pos = player_height * 0.5f;
		origin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
		transform.position = origin + new Vector3(-origin.x, y_pos, -origin.z);
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
			Move();
		if (Input.GetMouseButton(0))
			Move();
		/*
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
		if (hit.collider.CompareTag("Player"))
			Move();*/
    }
	
	private void SetPlayerSkin(){
		sprite_renderer = GetComponent<SpriteRenderer>();
		sprite_renderer.sprite = IM.player_image[PlayerPrefs.GetInt("skin")].image[0];
		player_width = sprite_renderer.sprite.rect.width / 256;
		player_height = sprite_renderer.sprite.rect.height / 256;
		GetComponent<BoxCollider2D>().size = new Vector2(player_width, player_height);
		
		Transform catcher = transform.Find("Catcher");
		catcher.GetComponent<SpriteRenderer>().sprite = IM.player_image[PlayerPrefs.GetInt("skin")].image[1];
		catcher.position = new Vector3(0, transform.position.y + IM.player_image[PlayerPrefs.GetInt("skin")].shift, 0);
		Rect catcher_rect = IM.player_image[PlayerPrefs.GetInt("skin")].image[1].rect;
		catcher.GetComponent<BoxCollider2D>().size = new Vector2(catcher_rect.width * 0.9f / 256f, 0.04f);
		//catcher.GetComponent<BoxCollider2D>().offset = new Vector2(0, IM.player_image[PlayerPrefs.GetInt("skin")].offset);
	}
	
	private void Move(){
		Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		if (mouse_pos.y < origin.y + player_height * 1.8f){
			if (AbsFloat(mouse_pos.x) < AbsFloat(origin.x + player_width*boundary_endurance))
				transform.position += new Vector3(mouse_pos.x - transform.position.x, 0, 0);
			else{
				if (mouse_pos.x < 0)
					transform.position += new Vector3(origin.x + player_width*boundary_endurance - transform.position.x, 0, 0);
				else
					transform.position += new Vector3(-origin.x - player_width*boundary_endurance - transform.position.x, 0, 0);
			}
		}
	}
	
	private float AbsFloat(float num){
		return num > 0 ? num : -num;
	}
}
