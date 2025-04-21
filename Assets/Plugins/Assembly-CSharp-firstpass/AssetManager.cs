using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
	protected static AssetManager instance;

	protected Dictionary<long, AssetDescriptor> assetBundleMap;

	protected Dictionary<long, AssetBundle> loadedAssetBundleMap;

	[method: MethodImpl(32)]
	public static event OnBundleLoadedDelegate OnBundleLoaded;

	[method: MethodImpl(32)]
	public static event OnResourceLoadedDelegate OnResourceLoaded;

	private void PopulateDescriptorMap()
	{
		TextAsset textAsset = Resources.Load("Data/BundleList") as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("Error loading BundleList.txt!");
			return;
		}
		string[] array = textAsset.text.Split('\n');
		assetBundleMap = new Dictionary<long, AssetDescriptor>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			long result;
			long.TryParse(array3[0], out result);
			int result2;
			int.TryParse(array3[2], out result2);
			assetBundleMap.Add(result, new AssetDescriptor(array3[1], result2));
		}
	}

	private void PopulateDescriptorMapFromCML()
	{
		assetBundleMap = new Dictionary<long, AssetDescriptor>();
		cmlReader cmlReader2 = new cmlReader("Data/BundleList");
		if (cmlReader2 == null)
		{
			Debug.LogError("Error loading BundleList.txt!");
			return;
		}
		List<cmlData> list = cmlReader2.Children();
		foreach (cmlData item in list)
		{
			assetBundleMap.Add(long.Parse(item["id"]), new AssetDescriptor(item["url"], int.Parse(item["version"])));
		}
	}

	private void Start()
	{
		if (instance != null)
		{
			Debug.LogError("Multiple AssetManager instances");
			return;
		}
		instance = this;
		PopulateDescriptorMapFromCML();
		loadedAssetBundleMap = new Dictionary<long, AssetBundle>();
	}

	protected IEnumerator PrefetchBundle(long bundleId)
	{
		while (!Caching.ready)
		{
			yield return null;
		}
		AssetDescriptor descriptor = assetBundleMap[bundleId];
		WWW loader2 = WWW.LoadFromCacheOrDownload("jar:file://" + Application.dataPath + "!/assets/Android/" + descriptor.url, descriptor.version);
		yield return loader2;
		if (loader2.error != null)
		{
			Debug.LogError("Unable to prefetch bundle '" + descriptor.url + "' : " + loader2.error);
			yield break;
		}
		loadedAssetBundleMap[bundleId] = loader2.assetBundle;
		loader2.Dispose();
		loader2 = null;
		AssetManager.OnBundleLoaded(bundleId);
	}

	public static void PreloadBundle(long bundleId)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
		}
		else if (!instance.assetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("Unknown AssetBundleID " + bundleId);
		}
		else if (instance.loadedAssetBundleMap.ContainsKey(bundleId))
		{
			AssetManager.OnBundleLoaded(bundleId);
		}
		else
		{
			instance.StartCoroutine(instance.PrefetchBundle(bundleId));
		}
	}

	public static void UnloadBundle(long bundleId, bool unloadAllLoadedObjects)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
		}
		else if (!instance.assetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("Unknown AssetBundleID " + bundleId);
		}
		else if (instance.loadedAssetBundleMap.ContainsKey(bundleId))
		{
			instance.loadedAssetBundleMap[bundleId].Unload(unloadAllLoadedObjects);
			instance.loadedAssetBundleMap.Remove(bundleId);
		}
	}

	public static bool IsBundleLoaded(long bundleId)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
			return false;
		}
		if (!instance.assetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("Unknown AssetBundleID " + bundleId);
			return false;
		}
		return instance.loadedAssetBundleMap.ContainsKey(bundleId);
	}

	public static void UnloadAllBundles(bool unloadAllLoadedObjects)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
			return;
		}
		foreach (long key in instance.loadedAssetBundleMap.Keys)
		{
			instance.loadedAssetBundleMap[key].Unload(unloadAllLoadedObjects);
		}
		instance.loadedAssetBundleMap.Clear();
	}

	public static UnityEngine.Object LoadResource(long bundleId, string resourceName)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
			return null;
		}
		if (!instance.assetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("Unknown AssetBundleID " + bundleId);
			return null;
		}
		if (!instance.loadedAssetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("AssetBundle is not loaded! BundleID = " + bundleId);
			return null;
		}
		if (!instance.loadedAssetBundleMap[bundleId].Contains(resourceName))
		{
			Debug.LogError("Resource " + resourceName + " is not found in BundleID " + bundleId + "!");
			return null;
		}
		return instance.loadedAssetBundleMap[bundleId].LoadAsset(resourceName);
	}

	protected IEnumerator LoadResourceInBackground(long bundleId, string resourceName, Type type)
	{
		if (!assetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("Unknown AssetBundleID " + bundleId);
		}
		else if (!loadedAssetBundleMap.ContainsKey(bundleId))
		{
			Debug.LogError("AssetBundle is not loaded! BundleID = " + bundleId);
		}
		else if (!loadedAssetBundleMap[bundleId].Contains(resourceName))
		{
			Debug.LogError("Resource " + resourceName + " is not found in BundleID " + bundleId + "!");
		}
		else
		{
			AssetBundleRequest assetBundleRequest = loadedAssetBundleMap[bundleId].LoadAssetAsync(resourceName, type);
			yield return assetBundleRequest;
			AssetManager.OnResourceLoaded(assetBundleRequest.asset, bundleId, resourceName, type);
		}
	}

	public static void LoadResourceAsync(long bundleId, string resourceName, Type type)
	{
		if (!instance)
		{
			Debug.LogError("AssetManager is not initialized!");
		}
		else
		{
			instance.StartCoroutine(instance.LoadResourceInBackground(bundleId, resourceName, type));
		}
	}

	public static void DoCleanup()
	{
		Resources.UnloadUnusedAssets();
	}
}
