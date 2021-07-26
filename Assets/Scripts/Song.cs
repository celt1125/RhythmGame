using System;

[Serializable]
public class Song
{
	public string name;
	public float speed; // The total time to drop from top to bottom.
	//public float music_length;
	public Note[] notes;
}
