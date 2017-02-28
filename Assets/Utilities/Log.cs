using System;
using UnityEngine.UI;
using UnityEngine;

public class Log
{
	public enum Level
	{
		Message,
		Warning,
		Error
	}

	public static void Text (Text label, string message, string debugMessage = "", Level level = Level.Message)
	{
		switch (level) {
		case Level.Message:
			label.color = Color.white;
			if (!string.IsNullOrEmpty (debugMessage))
				Debug.Log (debugMessage);
			break;
		case Level.Warning:
			label.color = Color.yellow;
			if (!string.IsNullOrEmpty (debugMessage))
				Debug.LogWarning (debugMessage);
			break;
		case Level.Error:
			label.color = Color.red;
			if (!string.IsNullOrEmpty (debugMessage))
				Debug.LogError (debugMessage);
			break;
		}
		label.text = message;
	}
}

