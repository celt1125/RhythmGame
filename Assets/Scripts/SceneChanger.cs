using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
	public Animator animator;
	
	private int next_scene_idx;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void FadeToScene (int idx){
		next_scene_idx = idx;
		animator.SetTrigger("fade_out");
	}
	
	public void OnFadeComplete(){
		SceneManager.LoadScene(next_scene_idx);
	}
}
