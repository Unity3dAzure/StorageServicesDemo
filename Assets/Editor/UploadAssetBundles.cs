using UnityEditor;
using UnityEngine;
using System.IO;
using Unity3dAzure.StorageServices;
using System.Collections;
using System.Collections.Generic;

public class UploadAssetBundles : EditorWindow
{
	[MenuItem ("Window/Upload Asset Bundles...")]
	static void BuildAllAssetBundles ()
	{
		EditorWindow window = EditorWindow.GetWindow<UploadAssetBundles> ();
		window.position = new Rect (20, 40, 360, 100);
		window.Show ();
	}

	private string storageAccount = "";
	private string accessKey = "";
	private string container = "";

	private StorageServiceClient client;
	private BlobService service;

	private IEnumerator enumerator;

	private List<string> files;

	void OnGUI ()
	{
		GUILayout.Label ("Enter Azure Storage Service details to upload asset bundles");

		// Azure Storage Service details
		storageAccount = EditorGUILayout.TextField ("Storage", storageAccount);
		accessKey = EditorGUILayout.TextField ("Access Key", accessKey);
		container = EditorGUILayout.TextField ("Container", container);


		bool isSelected = GUILayout.Button ("Upload Asset Bundle");

		if (isSelected) {
			string buildPath = Path.GetFullPath (CreateAssetBundles.buildPath);

			Debug.Log ("Build path:" + buildPath);

			if (!FindAllAssetBundles (buildPath)) {
				EditorUtility.DisplayDialog ("Build Asset Bundle", "No asset bundles found in dir: " + buildPath, "Doh");
				return;
			}

			TryUploadAssetBundles ();
		}
	}

	private bool FindAllAssetBundles (string buildPath)
	{
		bool hasAssetBundle = false;
		files = new List<string> ();

		string[] names = AssetDatabase.GetAllAssetBundleNames ();
		//Debug.Log ("names:" + names.Length);
		foreach (var name in names) {
			foreach (string file in Directory.GetFiles (buildPath)) {
				string filename = Path.GetFileName (file);
				if (filename.StartsWith (name) && !filename.EndsWith (".meta") && !filename.EndsWith (".manifest")) {
					files.Add (file);
					hasAssetBundle = true;
				}
			}
		}

		return hasAssetBundle;
	}

	private void TryUploadAssetBundles ()
	{
		if (files.Count > 0) {
			string file = files [0];
			files.RemoveAt (0);
			UploadAssetBundleFile (file);
		}
	}

	private void UploadAssetBundleFile (string path)
	{
		if (!File.Exists (path)) {
			Debug.LogError ("No asset bundle file found:" + path);
			return;
		}
		string filename = Path.GetFileName (path);
		byte[] bytes = File.ReadAllBytes (path);

		if (client == null) {
			client = new StorageServiceClient (storageAccount, accessKey);
			service = client.GetBlobService ();
			Debug.Log ("Created Storage Service");
		}

		Debug.Log ("Reading file bytes: " + path + "\nUpload asset bundle file: " + filename + " size:" + bytes.Length);
		StartCoroutine (service.PutAssetBundle (CompletedUploadingAssetBundle, bytes, container, filename));
	}

	private void CompletedUploadingAssetBundle (RestResponse response)
	{
		if (response.IsError) {
			EditorUtility.DisplayDialog ("Upload error", response.ErrorMessage, "Doh");
			return;
		}
		Debug.Log ("Uploaded AssetBundle");

		TryUploadAssetBundles ();
	}

	#region Handle StartCoroutine method for Unity Editor

	private void StartCoroutine (IEnumerator e)
	{
		enumerator = e;
		EditorApplication.update += EditorUpdate;
	}

	private void StopCoroutine ()
	{
		EditorApplication.update -= EditorUpdate;
	}

	private void EditorUpdate ()
	{
		if (enumerator == null) {
			return;
		}
		// iterate the coroutine until complete, then stop
		if (!enumerator.MoveNext ()) {
			StopCoroutine ();
		}
	}

	#endregion
}