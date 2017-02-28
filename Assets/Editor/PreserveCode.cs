using UnityEngine;
using UnityEditor;
using System.IO;

namespace AssetBundles
{
	/// Preserve code in Asset Bundles. See https://docs.unity3d.com/Manual/BuildingAssetBundles.html
	public class BuildScript
	{

		public static void BuildPlayer ()
		{

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions ();

			string manifestPath = Path.Combine (CreateAssetBundles.buildPath, "AssetBundles.manifest");

			// Manifest path
			buildPlayerOptions.assetBundleManifestPath = manifestPath;

			// Build the Player ensuring engine code is included for AssetBundles in the manifest.
			BuildPipeline.BuildPlayer (buildPlayerOptions);

			Debug.Log ("BuildPlayer using Asset Bundle manifest: " + manifestPath);
		}

	}

}