using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class Player : MonoBehaviour
{
	private Vector3 origin;
	private SpriteRenderer sprite_renderer;
	private float player_height;
	private float player_width;
	private ImageManager IM;
	
	private float boundary_endurance = 0.2f;
	private bool is_success;
	
	void OnEnable(){
		EnhancedTouchSupport.Enable();
		//Debug.Log("enabled");
	}
	
	void OnDisable(){
		EnhancedTouchSupport.Disable();
		//Debug.Log("disabled");
	}
	
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
		if (Time.timeScale > 0.5){
			is_success = false;
			//Debug.Log(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count);
			foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches){
				if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began){
					is_success = Move(touch.screenPosition);
					RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.screenPosition), Vector2.zero);
					if (hit.collider != null)
						if (hit.collider.name == "osu")
							hit.collider.GetComponent<DropNote>().TouchEvent();
				}
				else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
					is_success = Move(touch.screenPosition);
			}
		}
    }
	
	public void InitializePosition(){
		transform.position = origin + new Vector3(-origin.x, player_height * 0.5f, -origin.z);
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
	
	private bool Move(Vector2 raw_touch_pos){
		if (is_success)
			return true;
		Vector3 touch_pos = Camera.main.ScreenToWorldPoint(new Vector3(raw_touch_pos.x, raw_touch_pos.y, 0));
		if (touch_pos.y < origin.y + player_height * 1.8f){
			if (AbsFloat(touch_pos.x) < AbsFloat(origin.x + player_width * boundary_endurance))
				transform.position += new Vector3(touch_pos.x - transform.position.x, 0, 0);
			else{
				if (touch_pos.x < 0)
					transform.position += new Vector3(origin.x + player_width * boundary_endurance - transform.position.x, 0, 0);
				else
					transform.position += new Vector3(-origin.x - player_width * boundary_endurance - transform.position.x, 0, 0);
			}
			return true;
		}
		else return false;
	}
	
	private float AbsFloat(float num){
		return num > 0 ? num : -num;
	}
}
