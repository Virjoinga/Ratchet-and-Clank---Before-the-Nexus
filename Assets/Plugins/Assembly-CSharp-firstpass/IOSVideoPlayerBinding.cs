using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class IOSVideoPlayerBinding : MonoBehaviour
{
	public static IOSVideoPlayerBinding instance;

	private string goName = "IOSVideoPlayer";

	public bool isPlaying;

	[DllImport("__Internal")]
	private static extern void _setObjectName(string objName);

	[DllImport("__Internal")]
	private static extern void _playMovieWithName(string fileName);

	[DllImport("__Internal")]
	private static extern void _playMovieWithURL(string fileName);

	[DllImport("__Internal")]
	private static extern void _playMovieWithTrimmedDuration(string movieName, float startDuration, float endDuration);

	[DllImport("__Internal")]
	private static extern void _playMovieTrimmedFromStart(string movieName, float startDuration);

	[DllImport("__Internal")]
	private static extern void _playMovieTrimmedFromEnd(string movieName, float endDuration);

	[DllImport("__Internal")]
	private static extern void _addPlayPauseButton(string playImageName, string pauseImageName, float x, float y);

	[DllImport("__Internal")]
	private static extern void _addSkipButton(string skipImageName, float x, float y);

	[DllImport("__Internal")]
	private static extern void _addStopButton(string stopImageName, float x, float y);

	[DllImport("__Internal")]
	private static extern void _addSeekingBarAtPosition(float x, float y, string coveredImagename, string reamaningImageName, string thumbNormalName, string thumbDownName);

	[DllImport("__Internal")]
	private static extern void _playSubtitlesFromFile(string subtitleFileName);

	[DllImport("__Internal")]
	private static extern void _setSubtitleFontSize(float thisSize);

	[DllImport("__Internal")]
	private static extern void _setSubtitleYCoordiante(float thisCoordinate);

	[DllImport("__Internal")]
	private static extern void _removePlayPauseButton();

	[DllImport("__Internal")]
	private static extern void _removeSkipButton();

	[DllImport("__Internal")]
	private static extern void _removeStopButton();

	[DllImport("__Internal")]
	private static extern void _removeSeekingBar();

	[DllImport("__Internal")]
	private static extern float _getDeviceDisplayScale();

	[DllImport("__Internal")]
	private static extern void _shouldPauseUnity(bool flag);

	[DllImport("__Internal")]
	private static extern void _shouldResumeVideoPlaybackOnApplicationResume(bool flag);

	private void Start()
	{
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		if (base.gameObject != null && base.gameObject.name != null)
		{
			goName = base.gameObject.name;
			if (!Application.isEditor)
			{
				_setObjectName(goName);
			}
		}
		else
		{
			Debug.LogWarning("GameObject Not Found");
		}
	}

	public void PlayVideo(string name)
	{
		Debug.Log("playMovieWithName");
		if (!Application.isEditor)
		{
			if (File.Exists(Application.dataPath + "/" + name))
			{
				_playMovieWithName(name);
				isPlaying = true;
			}
			else if (File.Exists(Application.dataPath + "/Raw/" + name))
			{
				_playMovieWithName("/Data/Raw/" + name);
				isPlaying = true;
			}
			else
			{
				Debug.Log("File Not found at " + Application.dataPath + "/Raw/" + name);
			}
		}
	}

	public void PlayVideo(string path, string name)
	{
		Debug.Log("playMovieWithURL");
		if (Application.isEditor)
		{
			return;
		}
		if (path.EndsWith("/"))
		{
			if (File.Exists(path + name))
			{
				_playMovieWithURL(path + name);
				isPlaying = true;
			}
			else
			{
				Debug.Log("File Not found at " + path + name);
			}
		}
		else if (File.Exists(path + "/" + name))
		{
			_playMovieWithURL(path + "/" + name);
			isPlaying = true;
		}
		else
		{
			Debug.Log("File Not found at " + path + "/" + name);
		}
	}

	public void playMovieWithTrimmedDuration(string movieName, float startDuration, float endDuration)
	{
		if (!Application.isEditor)
		{
			_playMovieWithTrimmedDuration(movieName, startDuration, endDuration);
		}
	}

	public void playMovieTrimmedFromStart(string movieName, float startDuration)
	{
		if (!Application.isEditor)
		{
			_playMovieTrimmedFromStart(movieName, startDuration);
		}
	}

	public void playMovieTrimmedFromEnd(string movieName, float endDuration)
	{
		if (!Application.isEditor)
		{
			_playMovieTrimmedFromEnd(movieName, endDuration);
		}
	}

	public void addPlayPauseButton(string playImageName, string pauseImageName, float x, float y)
	{
		if (!Application.isEditor)
		{
			_addPlayPauseButton(playImageName, pauseImageName, x, y);
		}
	}

	public void addPlayPauseButton(float x, float y)
	{
		if (!Application.isEditor)
		{
			_addPlayPauseButton(string.Empty, string.Empty, x, y);
		}
	}

	public void addPlayPauseButton()
	{
		if (!Application.isEditor)
		{
			_addPlayPauseButton(string.Empty, string.Empty, 20f, 100f);
		}
	}

	public void addSkipButton(string skipImageName, float x, float y)
	{
		if (!Application.isEditor)
		{
			_addSkipButton(skipImageName, x, y);
		}
	}

	public void addSkipButton(float x, float y)
	{
		if (!Application.isEditor)
		{
			_addSkipButton(string.Empty, x, y);
		}
	}

	public void addSkipButton()
	{
		if (!Application.isEditor)
		{
			_addSkipButton(string.Empty, 20f, 150f);
		}
	}

	public void addStopButton(string stopImageName, float x, float y)
	{
		if (!Application.isEditor)
		{
			_addStopButton(stopImageName, x, y);
		}
	}

	public void addStopButton(float x, float y)
	{
		if (!Application.isEditor)
		{
			_addStopButton(string.Empty, x, y);
		}
	}

	public void addStopButton()
	{
		if (!Application.isEditor)
		{
			_addStopButton(string.Empty, 20f, 200f);
		}
	}

	public void addSeekingBar(float x, float y, string coveredImageName = "", string reamaningImageName = "", string thumbNormalName = "", string thumbDownName = "")
	{
		if (!Application.isEditor)
		{
			_addSeekingBarAtPosition(x, y, coveredImageName, reamaningImageName, thumbNormalName, thumbDownName);
		}
	}

	public void addSeekingBar(float x, float y)
	{
		if (!Application.isEditor)
		{
			_addSeekingBarAtPosition(x, y, string.Empty, string.Empty, string.Empty, string.Empty);
		}
	}

	public void addSeekingBar()
	{
		if (!Application.isEditor)
		{
			_addSeekingBarAtPosition((float)(Screen.width / 2) - 150f * _getDeviceDisplayScale(), Screen.height - 20, string.Empty, string.Empty, string.Empty, string.Empty);
		}
	}

	public void playSubtitlesFromFile(string subtitleFileName)
	{
		if (!Application.isEditor)
		{
			_playSubtitlesFromFile(subtitleFileName);
		}
	}

	public void setSubtitleFontSize(float thisSize)
	{
		if (!Application.isEditor)
		{
			_setSubtitleFontSize(thisSize);
		}
	}

	public void setSubtitleYCoordiante(float thisCoordinate)
	{
		if (!Application.isEditor)
		{
			_setSubtitleYCoordiante(thisCoordinate);
		}
	}

	public void removePlayPauseButton()
	{
		if (!Application.isEditor)
		{
			_removePlayPauseButton();
		}
	}

	public void removeSkipButton()
	{
		if (!Application.isEditor)
		{
			_removeSkipButton();
		}
	}

	public void removeStopButton()
	{
		if (!Application.isEditor)
		{
			_removeStopButton();
		}
	}

	public void removeSeekingBar()
	{
		if (!Application.isEditor)
		{
			_removeSeekingBar();
		}
	}

	public void removeAllButtons()
	{
		if (!Application.isEditor)
		{
			_removePlayPauseButton();
			_removeSkipButton();
			_removeStopButton();
			_removeSeekingBar();
		}
	}

	public void shouldPauseUnity(bool flag)
	{
		if (!Application.isEditor)
		{
			_shouldPauseUnity(flag);
		}
	}

	public void shouldAutoResumePlaybackOnAppResume(bool flag)
	{
		if (!Application.isEditor)
		{
			_shouldResumeVideoPlaybackOnApplicationResume(flag);
		}
	}
}
