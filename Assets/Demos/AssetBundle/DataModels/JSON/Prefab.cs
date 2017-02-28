using System;
using UnityEngine;

[Serializable]
public class Prefab
{
	public string name;
	public string color;
	public string location;
	public string scale;

	[NonSerialized]
	public Vector3 Position;

	[NonSerialized]
	public Vector3 Scale;

	[NonSerialized]
	public Color Colour;

	public void Init ()
	{
		string[] loc = location.Split (',');
		if (loc.Length != 3) {
			Position = Vector3.zero;
		} else {
			Position = new Vector3 (float.Parse (loc [0]), float.Parse (loc [1]), float.Parse (loc [2]));
		}

		string[] s = scale.Split (',');
		if (s.Length != 3) {
			Scale = new Vector3 (1, 1, 1);
		} else {
			Scale = new Vector3 (float.Parse (s [0]), float.Parse (s [1]), float.Parse (s [2]));
		}

		Colour = Color.clear;
		if (!string.IsNullOrEmpty (color)) {
			ColorUtility.TryParseHtmlString (color, out Colour);
		}
	}
}
