using System.Collections;
using UnityEngine;

public class CerulleanSwarmersController : EnemyController
{
	private int currentRail;

	public ParticleSystem spawnParticles;

	private GameObject chargeUpParticles;

	public float chargeupPlaybackSpeed = 1f;

	private bool bCanAttack;

	public override void Initialize()
	{
		base.Initialize();
		bCanAttack = false;
		StartCoroutine("SpawnSwarmer");
	}

	protected override void Start()
	{
		base.Start();
		bCanAttack = false;
		chargeUpParticles = base.gameObject.transform.Find("FX_SwarmerCharge").gameObject;
		base.GetComponent<Rigidbody>().useGravity = false;
	}

	private IEnumerator SpawnSwarmer()
	{
		GameObject particleObject = GameObjectPool.instance.GetNextFree(spawnParticles.name, true);
		Vector3 spawnPos = base.GetComponent<Rigidbody>().position;
		particleObject.transform.position = base.GetComponent<Rigidbody>().position;
		particleObject.GetComponent<ParticleSystem>().Play();
		spawnPos.y -= 10f;
		base.GetComponent<Rigidbody>().position = spawnPos;
		yield return new WaitForSeconds(1f);
		if (!InRangeOfPlayer())
		{
			spawnPos.y += 10f;
			base.GetComponent<Rigidbody>().MovePosition(spawnPos);
			bCanAttack = true;
		}
	}

	protected override bool UpdateTargetPosition(EnemyType type)
	{
		if (InRangeOfPlayer() && bCanAttack && base.GetComponent<Rigidbody>().position.z - 7f < GameController.instance.playerController.GetComponent<Rigidbody>().position.z && base.GetComponent<Rigidbody>().position.z + 7f > GameController.instance.playerController.GetComponent<Rigidbody>().position.z && base.GetComponent<Rigidbody>().position.x - 3f > GameController.instance.playerController.GetComponent<Rigidbody>().position.x)
		{
			targetPos = GameController.instance.playerController.GetComponent<Rigidbody>().position;
			StartCoroutine("BiteRatchet");
			return true;
		}
		return false;
	}

	protected override void MeleeStart()
	{
	}

	protected override void MoveEnemy()
	{
		if (InRangeOfPlayer())
		{
			Vector3 position = base.GetComponent<Rigidbody>().position;
			position.z = Mathf.Lerp(position.z, targetPos.z, Time.fixedDeltaTime * lerpSpeed);
			position.y = Mathf.Lerp(position.y, targetPos.y, Time.fixedDeltaTime * lerpSpeed);
			base.GetComponent<Rigidbody>().MovePosition(position);
			base.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(Vector3.zero));
		}
	}

	private IEnumerator BiteRatchet()
	{
		PlayChargeParticle();
		anim.SetBool("Attack", true);
		yield return new WaitForSeconds(0.2f);
		anim.SetBool("Attack", false);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void SpawnLocation(Vector3 nodePos)
	{
		if (onRail)
		{
			if (Random.Range(1, 100) > 50)
			{
				currentRail = 2;
			}
			else
			{
				currentRail = 1;
			}
			Vector3 zero = Vector3.zero;
			zero = TileSpawnManager.instance.getSpawnPosition(currentRail, nodePos.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x);
			base.GetComponent<Rigidbody>().MovePosition(zero);
		}
	}

	public override void SpawnMovement()
	{
		targetPos = base.GetComponent<Rigidbody>().position;
		targetPos.x = keepAtDistance;
	}

	public override void Death()
	{
		base.Death();
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.groundEnemiesKilled);
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.ceruleanSwarmerKilled);
	}

	protected override void PlayChargeParticle()
	{
		if (chargeUpParticles != null)
		{
			chargeUpParticles.GetComponent<ParticleSystem>().playbackSpeed = chargeupPlaybackSpeed;
			chargeUpParticles.GetComponent<ParticleSystem>().Play();
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == GameController.instance.playerController.gameObject)
		{
			GameController.instance.playerController.TakeDamage(meleeDamage);
		}
	}
}
