using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	public enum EnemyType
	{
		ground = 0,
		air = 1,
		hybrid = 2
	}

	public enum EnemyLoot
	{
		none = 0,
		small = 1,
		normal = 2,
		huge = 3,
		gigantic = 4,
		epic = 5
	}

	private class DOTEffect
	{
		public uint dotDamage;

		public float dotDuration;
	}

	public static Dictionary<string, cmlData> cachedEnenmyData = new Dictionary<string, cmlData>();

	public EnemyType etEnemyState;

	public uint HP;

	public uint MaxHP;

	public uint meleeDamage;

	public float meleeRate;

	public float meleeBuildUpSpeed;

	public uint meleeChargeDistance;

	public float accuracy;

	public uint fireDamage;

	public float fireRate;

	public float fireBuildUpSpeed;

	public uint keepAtDistance;

	public float projectileSpeed;

	public uint armor;

	public float dodgeCooldown;

	public float dodgeChance;

	public uint lifeSpan;

	public float meleeChance;

	public bool isSwarm;

	public EnemyLoot elLootType;

	public float minPosDelay;

	public float maxPosDelay;

	public bool onRail;

	public float lerpSpeed;

	public bool theBoss;

	public float enemyScale;

	public float meleeSpeed;

	public float minZ;

	public float maxZ;

	public float maxHeight;

	public float minHeight;

	protected float curMeleeTimer;

	protected float curFireTimer;

	protected float curDodgeTimer;

	protected float curLifeTime;

	protected float curMeleeChance;

	public bool isDead;

	public bool shouldDespawn;

	public Vector3 MinPos;

	public Vector3 MaxPos;

	protected Vector3 targetPos;

	protected float currentPosDelay;

	protected bool moved;

	protected bool meleeDash;

	protected Animator anim;

	protected AnimatorStateInfo currentBaseState;

	protected MegaWeaponManager.eMegaWeapons curMegaWeaponState = MegaWeaponManager.eMegaWeapons.mw_NONE;

	protected int IdleState = Animator.StringToHash("Base Layer.Idle");

	protected int HitState = Animator.StringToHash("Base Layer.Hit");

	protected int AttackState = Animator.StringToHash("Base Layer.Attack");

	protected int GrooveState = Animator.StringToHash("Base Layer.Groove");

	protected int TornadoState = Animator.StringToHash("Base Layer.Tornado");

	protected int RiftState = Animator.StringToHash("Base Layer.Rifted");

	private ParticleSystem particleOnHit;

	public ParticleSystem particleOnDeath;

	public ParticleSystem projectileParticles;

	public ParticleSystem particleGroundTarget;

	protected GameObject curParticleGroundTarget;

	protected GameObject curLaserSight;

	public AnimationCurve EnemySpeedOverDistance;

	protected Vector3 beginMovePos;

	private string soundOnHit;

	protected string weaponHitBy;

	protected int railToAttack;

	public bool atkRatchetRail;

	protected GameObject laserOriginObject;

	protected Color laserColor = Color.red;

	private FlashingTint flashEffect;

	public ParticleSystem lockOnEffect;

	protected GameObject curLockOnEffect;

	private List<DOTEffect> dots;

	public virtual void Initialize()
	{
		curMegaWeaponState = MegaWeaponManager.eMegaWeapons.mw_NONE;
		isDead = false;
		shouldDespawn = false;
		curLifeTime = lifeSpan;
		atkRatchetRail = false;
		base.GetComponent<Rigidbody>().useGravity = false;
		meleeDash = false;
		if (dots != null)
		{
			dots.Clear();
		}
	}

	protected virtual void Awake()
	{
		anim = GetComponent<Animator>();
	}

	protected virtual void Start()
	{
		curMeleeTimer = meleeRate;
		curFireTimer = fireRate;
		curDodgeTimer = 0f;
		curLifeTime = lifeSpan;
		curMeleeChance = Random.Range(1, 100);
		currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
		base.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		dots = new List<DOTEffect>();
		flashEffect = GetComponent<FlashingTint>();
	}

	protected bool InRangeOfPlayer()
	{
		if (base.GetComponent<Rigidbody>().position.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x <= (float)keepAtDistance)
		{
			return true;
		}
		return false;
	}

	protected virtual bool UpdateTargetPosition(EnemyType type)
	{
		if (InRangeOfPlayer() && currentPosDelay <= 0f)
		{
			targetPos.z = Random.Range(minZ, maxZ);
			if (type == EnemyType.air || type == EnemyType.hybrid)
			{
				targetPos.y = Random.Range(minHeight, maxHeight);
			}
			currentPosDelay = minPosDelay + Random.value * (maxPosDelay - minPosDelay);
			beginMovePos = base.GetComponent<Rigidbody>().position;
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (curLifeTime > 0f && !shouldDespawn)
		{
			curLifeTime -= Time.deltaTime;
			currentPosDelay -= Time.deltaTime;
			if (curDodgeTimer > 0f)
			{
				curDodgeTimer -= Time.deltaTime;
			}
			if (((currentBaseState.nameHash == IdleState && !anim.GetBool("ChargeUp") && !anim.GetBool("Attack") && !meleeDash) || theBoss) && !anim.IsInTransition(0) && UpdateTargetPosition(etEnemyState))
			{
				StartCoroutine("TiltEnemy", targetPos.z - base.GetComponent<Rigidbody>().position.z);
			}
			if (curMegaWeaponState == MegaWeaponManager.eMegaWeapons.mw_NONE)
			{
				HandleAttackChance();
			}
		}
		else
		{
			if (curParticleGroundTarget != null)
			{
				curParticleGroundTarget.transform.position = Vector3.zero;
				curParticleGroundTarget.GetComponent<ParticleSystem>().Stop();
				curParticleGroundTarget = null;
			}
			StopLaserSight();
			shouldDespawn = true;
		}
		if (curLockOnEffect != null)
		{
			curLockOnEffect.transform.position = base.GetComponent<Rigidbody>().position;
		}
	}

	protected virtual void HandleAttackChance()
	{
		if (meleeChance != 0f)
		{
			if (InRangeOfPlayer())
			{
				curMeleeTimer -= Time.deltaTime;
			}
			if (curMeleeTimer <= 0f)
			{
				if (curMeleeChance <= meleeChance * 100f)
				{
					MeleeStart();
				}
				else
				{
					curMeleeTimer = meleeRate;
				}
				curMeleeChance = Random.Range(1, 100);
			}
		}
		else
		{
			if (InRangeOfPlayer() && base.GetComponent<Rigidbody>().position.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x > 0f)
			{
				curFireTimer -= Time.deltaTime;
			}
			if (curFireTimer <= 0f && currentBaseState.nameHash == IdleState && !anim.IsInTransition(0))
			{
				StartCoroutine("BeginAttack");
			}
		}
	}

	protected virtual void MeleeStart()
	{
		if (currentBaseState.nameHash == IdleState && !anim.GetBool("ChargeUp") && !anim.GetBool("Attack") && !meleeDash && !anim.IsInTransition(0))
		{
			StartCoroutine("BeginMelee");
		}
	}

	protected void SetRailToFireAt()
	{
		if (atkRatchetRail || !EnemyManager.instance.shouldHavePrimaryEnemy)
		{
			railToAttack = GameController.instance.playerController.currentRail;
		}
		else
		{
			railToAttack = Random.Range(0, 2);
		}
	}

	protected virtual void SetGroundTargetEffect()
	{
		Vector3 zero = Vector3.zero;
		float num = 0f + fireBuildUpSpeed + (base.GetComponent<Rigidbody>().position.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x) / projectileSpeed;
		float xOffset = GameController.instance.playerController.GetComponent<Rigidbody>().position.x + GameController.instance.playerController.GetComponent<Rigidbody>().velocity.x * num;
		zero = TileSpawnManager.instance.getRailNodePosition(railToAttack, xOffset, TileSpawnManager.instance.railTileList[0]);
		if (particleGroundTarget != null)
		{
			curParticleGroundTarget = GameObjectPool.instance.GetNextFree(particleGroundTarget.name, true);
			curParticleGroundTarget.transform.position = zero;
			curParticleGroundTarget.GetComponent<ParticleSystem>().Play();
		}
		StartLaserSight();
	}

	protected void StartLaserSight()
	{
		if (curLaserSight == null)
		{
			curLaserSight = GameObjectPool.instance.GetNextFree("FX_LaserMesh", true);
		}
		else
		{
			StopLaserSight();
			curLaserSight = GameObjectPool.instance.GetNextFree("FX_LaserMesh", true);
		}
		curLaserSight.transform.Find("Mesh").GetComponent<Renderer>().material.color = laserColor;
		curLaserSight.transform.Find("Mesh").Find("Mesh2").GetComponent<Renderer>().material.color = laserColor;
		curLaserSight.transform.Find("Mesh").Find("Mesh3").GetComponent<Renderer>().material.color = laserColor;
		curLaserSight.transform.Find("Mesh").Find("Mesh4").GetComponent<Renderer>().material.color = laserColor;
		curLaserSight.transform.position = laserOriginObject.transform.position;
		curLaserSight.transform.LookAt(curParticleGroundTarget.transform.position);
	}

	protected void UpdateLaserSight()
	{
		if (curParticleGroundTarget != null && GameController.instance.playerController.GetComponent<Rigidbody>().position.x > curParticleGroundTarget.transform.position.x)
		{
			curParticleGroundTarget.transform.position = Vector3.zero;
			curParticleGroundTarget.GetComponent<ParticleSystem>().Stop();
			curParticleGroundTarget = null;
			StopLaserSight();
		}
		if (curLaserSight != null && curParticleGroundTarget != null)
		{
			curLaserSight.transform.position = laserOriginObject.transform.position;
			curLaserSight.transform.LookAt(curParticleGroundTarget.transform.position);
			Vector3 zero = Vector3.zero;
			zero.z = 1f;
			zero.y = 1f;
			zero.x = Vector3.Distance(curLaserSight.transform.position, curParticleGroundTarget.transform.position);
			curLaserSight.transform.Find("Mesh").transform.localScale = zero;
		}
	}

	protected void StopLaserSight()
	{
		if (curLaserSight != null)
		{
			GameObjectPool.instance.SetFree(curLaserSight);
			curLaserSight = null;
		}
	}

	protected IEnumerator BeginAttack()
	{
		SetRailToFireAt();
		SetGroundTargetEffect();
		curFireTimer = fireRate;
		anim.SetBool("ChargeUp", true);
		PlayChargeParticle();
		yield return new WaitForSeconds(fireBuildUpSpeed);
		anim.SetBool("ChargeUp", false);
		anim.SetBool("Attack", true);
		yield return new WaitForSeconds(0.2f);
		anim.SetBool("Attack", false);
		if (!isDead && !shouldDespawn)
		{
			FireProjectile();
		}
	}

	protected IEnumerator BeginMelee()
	{
		curMeleeTimer = meleeBuildUpSpeed;
		PlayChargeParticle();
		anim.SetBool("ChargeUp", true);
		anim.SetBool("Attack", false);
		yield return new WaitForSeconds(meleeBuildUpSpeed);
		if (curMegaWeaponState != MegaWeaponManager.eMegaWeapons.mw_NONE)
		{
			meleeDash = false;
			yield break;
		}
		anim.SetBool("ChargeUp", false);
		anim.SetBool("Attack", true);
		Vector3 originalPos = targetPos;
		targetPos.x = 2f;
		targetPos.z = GameController.instance.playerController.GetComponent<Rigidbody>().position.z;
		meleeDash = true;
		yield return new WaitForSeconds(meleeSpeed);
		if (curMegaWeaponState != MegaWeaponManager.eMegaWeapons.mw_NONE)
		{
			meleeDash = false;
			targetPos = originalPos;
			yield break;
		}
		anim.SetBool("Attack", false);
		originalPos.z += TileSpawnManager.instance.getRailNodePosition(0, base.GetComponent<Rigidbody>().position.x, TileSpawnManager.instance.railTileList[0]).z;
		targetPos = originalPos;
		yield return new WaitForSeconds(meleeSpeed);
		meleeDash = false;
		curMeleeTimer = meleeRate;
	}

	protected virtual void FixedUpdate()
	{
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		if (isDead)
		{
			if (base.GetComponent<Rigidbody>().useGravity)
			{
			}
			return;
		}
		foreach (DOTEffect dot in dots)
		{
			if ((int)dot.dotDuration > (int)(dot.dotDuration - Time.fixedDeltaTime))
			{
				TakeDamage(dot.dotDamage);
			}
			dot.dotDuration -= Time.fixedDeltaTime;
			if (dot.dotDuration <= 0f)
			{
				dots.Remove(dot);
				break;
			}
		}
		UpdateLaserSight();
		if (curMegaWeaponState != MegaWeaponManager.eMegaWeapons.mw_NONE)
		{
			if (currentBaseState.nameHash == GrooveState || currentBaseState.nameHash == RiftState || currentBaseState.nameHash == TornadoState)
			{
				anim.SetBool("MegaWeaponStart", false);
			}
			MegaWeaponMovement();
		}
		else
		{
			MoveEnemy();
		}
	}

	protected virtual void MegaWeaponMovement()
	{
		switch (curMegaWeaponState)
		{
		case MegaWeaponManager.eMegaWeapons.mw_NONE:
			break;
		case MegaWeaponManager.eMegaWeapons.mw_Groovitron:
			GroovitronMoveEffect();
			break;
		case MegaWeaponManager.eMegaWeapons.mw_RiftInducer:
			RiftInducerMoveEffect();
			break;
		case MegaWeaponManager.eMegaWeapons.mw_Tornado:
			GroovitronMoveEffect();
			break;
		case MegaWeaponManager.eMegaWeapons.mw_MAX:
			break;
		}
	}

	public virtual void Groove()
	{
		anim.SetBool("Groove", true);
		anim.SetBool("MegaWeaponStart", true);
		curMegaWeaponState = MegaWeaponManager.eMegaWeapons.mw_Groovitron;
	}

	public virtual void Rift()
	{
		anim.SetBool("Rift", true);
		anim.SetBool("MegaWeaponStart", true);
		curMegaWeaponState = MegaWeaponManager.eMegaWeapons.mw_RiftInducer;
	}

	public virtual void Tornado()
	{
		anim.SetBool("Tornado", true);
		anim.SetBool("MegaWeaponStart", true);
		curMegaWeaponState = MegaWeaponManager.eMegaWeapons.mw_Tornado;
	}

	protected virtual void GroovitronMoveEffect()
	{
		Vector3 position = base.GetComponent<Rigidbody>().position;
		Vector3 vector = targetPos;
		float num = GameController.instance.playerController.curVelocityX * 1.1f;
		if (num == 0f)
		{
			return;
		}
		if (targetPos.y <= 0f)
		{
			targetPos.y = 0.5f;
			vector.y = 0.5f;
		}
		vector.x += GameController.instance.playerController.GetComponent<Rigidbody>().position.x;
		Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.GetComponent<Rigidbody>().position.x, TileSpawnManager.instance.railTileList[0]);
		vector.y += railNodePosition.y;
		vector.z += railNodePosition.z;
		float num2 = ((vector.y - base.GetComponent<Rigidbody>().position.y != 0f) ? (Mathf.Abs(Mathf.Abs(vector.y - beginMovePos.y) - Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) / Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) : ((vector.z - base.GetComponent<Rigidbody>().position.z != 0f) ? (Mathf.Abs(Mathf.Abs(vector.z - beginMovePos.z) - Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) / Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) : 1f));
		if (num2 < 0f || num2 > 1f)
		{
			num2 = 1f;
		}
		float num3 = Vector3.Distance(vector, base.GetComponent<Rigidbody>().position) / (EnemySpeedOverDistance.Evaluate(num2) * num);
		if (num3 <= Time.fixedDeltaTime)
		{
			position = vector;
			num3 = 1f;
		}
		if (!shouldDespawn)
		{
			if (InRangeOfPlayer())
			{
				position.x = vector.x;
			}
			if (etEnemyState == EnemyType.air || etEnemyState == EnemyType.hybrid)
			{
				position.y = Mathf.Lerp(position.y, vector.y, Time.fixedDeltaTime / num3);
			}
			else
			{
				position.y = vector.y;
			}
			position.z = Mathf.Lerp(position.z, vector.z, Time.fixedDeltaTime / num3);
		}
		base.GetComponent<Rigidbody>().MovePosition(position);
	}

	protected virtual void TornadoMoveEffect()
	{
		Vector3 effectPosition = MegaWeaponManager.instance.GetEffectPosition();
		Vector3 position = base.GetComponent<Rigidbody>().position;
		float num = GameController.instance.playerController.curVelocityX * 1.1f;
		if (num == 0f)
		{
			return;
		}
		effectPosition.x += GameController.instance.playerController.GetComponent<Rigidbody>().position.x;
		Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.GetComponent<Rigidbody>().position.x, TileSpawnManager.instance.railTileList[0]);
		if (effectPosition.y < railNodePosition.y)
		{
			effectPosition.y = railNodePosition.y + 1f;
		}
		else
		{
			effectPosition.y += railNodePosition.y;
		}
		effectPosition.z += railNodePosition.z;
		float num2 = ((effectPosition.y - base.GetComponent<Rigidbody>().position.y != 0f) ? (Mathf.Abs(Mathf.Abs(effectPosition.y - beginMovePos.y) - Mathf.Abs(effectPosition.y - base.GetComponent<Rigidbody>().position.y)) / Mathf.Abs(effectPosition.y - base.GetComponent<Rigidbody>().position.y)) : ((effectPosition.z - base.GetComponent<Rigidbody>().position.z != 0f) ? (Mathf.Abs(Mathf.Abs(effectPosition.z - beginMovePos.z) - Mathf.Abs(effectPosition.z - base.GetComponent<Rigidbody>().position.z)) / Mathf.Abs(effectPosition.z - base.GetComponent<Rigidbody>().position.z)) : 1f));
		if (num2 < 0f || num2 > 1f)
		{
			num2 = 1f;
		}
		float num3 = Vector3.Distance(effectPosition, base.GetComponent<Rigidbody>().position) / (EnemySpeedOverDistance.Evaluate(num2) * num);
		if (num3 <= Time.fixedDeltaTime)
		{
			position = effectPosition;
			num3 = 1f;
		}
		if (!shouldDespawn)
		{
			position.x = effectPosition.x;
			if (etEnemyState == EnemyType.air || etEnemyState == EnemyType.hybrid)
			{
				position.y = Mathf.Lerp(position.y, effectPosition.y, Time.fixedDeltaTime / num3);
			}
			else
			{
				position.y = effectPosition.y;
			}
			position.z = Mathf.Lerp(position.z, effectPosition.z, Time.fixedDeltaTime / num3);
		}
		if (base.GetComponent<Rigidbody>().position.y < railNodePosition.y)
		{
			position.y = railNodePosition.y;
		}
		base.GetComponent<Rigidbody>().MovePosition(position);
	}

	protected virtual void RiftInducerMoveEffect()
	{
		Vector3 position = base.GetComponent<Rigidbody>().position;
		Vector3 vector = targetPos;
		float num = GameController.instance.playerController.curVelocityX * 1.1f;
		if (num == 0f)
		{
			return;
		}
		targetPos.y += 0.5f;
		vector = targetPos;
		vector.x += GameController.instance.playerController.GetComponent<Rigidbody>().position.x;
		Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.GetComponent<Rigidbody>().position.x, TileSpawnManager.instance.railTileList[0]);
		vector.y += railNodePosition.y;
		vector.z += railNodePosition.z;
		float num2 = ((vector.y - base.GetComponent<Rigidbody>().position.y != 0f) ? (Mathf.Abs(Mathf.Abs(vector.y - beginMovePos.y) - Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) / Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) : ((vector.z - base.GetComponent<Rigidbody>().position.z != 0f) ? (Mathf.Abs(Mathf.Abs(vector.z - beginMovePos.z) - Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) / Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) : 1f));
		if (num2 < 0f || num2 > 1f)
		{
			num2 = 1f;
		}
		float num3 = Vector3.Distance(vector, base.GetComponent<Rigidbody>().position) / (EnemySpeedOverDistance.Evaluate(num2) * num);
		if (num3 <= Time.fixedDeltaTime)
		{
			position = vector;
			num3 = 1f;
		}
		if (!shouldDespawn)
		{
			if (InRangeOfPlayer())
			{
				position.x = vector.x;
			}
			if (etEnemyState == EnemyType.air || etEnemyState == EnemyType.hybrid)
			{
				position.y = Mathf.Lerp(position.y, vector.y, Time.fixedDeltaTime / num3);
			}
			else
			{
				position.y = vector.y;
			}
			position.z = Mathf.Lerp(position.z, vector.z, Time.fixedDeltaTime / num3);
		}
		base.GetComponent<Rigidbody>().MovePosition(position);
	}

	protected virtual void MoveEnemy()
	{
		if (!InRangeOfPlayer())
		{
			return;
		}
		Vector3 position = base.GetComponent<Rigidbody>().position;
		Vector3 vector = targetPos;
		float num = GameController.instance.playerController.curVelocityX * 1.1f;
		if (num == 0f)
		{
			return;
		}
		vector.x += GameController.instance.playerController.GetComponent<Rigidbody>().position.x;
		Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.GetComponent<Rigidbody>().position.x, TileSpawnManager.instance.railTileList[0]);
		vector.y += railNodePosition.y;
		if (!meleeDash)
		{
			vector.z += railNodePosition.z;
		}
		float num2 = ((vector.y - base.GetComponent<Rigidbody>().position.y != 0f) ? (Mathf.Abs(Mathf.Abs(vector.y - beginMovePos.y) - Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) / Mathf.Abs(vector.y - base.GetComponent<Rigidbody>().position.y)) : ((vector.z - base.GetComponent<Rigidbody>().position.z != 0f) ? (Mathf.Abs(Mathf.Abs(vector.z - beginMovePos.z) - Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) / Mathf.Abs(vector.z - base.GetComponent<Rigidbody>().position.z)) : 1f));
		if (num2 < 0f || num2 > 1f)
		{
			num2 = 1f;
		}
		float num3 = Vector3.Distance(vector, base.GetComponent<Rigidbody>().position) / (EnemySpeedOverDistance.Evaluate(num2) * num);
		if (num3 <= Time.fixedDeltaTime)
		{
			position = vector;
			num3 = 1f;
		}
		if (!shouldDespawn && !GadgetManager.instance.GetJetpackStatus())
		{
			if (meleeDash)
			{
				num += (float)keepAtDistance / meleeSpeed;
				num3 = Mathf.Abs(base.GetComponent<Rigidbody>().position.x - vector.x) / num;
				if (num3 <= Time.fixedDeltaTime)
				{
					position.x = vector.x;
				}
				else
				{
					position.x = Mathf.Lerp(position.x, vector.x, Time.fixedDeltaTime / num3);
				}
			}
			else
			{
				position.x = vector.x;
			}
			if (etEnemyState == EnemyType.air || etEnemyState == EnemyType.hybrid)
			{
				position.y = Mathf.Lerp(position.y, vector.y, Time.fixedDeltaTime / num3);
			}
			else
			{
				position.y = vector.y;
			}
			position.z = Mathf.Lerp(position.z, vector.z, Time.fixedDeltaTime / num3);
		}
		base.GetComponent<Rigidbody>().MovePosition(position);
	}

	protected bool MeleeRangeCheck()
	{
		if (Vector3.Distance(base.GetComponent<Rigidbody>().position, GameController.instance.playerController.GetComponent<Rigidbody>().position) <= 5f)
		{
			return true;
		}
		return false;
	}

	protected IEnumerator TiltEnemy(float z)
	{
		if (z > 0f)
		{
			anim.SetBool("MoveRight", true);
		}
		else if (z < 0f)
		{
			anim.SetBool("MoveLeft", true);
		}
		yield return new WaitForSeconds(minPosDelay / 2f);
		if (z > 0f)
		{
			anim.SetBool("MoveRight", false);
		}
		else if (z < 0f)
		{
			anim.SetBool("MoveLeft", false);
		}
	}

	protected virtual void FireProjectile()
	{
		if (curParticleGroundTarget != null && laserOriginObject != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree("BulletThing");
			nextFree.GetComponent<EnemyProjectiles>().particles = projectileParticles;
			nextFree.GetComponent<EnemyProjectiles>().ecOwner = this;
			nextFree.GetComponent<EnemyProjectiles>().SetProjectileData(fireDamage, projectileSpeed, laserOriginObject.transform.position, railToAttack, curParticleGroundTarget.transform.position);
		}
	}

	public void ClearWeaponHitBy()
	{
		weaponHitBy = null;
	}

	public void EnemyTapped(Weapon weap)
	{
		if (isDead)
		{
			return;
		}
		weaponHitBy = weap.weaponName;
		soundOnHit = weap.soundOnHit;
		particleOnHit = weap.particleOnHit;
		if (weap.AOE != 0)
		{
			List<GameObject> list;
			if (weap.spreadShot)
			{
				list = EnemyManager.instance.WithinRadius(base.GetComponent<Rigidbody>().position, weap.AOE);
				{
					foreach (GameObject item in list)
					{
						item.GetComponent<EnemyController>().particleOnHit = weap.particleOnHit;
						item.GetComponent<EnemyController>().soundOnHit = weap.soundOnHit;
						item.GetComponent<EnemyController>().CheckHit(weap);
					}
					return;
				}
			}
			if (!CheckHit(weap))
			{
				return;
			}
			list = EnemyManager.instance.WithinRadius(base.GetComponent<Rigidbody>().position, weap.AOE);
			foreach (GameObject item2 in list)
			{
				if (item2.GetComponent<EnemyController>() != this)
				{
					item2.GetComponent<EnemyController>().particleOnHit = weap.particleOnHit;
					item2.GetComponent<EnemyController>().soundOnHit = weap.soundOnHit;
					item2.GetComponent<EnemyController>().StartCoroutine("HitReact", weap.weaponName);
					item2.GetComponent<EnemyController>().StartCoroutine("DealDamageIn", weap);
				}
			}
			GameObject[] array = GameObject.FindGameObjectsWithTag("Crates");
			foreach (GameObject gameObject in array)
			{
				if (Vector3.Distance(gameObject.transform.position, base.GetComponent<Rigidbody>().position) < (float)weap.AOE)
				{
					float num = 0.05f;
					num = GetBulletTime(weap.weaponName);
					gameObject.GetComponent<Crate>().StartCoroutine("CrateDestroy", num);
				}
			}
		}
		else
		{
			CheckHit(weap);
		}
	}

	public bool CheckHit(Weapon weap)
	{
		if (currentBaseState.nameHash == IdleState && !anim.IsInTransition(0) && curDodgeTimer <= 0f && (float)Random.Range(1, 100) <= dodgeChance * 100f)
		{
			curDodgeTimer = dodgeCooldown;
			StartCoroutine("DodgeEnemy", weap.weaponName);
			return false;
		}
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalShotsHit);
		switch (weap.weaponName)
		{
		case "ConstructoPistol":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolShotsHit);
			break;
		case "ConstructoShotgun":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.shotgunShotsHit);
			break;
		case "BuzzBlades":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesShotsHit);
			break;
		case "PredatorLauncher":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.predatorShotsHit);
			break;
		case "RynoM":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoShotsHit);
			break;
		default:
			Debug.LogError("Weapon: Unknown weapon");
			break;
		}
		if (weap.dotDamagePerSecond != 0)
		{
			DOTEffect dOTEffect = new DOTEffect();
			dOTEffect.dotDamage = weap.dotDamagePerSecond;
			dOTEffect.dotDuration = weap.dotDuration;
			dots.Add(dOTEffect);
		}
		StartCoroutine("HitReact", weap.weaponName);
		StartCoroutine("RatchetShoot", weap);
		return true;
	}

	protected IEnumerator RatchetShoot(Weapon weap)
	{
		int damage3 = 0;
		float timeUntilHit2 = 0.05f;
		damage3 = ((!(weap.weaponName == "ConstructoShotgun") || WeaponsManager.instance.shotgunDamageFalloffPerUnit == null || WeaponsManager.instance.shotgunDamageFalloffPerUnit.Length <= weap.GetWeaponUpgradeLevel() - 1) ? ((int)weap.damage) : ((int)((float)weap.damage - Vector3.Distance(base.GetComponent<Rigidbody>().position, GameController.instance.playerController.rightHand.position) * WeaponsManager.instance.shotgunDamageFalloffPerUnit[weap.GetWeaponUpgradeLevel() - 1])));
		damage3 = ((damage3 >= armor) ? (damage3 - (int)armor) : 0);
		timeUntilHit2 = GetBulletTime(weap.weaponName);
		GameController.instance.playerController.myWeap.FireProjectile(base.GetComponent<Rigidbody>().position, this);
		yield return new WaitForSeconds(timeUntilHit2);
		TakeDamage((uint)damage3);
	}

	public void TakeDamage(uint damage)
	{
		if (soundOnHit != null)
		{
			SFXManager.instance.PlaySound(soundOnHit);
		}
		soundOnHit = null;
		if (damage > HP)
		{
			HP = 0u;
		}
		else
		{
			HP -= damage;
		}
		if (HP == 0)
		{
			Death();
		}
		else if (theBoss)
		{
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				hUD.UpdateBossHealth(HP, MaxHP);
			}
		}
	}

	public virtual void Death()
	{
		if (curParticleGroundTarget != null)
		{
			curParticleGroundTarget.transform.position = Vector3.zero;
			curParticleGroundTarget.GetComponent<ParticleSystem>().Stop();
			curParticleGroundTarget = null;
		}
		StopLaserSight();
		LockOnStop();
		PlayDeathParticle();
		isDead = true;
		if (curMegaWeaponState == MegaWeaponManager.eMegaWeapons.mw_RiftInducer)
		{
			PickupManager.instance.SpawnExplosionOfBolts(MegaWeaponManager.instance.curMegaObject.transform.position);
		}
		else
		{
			PickupManager.instance.SpawnExplosionOfBolts(base.GetComponent<Rigidbody>().position);
		}
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.totalEnemiesKilled);
		if (GadgetManager.instance.GetJetpackStatus())
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetPackCurrentKills);
		}
		if (MegaWeaponManager.instance.groovitronOn)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.dancingEnemiesKilled);
		}
		switch (weaponHitBy)
		{
		case "ConstructoPistol":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.pistolKills);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.oneShotPistolKills);
			break;
		case "ConstructoShotgun":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.shotgunKills);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.oneShotShotgunKills);
			break;
		case "BuzzBlades":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.buzzbladesKills);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.oneShotBuzzbladeKills);
			break;
		case "PredatorLauncher":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.predatorKills);
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.oneShotPredatorKills);
			break;
		case "RynoM":
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.rynoKills);
			break;
		default:
			Debug.Log("Enemy killed by unknown weapon: " + weaponHitBy);
			break;
		}
	}

	private void PlayHitParticle()
	{
		if (particleOnHit != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(particleOnHit.name, true);
			nextFree.transform.position = base.GetComponent<Rigidbody>().position;
			nextFree.GetComponent<ParticleSystem>().Play();
		}
	}

	protected virtual void PlayChargeParticle()
	{
		Debug.LogWarning("No Charge Particle Set!");
	}

	protected void PlayDeathParticle()
	{
		if (particleOnDeath != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(particleOnDeath.name, true);
			nextFree.transform.position = base.GetComponent<Rigidbody>().position;
			nextFree.GetComponent<ParticleSystem>().Play();
		}
	}

	protected void ParseEnemyData(cmlData Enemy)
	{
		etEnemyState = (EnemyType)int.Parse(Enemy["etEnemyState"]);
		HP = uint.Parse(Enemy["HP"]);
		MaxHP = HP;
		meleeDamage = uint.Parse(Enemy["meleeDamage"]);
		meleeRate = float.Parse(Enemy["meleeRate"]);
		meleeBuildUpSpeed = float.Parse(Enemy["meleeBuildUpSpeed"]);
		meleeChargeDistance = uint.Parse(Enemy["meleeChargeDistance"]);
		keepAtDistance = uint.Parse(Enemy["keepAtDistance"]);
		projectileSpeed = float.Parse(Enemy["projectileSpeed"]);
		fireDamage = uint.Parse(Enemy["fireDamage"]);
		fireRate = float.Parse(Enemy["fireRate"]);
		accuracy = float.Parse(Enemy["accuracy"]);
		fireBuildUpSpeed = float.Parse(Enemy["fireBuildUpSpeed"]);
		armor = uint.Parse(Enemy["armor"]);
		dodgeCooldown = float.Parse(Enemy["dodgeCooldown"]);
		dodgeChance = float.Parse(Enemy["dodgeChance"]);
		lifeSpan = uint.Parse(Enemy["lifeSpan"]);
		meleeChance = float.Parse(Enemy["meleeChance"]);
		isSwarm = bool.Parse(Enemy["isSwarm"]);
		elLootType = (EnemyLoot)int.Parse(Enemy["elLootType"]);
		minPosDelay = float.Parse(Enemy["minPosDelay"]);
		maxPosDelay = float.Parse(Enemy["maxPosDelay"]);
		onRail = bool.Parse(Enemy["onRail"]);
		lerpSpeed = float.Parse(Enemy["lerpSpeed"]);
		theBoss = bool.Parse(Enemy["theBoss"]);
		enemyScale = float.Parse(Enemy["enemyScale"]);
		meleeSpeed = float.Parse(Enemy["meleeSpeed"]);
	}

	public void LoadEnemyData(string EnemyName)
	{
		if (cachedEnenmyData.ContainsKey(EnemyName))
		{
			ParseEnemyData(cachedEnenmyData[EnemyName]);
		}
		cmlReader cmlReader2 = new cmlReader("Data/EnemyData");
		if (cmlReader2 == null)
		{
			return;
		}
		List<cmlData> list = cmlReader2.Children();
		foreach (cmlData item in list)
		{
			if (item["Name"] == EnemyName)
			{
				ParseEnemyData(item);
				break;
			}
		}
	}

	public IEnumerator HitReact(string weapName)
	{
		float timeUntilHit2 = 0.05f;
		timeUntilHit2 = GetBulletTime(weapName);
		yield return new WaitForSeconds(timeUntilHit2);
		anim.SetBool("HitReact", true);
		if (flashEffect != null)
		{
			base.gameObject.SendMessage("FlashTint", this, SendMessageOptions.DontRequireReceiver);
		}
		PlayHitParticle();
		yield return new WaitForSeconds(0.13f);
		anim.SetBool("HitReact", false);
	}

	public IEnumerator DealDamageIn(Weapon weap)
	{
		int damage3 = 0;
		float timeUntilHit2 = 0.05f;
		damage3 = ((!(weap.weaponName == "ConstructoShotgun") || WeaponsManager.instance.shotgunDamageFalloffPerUnit == null || WeaponsManager.instance.shotgunDamageFalloffPerUnit.Length <= weap.GetWeaponUpgradeLevel() - 1) ? ((int)weap.damage) : ((int)((float)weap.damage - Vector3.Distance(base.GetComponent<Rigidbody>().position, GameController.instance.playerController.rightHand.position) * WeaponsManager.instance.shotgunDamageFalloffPerUnit[weap.GetWeaponUpgradeLevel() - 1])));
		damage3 = ((damage3 >= armor) ? (damage3 - (int)armor) : 0);
		timeUntilHit2 = GetBulletTime(weap.weaponName);
		yield return new WaitForSeconds(timeUntilHit2);
		weaponHitBy = weap.weaponName;
		TakeDamage((uint)damage3);
	}

	protected IEnumerator DodgeEnemy(string weapName)
	{
		int rand = Random.Range(0, 1);
		float timeUntilHit = 0.05f;
		timeUntilHit = GetBulletTime(weapName);
		Vector3 dodgeShot = base.GetComponent<Rigidbody>().position;
		if (rand == 1)
		{
			anim.SetBool("LDodge", true);
			targetPos.z += 8f;
			dodgeShot.z -= 2f;
		}
		else
		{
			anim.SetBool("RDodge", true);
			targetPos.z -= 8f;
			dodgeShot.z += 2f;
		}
		dodgeShot += (dodgeShot - GameController.instance.playerController.GetComponent<Rigidbody>().position) * 1.2f;
		yield return new WaitForSeconds(timeUntilHit);
		GameController.instance.playerController.myWeap.FireProjectile(dodgeShot);
		if (rand == 1)
		{
			anim.SetBool("LDodge", false);
			targetPos.z -= 8f;
		}
		else
		{
			anim.SetBool("RDodge", false);
			targetPos.z += 8f;
		}
	}

	public virtual void SpawnLocation(Vector3 nodePos)
	{
		Vector3 zero = Vector3.zero;
		zero = nodePos;
		if ((float)keepAtDistance > zero.x - GameController.instance.playerController.GetComponent<Rigidbody>().position.x)
		{
			zero.x = GameController.instance.playerController.GetComponent<Rigidbody>().position.x + (float)keepAtDistance + 2f;
		}
		zero.y = TileSpawnManager.instance.getRailNodePosition(0, nodePos.x, TileSpawnManager.instance.railTileList[0]).y;
		base.transform.position = zero;
	}

	public virtual void SpawnMovement()
	{
		Vector3 zero = Vector3.zero;
		zero.x = keepAtDistance;
		if (etEnemyState == EnemyType.air)
		{
			zero.y = Random.Range(1, 7);
		}
		zero.z = Random.Range(0, 10) - 5;
		targetPos = zero;
	}

	public void LockOnStart()
	{
		if (lockOnEffect != null)
		{
			if (curLockOnEffect != null)
			{
				GameObjectPool.instance.SetFree(curLockOnEffect);
			}
			curLockOnEffect = GameObjectPool.instance.GetNextFree(lockOnEffect.name, true);
			curLockOnEffect.transform.position = base.GetComponent<Rigidbody>().position;
			curLockOnEffect.GetComponent<ParticleSystem>().Play();
		}
	}

	public void LockOnStop()
	{
		if (curLockOnEffect != null)
		{
			GameObjectPool.instance.SetFree(curLockOnEffect);
			curLockOnEffect = null;
		}
	}

	private float GetBulletTime(string name)
	{
		switch (name)
		{
		case "PredatorLauncher":
			return WeaponsManager.instance.predatorTimeToHit;
		case "RynoM":
			return WeaponsManager.instance.predatorTimeToHit;
		case "BuzzBlades":
			return WeaponsManager.instance.buzzBladeTimeToHit;
		default:
			return 0.05f;
		}
	}
}
