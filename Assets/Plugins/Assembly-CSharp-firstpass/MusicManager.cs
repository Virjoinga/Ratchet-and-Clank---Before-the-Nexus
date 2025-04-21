using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public enum eMusicTrackType
	{
		None = -1,
		InGame1 = 0,
		Menu = 1,
		InGame2 = 2,
		InGame3 = 3,
		Boss = 4,
		Groove = 5,
		IntroMovie = 6,
		Max = 7
	}

	public AudioClip[] musicTracks;

	public ArrayOfAudioClips[] stingerIncidenceMatrix;

	public AnimationCurve crossFadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float crossFadeTime = 5f;

	public bool tryToMatchCrossFadeTimeWithStinger;

	protected eMusicTrackType currentTrackType;

	protected eMusicTrackType nextTrackType;

	protected AudioSource currentTrack;

	protected AudioSource nextTrack;

	protected AudioSource stinger;

	protected float changeStartTime;

	protected float lastPauseTime;

	protected bool paused;

	protected bool inTransition;

	protected bool muted;

	public static MusicManager instance;

	public eMusicTrackType CurrentTrackType
	{
		get
		{
			return currentTrackType;
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("MusicManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		muted = false;
		currentTrackType = eMusicTrackType.None;
		nextTrackType = eMusicTrackType.None;
		currentTrack = base.gameObject.AddComponent<AudioSource>();
		currentTrack.clip = null;
		currentTrack.playOnAwake = false;
		currentTrack.volume = 0f;
		currentTrack.loop = true;
		currentTrack.ignoreListenerVolume = true;
		nextTrack = base.gameObject.AddComponent<AudioSource>();
		nextTrack.clip = null;
		nextTrack.playOnAwake = false;
		nextTrack.volume = 0f;
		nextTrack.loop = true;
		currentTrack.ignoreListenerVolume = true;
		stinger = base.gameObject.AddComponent<AudioSource>();
		stinger.volume = 1f;
		stinger.clip = null;
		stinger.playOnAwake = false;
		stinger.loop = false;
		stinger.ignoreListenerVolume = true;
	}

	private void Start()
	{
	}

	public void Pause()
	{
		if (!paused)
		{
			if (currentTrack.clip != null && currentTrack.isPlaying)
			{
				currentTrack.Pause();
			}
			if (nextTrack.clip != null && nextTrack.isPlaying)
			{
				nextTrack.Pause();
			}
			if (stinger.clip != null)
			{
				stinger.Pause();
			}
			paused = true;
			lastPauseTime = Time.realtimeSinceStartup;
		}
	}

	public void Resume()
	{
		if (paused)
		{
			if (currentTrack.clip != null && currentTrack.volume >= 0.01f)
			{
				currentTrack.Play();
			}
			if (nextTrack.clip != null && nextTrack.volume >= 0.01f)
			{
				nextTrack.Play();
			}
			if (stinger.clip != null)
			{
				stinger.Play();
			}
			paused = false;
			changeStartTime += Time.realtimeSinceStartup - lastPauseTime;
			lastPauseTime = 0f;
		}
	}

	public void Play(eMusicTrackType track, bool bForceReset, float fCrossfadeTime)
	{
		crossFadeTime = fCrossfadeTime;
		lastPauseTime = 0f;
		paused = false;
		if (nextTrackType == track || (currentTrackType == track && !inTransition))
		{
			if (bForceReset)
			{
				if (nextTrackType == track && nextTrackType != eMusicTrackType.None)
				{
					nextTrack.Stop();
					nextTrack.Play();
				}
				if (currentTrackType == track && !inTransition && currentTrackType != eMusicTrackType.None)
				{
					currentTrack.Stop();
					currentTrack.Play();
				}
			}
			return;
		}
		if (inTransition)
		{
			SwitchTracks();
		}
		nextTrackType = track;
		if (nextTrackType != eMusicTrackType.None)
		{
			changeStartTime = Time.realtimeSinceStartup;
			if (currentTrackType != eMusicTrackType.None && (int)currentTrackType < stingerIncidenceMatrix.Length && (int)nextTrackType < stingerIncidenceMatrix[(int)currentTrackType].incidentTracks.Length)
			{
				stinger.clip = stingerIncidenceMatrix[(int)currentTrackType].incidentTracks[(int)nextTrackType];
				if (stinger.clip != null)
				{
					stinger.Play();
				}
			}
			inTransition = true;
			nextTrack.clip = musicTracks[(int)nextTrackType];
			nextTrack.volume = 0f;
			if (bForceReset || !nextTrack.isPlaying)
			{
				nextTrack.Stop();
				nextTrack.Play();
			}
		}
		else
		{
			changeStartTime = Time.realtimeSinceStartup;
			inTransition = true;
			nextTrack.clip = null;
			nextTrack.volume = 0f;
		}
	}

	public void Stop()
	{
		currentTrack.Stop();
		currentTrack.clip = null;
		currentTrack.volume = 1f;
		currentTrackType = eMusicTrackType.None;
		nextTrack.Stop();
		nextTrack.clip = null;
		nextTrack.volume = 0f;
		nextTrackType = eMusicTrackType.None;
		stinger.Stop();
		stinger.clip = null;
		inTransition = false;
		paused = false;
		lastPauseTime = 0f;
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	protected void SwitchTracks()
	{
		currentTrackType = nextTrackType;
		AudioSource audioSource = currentTrack;
		currentTrack.Stop();
		nextTrack.ignoreListenerVolume = true;
		currentTrack = nextTrack;
		nextTrack = audioSource;
	}

	private void Update()
	{
		if (inTransition && !paused)
		{
			float num = crossFadeTime;
			if (crossFadeTime == 0f)
			{
				num = 1E-06f;
			}
			if (stinger.clip != null && tryToMatchCrossFadeTimeWithStinger)
			{
				num = stinger.clip.length;
			}
			float num2 = Time.realtimeSinceStartup - changeStartTime;
			float volume = nextTrack.volume;
			nextTrack.volume = crossFadeCurve.Evaluate(num2 / num);
			currentTrack.volume = Mathf.Max(0f, currentTrack.volume - (nextTrack.volume - volume));
			if (nextTrack.volume > 0.9999f)
			{
				inTransition = false;
				stinger.clip = null;
				currentTrack.volume = 0f;
				nextTrack.volume = 1f;
				SwitchTracks();
			}
		}
	}

	public void MuteMusic(bool bMute)
	{
		stinger.mute = bMute;
		currentTrack.mute = bMute;
		nextTrack.mute = bMute;
		muted = bMute;
	}

	public bool isMuted()
	{
		return muted;
	}

	public void SetVolume(float fVolume)
	{
		currentTrack.volume = fVolume;
	}
}
