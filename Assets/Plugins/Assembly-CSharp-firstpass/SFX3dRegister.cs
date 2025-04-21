using UnityEngine;

public class SFX3dRegister : MonoBehaviour
{
	private void Start()
	{
		AudioSource[] components = base.gameObject.GetComponents<AudioSource>();
		if (components == null)
		{
			return;
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				SFXManager.instance.Register3DSound(components[i]);
				SFXManager.instance.ModulatePitch(components[i], 0.8f, 1.2f, components[i].clip.length);
				if (components[i].clip.name == "amb_Engine")
				{
					components[i].volume = 0.35f;
				}
			}
		}
	}

	private void Update()
	{
		AudioSource[] components = base.gameObject.GetComponents<AudioSource>();
		if (components == null)
		{
			return;
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (GameController.instance.inMenu)
			{
				components[i].Stop();
			}
			if (!GameController.instance.isPaused && !components[i].isPlaying && !GameController.instance.inMenu)
			{
				components[i].Play();
			}
		}
	}
}
