using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot ("scene")]
public class SceneDoc
{
	[XmlArray ("levels")]
	[XmlArrayItem ("level")] // NB: watch out for lowercase fields in xml doc
	public LevelDoc[] Levels;
}

[Serializable]
public class LevelDoc
{
	[XmlAttribute ("number")]
	public string Number;

	[XmlArray ("prefabs")]
	[XmlArrayItem ("prefab")]
	public PrefabDoc[] Prefabs;
}

[Serializable]
public class PrefabDoc
{
	[XmlAttribute ("name")]
	public string name;

	[XmlElement ("color")]
	public string color;

	[XmlElement ("location")]
	public string location;

	[XmlElement ("scale")]
	public string scale;

	private Vector3 _position;

	public Vector3 GetPosition ()
	{
		return _position;
	}

	private Vector3 _scale;

	public Vector3 GetScale ()
	{
		return _scale;
	}

	private Color _color;

	public Color GetColour ()
	{
		return _color;
	}

	public void Init ()
	{
		string[] loc = location.Split (',');
		if (loc.Length != 3) {
			_position = Vector3.zero;
		} else {
			_position = new Vector3 (float.Parse (loc [0]), float.Parse (loc [1]), float.Parse (loc [2]));
		}

		string[] s = scale.Split (',');
		if (s.Length != 3) {
			_scale = new Vector3 (1, 1, 1);
		} else {
			_scale = new Vector3 (float.Parse (s [0]), float.Parse (s [1]), float.Parse (s [2]));
		}

		_color = Color.clear;
		if (!string.IsNullOrEmpty (color)) {
			ColorUtility.TryParseHtmlString (color, out _color);
		}
	}
}
