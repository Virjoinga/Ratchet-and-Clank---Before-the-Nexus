using UnityEngine;

public class Boltz : Pickup
{
	private const float C = 1f;

	private const float Citt = 0.030615f;

	private const float Db = 1.05946f;

	private const float D = 1.12246f;

	private const float Ditt = 0.016688f;

	private const float Eb = 1.18921f;

	private const float Ebitt = 0.036405f;

	private const float E = 1.25992f;

	private const float F = 1.33483f;

	private const float Fitt = 0.04087f;

	private const float Gb = 1.41421f;

	private const float G = 1.49831f;

	private const float Gitt = 0.022273f;

	private const float Ab = 1.5874f;

	private const float Abitt = 0.0486f;

	private const float A = 1.6818f;

	private const float Bb = 1.7818f;

	private const float Bbitt = 0.027275f;

	private const float B = 1.88775f;

	private const float C_2 = 2f;

	public int boltValue = 1;

	public bool ExplodeFromObject;

	public Vector3 TargetPosForArc = Vector3.zero;

	private float minBoltPitch = 0.5f;

	private float boltPitchResetTime = 0.3f;

	private float boltPitchInterval = 0.005f;

	public float boltRatchetEffectStartSizeRate = 0.2f;

	private static float boltRatchetEffectStartSize;

	private static float curBoltSize;

	public int numberOfBoltsInSeries = 1;

	private float[] boltPitchRatios = new float[32]
	{
		1f, 1.030615f, 1.06123f, 1.0918449f, 1.12246f, 1.139148f, 1.155836f, 1.172524f, 1.18921f, 1.225615f,
		1.26202f, 1.298425f, 1.33483f, 1.3757f, 1.41657f, 1.4574399f, 1.49831f, 1.5205829f, 1.5428559f, 1.5651288f,
		1.5874f, 1.6359999f, 1.6845999f, 1.7331998f, 1.7818f, 1.809075f, 1.83635f, 1.8636249f, 1.8908999f, 1.9181749f,
		1.9454498f, 2f
	};

	protected static int boltPitchBaseIndex;

	private static int currentPitchRatioIndex;

	private static float currentBoltPitchBase = 0.5f;

	private static int nCount;

	private static float fSeriesCount = 1f;

	private void OnTriggerEnter(Collider other)
	{
		if (!base.enabled || !base.gameObject.activeSelf || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		PlayerController playerController = null;
		if (other.tag.Equals("Player"))
		{
			playerController = GameController.instance.playerController;
		}
		if (!(playerController != null))
		{
			return;
		}
		GameObjectPool.instance.SetFree(base.gameObject);
		playerController.Bolts.Remove(base.gameObject.GetComponent<Boltz>());
		if (numberOfBoltsInSeries > 10)
		{
			numberOfBoltsInSeries = 32;
		}
		ReportCollection(playerController);
		if (Time.time - PickupManager.instance.boltPrevTime > boltPitchResetTime)
		{
			PickupManager.instance.boltPrevTime = Time.time;
			currentBoltPitchBase = boltPitchRatios[boltPitchBaseIndex] * minBoltPitch;
			if (++boltPitchBaseIndex >= boltPitchRatios.Length)
			{
				boltPitchBaseIndex = 0;
			}
			PickupManager.instance.boltComboPitch = currentBoltPitchBase;
			currentPitchRatioIndex = 0;
			nCount = 0;
			fSeriesCount = 1f;
			curBoltSize = boltRatchetEffectStartSize;
		}
		SFXManager.instance.StopSound("UI_tally");
		if (Time.time - PickupManager.instance.boltIntervalTime > boltPitchInterval)
		{
			SFXManager.instance.GetAudioSource("bolt_3").volume = 0.9f;
			if (PickupManager.instance.boltComboPitch > 2.5f)
			{
				PickupManager.instance.boltComboPitch = 2.5f;
			}
			SFXManager.instance.ModulatePitch("bolt_3", PickupManager.instance.boltComboPitch, PickupManager.instance.boltComboPitch, 0f);
			SFXManager.instance.PlaySound("bolt_3");
			if (nCount < numberOfBoltsInSeries)
			{
				nCount++;
			}
			else
			{
				nCount = 0;
				fSeriesCount *= 2f;
				curBoltSize = boltRatchetEffectStartSize;
			}
			currentPitchRatioIndex = (int)((float)nCount / (float)numberOfBoltsInSeries * (float)boltPitchRatios.Length);
			currentPitchRatioIndex = Mathf.Clamp(currentPitchRatioIndex, 0, boltPitchRatios.Length - 1);
			PickupManager.instance.boltComboPitch = fSeriesCount * currentBoltPitchBase * boltPitchRatios[currentPitchRatioIndex];
			PickupManager.instance.boltIntervalTime = Time.time;
			if (boltRatchetEffectStartSize == 0f && playerController.GetBoltPickupEffect() != null)
			{
				boltRatchetEffectStartSize = playerController.GetBoltPickupEffect().GetComponent<ParticleSystem>().startSize;
			}
			if (playerController.activeJumpPad == null && playerController.activeSwingShot == null && !playerController.isJumping)
			{
				if (curBoltSize == 0f)
				{
					curBoltSize = playerController.PlayBoltPickupEffect(curBoltSize);
					boltRatchetEffectStartSize = curBoltSize;
				}
				else
				{
					playerController.PlayBoltPickupEffect(curBoltSize);
				}
			}
			curBoltSize += boltRatchetEffectStartSizeRate;
		}
		PickupManager.instance.boltPrevTime = Time.time;
	}

	protected override void Update()
	{
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.enabled || !base.gameObject.activeSelf || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (ExplodeFromObject)
		{
			Vector3 railNodePosition = TileSpawnManager.instance.getRailNodePosition(0, base.transform.position.x, TileSpawnManager.instance.railTileList[0]);
			if (base.transform.position.y < railNodePosition.y)
			{
				railNodePosition.x = base.transform.position.x;
				railNodePosition.z = base.transform.position.z;
				base.transform.position = railNodePosition;
				ExplodeFromObject = false;
			}
			else
			{
				base.transform.position -= Vector3.Normalize(base.transform.position - TargetPosForArc);
				TargetPosForArc.y -= 2f;
			}
		}
		if (base.transform.position.x < GameController.instance.playerController.transform.position.x - 10f)
		{
			GameController.instance.playerController.Bolts.Remove(base.gameObject.GetComponent<Boltz>());
			GameObjectPool.instance.SetFree(base.gameObject, true);
			base.enabled = false;
		}
	}

	protected virtual void ReportCollection(PlayerController controller)
	{
		controller.addBoltz(boltValue);
	}
}
