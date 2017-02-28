using UnityEditor;
using System.IO;
using UnityEngine;
using System.Text;

public class CreateAssetBundles
{
	public const string buildPath = "AssetBundles";

	[MenuItem ("Assets/Build Asset Bundles...")]
	static void BuildMacAssetBundles ()
	{
		#if UNITY_EDITOR_WIN
		BuildAssetBundleAndRename (BuildTarget.WSAPlayer, "WSA");
		#endif

		BuildAssetBundleAndRename (BuildTarget.StandaloneWindows, "x86");
		BuildAssetBundleAndRename (BuildTarget.StandaloneWindows64, "x64");

		BuildAssetBundleAndRename (BuildTarget.Android, "Android");

		BuildAssetBundleAndRename (BuildTarget.iOS, "iOS");
		BuildAssetBundleAndRename (BuildTarget.StandaloneOSXUniversal, "OSX");
	}

	static private void BuildAssetBundleAndRename (BuildTarget buildTarget, string suffix)
	{
		if (!Directory.Exists (buildPath)) {
			Debug.Log ("Created build dir: " + buildPath);
			Directory.CreateDirectory (buildPath);
		}

		BuildPipeline.BuildAssetBundles (buildPath, BuildAssetBundleOptions.StrictMode, buildTarget);

		string[] files = Directory.GetFiles (buildPath);
		foreach (string file in files) {
			if (!file.Contains (".") && !file.EndsWith ("AssetBundles")) {
				string renamedBundle = file + "-" + suffix + ".unity3d";
				// clean-up old files
				if (File.Exists (renamedBundle)) {
					File.Delete (renamedBundle);
				}
				if (File.Exists (renamedBundle + ".meta")) {
					File.Delete (renamedBundle + ".meta");
				}
				File.Move (file, renamedBundle);
				if (File.Exists (file + ".meta")) {
					File.Move (file + ".meta", renamedBundle + ".meta");
				}
				Debug.Log ("Built bundle: " + renamedBundle);
			}
		}
	}

	[MenuItem ("Assets/Get Asset Bundle Names")]
	static void GetAssetBundleNames ()
	{
		string[] names = AssetDatabase.GetAllAssetBundleNames ();
		foreach (var name in names) {
			StringBuilder sb = new StringBuilder ("AssetBundle: " + name + "\n");
			foreach (string file in Directory.GetFiles (buildPath)) {
				string filename = Path.GetFileName (file);
				if (filename.StartsWith (name) && !filename.EndsWith (".meta") && !filename.EndsWith (".manifest")) {
					sb.Append ("\t" + filename);
				}
			}
			Debug.Log (sb.ToString ());
		}
	}
}