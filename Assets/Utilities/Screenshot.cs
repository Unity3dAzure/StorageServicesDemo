using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Screenshot
{
	public static string Capture (string filename)
	{
		string filePath = Path.Combine (Application.persistentDataPath, filename);
        // NB: Be aware that "On mobile platforms the filename is appended to the persistent data path." as mentioned in the documentation https://docs.unity3d.com/ScriptReference/Application.CaptureScreenshot.html   
        // But unfortunately there is no filepath returned from CaptureScreenshot and the 'mobile' Runtime Platforms are not listed hence the reason for this switch statement to return the absolute filepath. 
        switch (Application.platform) {
		case RuntimePlatform.Android:
		case RuntimePlatform.IPhonePlayer:
		//case RuntimePlatform.WSAPlayerARM:
		//case RuntimePlatform.WSAPlayerX64:
		//case RuntimePlatform.WSAPlayerX86:
			ScreenCapture.CaptureScreenshot (filename);
			break;
		default:
			ScreenCapture.CaptureScreenshot (filePath);
			break;
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
        return filePath.Replace("/", "\\");
#else
        return filePath;
#endif
	}
}