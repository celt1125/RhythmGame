using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour, IPooledObject
{
	private ImageManager IM;
	private SpriteRenderer sprite_renderer;
	
	private ObjectPooler object_pooler;
	
	public float speed;
	private float width;
	private float height;
	private float size_factor = 384f;
	private bool active = false;
	
    // Start is called before the first frame update
    void Start()
    {
		IM = FindObjectOfType<ImageManager>();
		sprite_renderer = GetComponent<SpriteRenderer>();
		object_pooler = ObjectPooler.instance;
		
        SetType();
		if (active)
			StartCoroutine(ScoreAnimation());
    }
	
	public void OnObjectSpawn(){
		StartCoroutine(ScoreAnimation());
	}
	
	private IEnumerator ScoreAnimation(){
		sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 1f);
		for (float time = 0.4f; time < 1f; time += Time.deltaTime * 4){
			sprite_renderer.size = new Vector2(width * time, height * time);
			yield return null;
		}
		sprite_renderer.size = new Vector2(width, height);
		
		for (float time = 0f; time < speed * 0.4f; time += Time.deltaTime){
			yield return null;
		}
		
		for (float time = 1f; time > 0f; time -= Time.deltaTime / speed * 4){
			sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, time);
			yield return null;
		}
		sprite_renderer.color = new Color(sprite_renderer.color.r, sprite_renderer.color.g, sprite_renderer.color.b, 0f);
		object_pooler.ToPool(gameObject, transform.name);
	}
	
	private void SetType(){
		Rect score_rect;
		switch(transform.name){
			case "excellent":
				sprite_renderer.sprite = IM.score_image.image[0];
				score_rect = sprite_renderer.sprite.rect;
				width = score_rect.width / size_factor * 0.8f;
				height = score_rect.height / size_factor * 0.8f;
				
				active = true;
				break;
			
			case "good":
				sprite_renderer.sprite = IM.score_image.image[1];
				score_rect = sprite_renderer.sprite.rect;
				width = score_rect.width / size_factor;
				height = score_rect.height / size_factor;
				
				active = true;
				break;
			
			case "bad":
				sprite_renderer.sprite = IM.score_image.image[2];
				score_rect = sprite_renderer.sprite.rect;
				width = score_rect.width / size_factor;
				height = score_rect.height / size_factor;
				
				active = true;
				break;
			
			default:
				break;
		}
	}
}
