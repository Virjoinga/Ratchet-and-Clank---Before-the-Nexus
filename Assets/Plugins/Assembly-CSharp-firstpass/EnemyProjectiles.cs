using UnityEngine;

public class EnemyProjectiles : MonoBehaviour
{
	private float timeAlive;

	public ParticleSystem particles;

	private GameObject projParticle;

	private float speed;

	public EnemyController ecOwner;

	private void FixedUpdate()
	{
		timeAlive -= Time.fixedDeltaTime;
		if (projParticle != null)
		{
			projParticle.transform.position = base.GetComponent<Rigidbody>().position;
		}
		if (timeAlive <= 0f)
		{
			if (projParticle != null)
			{
				GameObjectPool.instance.SetFree(projParticle);
				projParticle = null;
			}
			GameObjectPool.instance.SetFree(base.gameObject);
		}
		else if (ecOwner != null && ecOwner.isDead)
		{
			timeAlive = 0f;
			if (projParticle != null)
			{
				GameObjectPool.instance.SetFree(projParticle);
				projParticle = null;
			}
			GameObjectPool.instance.SetFree(base.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == GameController.instance.playerController.gameObject)
		{
			if (projParticle != null)
			{
				GameObjectPool.instance.SetFree(projParticle);
				projParticle = null;
			}
			GameObjectPool.instance.SetFree(base.gameObject);
		}
	}

	public void SetProjectileData(uint projDamage, float projSpeed, Vector3 enemyPos, int rail, Vector3 target)
	{
		Vector3 zero = Vector3.zero;
		speed = projSpeed;
		base.GetComponent<Rigidbody>().MovePosition(enemyPos);
		zero = target;
		float num = GameController.instance.playerController.GetComponent<Rigidbody>().velocity.x;
		if (num <= 1f)
		{
			num = 0.4f;
		}
		float f = (zero.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x) / num;
		f = Mathf.Abs(f);
		speed = Vector3.Distance(enemyPos, zero) / f;
		base.GetComponent<Rigidbody>().velocity = Vector3.Normalize(zero - enemyPos) * speed;
		timeAlive = 0.5f + Vector3.Distance(enemyPos, zero) / speed;
		if (timeAlive > 5f)
		{
			timeAlive = 5f;
		}
		if (particles != null)
		{
			projParticle = GameObjectPool.instance.GetNextFree(particles.name, true);
			projParticle.transform.position = base.GetComponent<Rigidbody>().position;
			projParticle.GetComponent<ParticleSystem>().Play();
		}
	}

	public void SetProjectileDataForBoss(uint projDamage, float projSpeed, Vector3 enemyPos, bool leftRail, Vector3 target, bool rightAttack)
	{
		Vector3 target2 = target;
		if (rightAttack)
		{
			target2.z -= 3.25f;
			if (leftRail)
			{
				SetProjectileData(projDamage, projSpeed, enemyPos, 0, target2);
			}
			else
			{
				SetProjectileData(projDamage, projSpeed, enemyPos, 1, target2);
			}
		}
		else
		{
			target2.z += 3.25f;
			if (leftRail)
			{
				SetProjectileData(projDamage, projSpeed, enemyPos, 2, target2);
			}
			else
			{
				SetProjectileData(projDamage, projSpeed, enemyPos, 0, target2);
			}
		}
	}

	private Vector3 SpreadOffset(Vector3 targetPos)
	{
		targetPos.x += Random.Range(-0.1f, 0.1f);
		targetPos.y += Random.Range(-0.2f, 0.2f);
		targetPos.z += Random.Range(-0.3f, 0.3f);
		return targetPos;
	}
}
