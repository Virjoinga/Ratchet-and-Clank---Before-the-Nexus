using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicComponent : MonoBehaviour
{
	public MusicManager.eMusicTrackType track = MusicManager.eMusicTrackType.None;

	public bool stopMusic;

	public bool forceReset;

	public float crossfadeTime = 2f;

	public bool filterByTag;

	public string tagFilter = string.Empty;

	public bool filterByLayers;

	public LayerMask layersFilter;

	private void OnTriggerEnter(Collider other)
	{
		if ((!filterByTag || other.CompareTag(tagFilter)) && (!filterByLayers || ((int)layersFilter & other.gameObject.layer) == 0))
		{
			Activate();
		}
	}

	public void Activate()
	{
		if (stopMusic)
		{
			StopMusic();
		}
		else
		{
			StartMusic();
		}
	}

	public void StopMusic()
	{
		MusicManager.instance.Stop();
	}

	public void StartMusic()
	{
		if (!GameController.instance.inMenu && MegaWeaponManager.instance.megaWeaponState != 0)
		{
			MusicManager.instance.Play(track, forceReset, crossfadeTime);
		}
	}
}
