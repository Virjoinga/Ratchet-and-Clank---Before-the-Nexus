using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
	public class PoolMeta
	{
		public string objectType;

		public string objectPath;

		public Object originalObject;

		public Queue freeList;

		public LinkedList<GameObject> poolList;

		public bool canUse;

		public string delayLoadingTag = string.Empty;

		public int numberOfInstances;
	}

	protected const string poolFileName = "Data/GameObjectPool";

	public static GameObjectPool instance;

	public Vector3 outOfScreen = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

	public int maxActivationsPerFrame = 3;

	public int maxDeactivationsPerFrame = 3;

	public Hashtable pools;

	protected List<string> tagsToLoad = new List<string>();

	protected List<string> tagsToUnload = new List<string>();

	protected List<GameObject> individualObjectToUnload = new List<GameObject>();

	protected List<string> coroutineTagsToLoad = new List<string>();

	protected List<string> coroutineTagsToUnload = new List<string>();

	protected List<GameObject> coroutineIndividualObjectToUnload = new List<GameObject>();

	protected LinkedList<GameObject> deactivationList = new LinkedList<GameObject>();

	protected LinkedList<GameObject> activationList = new LinkedList<GameObject>();

	protected AsyncOperation unloadingAssets;

	protected bool bStreamingSupported;

	protected bool bNeedsLoadUnload;

	protected bool bStreaming;

	protected bool bDelayedInstantiationSupported;

	protected bool bNeedsInstantiationUpdate;

	protected bool bUpdatingInstantiation;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("GameObjectPool: Multiple instances spawned!");
			Object.Destroy(base.gameObject);
			return;
		}
		Debug.Log("Playing with device: " + SystemInfo.deviceModel);
		bStreamingSupported = false;
		Debug.Log("Streaming Supported: " + bStreamingSupported);
		bDelayedInstantiationSupported = false;
		if (IsMemoryRestricted())
		{
			QualitySettings.masterTextureLimit = 1;
		}
		pools = new Hashtable();
		cmlReader cmlReader2 = new cmlReader("Data/GameObjectPool");
		if (cmlReader2 != null)
		{
			foreach (cmlData item in cmlReader2.Children())
			{
				string text = item["objectPath"].Trim();
				string text2 = text.Substring(text.LastIndexOf('/') + 1);
				if (!IsMemoryRestricted() || !text2.StartsWith("POL_"))
				{
					PoolMeta poolMeta = new PoolMeta();
					poolMeta.objectPath = text;
					poolMeta.objectType = text2;
					poolMeta.poolList = new LinkedList<GameObject>();
					poolMeta.freeList = new Queue();
					poolMeta.canUse = bool.Parse(item["canUse"]);
					poolMeta.delayLoadingTag = item["delayLoadingTag"];
					poolMeta.numberOfInstances = int.Parse(item["numberOfInstances"]);
					if (string.IsNullOrEmpty(poolMeta.delayLoadingTag) || (!StreamingSupported() && !poolMeta.delayLoadingTag.Contains("InitialLoad")))
					{
						LoadPool(poolMeta);
					}
					pools[poolMeta.objectType] = poolMeta;
				}
			}
		}
		LoadTag("InitialLoad");
		instance = this;
	}

	public bool IsMemoryRestricted()
	{
		string deviceModel = SystemInfo.deviceModel;
		return deviceModel.ToLower().Contains("iphone4") || deviceModel.ToLower().Contains("droidx");
	}

	public bool StreamingSupported()
	{
		return bStreamingSupported;
	}

	protected bool LoadPool(PoolMeta pool)
	{
		bool result = false;
		if (pool.originalObject == null)
		{
			pool.originalObject = Resources.Load(pool.objectPath);
			if (pool.originalObject == null)
			{
				Debug.LogError("Object not found in the resources list: " + pool.objectPath);
				return true;
			}
			if (bDelayedInstantiationSupported)
			{
				bNeedsInstantiationUpdate = true;
			}
			else
			{
				InstantiatePool(pool);
			}
			result = true;
		}
		return result;
	}

	protected bool UnloadPool(PoolMeta pool)
	{
		bool result = false;
		if (pool.originalObject != null)
		{
			foreach (GameObject pool2 in pool.poolList)
			{
				Object.Destroy(pool2);
			}
			pool.poolList.Clear();
			pool.freeList.Clear();
			pool.originalObject = null;
			result = true;
		}
		if (bDelayedInstantiationSupported)
		{
			bNeedsInstantiationUpdate = true;
		}
		return result;
	}

	protected void InstantiatePool(PoolMeta pool, int numberToInstantiate = -1)
	{
		if (pool == null || !(pool.originalObject != null))
		{
			return;
		}
		if (numberToInstantiate < 0)
		{
			numberToInstantiate = pool.numberOfInstances;
		}
		for (int i = 0; i < numberToInstantiate; i++)
		{
			if (pool.poolList.Count >= pool.numberOfInstances)
			{
				break;
			}
			GameObject gameObject = (GameObject)Object.Instantiate(pool.originalObject);
			gameObject.SetActive(false);
			gameObject.transform.parent = base.transform;
			gameObject.name = pool.objectType;
			pool.poolList.AddFirst(gameObject);
			pool.freeList.Enqueue(gameObject);
		}
	}

	public void UnloadTag(string szTag)
	{
		if (StreamingSupported())
		{
			Debug.Log("++UnloadTag: " + szTag);
			tagsToUnload.Add(szTag);
			bNeedsLoadUnload = true;
		}
	}

	public void LoadTag(string szTag)
	{
		if (StreamingSupported() || szTag.Contains("InitialLoad"))
		{
			Debug.Log("++LoadTag: " + szTag);
			tagsToLoad.Add(szTag);
			bNeedsLoadUnload = true;
		}
	}

	public void UnloadIndividualObject(GameObject go)
	{
		if (!individualObjectToUnload.Contains(go))
		{
			individualObjectToUnload.Add(go);
			bNeedsLoadUnload = true;
		}
	}

	public GameObject GetNextFree(GameObject prefab, bool freeOldestIfNecessary = false, bool deferred = false)
	{
		return GetNextFree(prefab.name.Trim(), freeOldestIfNecessary, deferred);
	}

	public GameObject GetNextFree(string objectType, bool freeOldestIfNecessary = false, bool deferred = false)
	{
		objectType = objectType.Trim();
		if (!pools.ContainsKey(objectType))
		{
			Debug.LogError("GameObjectPool: Trying to instantiate objectType with no pool allocated:\n" + objectType + "\nhashcode: " + objectType.GetHashCode() + "\nchar array length: " + objectType.ToCharArray().Length);
			return null;
		}
		PoolMeta poolMeta = (PoolMeta)pools[objectType];
		if (poolMeta.poolList.Count < poolMeta.numberOfInstances)
		{
			InstantiatePool(poolMeta, 1);
		}
		if (poolMeta.freeList.Count < 1)
		{
			if (!freeOldestIfNecessary)
			{
				Debug.LogError("GameObjectPool: Pool overflow for: " + objectType);
				return null;
			}
			if (poolMeta.poolList.Count <= 0)
			{
				Debug.LogError("GameObjectPool: trying to free an object that hasn't been properly loaded!\n" + objectType);
				return null;
			}
			GameObject value = poolMeta.poolList.First.Value;
			SetFree(value);
			poolMeta.poolList.RemoveFirst();
			poolMeta.poolList.AddLast(value);
		}
		GameObject gameObject = poolMeta.freeList.Dequeue() as GameObject;
		if (gameObject.GetComponent<Rigidbody>() != null && !gameObject.GetComponent<Rigidbody>().isKinematic)
		{
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		if (deferred)
		{
			if (deactivationList.Contains(gameObject))
			{
				deactivationList.Remove(gameObject);
			}
			if (!activationList.Contains(gameObject))
			{
				activationList.AddLast(gameObject);
			}
		}
		else
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			MonoBehaviour[] array = components;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				monoBehaviour.enabled = true;
			}
			gameObject.SetActive(true);
		}
		return gameObject;
	}

	public bool CanGetObject(string objectType)
	{
		PoolMeta poolMeta = (PoolMeta)pools[objectType.Trim()];
		return poolMeta.canUse;
	}

	public void SetCanGetObject(string objectType, bool enabled)
	{
		PoolMeta poolMeta = (PoolMeta)pools[objectType.Trim()];
		poolMeta.canUse = enabled;
	}

	public void SetFree(GameObject target, bool deferred = false)
	{
		string text = target.name.Trim();
		if (!pools.ContainsKey(text))
		{
			Debug.LogError("GameObjectPool: Trying to free objectType with no pool allocated: " + text);
			return;
		}
		PoolMeta poolMeta = (PoolMeta)pools[text];
		if (deferred)
		{
			target.transform.position.Set(outOfScreen.x, outOfScreen.y, outOfScreen.z);
			if (activationList.Contains(target))
			{
				activationList.Remove(target);
			}
			if (!deactivationList.Contains(target))
			{
				deactivationList.AddLast(target);
			}
		}
		else
		{
			if (target.name.Contains("TER_ART"))
			{
				Debug.Log("GameObjectPool.SetFree(): " + target.name + ".SetActive(false)");
			}
			target.SetActive(false);
		}
		poolMeta.freeList.Enqueue(target);
	}

	public void SetAllFree(string objectType, bool deferred = false)
	{
		objectType = objectType.Trim();
		if (!pools.ContainsKey(objectType))
		{
			Debug.LogError("GameObjectPool: Trying to free objectType with no pool allocated: " + objectType);
			return;
		}
		PoolMeta poolMeta = (PoolMeta)pools[objectType];
		if (poolMeta.freeList.Count == poolMeta.poolList.Count)
		{
			return;
		}
		foreach (GameObject pool in poolMeta.poolList)
		{
			if (deferred)
			{
				pool.transform.position.Set(outOfScreen.x, outOfScreen.y, outOfScreen.z);
				if (activationList.Contains(pool))
				{
					activationList.Remove(pool);
				}
				if (!deactivationList.Contains(pool))
				{
					deactivationList.AddLast(pool);
				}
			}
			else
			{
				if (pool.name.Contains("TER_ART"))
				{
					Debug.Log("GameObjectPool.SetAllFree(): " + pool.name + ".SetActive(false)");
				}
				pool.SetActive(false);
			}
			if (!poolMeta.freeList.Contains(pool))
			{
				poolMeta.freeList.Enqueue(pool);
			}
		}
	}

	public void FreeAllObjects()
	{
		foreach (string key in pools.Keys)
		{
			SetAllFree(key);
		}
	}

	public int GetFreeCount(string objectType)
	{
		objectType.Trim();
		if (!pools.ContainsKey(objectType))
		{
			Debug.LogError("GameObjectPool: Trying to free objectType with no pool allocated: " + objectType);
			return -1;
		}
		PoolMeta poolMeta = (PoolMeta)pools[objectType];
		return poolMeta.freeList.Count;
	}

	public void OnDestroy()
	{
		instance = null;
	}

	private IEnumerator LoadUnloadAll()
	{
		bStreaming = true;
		bool bWaitForUnload = false;
		foreach (string szTag2 in coroutineTagsToUnload)
		{
			foreach (string szKey2 in pools.Keys)
			{
				PoolMeta pool2 = (PoolMeta)pools[szKey2];
				if (pool2.delayLoadingTag == szTag2 && pool2.originalObject != null)
				{
					UnloadPool(pool2);
					bWaitForUnload = true;
					yield return new WaitForSeconds(0.02f);
				}
			}
		}
		coroutineTagsToUnload.Clear();
		foreach (GameObject go in coroutineIndividualObjectToUnload)
		{
			Object.Destroy(go);
			bWaitForUnload = true;
		}
		coroutineIndividualObjectToUnload.Clear();
		if (bWaitForUnload)
		{
			yield return new WaitForSeconds(0.2f);
			yield return Resources.UnloadUnusedAssets();
		}
		foreach (string szTag in coroutineTagsToLoad)
		{
			foreach (string szKey in pools.Keys)
			{
				PoolMeta pool = (PoolMeta)pools[szKey];
				if (pool.delayLoadingTag == szTag && pool.originalObject == null)
				{
					LoadPool(pool);
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
		coroutineTagsToLoad.Clear();
		yield return new WaitForSeconds(0.02f);
		bStreaming = false;
		UIManager.instance.GetSplashScreen().LoadingFinished();
	}

	private IEnumerator UpdateAllInstantiation()
	{
		bUpdatingInstantiation = true;
		foreach (string szKey in pools.Keys)
		{
			PoolMeta pool = (PoolMeta)pools[szKey];
			if (pool.originalObject != null)
			{
				while (pool.poolList.Count < pool.numberOfInstances)
				{
					InstantiatePool(pool, 1);
					yield return new WaitForSeconds(0.001f);
				}
			}
		}
		bUpdatingInstantiation = false;
	}

	private void Update()
	{
		if (!bStreaming)
		{
			if (bNeedsLoadUnload)
			{
				bNeedsLoadUnload = false;
				foreach (string item in tagsToLoad)
				{
					coroutineTagsToLoad.Add(item);
				}
				tagsToLoad.Clear();
				foreach (string item2 in tagsToUnload)
				{
					coroutineTagsToUnload.Add(item2);
				}
				tagsToUnload.Clear();
				foreach (GameObject item3 in individualObjectToUnload)
				{
					coroutineIndividualObjectToUnload.Add(item3);
				}
				individualObjectToUnload.Clear();
				StartCoroutine(LoadUnloadAll());
			}
			else if (!bUpdatingInstantiation && bNeedsInstantiationUpdate)
			{
				bNeedsInstantiationUpdate = false;
				StartCoroutine(UpdateAllInstantiation());
			}
		}
		for (int i = 0; i < maxDeactivationsPerFrame; i++)
		{
			if (deactivationList.Count <= 0)
			{
				break;
			}
			GameObject value = deactivationList.First.Value;
			if (value.name.Contains("TER_ART"))
			{
				Debug.Log("GameObjectPool.Update(): " + value.name + ".SetActive(false)");
			}
			value.SetActive(false);
			deactivationList.RemoveFirst();
		}
		for (int j = 0; j < maxActivationsPerFrame; j++)
		{
			if (activationList.Count <= 0)
			{
				break;
			}
			GameObject value2 = activationList.First.Value;
			value2.SetActive(true);
			MonoBehaviour[] components = value2.GetComponents<MonoBehaviour>();
			MonoBehaviour[] array = components;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				monoBehaviour.enabled = true;
			}
			activationList.RemoveFirst();
		}
	}

	public bool IsStreaming()
	{
		return bNeedsLoadUnload || bStreaming;
	}
}
