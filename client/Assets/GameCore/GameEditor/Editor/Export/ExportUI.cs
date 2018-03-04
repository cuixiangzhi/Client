using System;
using UnityEditor;
using UnityEngine;

namespace AssemblyCSharpEditor
{
	public class ExportUI
	{
		[MenuItem("工具/导出资源/导出UI")]
		public static void CExportUI ()
		{
			BuildAssetBundleOptions op = BuildAssetBundleOptions.UncompressedAssetBundle 
				 					   | BuildAssetBundleOptions.DeterministicAssetBundle;

			AssetBundleBuild[] obs = new AssetBundleBuild[2];

			obs[0].assetBundleName = "ai";
			obs[0].assetNames = new string[] { "Assets/GameAssets/ai.mat" };

			obs[1].assetBundleName = "timg";
			obs [1].assetNames = new string[]{ "Assets/GameAssets/timg.jpg" };


			BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, obs, op, BuildTarget.StandaloneOSXIntel);
		}
	}
}

