using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
	[Serializable]
	public class SFXItem
	{
		public AudioClip sfx;

		public float sfxVolume = 1f;

		public bool looping;

		public int sfxInstances = 1;

		public List<AudioSource> sfxSource { get; set; }
	}

	public class SFXModulator
	{
		public class SFXModPiece
		{
			public float modMin;

			public float modMax;

			public float modTotalTime;

			public float modCurrentTime;

			public float lastValue;

			public float targetValue;

			public void ResetValues(float min, float max, float time)
			{
				modCurrentTime = 0f;
				modMin = min;
				modMax = max;
				modTotalTime = time;
				lastValue = 0f;
				targetValue = 1f;
			}

			public void ResetCurve(float current)
			{
				modCurrentTime = 0f;
				lastValue = current;
				targetValue = Random.Range(modMin, modMax);
			}

			public float Evaluate(float t)
			{
				float num = 1f;
				if (modTotalTime == 0f)
				{
					return targetValue;
				}
				return Mathf.Clamp(Mathf.Lerp(lastValue, targetValue, Mathf.Clamp01(t / modTotalTime)), modMin, modMax);
			}
		}

		public AudioSource sfx;

		public SFXModPiece pitchMod;

		public SFXModPiece volumeMod;

		public void ProcessMod()
		{
			if (sfx != null)
			{
				if (pitchMod != null)
				{
					pitchMod.modCurrentTime += Time.deltaTime;
					if (pitchMod.modCurrentTime > pitchMod.modTotalTime)
					{
						pitchMod.ResetCurve(sfx.pitch);
					}
					sfx.pitch = pitchMod.Evaluate(pitchMod.modCurrentTime);
				}
				if (volumeMod != null)
				{
					volumeMod.modCurrentTime += Time.deltaTime;
					if (volumeMod.modCurrentTime > volumeMod.modTotalTime)
					{
						volumeMod.ResetCurve(sfx.volume);
					}
					sfx.volume = volumeMod.Evaluate(volumeMod.modCurrentTime);
				}
			}
			else
			{
				pitchMod = null;
				volumeMod = null;
			}
		}
	}

	public SFXItem[] sfxMap;

	protected Dictionary<string, SFXItem> sfxLookup = new Dictionary<string, SFXItem>(StringComparer.OrdinalIgnoreCase);

	protected List<AudioSource> sfx3Ds = new List<AudioSource>();

	protected List<SFXModulator> sfxMods = new List<SFXModulator>();

	public static SFXManager instance;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("SFXManager: Multiple instances spawned");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		SFXItem[] array = sfxMap;
		foreach (SFXItem sFXItem in array)
		{
			if (sFXItem == null || !(sFXItem.sfx != null))
			{
				continue;
			}
			sFXItem.sfxSource = new List<AudioSource>();
			for (int j = 0; j < sFXItem.sfxInstances; j++)
			{
				AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
				audioSource.clip = sFXItem.sfx;
				audioSource.loop = sFXItem.looping;
				audioSource.volume = sFXItem.sfxVolume;
				if (audioSource.clip.name == "Intro_Stinger" || audioSource.clip.name == "Endgame_Stinger" || audioSource.clip.name == "Death_Music")
				{
					audioSource.ignoreListenerVolume = true;
				}
				sFXItem.sfxSource.Add(audioSource);
				sfxLookup[sFXItem.sfx.name] = sFXItem;
			}
		}
	}

	private void Start()
	{
		ModulateVolume("cha_Ratchet_GrindBoots", 0.2f, 0.35f, 0.5f);
		ModulatePitch("cha_Ratchet_GrindBoots", 0.85f, 0.95f, 0.05f);
		ModulateVolume("cha_Ratchet_HoverBoot", 0.4f, 0.65f, 0.5f);
		ModulatePitch("cha_Ratchet_HoverBoot", 0.85f, 0.95f, 0.05f);
		ModulateVolume("cha_Ratchet_Jet_Idle", 0.4f, 0.55f, 0.5f);
		foreach (AudioSource sfx3D in sfx3Ds)
		{
			if (sfx3D.loop)
			{
				ModulatePitch(sfx3D, 0.8f, 1.2f, 0.05f);
			}
		}
	}

	public void Register3DSound(AudioSource sfx)
	{
		if (!sfx3Ds.Contains(sfx))
		{
			sfx3Ds.Add(sfx);
		}
	}

	private SFXItem GetSFXItem(string szSound)
	{
		SFXItem value = null;
		return (!sfxLookup.TryGetValue(szSound, out value)) ? null : value;
	}

	public AudioSource GetAudioSource(string szSound)
	{
		SFXItem sFXItem = GetSFXItem(szSound);
		if (sFXItem != null)
		{
			return sFXItem.sfxSource[0];
		}
		foreach (AudioSource sfx3D in sfx3Ds)
		{
			if (sfx3D.clip != null && sfx3D.clip.name == szSound)
			{
				return sfx3D;
			}
		}
		return null;
	}

	public bool IsSoundLooping(string szSound)
	{
		bool result = false;
		SFXItem sFXItem = GetSFXItem(szSound);
		if (sFXItem != null)
		{
			result = sFXItem.looping;
		}
		return result;
	}

	public void PlayUniqueSound(AudioSource sfx)
	{
		if (sfx != null && !sfx.isPlaying)
		{
			sfx.Play();
		}
	}

	public void PlayUniqueSound(string szSound)
	{
		SFXItem sFXItem = GetSFXItem(szSound);
		if (sFXItem != null)
		{
			PlayUniqueSound(sFXItem.sfxSource[0]);
			AudioSource item = sFXItem.sfxSource[0];
			sFXItem.sfxSource.Remove(item);
			sFXItem.sfxSource.Add(item);
		}
	}

	public void PlaySound(AudioSource sfx)
	{
		if (sfx != null)
		{
			sfx.Play();
		}
	}

	public void PlaySound(string szSound)
	{
		SFXItem sFXItem = GetSFXItem(szSound);
		if (sFXItem != null)
		{
			PlaySound(sFXItem.sfxSource[0]);
			AudioSource item = sFXItem.sfxSource[0];
			sFXItem.sfxSource.Remove(item);
			sFXItem.sfxSource.Add(item);
		}
	}

	public void StopSound(AudioSource sfx)
	{
		if (sfx != null)
		{
			sfx.Stop();
		}
	}

	public void StopSound(string szSound)
	{
		SFXItem sFXItem = GetSFXItem(szSound);
		if (sFXItem != null)
		{
			for (int i = 0; i < sFXItem.sfxInstances; i++)
			{
				StopSound(sFXItem.sfxSource[i]);
			}
		}
	}

	public void StopAllSounds()
	{
		SFXItem[] array = sfxMap;
		foreach (SFXItem sFXItem in array)
		{
			if (sFXItem.sfx != null)
			{
				StopSound(sFXItem.sfxSource[0]);
			}
		}
		foreach (AudioSource sfx3D in sfx3Ds)
		{
			StopSound(sfx3D);
		}
	}

	public void MuteSound(AudioSource sfx, bool bMute)
	{
		if (sfx != null)
		{
			sfx.mute = bMute;
		}
	}

	public void MuteAllSfx(bool bMute)
	{
		SFXItem[] array = sfxMap;
		foreach (SFXItem sFXItem in array)
		{
			if (sFXItem.sfx != null)
			{
				for (int j = 0; j < sFXItem.sfxInstances; j++)
				{
					MuteSound(sFXItem.sfxSource[j], bMute);
				}
			}
		}
		foreach (AudioSource sfx3D in sfx3Ds)
		{
			MuteSound(sfx3D, bMute);
		}
	}

	protected SFXModulator GetSFXModulator(AudioSource source)
	{
		SFXModulator result = null;
		foreach (SFXModulator sfxMod in sfxMods)
		{
			if (sfxMod.sfx == source)
			{
				result = sfxMod;
				break;
			}
		}
		return result;
	}

	public void ModulatePitch(string szSound, float min, float max, float time)
	{
		AudioSource audioSource = GetAudioSource(szSound);
		if (audioSource != null)
		{
			SFXModulator sFXModulator = GetSFXModulator(audioSource);
			if (sFXModulator == null)
			{
				sFXModulator = new SFXModulator();
				sFXModulator.sfx = audioSource;
				sfxMods.Add(sFXModulator);
			}
			if (sFXModulator.pitchMod == null)
			{
				sFXModulator.pitchMod = new SFXModulator.SFXModPiece();
			}
			sFXModulator.pitchMod.ResetValues(min, max, time);
			sFXModulator.pitchMod.ResetCurve(audioSource.pitch);
		}
	}

	public void ModulatePitch(AudioSource source, float min, float max, float time)
	{
		if (source != null)
		{
			SFXModulator sFXModulator = GetSFXModulator(source);
			if (sFXModulator == null)
			{
				sFXModulator = new SFXModulator();
				sFXModulator.sfx = source;
				sfxMods.Add(sFXModulator);
			}
			if (sFXModulator.pitchMod == null)
			{
				sFXModulator.pitchMod = new SFXModulator.SFXModPiece();
			}
			sFXModulator.pitchMod.ResetValues(min, max, time);
			sFXModulator.pitchMod.ResetCurve(source.pitch);
		}
	}

	public void StopPitchModulation(string szSound)
	{
		AudioSource audioSource = GetAudioSource(szSound);
		if (audioSource != null)
		{
			SFXModulator sFXModulator = GetSFXModulator(audioSource);
			if (sFXModulator != null)
			{
				sFXModulator.pitchMod = null;
				CheckRemoveMod(sFXModulator);
			}
		}
	}

	public void ModulateVolume(string szSound, float min, float max, float time)
	{
		AudioSource audioSource = GetAudioSource(szSound);
		if (audioSource != null)
		{
			SFXModulator sFXModulator = GetSFXModulator(audioSource);
			if (sFXModulator == null)
			{
				sFXModulator = new SFXModulator();
				sFXModulator.sfx = audioSource;
				sfxMods.Add(sFXModulator);
			}
			if (sFXModulator.volumeMod == null)
			{
				sFXModulator.volumeMod = new SFXModulator.SFXModPiece();
			}
			sFXModulator.volumeMod.ResetValues(min, max, time);
			sFXModulator.volumeMod.ResetCurve(audioSource.volume);
		}
	}

	public void StopVolumeModulation(string szSound)
	{
		AudioSource audioSource = GetAudioSource(szSound);
		if (audioSource != null)
		{
			SFXModulator sFXModulator = GetSFXModulator(audioSource);
			if (sFXModulator != null)
			{
				sFXModulator.volumeMod = null;
				CheckRemoveMod(sFXModulator);
			}
		}
	}

	protected void CheckRemoveMod(SFXModulator mod)
	{
		if (mod.pitchMod == null && mod.volumeMod == null)
		{
			sfxMods.Remove(mod);
		}
	}

	private void Update()
	{
		foreach (SFXModulator sfxMod in sfxMods)
		{
			sfxMod.ProcessMod();
		}
		foreach (SFXModulator sfxMod2 in sfxMods)
		{
			CheckRemoveMod(sfxMod2);
		}
	}

	public void StopAllLoopingSounds()
	{
		SFXItem[] array = sfxMap;
		foreach (SFXItem sFXItem in array)
		{
			if (sFXItem.sfx != null && sFXItem.sfxSource[0].loop)
			{
				StopSound(sFXItem.sfxSource[0]);
			}
		}
		foreach (AudioSource sfx3D in sfx3Ds)
		{
			if (sfx3D != null && sfx3D.loop)
			{
				StopSound(sfx3D);
			}
		}
	}
}
