using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
	protected List<string> looping = new List<string>();

	protected List<AudioSource> loopingSfx = new List<AudioSource>();

	private void Start()
	{
		AudioSource[] components = base.gameObject.GetComponents<AudioSource>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				SFXManager.instance.Register3DSound(components[i]);
			}
		}
	}

	protected AudioSource GetLocalAudioSource(string szSoundToPlay)
	{
		AudioSource result = null;
		AudioSource[] components = base.gameObject.GetComponents<AudioSource>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null && components[i].clip != null && components[i].clip.name == szSoundToPlay)
			{
				result = components[i];
				break;
			}
		}
		return result;
	}

	public void AE_PlaySound(string szSoundToPlay)
	{
		AudioSource localAudioSource = GetLocalAudioSource(szSoundToPlay);
		if ((szSoundToPlay == "Death_Music" || szSoundToPlay == "Intro_Stinger") && MusicManager.instance.isMuted())
		{
			szSoundToPlay = "null";
		}
		if (localAudioSource != null)
		{
			if (localAudioSource.loop && !loopingSfx.Contains(localAudioSource))
			{
				loopingSfx.Add(localAudioSource);
			}
			SFXManager.instance.PlayUniqueSound(localAudioSource);
		}
		else
		{
			if (SFXManager.instance.IsSoundLooping(szSoundToPlay) && !looping.Contains(szSoundToPlay))
			{
				looping.Add(szSoundToPlay);
			}
			SFXManager.instance.PlayUniqueSound(szSoundToPlay);
		}
	}

	public void AE_StopSound(string szSoundToStop)
	{
		AudioSource localAudioSource = GetLocalAudioSource(szSoundToStop);
		if (localAudioSource != null)
		{
			SFXManager.instance.StopSound(localAudioSource);
		}
		else
		{
			SFXManager.instance.StopSound(szSoundToStop);
		}
	}

	public void AE_StopAllLoopingSounds()
	{
		foreach (string item in looping)
		{
			SFXManager.instance.StopSound(item);
		}
		looping.Clear();
		foreach (AudioSource item2 in loopingSfx)
		{
			SFXManager.instance.StopSound(item2);
		}
		loopingSfx.Clear();
	}

	public void AE_StartNeftinFire()
	{
		GameObject gameObject = GameObject.Find("Leviathan_PF");
		GameObject gameObject2 = gameObject.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeL")
			.gameObject;
		GameObject gameObject3 = gameObject.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeR")
			.gameObject;
		gameObject2.particleSystem.Play();
		gameObject3.particleSystem.Play();
		if (gameObject != null && GameController.instance.playerController.IsDoingIntro())
		{
			StartCoroutine("FireIntroProjectile");
		}
		else
		{
			StartCoroutine("FireEMPProjectile");
		}
	}

	private IEnumerator FireIntroProjectile()
	{
		GameObject Neftin = GameObject.Find("Leviathan_PF");
		GameObject RWeaponEffect = Neftin.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeR")
			.gameObject;
		yield return new WaitForSeconds(1.25f);
		GameObject projectile = GameObjectPool.instance.GetNextFree("BulletThing");
		projectile.GetComponent<EnemyProjectiles>().particles = Neftin.GetComponent<LeviathanController>().projectileParticles;
		projectile.GetComponent<EnemyProjectiles>().SetProjectileData(0u, 20f, RWeaponEffect.transform.position, 0, Vector3.zero);
	}

	private IEnumerator FireEMPProjectile()
	{
		GameObject Neftin = GameObject.Find("Leviathan_PF");
		GameObject RWeaponEffect = Neftin.transform.Find("Root").Find("ShipBody").Find("FX_WeaponChargeR")
			.gameObject;
		yield return new WaitForSeconds(1.25f);
		Vector3 EMPDestination = Vector3.zero;
		UIHUD HUD = UIManager.instance.GetHUD();
		if (HUD != null)
		{
			Vector3 GadgetButtonLoc = new Vector3(0f, 0f, GameController.instance.mainCamera.GetComponent<Camera>().nearClipPlane);
			GadgetButtonLoc.x = Mathf.Clamp01(GadgetButtonLoc.x / (float)Screen.width);
			GadgetButtonLoc.y = Mathf.Clamp01(GadgetButtonLoc.y / (float)Screen.height);
			EMPDestination = GameController.instance.mainCamera.GetComponent<Camera>().ViewportToWorldPoint(GadgetButtonLoc);
		}
		GameObject projectile = GameObjectPool.instance.GetNextFree("BulletThing");
		projectile.GetComponent<EnemyProjectiles>().particles = GameObjectPool.instance.GetNextFree("FX_LeviathanProjectileEMP").particleSystem;
		projectile.GetComponent<EnemyProjectiles>().SetProjectileData(0u, 5f, RWeaponEffect.transform.position, 0, EMPDestination);
	}

	public void AE_StartNeftinFireImpact(string impactExplosionParticles)
	{
		if (impactExplosionParticles != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(impactExplosionParticles, true);
			if (nextFree != null)
			{
				Vector3 position = GameController.instance.playerController.rigidbody.position;
				position.y += 1.5f;
				nextFree.transform.position = position;
				nextFree.particleSystem.Play();
			}
		}
	}

	public void AE_FinishIntro()
	{
		GameController.instance.playerController.EndIntro();
	}

	public void AE_PlayMusic(MusicManager.eMusicTrackType szMusicToPlay)
	{
		MusicManager.instance.Play(szMusicToPlay, false, 0f);
	}

	public void AE_StopMusic()
	{
		MusicManager.instance.Stop();
	}

	public void AE_PauseMusic()
	{
		MusicManager.instance.Pause();
	}

	public void AE_ResumeMusic()
	{
		MusicManager.instance.Resume();
	}

	public void AE_LowerEngineVol(float fVolume)
	{
		if (fVolume == 0.8f)
		{
			SFXManager.instance.ModulateVolume("cha_Leviathan_engine_loop", fVolume, fVolume, 0.5f);
		}
		else
		{
			SFXManager.instance.ModulateVolume("cha_Leviathan_engine_loop", fVolume, fVolume, 3f);
		}
	}

	public void AE_IntroEngine()
	{
		SFXManager.instance.GetAudioSource("cha_Leviathan_engine_loop").volume = 0f;
		SFXManager.instance.ModulateVolume("cha_Leviathan_engine_loop", 1f, 1f, 2f);
		SFXManager.instance.PlaySound("cha_Leviathan_engine_loop");
	}

	public void AE_SetMusicVolume(float fVolume)
	{
		MusicManager.instance.SetVolume(fVolume);
	}

	public void AE_ClankBlink(float time)
	{
		GameController.instance.playerController.ClankBlink(time);
	}

	public void AE_RatchetBlink(float time)
	{
		GameController.instance.playerController.RatchetBlink(time);
	}

	public void AE_EyeLidDeathAnimation()
	{
		GameController.instance.playerController.EyeLidDeathAnimation();
	}
}
