using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropNote : MonoBehaviour
{
	public Sprite[] note_img;
	private SpriteRenderer sprite_renderer;
	private CircleCollider2D circle_collider;
	private Transform osu_ring;
	private float speed;
	private float drop_height;
	private float bottom;
	private float time;
	public bool dropping = false;
	private bool is_osu = false;
	private bool is_type_set = false;
	
	private RhythmGame rhythm_game;
	
    // Start is called before the first frame update
    void Start()
    {
		rhythm_game = FindObjectOfType<RhythmGame>();
		
        sprite_renderer = GetComponent<SpriteRenderer>();
		circle_collider = GetComponent<CircleCollider2D>();
		osu_ring = transform.Find("OsuRing");
		speed = rhythm_game.speed;
		drop_height = -2 * Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 1.2f;
		bottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 0.5f;
		time = 0f;
    }

    // Update is called once per frame
    void Update()
    {
		if (!is_type_set)
			SetType();
		
		if (dropping)
			Dropping();
		if (is_osu)
			Osu();
		
		time += Time.deltaTime;
    }
	
	private void SetType(){
		switch (transform.name){
			case "big":
				sprite_renderer.sprite = note_img[0];
				dropping = true;
				circle_collider.radius = 0.4f;
				is_type_set = true;
				break;
				
			case "small":
				sprite_renderer.sprite = note_img[1];
				dropping = true;
				circle_collider.radius = 0.15f;
				is_type_set = true;
				break;
				
			case "osu":
				sprite_renderer.sprite = note_img[2];
				sprite_renderer.sortingLayerName = "OsuNote";
				dropping = false;
				is_osu = true;
				circle_collider.radius = 0.4f;
				is_type_set = true;
				break;
				
			default:
				sprite_renderer.sprite = note_img[0];
				break;
		}
	}
	
	private void Dropping(){
		float y_pos = drop_height - (drop_height - bottom) * time / speed;
		if (y_pos > bottom)
			transform.position = new Vector3(transform.position.x, y_pos, 0);
		else{
			Destroy(gameObject);
			rhythm_game.CatchNote(false, 0);
		}
	}
	
	private void Osu(){
		if (time > speed * 0.5f){
			osu_ring.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
			transform.position = new Vector3(transform.position.x, 0, 0);
			float ratio = (speed - time) / speed;
			osu_ring.GetComponent<SpriteRenderer>().size = new Vector2(1f + ratio, 1f + ratio);
		}
		if (time > speed * 1.2f){
			rhythm_game.CatchNote(false, 0);
			Destroy(gameObject);
		}
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if (dropping){
			if (other.gameObject.CompareTag("Catcher")){
				rhythm_game.CatchNote(true, 2);
				Destroy(gameObject);
			}
		}
	}
	
	void OnMouseDown(){
		if (!dropping){
			float ratio = AbsFloat((speed - time) / speed * 2f);
			if (ratio < 0.2f)
				rhythm_game.CatchNote(true, 2);
			else if (ratio < 0.4f)
				rhythm_game.CatchNote(true, 1);
			else if (ratio < 0.7f)
				rhythm_game.CatchNote(true, 0);
			else
				rhythm_game.CatchNote(false, 0);
			Destroy(gameObject);
		}
	}
	
	private float AbsFloat(float num){
		return num > 0 ? num : -num;
	}
}
