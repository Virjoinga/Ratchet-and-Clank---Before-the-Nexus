using UnityEngine;

public class RatchetProjectile : MonoBehaviour
{
	public float projSpeed;

	private Vector3 curVelocity;

	private float timeAlive;

	private float delayBounce;

	public Weapon weap;

	public EnemyController ecTarget;

	public int bounces;

	private GameObject buzzExplode;

	private void FixedUpdate()
	{
		if (timeAlive > 0f)
		{
			timeAlive -= Time.fixedDeltaTime;
			if (timeAlive <= 0f)
			{
				if (ecTarget != null && weap.weaponName == "BuzzBlades")
				{
					delayBounce = WeaponsManager.instance.delayTillBounce;
				}
				else
				{
					CleanUpProjectile();
				}
			}
			else if (weap.weaponName == "BuzzBlades" && ecTarget != null && !ecTarget.isDead && !ecTarget.shouldDespawn)
			{
				Vector3 zero = Vector3.zero;
				base.GetComponent<Rigidbody>().velocity = Vector3.Normalize(ecTarget.GetComponent<Rigidbody>().position - base.transform.position) * projSpeed;
				if (timeAlive < Vector3.Distance(ecTarget.GetComponent<Rigidbody>().position, base.transform.position) / projSpeed)
				{
					base.GetComponent<Rigidbody>().position = ecTarget.transform.position;
					base.GetComponent<Rigidbody>().velocity = Vector3.zero;
					timeAlive = 0f;
					delayBounce = WeaponsManager.instance.delayTillBounce;
				}
				else
				{
					zero.x = GameController.instance.playerController.curVelocityX;
					base.GetComponent<Rigidbody>().velocity = base.GetComponent<Rigidbody>().velocity + zero;
				}
			}
		}
		else if (weap.weaponName == "BuzzBlades" && delayBounce > 0f)
		{
			delayBounce -= Time.fixedDeltaTime;
			if (ecTarget != null && !ecTarget.isDead && !ecTarget.shouldDespawn)
			{
				base.transform.position = ecTarget.GetComponent<Rigidbody>().position;
			}
			else if (bounces > 0)
			{
				BuzzBounce();
			}
			else
			{
				CleanUpProjectile();
			}
			if (bounces > 0 && delayBounce <= 0f)
			{
				BuzzBounce();
			}
		}
		else
		{
			CleanUpProjectile();
		}
	}

	public void SetProjectileData(Vector3 target)
	{
		Vector3 zero = Vector3.zero;
		Vector3 position = GameController.instance.playerController.rightHand.position;
		position.x += 2f;
		if (weap.weaponName == "PredatorLauncher")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.predatorTimeToHit;
			timeAlive = WeaponsManager.instance.predatorTimeToHit + 0.1f;
		}
		else if (weap.weaponName == "RynoM")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.predatorTimeToHit;
			timeAlive = WeaponsManager.instance.predatorTimeToHit + 0.1f;
		}
		else if (weap.weaponName == "BuzzBlades")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.buzzBladeTimeToHit;
			timeAlive = WeaponsManager.instance.buzzBladeTimeToHit + 0.1f;
		}
		else
		{
			projSpeed = Vector3.Distance(target, position) / 0.1f;
			timeAlive = 0.2f;
		}
		base.transform.position = position;
		base.GetComponent<Rigidbody>().velocity = Vector3.Normalize(target - base.transform.position) * projSpeed;
		zero.x = GameController.instance.playerController.curVelocityX;
		base.GetComponent<Rigidbody>().velocity = base.GetComponent<Rigidbody>().velocity + zero;
		if (timeAlive > 15f)
		{
			timeAlive = 5f;
		}
		ParticleSystem componentInChildren = base.gameObject.GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.Clear(true);
		}
	}

	private void BuzzBounce()
	{
		ecTarget = EnemyManager.instance.GetNewBuzzBladeTarget(ecTarget);
		if (ecTarget != null)
		{
			bounces--;
			ecTarget.StartCoroutine("HitReact", weap.weaponName);
			ecTarget.StartCoroutine("DealDamageIn", weap);
			SetBounceData(ecTarget.GetComponent<Rigidbody>().position);
		}
		else
		{
			bounces = 0;
			CleanUpProjectile();
		}
	}

	public void SetBounceData(Vector3 target)
	{
		Vector3 zero = Vector3.zero;
		Vector3 position = base.transform.position;
		position.x += 2f;
		if (weap.weaponName == "PredatorLauncher")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.predatorTimeToHit;
			timeAlive = WeaponsManager.instance.predatorTimeToHit + 0.1f;
		}
		else if (weap.weaponName == "RynoM")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.predatorTimeToHit;
			timeAlive = WeaponsManager.instance.predatorTimeToHit + 0.1f;
		}
		else if (weap.weaponName == "BuzzBlades")
		{
			projSpeed = Vector3.Distance(target, position) / WeaponsManager.instance.buzzBladeTimeToHit;
			timeAlive = WeaponsManager.instance.buzzBladeTimeToHit + 0.1f;
		}
		else
		{
			projSpeed = Vector3.Distance(target, position) / 0.1f;
			timeAlive = 0.2f;
		}
		base.transform.position = position;
		base.GetComponent<Rigidbody>().velocity = Vector3.Normalize(target - base.transform.position) * projSpeed;
		zero.x = GameController.instance.playerController.curVelocityX;
		base.GetComponent<Rigidbody>().velocity = base.GetComponent<Rigidbody>().velocity + zero;
		if (timeAlive > 15f)
		{
			timeAlive = 5f;
		}
	}

	private void CleanUpProjectile()
	{
		if (weap.weaponName == "BuzzBlades" && weap.GetWeaponUpgradeLevel() == 3)
		{
			buzzExplode = GameObjectPool.instance.GetNextFree("FX_Explosion", true);
			if (buzzExplode != null)
			{
				buzzExplode.transform.position = base.GetComponent<Rigidbody>().position;
				buzzExplode.GetComponent<ParticleSystem>().Play(true);
			}
		}
		ParticleSystem componentInChildren = base.gameObject.GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.Clear(true);
		}
		Vector3 zero = Vector3.zero;
		zero.y = -100f;
		base.transform.position = zero;
		GameObjectPool.instance.SetFree(base.gameObject);
	}
}
