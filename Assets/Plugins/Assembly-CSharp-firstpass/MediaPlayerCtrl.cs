using UnityEngine;

public class MediaPlayerCtrl : MonoBehaviour
{
	public enum MEDIAPLAYER_STATE
	{
		NOT_READY = 0,
		READY = 1,
		END = 2,
		PLAYING = 3,
		PAUSED = 4,
		STOPPED = 5,
		ERROR = 6
	}

	public enum MEDIA_SCALE
	{
		SCALE_X_TO_Y = 0,
		SCALE_X_TO_Z = 1,
		SCALE_Y_TO_X = 2,
		SCALE_Y_TO_Z = 3,
		SCALE_Z_TO_X = 4,
		SCALE_Z_TO_Y = 5
	}

	public string m_strFileName;

	private Texture2D m_VideoTexture;

	private MEDIAPLAYER_STATE m_CurrentState;

	private int m_iCurrentSeekPosition;

	public static MediaPlayerCtrl Instance;

	private bool m_bFirst;

	public MEDIA_SCALE m_ScaleValue;

	public GameObject m_objResize;

	public bool m_bLoop;

	public bool m_bAutoPlay = true;

	private bool m_bStop;

	private AndroidJavaObject javaObj;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(Instance.gameObject);
		}
		m_VideoTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
		m_VideoTexture.filterMode = FilterMode.Bilinear;
		m_VideoTexture.wrapMode = TextureWrapMode.Clamp;
		Instance = this;
		Call_SetUnityActivity();
	}

	private void Start()
	{
	}

	private void OnRenderObject()
	{
		if (!m_bFirst)
		{
			Call_SetUnityTexture(m_VideoTexture.GetNativeTextureID());
			string strFileName = m_strFileName.Trim();
			Call_Load(strFileName, 0);
			m_bFirst = true;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_UpdateVideoTexture();
			m_iCurrentSeekPosition = Call_GetSeekPosition();
		}
		if (m_CurrentState == Call_GetStatus())
		{
			return;
		}
		m_CurrentState = Call_GetStatus();
		if (m_CurrentState == MEDIAPLAYER_STATE.READY)
		{
			if (m_objResize != null)
			{
				int num = Call_GetVideoWidth();
				int num2 = Call_GetVideoHeight();
				float num3 = (float)num2 / (float)num;
				if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Y)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.x, m_objResize.transform.localScale.x * num3, m_objResize.transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Z)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.x, m_objResize.transform.localScale.y, m_objResize.transform.localScale.x * num3);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_X)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.y * num3, m_objResize.transform.localScale.y, m_objResize.transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_Z)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.x, m_objResize.transform.localScale.y, m_objResize.transform.localScale.y * num3);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_X)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.z * num3, m_objResize.transform.localScale.y, m_objResize.transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_Y)
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.x, m_objResize.transform.localScale.z * num3, m_objResize.transform.localScale.z);
				}
				else
				{
					m_objResize.transform.localScale = new Vector3(m_objResize.transform.localScale.x, m_objResize.transform.localScale.y, m_objResize.transform.localScale.z);
				}
			}
			Call_SetWindowSize();
			if (m_bAutoPlay)
			{
				Call_Play(0);
			}
		}
		else if (m_CurrentState == MEDIAPLAYER_STATE.END && m_bLoop)
		{
			Call_Play(0);
		}
	}

	private void OnDestroy()
	{
		Call_UnLoad();
		if (m_VideoTexture != null)
		{
			Object.Destroy(m_VideoTexture);
		}
		Call_Destroy();
	}

	private void OnApplicationPause(bool bPause)
	{
		if (bPause)
		{
			Call_Pause();
		}
		else
		{
			Call_RePlay();
		}
	}

	public Texture2D GetVideoTexture()
	{
		return m_VideoTexture;
	}

	public MEDIAPLAYER_STATE GetCurrentState()
	{
		return m_CurrentState;
	}

	public void Play()
	{
		Debug.Log("playing on the media controller");
		if (m_bStop)
		{
			Debug.Log("stopping");
			Call_Play(0);
			m_bStop = false;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
		{
			Debug.Log("replaying");
			Call_RePlay();
		}
		else if (m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			Debug.Log("playing from frame 0");
			Call_Play(0);
		}
	}

	public bool IsPlaying()
	{
		return m_CurrentState == MEDIAPLAYER_STATE.PLAYING;
	}

	public void Stop()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_bStop = true;
		m_iCurrentSeekPosition = 0;
	}

	public void Pause()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_CurrentState = MEDIAPLAYER_STATE.PAUSED;
	}

	public void Load(string strFileName)
	{
		Debug.Log("loading: " + strFileName);
		m_strFileName = strFileName;
		Call_Load(m_strFileName, 0);
		m_CurrentState = MEDIAPLAYER_STATE.NOT_READY;
	}

	public void SetVolume(float fVolume)
	{
		Call_SetVolume(fVolume);
	}

	public int GetSeekPosition()
	{
		return m_iCurrentSeekPosition;
	}

	public int GetDuration()
	{
		return Call_GetDuration();
	}

	public int GetCurrentSeekPercent()
	{
		return Call_GetCurrentSeekPercent();
	}

	public int GetVideoWidth()
	{
		return Call_GetVideoWidth();
	}

	public int GetVideoHeight()
	{
		return Call_GetVideoHeight();
	}

	public void UnLoad()
	{
		Call_UnLoad();
	}

	private AndroidJavaObject GetJavaObject()
	{
		if (javaObj == null)
		{
			javaObj = new AndroidJavaObject("com.EasyMovieTexture.EasyMovieTexture");
		}
		return javaObj;
	}

	private void Call_Destroy()
	{
		GetJavaObject().Call("Destroy");
	}

	private void Call_UnLoad()
	{
		GetJavaObject().Call("UnLoad");
	}

	private bool Call_Load(string strFileName, int iSeek)
	{
		Debug.Log("calling Call_Load " + strFileName + " " + iSeek);
		return GetJavaObject().Call<bool>("Load", new object[2] { strFileName, iSeek });
	}

	private void Call_UpdateVideoTexture()
	{
		GetJavaObject().Call("UpdateVideoTexture");
	}

	private void Call_SetVolume(float fVolume)
	{
		GetJavaObject().Call("SetVolume", fVolume);
	}

	private void Call_SetSeekPosition(int iSeek)
	{
		GetJavaObject().Call("SetSeekPosition", iSeek);
	}

	private int Call_GetSeekPosition()
	{
		return GetJavaObject().Call<int>("GetSeekPosition", new object[0]);
	}

	private void Call_Play(int iSeek)
	{
		GetJavaObject().Call("Play", iSeek);
	}

	private void Call_Reset()
	{
		GetJavaObject().Call("Reset");
	}

	private void Call_Stop()
	{
		GetJavaObject().Call("Stop");
	}

	private void Call_RePlay()
	{
		GetJavaObject().Call("RePlay");
	}

	private void Call_Pause()
	{
		GetJavaObject().Call("Pause");
	}

	private int Call_GetVideoWidth()
	{
		return GetJavaObject().Call<int>("GetVideoWidth", new object[0]);
	}

	private int Call_GetVideoHeight()
	{
		return GetJavaObject().Call<int>("GetVideoHeight", new object[0]);
	}

	private void Call_SetUnityTexture(int iTextureID)
	{
		GetJavaObject().Call("SetUnityTexture", iTextureID);
	}

	private void Call_SetWindowSize()
	{
		GetJavaObject().Call("SetWindowSize");
	}

	private int Call_GetDuration()
	{
		return GetJavaObject().Call<int>("GetDuration", new object[0]);
	}

	private int Call_GetCurrentSeekPercent()
	{
		return GetJavaObject().Call<int>("GetCurrentSeekPercent", new object[0]);
	}

	private void Call_SetUnityActivity()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		GetJavaObject().Call("SetUnityActivity", @static);
	}

	private MEDIAPLAYER_STATE Call_GetStatus()
	{
		return (MEDIAPLAYER_STATE)GetJavaObject().Call<int>("GetStatus", new object[0]);
	}
}
