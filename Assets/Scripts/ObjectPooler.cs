using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
	[System.Serializable]
	public class Pool{
		public string tag;
		public GameObject prefab;
	}
	
	public static ObjectPooler instance;
	private void Awake(){
		instance = this;
	}
	
	public Dictionary<string, Queue<GameObject>> pool_dict;
	public List<Pool> pools;
	
    // Start is called before the first frame update
    void Start()
    {
        pool_dict = new Dictionary<string, Queue<GameObject>>();
		
		foreach (Pool pool in pools){
			Queue<GameObject> object_queue = new Queue<GameObject>();
			pool_dict.Add(pool.tag, object_queue);
		}
    }
	
	public GameObject SpawnFromPool (string tag, Vector3 position, Quaternion rotation, Transform parent = null){
		if (!pool_dict.ContainsKey(tag)){
			print("Spawn from pool with wrong key");
			return null;
		}
		
		if (pool_dict[tag].Count == 0){
			GameObject new_note = Instantiate(pools.Find(pool => pool.tag == tag).prefab, position, rotation, parent);
			new_note.name = tag;
			return new_note;
		}
		else{
			GameObject object_to_spawn = pool_dict[tag].Dequeue();
			object_to_spawn.SetActive(true);
			object_to_spawn.transform.position = position;
			object_to_spawn.transform.rotation = rotation;
			object_to_spawn.transform.parent = parent;
			
			IPooledObject pooled_obj = object_to_spawn.GetComponent<IPooledObject>();
			if (pooled_obj != null)
				pooled_obj.OnObjectSpawn();
			return object_to_spawn;
		}
	}
	
	public void ToPool (GameObject object_to_pool, string tag){
		pool_dict[tag].Enqueue(object_to_pool);
		object_to_pool.SetActive(false);
	}
}
