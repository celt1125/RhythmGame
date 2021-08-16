using System;

[Serializable]
public class Song
{
	public string name;
	public float speed; // The total time to drop from top to bottom.
	public int difficulty;
	public int BPM;
	//public float music_length;
	public Note[] notes;
}
