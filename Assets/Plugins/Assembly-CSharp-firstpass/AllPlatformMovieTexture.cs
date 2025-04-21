using System.Runtime.CompilerServices;
using UnityEngine;

public class AllPlatformMovieTexture : MonoBehaviour
{
	public delegate void OnFinished();

	protected bool bNeedsEvent;

	protected string szMovieName;

	[method: MethodImpl(32)]
	public event OnFinished onFinished;

	private void Start()
	{
	}

	private void Update()
	{
		if (bNeedsEvent && !IsPlaying())
		{
			if (this.onFinished != null)
			{
				this.onFinished();
			}
			bNeedsEvent = false;
		}
	}

	public void Load(string szMovie)
	{
		szMovieName = szMovie;
		Debug.Log("loading movie on android");
		MediaPlayerCtrl component = base.gameObject.GetComponent<MediaPlayerCtrl>();
		if (component != null)
		{
			component.Load(szMovie);
		}
	}

	public bool Play()
	{
		bool flag = false;
		Debug.Log("playing all platform movie android");
		MediaPlayerCtrl component = base.gameObject.GetComponent<MediaPlayerCtrl>();
		if (component != null)
		{
			Debug.Log("media player controller found");
			if (component.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.READY)
			{
				Debug.Log("Texture before: " + base.GetComponent<Renderer>().material.mainTexture.ToString());
				base.GetComponent<Renderer>().material.mainTexture = component.GetVideoTexture();
				Debug.Log("Texture after: " + base.GetComponent<Renderer>().material.mainTexture.ToString());
				component.Play();
				flag = true;
			}
			else
			{
				Debug.Log("media player not ready");
			}
		}
		if (flag)
		{
			bNeedsEvent = true;
		}
		return flag;
	}

	public void Stop()
	{
		MediaPlayerCtrl component = base.gameObject.GetComponent<MediaPlayerCtrl>();
		if (component != null)
		{
			component.Stop();
		}
	}

	public bool IsPlaying()
	{
		bool result = false;
		MediaPlayerCtrl component = base.gameObject.GetComponent<MediaPlayerCtrl>();
		if (component != null)
		{
			result = component.IsPlaying();
		}
		return result;
	}

	private void OnApplicationPause(bool bPause)
	{
		Debug.Log("AllPlatformMovieTexture.OnApplicationPause() bPause=" + bPause);
	}
}
