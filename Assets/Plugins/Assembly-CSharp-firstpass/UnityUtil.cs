using System;
using System.Collections.Generic;
using Uniject;
using UnityEngine;

public class UnityUtil : IUtil
{
	private static List<RuntimePlatform> PCControlledPlatforms = new List<RuntimePlatform>
	{
		RuntimePlatform.FlashPlayer,
		RuntimePlatform.LinuxPlayer,
		RuntimePlatform.NaCl,
		RuntimePlatform.OSXEditor,
		RuntimePlatform.OSXPlayer,
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.WindowsPlayer,
		RuntimePlatform.WebGLPlayer
	};

	public DateTime currentTime
	{
		get
		{
			return DateTime.Now;
		}
	}

	public string persistentDataPath
	{
		get
		{
			return Application.persistentDataPath;
		}
	}

	public RuntimePlatform Platform
	{
		get
		{
			return Application.platform;
		}
	}

	public T[] getAnyComponentsOfType<T>() where T : class
	{
		GameObject[] array = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		List<T> list = new List<T>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components)
			{
				if (monoBehaviour is T)
				{
					list.Add(monoBehaviour as T);
				}
			}
		}
		return list.ToArray();
	}

	public string loadedLevelName()
	{
		return Application.loadedLevelName;
	}

	public static T findInstanceOfType<T>() where T : MonoBehaviour
	{
		return (T)UnityEngine.Object.FindObjectOfType(typeof(T));
	}

	public static T loadResourceInstanceOfType<T>() where T : MonoBehaviour
	{
		return ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(typeof(T).ToString()))).GetComponent<T>();
	}

	public static bool pcPlatform()
	{
		return PCControlledPlatforms.Contains(Application.platform);
	}

	public static void DebugLog(string message, params object[] args)
	{
		try
		{
			Debug.Log(string.Format("com.ballatergames.debug - {0}", string.Format(message, args)));
		}
		catch (ArgumentNullException message2)
		{
			Debug.Log(message2);
		}
		catch (FormatException message3)
		{
			Debug.Log(message3);
		}
	}

	public static float[] getFrustumBoundaries(Camera camera)
	{
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
		return new float[6]
		{
			(-array[0].normal * array[0].distance).x,
			(-array[1].normal * array[1].distance).x,
			(-array[5].normal * array[5].distance).y,
			(-array[4].normal * array[4].distance).y,
			(-array[2].normal * array[2].distance).z,
			(-array[3].normal * array[3].distance).z
		};
	}
}
