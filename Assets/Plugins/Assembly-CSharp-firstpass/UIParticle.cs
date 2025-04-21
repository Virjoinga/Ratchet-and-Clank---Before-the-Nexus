using UnityEngine;

public class UIParticle : MonoBehaviour
{
	private double lastInterval;

	private ParticleSystem pSystem;

	private float deltaTime;

	private float age;

	public void Awake()
	{
		lastInterval = Time.realtimeSinceStartup;
		pSystem = base.gameObject.GetComponent<ParticleSystem>();
	}

	public void Play()
	{
		age = 0f;
		pSystem.time = 0f;
		lastInterval = Time.realtimeSinceStartup;
		if (Time.timeScale != 0f)
		{
			pSystem.Play(true);
		}
	}

	public void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		deltaTime = realtimeSinceStartup - (float)lastInterval;
		age += deltaTime;
		lastInterval = realtimeSinceStartup;
		if (Time.timeScale == 0f)
		{
			pSystem.Simulate(deltaTime, true, false);
			pSystem.Play(true);
		}
	}
}
