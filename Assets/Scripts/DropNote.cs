using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropNote : MonoBehaviour, IPooledObject
{
	private SpriteRenderer sprite_renderer;
	private CircleCollider2D circle_collider;
	private Transform osu_ring;
	public Transform catcher;
	private float speed;
	private float drop_height;
	private float bottom;
	private float time;
	private bool dropping = false;
	private bool is_osu = false;
	public bool freeze = false;
	public bool clear = false;
	private float regular_size = 1f;
	
	private RhythmGame rhythm_game;
	private ImageManager IM;
	private ObjectPooler object_pooler;
	
    // Start is called before the first frame update
    void Start()
    {
		rhythm_game = FindObjectOfType<RhythmGame>();
		IM = FindObjectOfType<ImageManager>();
		object_pooler = ObjectPooler.instance;
		
        sprite_renderer = GetComponent<SpriteRenderer>();
		circle_collider = GetComponent<CircleCollider2D>();
		GetComponent<Rigidbody2D>().gravityScale = 0;
		osu_ring = transform.Find("OsuRing");
		drop_height = -2 * Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 1.2f;
		bottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 0.5f;
		time = 0f;
		speed = rhythm_game.speed;
		SetType();
    }
	
	public void OnObjectSpawn(){
		if (GetComponent<Rigidbody2D>() != null)
			GetComponent<Rigidbody2D>().gravityScale = 0;
		time = 0f;
		osu_ring.gameObject.SetActive(true);
		freeze = false;
		speed = rhythm_game.speed;
		sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 1f);
		circle_collider.enabled = true;
		
		switch (transform.name){
			case "main":
				sprite_renderer.size = new Vector2(regular_size, regular_size);
				GetComponent<Rigidbody2D>().AddTorque(Random.Range(100.0f, 150.0f));
				break;
			
			case "ornament":
				sprite_renderer.size = new Vector2(regular_size * 0.35f, regular_size * 0.5f);
				GetComponent<Rigidbody2D>().AddTorque(Random.Range(150.0f, 200.0f));
				break;
			
			case "osu":
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[3];
				sprite_renderer.size = new Vector2(regular_size, regular_size);
				break;
				
			default:
				break;
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (!freeze){
			if (dropping)
				Dropping();
			if (is_osu)
				Osu();
			time += Time.deltaTime;
		}
		else
			if (transform.position.y < bottom - 2.5f)
				object_pooler.ToPool(gameObject, transform.name);
    }
	
	private void SetType(){
		switch (transform.name){
			case "main":
				osu_ring.GetComponent<SpriteRenderer>().sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[4];
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[0];
				sprite_renderer.size = new Vector2(regular_size, regular_size);
				dropping = true;
				circle_collider.radius = regular_size * 0.32f;
				GetComponent<Rigidbody2D>().AddTorque(Random.Range(50.0f, 150.0f));
				break;
				
			case "ornament":
				osu_ring.GetComponent<SpriteRenderer>().sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[4];
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[1];
				sprite_renderer.size = new Vector2(regular_size * 0.35f, regular_size * 0.5f);
				dropping = true;
				circle_collider.radius = regular_size * 0.16f;
				GetComponent<Rigidbody2D>().AddTorque(Random.Range(150.0f, 200.0f));
				break;
				
			case "auxiliary":
				osu_ring.GetComponent<SpriteRenderer>().sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[4];
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[2];
				sprite_renderer.size = new Vector2(regular_size / 4f, regular_size / 4f);
				dropping = true;
				circle_collider.radius = regular_size * 0.08f;
				break;
				
			case "osu":
				osu_ring.GetComponent<SpriteRenderer>().sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[4];
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[3];
				sprite_renderer.size = new Vector2(regular_size, regular_size);
				sprite_renderer.sortingLayerName = "OsuNote";
				dropping = false;
				is_osu = true;
				circle_collider.radius = regular_size * 0.5f;
				if (GetComponent<Rigidbody2D>() != null)
					Destroy(GetComponent<Rigidbody2D>());
				break;
				
			default:
				break;
		}
	}
	
	private void Dropping(){
		float y_pos = drop_height - (drop_height - catcher.position.y) * time / speed;
		if (y_pos > bottom)
			transform.position = new Vector3(transform.position.x, y_pos, 0);
		else{
			object_pooler.ToPool(gameObject, transform.name);
			if (transform.name == "main")
				rhythm_game.CatchNote(false, false, 3);
			else if (transform.name == "ornament")
				rhythm_game.CatchNote(false, false, 2);
			else if (transform.name == "auxiliary")
				rhythm_game.CatchNote(false, false, 1);
			
			if (clear)
				rhythm_game.CleanCatcher();
		}
	}
	
	private void Osu(){
		if (time > speed * 0.5f){
			osu_ring.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
			transform.position = new Vector3(transform.position.x, 0, 0);
			float ratio = (speed - time) / speed;
			osu_ring.GetComponent<SpriteRenderer>().size = new Vector2(regular_size * (1f + ratio), regular_size * (1f + ratio));
		}
		if (time > speed * 1.2f){
			rhythm_game.CatchNote(true, false, 0);
			freeze = true;
			circle_collider.enabled = false;
			StartCoroutine(FadeOut(false, () => {object_pooler.ToPool(gameObject, transform.name);}));
		}
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if (dropping){
			if (other.gameObject.CompareTag("Catcher")){
				if (transform.name == "auxiliary"){
					rhythm_game.CatchNote(false, true, 1);
					object_pooler.ToPool(gameObject, transform.name);
				}
				else if (transform.name == "ornament"){
					rhythm_game.CatchNote(false, true, 2);
					PutOnCatcher();
				}
				else if (transform.name == "main"){
					rhythm_game.CatchNote(false, true, 3);
					PutOnCatcher();
					if (clear){
						rhythm_game.CleanCatcher();
					}
				}
			}
		}
	}
	
	public void TouchEvent(){
		if (!dropping){
			float ratio = AbsFloat((speed - time) / speed * 2f);
			freeze = true;
			if (ratio < 0.2f){
				rhythm_game.CatchNote(true, true, 3);
				StartCoroutine(FadeOut(true, () => {object_pooler.ToPool(gameObject, transform.name);}));
				GameObject score = object_pooler.SpawnFromPool("excellent", transform.position, Quaternion.identity);
				score.GetComponent<Score>().speed = speed;
			}
			else if (ratio < 0.5f){
				rhythm_game.CatchNote(true, true, 2);
				StartCoroutine(FadeOut(true, () => {object_pooler.ToPool(gameObject, transform.name);}));
				GameObject score = object_pooler.SpawnFromPool("good", transform.position, Quaternion.identity);
				score.GetComponent<Score>().speed = speed;
			}
			else{
				rhythm_game.CatchNote(true, true, 1);
				StartCoroutine(FadeOut(true, () => {object_pooler.ToPool(gameObject, transform.name);}));
				GameObject score = object_pooler.SpawnFromPool("bad", transform.position, Quaternion.identity);
				score.GetComponent<Score>().speed = speed;
			}
		}
	}
	
	private float AbsFloat(float num){
		return num > 0 ? num : -num;
	}
	
	private void PutOnCatcher(){
		freeze = true;
		transform.position = new Vector3(transform.position.x, catcher.position.y + 0.15f, 0);
		transform.parent = catcher;
		circle_collider.enabled = false;
		sprite_renderer.size = new Vector2(sprite_renderer.size.x * 0.5f, sprite_renderer.size.y * 0.5f);
	}
	
	public void CleanOut(){
		// StartRoutine(FadeOut())
		// increase jumping height
		freeze = true;
		GetComponent<Rigidbody2D>().gravityScale = 1;
		float horizontal_force = transform.position.x - catcher.position.x;
		GetComponent<Rigidbody2D>().AddForce(new Vector2 (horizontal_force, 2.5f), ForceMode2D.Impulse);
		StartCoroutine(FadeOut(true));
	}
	
	private IEnumerator FadeOut(bool success, System.Action callback = null){
		//print(transform.Find("OsuRing") == null);
		osu_ring.gameObject.SetActive(false);
		if (transform.name == "osu"){ // time: speed / 4
			float total_time = speed * 0.25f;
			if (success){
				for (float ratio = 0.25f; ratio > 0f; ratio -= Time.deltaTime * 0.25f / total_time){
					sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0.5f + ratio * 2f);
					sprite_renderer.size = new Vector2(regular_size * (1.25f - ratio), regular_size * (1.25f - ratio));
					yield return null;
				}
				sprite_renderer.size = new Vector2(regular_size * 1.25f, regular_size * 1.25f);
				sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0);
			}
			else{
				sprite_renderer.sprite = IM.note_image[PlayerPrefs.GetInt("skin")].image[5];
				sprite_renderer.size = new Vector2(regular_size * 0.5f, regular_size * 0.5f);
				
				if (GetComponent<Rigidbody2D>() == null){
					Rigidbody2D rigid_body = gameObject.AddComponent<Rigidbody2D>() as Rigidbody2D;
					rigid_body.gravityScale = 0.5f;
				}
				for (float ratio = 0.5f; ratio > 0f; ratio -= Time.deltaTime * 0.5f / total_time){
					sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0.5f + ratio);
					yield return null;
				}
				sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0);
				Destroy(GetComponent<Rigidbody2D>());
			}
		}
		else{ // time: speed / 2
			float total_time = speed * 0.5f;
			for (float ratio = 1f; ratio > 0f; ratio -= Time.deltaTime / total_time){
				sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, ratio);
				yield return null;
			}
			sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0);
		}
		
		callback?.Invoke();
	}
}
