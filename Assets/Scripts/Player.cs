using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	private Vector3 origin;
	private SpriteRenderer sprite_renderer;
	private float player_height;
	private float player_width;
	
    // Start is called before the first frame update
    void Start()
    {
		sprite_renderer = GetComponent<SpriteRenderer>();
		float y_pos = sprite_renderer.sprite.rect.height * transform.localScale.y * 0.5f / 256;
		player_height = sprite_renderer.sprite.rect.height * transform.localScale.y / 256;
		player_width = sprite_renderer.sprite.rect.width * transform.localScale.x / 256;
		
		origin = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
		transform.position = origin + new Vector3(-origin.x, y_pos, -origin.z);
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
			Move();
    }
	
	private void Move(){
		Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		if (mouse_pos.y < origin.y + player_height){
			if (AbsFloat(mouse_pos.x) < AbsFloat(origin.x + player_width*0.5f))
				transform.position += new Vector3(mouse_pos.x - transform.position.x, 0, 0);
			else{
				if (mouse_pos.x < 0)
					transform.position += new Vector3(origin.x + player_width*0.5f - transform.position.x, 0, 0);
				else
					transform.position += new Vector3(-origin.x - player_width*0.5f - transform.position.x, 0, 0);
			}
		}
	}
	
	private float AbsFloat(float num){
		return num > 0 ? num : -num;
	}
}
