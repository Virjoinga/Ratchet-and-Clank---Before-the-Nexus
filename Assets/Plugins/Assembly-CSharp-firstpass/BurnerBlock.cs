using UnityEngine;

public class BurnerBlock : Block
{
	private GameObject fireEffect;

	public float onMaxPeriod = 1.8f;

	public float onMinPeriod = 1f;

	public float offMaxPeriod = 2.7f;

	public float offMinPeriod = 1.4f;

	public float fireChargeTime = 1.4f;

	private bool fireState = true;

	private bool fireChargeTell;

	private float fireTimer;

	public float burnerTellLifetime = 0.05f;

	private float onPeriod;

	private float offPeriod;

	private float oldLifetime;

	private void Start()
	{
		fireEffect = base.gameObject.transform.Find("FX_Burner").gameObject;
		onPeriod = Random.Range(onMinPeriod, onMaxPeriod);
		offPeriod = Random.Range(offMinPeriod, offMaxPeriod);
		fireTimer = onPeriod;
	}

	private void FixedUpdate()
	{
		if (fireTimer > 0f)
		{
			fireTimer -= Time.fixedDeltaTime;
			return;
		}
		if (fireChargeTell)
		{
			oldLifetime = fireEffect.GetComponent<ParticleSystem>().startLifetime;
			fireEffect.GetComponent<ParticleSystem>().startLifetime = burnerTellLifetime;
			fireEffect.GetComponent<ParticleSystem>().Play();
			fireEffect.GetComponent<ParticleSystem>().Clear();
			fireChargeTell = false;
			fireTimer = fireChargeTime;
			return;
		}
		fireState = !fireState;
		if (fireState)
		{
			fireEffect.GetComponent<ParticleSystem>().startLifetime = oldLifetime;
			fireTimer = onPeriod;
			base.gameObject.GetComponent<Collider>().isTrigger = false;
		}
		else
		{
			fireEffect.GetComponent<ParticleSystem>().Stop();
			fireEffect.GetComponent<ParticleSystem>().Clear();
			fireTimer = offPeriod;
			fireChargeTell = true;
			base.gameObject.GetComponent<Collider>().isTrigger = true;
		}
	}

	protected override void OnCollisionEnter(Collision other)
	{
		if (fireState)
		{
			base.OnCollisionEnter(other);
		}
	}
}
