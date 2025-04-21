using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public enum PickupTypes
	{
		Magnetizer = 0,
		Reflector = 1,
		Multiplier = 2,
		Jetpack = 3,
		Rift = 4,
		Tornado = 5,
		Groove = 6,
		None = 7,
		MAX = 8
	}

	[Serializable]
	public class RailBehavior
	{
		public AnimationCurve ratchetJumpCurve;

		public AnimationCurve ratchetLaneSwitchCurve;

		public AnimationCurve ratchetSideJumpCurve;

		public AnimationCurve ratchetHeliBoostCurve;

		public float heliSlowModifier = 0.8f;

		public float boostHeight = 2f;
	}

	[Serializable]
	public class GroundBehavior
	{
		public AnimationCurve ratchetJumpCurve;

		public AnimationCurve ratchetLaneSwitchCurve;

		public AnimationCurve ratchetSideJumpCurve;

		public AnimationCurve ratchetHeliBoostCurve;

		public float heliSlowModifier = 1f;

		public float boostHeight = 2f;
	}

	public enum EDeathDealer
	{
		EDeath_Proj = 0,
		EDeath_Obs = 1,
		EDeath_Elem = 2,
		EDeath_Fall = 3,
		EDeath_Explode = 4,
		EDeath_Fire = 5,
		EDeath_Quit = 6
	}

	private enum ClankEyeLids
	{
		LeftTop = 0,
		LeftBot = 1,
		RightTop = 2,
		RightBot = 3
	}

	public enum SwipeDirection
	{
		SD_None = 0,
		SD_Left = 1,
		SD_Right = 2,
		SD_Up = 3
	}

	public const int BASE_HEALTH = 2;

	public const int maxHealth = 5;

	public const string ARMOR_LEVEL_KEY = "ArmorLevel";

	public float respawnInvulTime = 3f;

	public bool GodMode;

	public float respawnTime = 0.4f;

	public float maxVelocityX = 25f;

	private float startVelocityX;

	public float curVelocityX;

	public float jumpSpeed = 10f;

	public float swingSpeed = 15f;

	public bool isJumping;

	private int targetRail;

	private float jumpTimer;

	private bool airSideJump;

	private bool isPlayingIntro = true;

	private LeviathanController introNeftin;

	public int heroBoltSafeDist = 200;

	private float cameraYLook;

	public bool playIntroAnim;

	public bool friendChallengeEnabled;

	private bool prevFriendChallengeFailure;

	private Shader transparancyShader;

	private Vector3 deathBoxPos;

	public int ChallengeReward = 250;

	public int TerachnoidBoltValue = 100;

	public PickupTypes CurrentPickupType = PickupTypes.None;

	public Pickup CurrentHeldPickup;

	public RailBehavior railBehaviors;

	public GroundBehavior groundBehaviors;

	private float curLaneSwitchTime;

	private float curJumpTime;

	private AnimationCurve curRatchetJumpCurve;

	private AnimationCurve curRatchetLaneSwitchCurve;

	private AnimationCurve curRatchetSideJumpCurve;

	private AnimationCurve curRatchetHeliBoostCurve;

	private float curHeliSlowModifier;

	private float boostHeight;

	public float difficultySpeedInc = 5f;

	public float[] difficultyDist = new float[10];

	public Vector3 targetPos;

	public Vector3 targetDir;

	public Vector3 deathPos;

	public float targetPosDist;

	private Vector3 futurePos;

	private Vector3 touchStartPos;

	private Vector3 touchEndPos;

	private Vector3 touchDelta;

	public float swipeSens = 29.12f;

	private float touchHoldTime;

	public Vector3 lastTouchPos = Vector3.zero;

	[HideInInspector]
	public float travelDist;

	private Vector3 lastPos;

	private bool buttonDown;

	private bool isDead;

	private bool wasInTransition;

	private bool lastDead;

	private bool jumpToRail;

	private float jumpDirection;

	private bool isHeliActive;

	private bool isHeliBoost;

	private bool heliSetCurve;

	private float ZVelocity;

	private float deathDelay;

	public int currentRail;

	public int runningRail;

	public int currentRailListIndex;

	public Swing activeSwingShot;

	public bool wasSwinging;

	public JumpPad activeJumpPad;

	public bool inBossBattle;

	public ParticleSystem effectParticles;

	private Animator anim;

	private AnimatorStateInfo currentBaseState;

	private string a_CurrentIdleState = "Rail";

	private string a_CurrentDeathState = "DeathProj";

	private int a_GroundFireOH = Animator.StringToHash("Base Layer.Ground_FireOH");

	private int a_JetFire = Animator.StringToHash("Jet.Jet_Fire");

	private int a_RailJumpCenter = Animator.StringToHash("RailJump.RailJumpCenter");

	private int a_GroundJumpCenter = Animator.StringToHash("GroundJump.GroundJumpCenter");

	private int a_Falling = Animator.StringToHash("Base Layer.Falling");

	private int a_RailHitReact = Animator.StringToHash("Base Layer.Rail_HitReact");

	private int a_GroundHitReact = Animator.StringToHash("Base Layer.Ground_HitReactShot");

	private int a_HeliCenter = Animator.StringToHash("Heli.Heli_Center");

	private int a_DeathTag = Animator.StringToHash("Death");

	private int a_JumpPad = Animator.StringToHash("Base Layer.JumpPad");

	private int a_Swingshot = Animator.StringToHash("Swingshot.Swingshot_Swing");

	private int a_Start = Animator.StringToHash("Base Layer.Start");

	private GameObject HeliTop;

	private GameObject ClankJet;

	private GameObject grindEffect;

	private GameObject hoverEffectLeft;

	private GameObject hoverEffectRight;

	private GameObject RatchetLeftEye;

	private GameObject RatchetRightEye;

	private GameObject[] clankEyeLid;

	private bool RandomBlinkingOn;

	private int ClankBlinkCounter;

	private int RatchetBlinkCounter = 13;

	public Texture[] HFlux;

	public Texture[] EFlux;

	public Texture[] TFlux;

	private Texture[] ArmorLevels;

	private GameObject LeftArmor;

	private GameObject RightArmor;

	private GameObject HeadArmor;

	private Renderer RatchetRenderer;

	public ParticleSystem loseShoulderArmor;

	public ParticleSystem landingTargetParticles;

	private GameObject landingTarget;

	public ParticleSystem boltPickupEffectParticles;

	private GameObject boltPickupEffect;

	public Transform RatchetHips;

	public Transform RatchetRoot;

	public Weapon myWeap;

	public Transform rightHand;

	public Transform leftHand;

	private int HP;

	protected int terachnoidInventory;

	protected int heroBoltInventory;

	public List<Boltz> Bolts;

	private Vector3[] debugPickPos = new Vector3[3];

	private Vector3[] debugDrawPos = new Vector3[100];

	private Color[] debugDrawColor = new Color[100];

	private int debugDrawCount;

	private bool quitToMenu;

	private float currentBank;

	private float lastDirZ1;

	private float lastDirZ2;

	public float bankDelta = 0.005f;

	public float bankEpsilon = 0.0001f;

	public float bankLerp = 1f;

	private float[] railBaseValues = new float[3];

	public float fireIdleTime = 1f;

	private float fireIdleTimer;

	private float lerpFireIdle;

	public float lerpFireIdleAmount = 0.04f;

	private List<float> laneSwitches;

	public ParticleSystem onHitEffect;

	public Vector3 offsetEffect;

	public float onHoldWait = 0.3f;

	private float holdWaitTimer;

	private bool wasHeliActive;

	public ParticleSystem onHitJumpPadEffect;

	public Vector3 jumpPadEffectOffset;

	public FlashingTint flashEffect;

	public string armorSkinString = string.Empty;

	private bool swipeRegistered;

	private SwipeDirection nextSwipe;

	private float[] railNodeClosestDx = new float[3];

	private Vector3[] railNodeClosestPos = new Vector3[3];

	private Vector3[] railNodeIndices;

	private int railNodeListIndex;

	public RailComponent railNodePositionComponent;

	public bool dontFire;

	private void Awake()
	{
		deathBoxPos = new Vector3(-50f, 0f, 0f);
		anim = base.gameObject.GetComponent<Animator>();
		laneSwitches = new List<float>();
	}

	public void TakeDamage(uint damage)
	{
		if (GodMode || GadgetManager.instance.IsJetpackInvincibleLanding())
		{
			return;
		}
		RatchetHitReact();
		if (GadgetManager.instance.GetReflectorHealth() > 0)
		{
			GadgetManager.instance.ReflectorHit();
		}
		else
		{
			HP -= (int)damage;
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.damageTaken, damage);
			StatsTracker.instance.SetStat(StatsTracker.Stats.noDamageDistanceBase, travelDist);
		}
		UIManager.instance.GetHUD().UpdateHP();
		if (HP <= 0)
		{
			Kill(EDeathDealer.EDeath_Proj);
		}
		else
		{
			if (HP == 2)
			{
				SetArmorLevel(2);
			}
			SFXManager.instance.PlaySound("cha_Ratchet_ArmorBreak");
		}
		PlayOnHitParticle();
	}

	public Vector3 getRailNodePosition(Vector3 targetPos, Vector3 normDir, int entryRailNum, out int resultingRailNum, bool anyPoint)
	{
		bool flag = false;
		debugDrawCount = 2;
		railNodeClosestDx[0] = float.PositiveInfinity;
		railNodeClosestDx[1] = float.PositiveInfinity;
		railNodeClosestDx[2] = float.PositiveInfinity;
		railNodeClosestPos[0] = Vector3.zero;
		railNodeClosestPos[1] = Vector3.zero;
		railNodeClosestPos[2] = Vector3.zero;
		railNodePositionComponent = null;
		for (int i = 0; i < TileSpawnManager.instance.maxPieces; i++)
		{
			if (TileSpawnManager.instance.railTileList[i] == null)
			{
				continue;
			}
			for (int j = 0; j < 3; j++)
			{
				switch (j)
				{
				case 0:
					railNodeIndices = TileSpawnManager.instance.railTileList[i].rail1TransformedIndices;
					break;
				case 1:
					railNodeIndices = TileSpawnManager.instance.railTileList[i].rail2TransformedIndices;
					break;
				default:
					railNodeIndices = TileSpawnManager.instance.railTileList[i].rail3TransformedIndices;
					break;
				}
				for (int k = 0; k < railNodeIndices.Length; k++)
				{
					Vector3 vector = railNodeIndices[k];
					if (debugDrawCount < debugDrawPos.Length)
					{
						debugDrawPos[debugDrawCount] = vector;
					}
					if (vector.x < targetPos.x)
					{
						if (debugDrawCount < debugDrawColor.Length)
						{
							debugDrawColor[debugDrawCount] = Color.blue;
							debugDrawCount++;
						}
						continue;
					}
					railNodeListIndex = i;
					railNodeClosestDx[j] = Vector3.SqrMagnitude(targetPos - vector);
					railNodeClosestPos[j] = vector;
					if (debugDrawCount < debugDrawColor.Length)
					{
						debugDrawColor[debugDrawCount] = Color.cyan;
					}
					debugPickPos[j] = vector;
					debugDrawCount++;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				railNodePositionComponent = TileSpawnManager.instance.railTileList[i];
				break;
			}
		}
		resultingRailNum = -1;
		Vector3 result = Vector3.zero;
		if (flag)
		{
			if (entryRailNum == -1)
			{
				if (railNodeClosestDx[0] < railNodeClosestDx[1] && railNodeClosestDx[0] < railNodeClosestDx[2])
				{
					result = railNodeClosestPos[0];
					resultingRailNum = 2;
				}
				else if (railNodeClosestDx[1] < railNodeClosestDx[2])
				{
					result = railNodeClosestPos[1];
					resultingRailNum = 0;
				}
				else
				{
					result = railNodeClosestPos[2];
					resultingRailNum = 1;
				}
			}
			else if (entryRailNum == 2 && railNodeClosestPos[0] != Vector3.zero)
			{
				resultingRailNum = entryRailNum;
				result = railNodeClosestPos[0];
			}
			else if (entryRailNum == 1 && railNodeClosestPos[2] != Vector3.zero)
			{
				resultingRailNum = entryRailNum;
				result = railNodeClosestPos[2];
			}
			else if (entryRailNum == 0 && railNodeClosestPos[1] != Vector3.zero)
			{
				resultingRailNum = entryRailNum;
				result = railNodeClosestPos[1];
			}
			else if (GodMode && railNodeClosestPos[1] != Vector3.zero)
			{
				resultingRailNum = 0;
				result = railNodeClosestPos[1];
			}
		}
		if (resultingRailNum == -1)
		{
			resultingRailNum = entryRailNum;
			result = targetPos;
		}
		return result;
	}

	public void ActivateGadget()
	{
		CurrentHeldPickup.Activate();
		CurrentHeldPickup = null;
		SetCurrentPickupType(PickupTypes.None);
	}

	public void SetCurrentPickup(Pickup newPickup)
	{
		if (CurrentHeldPickup != null)
		{
			GameObjectPool.instance.SetFree(CurrentHeldPickup.transform.parent.gameObject, true);
		}
		CurrentHeldPickup = newPickup;
	}

	public void SetCurrentPickupType(PickupTypes newPickupType)
	{
		CurrentPickupType = newPickupType;
		UIHUD hUD = UIManager.instance.GetHUD();
		if (hUD != null)
		{
			hUD.UpdateHeldGadget();
			if (newPickupType == PickupTypes.Jetpack && (GameController.instance.gameState == GameController.eGameState.GS_TransitToGnd || GameController.instance.gameState == GameController.eGameState.GS_TransitToRail))
			{
				hUD.DisableGadgets();
			}
		}
	}

	private void Start()
	{
		GameObject gameObject = GameObject.Find("EffectParticles");
		if (gameObject != null)
		{
			effectParticles = gameObject.GetComponent<ParticleSystem>();
		}
		lastPos = base.transform.position;
		currentRail = 0;
		isDead = true;
		playIntroAnim = true;
		startVelocityX = maxVelocityX;
		HP = GetInitialHP();
		if (PlayerPrefs.HasKey("Skin"))
		{
			SetSkin(PlayerPrefs.GetString("Skin"));
		}
		else
		{
			ArmorLevels = HFlux;
		}
		HeliTop = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Heli_Top")
			.gameObject;
		ClankJet = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Clank_Jet")
			.gameObject;
		grindEffect = base.gameObject.transform.Find("FX_RailGrind").gameObject;
		hoverEffectLeft = base.gameObject.transform.Find("Root").Find("Hips").Find("LeftUpLeg")
			.Find("LeftLeg")
			.Find("LeftFoot")
			.Find("FX_RocketBoots")
			.gameObject;
		hoverEffectRight = base.gameObject.transform.Find("Root").Find("Hips").Find("RightUpLeg")
			.Find("RightLeg")
			.Find("RightFoot")
			.Find("FX_RocketBoots")
			.gameObject;
		HeliTop.SetActive(false);
		ClankJet.SetActive(false);
		RatchetRoot = base.gameObject.transform.Find("Root");
		RatchetHips = base.gameObject.transform.Find("Root").Find("Hips");
		LeftArmor = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("L_ShoulderArmor2")
			.gameObject;
		RightArmor = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("R_ShoulderArmor_2")
			.gameObject;
		HeadArmor = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Neck")
			.Find("Head")
			.Find("ArmorHead")
			.gameObject;
		RatchetRenderer = base.gameObject.transform.Find("Ratchet").renderer;
		SetArmorLevel(HP);
		rightHand = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("RightArm")
			.Find("RightForeArm")
			.Find("RightHand");
		leftHand = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("LeftArm")
			.Find("LeftForeArm")
			.Find("LeftHand");
		RatchetLeftEye = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Neck")
			.Find("Head")
			.Find("Ratchet_L_lid")
			.gameObject;
		RatchetRightEye = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Neck")
			.Find("Head")
			.Find("Ratchet_R_lid")
			.gameObject;
		RatchetEyeLidsOff();
		RatchetBlinkCounter = 0;
		int num = Enum.GetNames(typeof(ClankEyeLids)).Length;
		clankEyeLid = new GameObject[num];
		clankEyeLid[0] = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Clank_LT_Lid")
			.gameObject;
		clankEyeLid[2] = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Clank_RT_Lid")
			.gameObject;
		clankEyeLid[1] = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Clank_LB_Lid")
			.gameObject;
		clankEyeLid[3] = base.gameObject.transform.Find("Root").Find("Hips").Find("Spine")
			.Find("Clank_RB_Lid")
			.gameObject;
		ClankEyeLidsOff();
		ClankBlinkCounter = 0;
		Bolts = new List<Boltz>();
		anim.enabled = false;
		flashEffect = GetComponent<FlashingTint>();
	}

	public void setNewRail(int rail)
	{
		if ((currentRail == 2 && rail == 0) || (currentRail == 0 && rail == 1))
		{
			jumpDirection = -1f;
		}
		else if ((currentRail == 1 && rail == 0) || (currentRail == 0 && rail == 2))
		{
			jumpDirection = 1f;
		}
		DoJump(true);
		currentRail = rail;
		jumpToRail = true;
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.lanesSwitched);
		if (GadgetManager.instance.GetJetpackStatus())
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.jetPackLanesSwitched);
		}
		railBaseValues[currentRail] = travelDist;
		ChallengeSystem.ChallengeInfo challengeInfo = ChallengeSystem.instance.challengeList[1];
		float num = challengeInfo.PrimaryInit + challengeInfo.PrimaryScale * (float)challengeInfo.CurrentLevel;
		if (laneSwitches.Count < (int)num)
		{
			laneSwitches.Add(travelDist);
		}
		else
		{
			laneSwitches.RemoveAt(0);
			laneSwitches.Add(travelDist);
		}
		if (laneSwitches[laneSwitches.Count - 1] - laneSwitches[0] <= 200f)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.withinDistanceLanesSwitched);
		}
		else
		{
			StatsTracker.instance.SetStat(StatsTracker.Stats.withinDistanceLanesSwitched, 0f);
		}
	}

	public bool isOnJumpPad()
	{
		if (TileSpawnManager.instance.railTileList[0] != null && TileSpawnManager.instance.railTileList[0].name.Contains("TIL_PJO01"))
		{
			return true;
		}
		return false;
	}

	public bool isOnTunnelRail()
	{
		if (TileSpawnManager.instance.railTileList[0] != null && TileSpawnManager.instance.railTileList[0].name.Contains("TIL_TUN"))
		{
			return true;
		}
		if (currentRail == 0 && TileSpawnManager.instance.railTileList[1] != null && TileSpawnManager.instance.railTileList[1].name.Contains("TIL_TUN"))
		{
			return true;
		}
		return false;
	}

	public bool isNearTunnelRail()
	{
		if (TileSpawnManager.instance.railTileList[0] != null && TileSpawnManager.instance.railTileList[0].name.Contains("TIL_TUN"))
		{
			return true;
		}
		if (TileSpawnManager.instance.railTileList[1] != null && TileSpawnManager.instance.railTileList[1].name.Contains("TIL_TUN"))
		{
			return true;
		}
		return false;
	}

	public bool isOnGrappleRail()
	{
		int num = currentRailListIndex;
		if (TileSpawnManager.instance.railTileList[num] != null)
		{
			if (TileSpawnManager.instance.railTileList[num].name.Contains("TIL_PGO01"))
			{
				return true;
			}
			if (TileSpawnManager.instance.railTileList[num].name.Contains("TIL_PJO01"))
			{
				return true;
			}
		}
		return false;
	}

	private void touchBegan(Touch touch)
	{
		touchStartPos.x = touch.position.x;
		touchStartPos.y = touch.position.y;
		touchDelta = Vector3.zero;
	}

	private void touchEnded(Touch touch)
	{
		touchEndPos.x = touch.position.x;
		touchEndPos.y = touch.position.y;
		touched(touchEndPos, touch.tapCount);
		swipeRegistered = false;
		wasHeliActive = false;
	}

	private void touchMoved(Touch touch)
	{
		touchDelta = touch.deltaPosition;
		if (swipeRegistered || holdWaitTimer <= 0f)
		{
			return;
		}
		bool flag = false;
		int num = currentRail;
		if (Mathf.Abs(touchDelta.x) > Mathf.Abs(touchDelta.y))
		{
			if (touchDelta.x < 0f - swipeSens)
			{
				if (!isOnTunnelRail() && !isOnGrappleRail() && !isOnJumpPad())
				{
					if (!IsSideJumping())
					{
						if (currentRail == 0)
						{
							num = 2;
							targetRail = 2;
							RatchetJumpLeft();
						}
						else if (currentRail == 1)
						{
							num = 0;
							targetRail = 0;
							RatchetJumpLeft();
						}
					}
					else
					{
						nextSwipe = SwipeDirection.SD_Left;
					}
				}
			}
			else if (touchDelta.x > swipeSens && !isOnTunnelRail() && !isOnGrappleRail() && !isOnJumpPad())
			{
				if (!IsSideJumping())
				{
					if (currentRail == 2)
					{
						num = 0;
						targetRail = 0;
						RatchetJumpRight();
					}
					else if (currentRail == 0)
					{
						num = 1;
						targetRail = 1;
						RatchetJumpRight();
					}
				}
				else
				{
					nextSwipe = SwipeDirection.SD_Right;
				}
			}
		}
		else if (Mathf.Abs(touchDelta.y) > Mathf.Abs(touchDelta.x) && !GadgetManager.instance.GetJetpackStatus() && touchDelta.y > swipeSens)
		{
			flag = true;
			if (!isJumping)
			{
				RatchetJumpCenter();
			}
			else
			{
				nextSwipe = SwipeDirection.SD_Up;
			}
		}
		if (flag)
		{
			DoJump(false);
			swipeRegistered = true;
		}
		else if (currentRail != num)
		{
			setNewRail(num);
			swipeRegistered = true;
		}
	}

	protected bool CheckForSwingshot(Vector3 pos, int tapCount)
	{
		bool result = false;
		if (tapCount > 0)
		{
			Ray ray = Camera.mainCamera.ScreenPointToRay(pos);
			RaycastHit[] array = Physics.RaycastAll(ray.origin, ray.direction);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].collider != null)
				{
					GameObject gameObject = array[i].collider.gameObject;
					Swing component = gameObject.GetComponent<Swing>();
					if (component != null && !component.isActive && currentRail == 0)
					{
						StatsTracker.instance.UpdateStat(StatsTracker.Stats.swingShotUsed);
						component.isSwinging = false;
						component.isActive = true;
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public void clicked(Vector3 pos, int tapCount)
	{
		if (!CheckForSwingshot(pos, tapCount))
		{
			FireCheck(pos);
		}
	}

	public void touched(Vector3 pos, int tapCount)
	{
		if (!CheckForSwingshot(pos, tapCount))
		{
			FireCheck(pos);
		}
	}

	public void Kill(EDeathDealer cause)
	{
		SFXManager.instance.StopAllSounds();
		SFXManager.instance.StopAllLoopingSounds();
		if ((GodMode || GadgetManager.instance.IsJetpackInvincibleLanding()) && cause != EDeathDealer.EDeath_Quit)
		{
			return;
		}
		RatchetHeliStop();
		GadgetManager.instance.DeactivateAllOnDeath();
		MegaWeaponManager.instance.DeactivateMegaWeapon(true);
		grindEffect.particleSystem.Stop();
		hoverEffectLeft.particleSystem.Stop();
		hoverEffectRight.particleSystem.Stop();
		if (myWeap.ammo == 0)
		{
			UIManager.instance.GetHUD().OutOfAmmoContainer.SetActive(false);
		}
		wasInTransition = anim.IsInTransition(0);
		if (!isDead)
		{
			quitToMenu = false;
			MusicManager.instance.Pause();
			anim.SetBool(a_CurrentIdleState, false);
			anim.SetFloat("Bank", 0f);
			anim.SetBool("DeathStart", true);
			switch (cause)
			{
			case EDeathDealer.EDeath_Proj:
			{
				a_CurrentDeathState = "DeathProj";
				int value = Random.Range(1, 2);
				anim.SetInteger("DeathProj_Int", value);
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathByEnemy);
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:enemy", 1f);
				break;
			}
			case EDeathDealer.EDeath_Obs:
			{
				a_CurrentDeathState = "DeathObstacle";
				int value = Random.Range(0, 4);
				anim.SetInteger("DeathObstacle_Int", value);
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathByCrash);
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:obstacle", 1f);
				break;
			}
			case EDeathDealer.EDeath_Elem:
				a_CurrentDeathState = "DeathElement";
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathByCrash);
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:crash", 1f);
				break;
			case EDeathDealer.EDeath_Fall:
				a_CurrentDeathState = "DeathFall";
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathByFall);
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:fall", 1f);
				SetHP(GetInitialHP());
				break;
			case EDeathDealer.EDeath_Explode:
				a_CurrentDeathState = "DeathExplode";
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathByCrash);
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:explosion", 1f);
				break;
			case EDeathDealer.EDeath_Fire:
				a_CurrentDeathState = "DeathFire";
				break;
			case EDeathDealer.EDeath_Quit:
				Analytics.Get().SendDesignEvent("roundEnd:killedBy:quit", 1f);
				a_CurrentDeathState = "DeathMenu";
				curVelocityX = 0.01f;
				quitToMenu = true;
				break;
			}
			if (cause != EDeathDealer.EDeath_Quit)
			{
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.deathTotal);
			}
			StatsTracker.instance.SetStat(StatsTracker.Stats.deathDistance, travelDist);
			deathPos = base.rigidbody.transform.position;
			if (cause == EDeathDealer.EDeath_Fall)
			{
				deathPos.x += 15f;
			}
			anim.SetBool(a_CurrentDeathState, true);
			anim.SetBool("Start", false);
			isDead = true;
			deathDelay = 0f;
			base.rigidbody.MovePosition(deathBoxPos);
			base.rigidbody.MoveRotation(Quaternion.identity);
			base.transform.position = deathBoxPos;
			base.transform.rotation = Quaternion.identity;
			GameController.instance.gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Death);
			GameController.instance.gameCamera.ResetCamera();
			UIHUD hUD = UIManager.instance.GetHUD();
			if (hUD != null)
			{
				hUD.NotifyKilled();
			}
		}
	}

	public void DestroyObstacles()
	{
		foreach (GameObject obstacle in ObstacleManager.instance.getObstacles())
		{
			GameObjectPool.instance.SetFree(obstacle);
		}
		ObstacleManager.instance.getObstacles().Clear();
	}

	public void HeroBoltRevive()
	{
		MusicManager.instance.Resume();
		Time.timeScale = 1f;
		SetHP(2);
		isDead = false;
		int resultingRailNum;
		targetPos = getRailNodePosition(deathPos, Vector3.right, -1, out resultingRailNum, true);
		DestroyObstacles();
		currentRail = resultingRailNum;
		GameController.instance.inMenu = false;
		base.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		base.rigidbody.MovePosition(targetPos);
		base.rigidbody.MoveRotation(Quaternion.identity);
		base.transform.position = targetPos;
		base.transform.rotation = Quaternion.identity;
		lastPos = base.rigidbody.position;
		anim.SetBool(a_CurrentDeathState, false);
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			RatchetGround();
			if (TileSpawnManager.instance.tileSpawnState == TileSpawnManager.TileSpawnState.Hero)
			{
				GameController.instance.gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Boss);
				GameController.instance.gameCamera.ResetCamera();
			}
			else
			{
				GameController.instance.gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Ground);
				GameController.instance.gameCamera.ResetCamera();
			}
		}
		else
		{
			RatchetRails();
			GameController.instance.gameCamera.SwitchCameraSettings(CameraFollow.CameraSettings.Rails);
			GameController.instance.gameCamera.ResetCamera();
		}
		GameController.instance.gameCamera.ResetCamera();
		anim.enabled = true;
		GodMode = true;
		StartCoroutine(RatchetRespawnInvul());
	}

	private IEnumerator RatchetRespawnInvul()
	{
		if (flashEffect != null)
		{
			flashEffect.duration = respawnInvulTime;
			base.gameObject.SendMessage("FlashTint", this, SendMessageOptions.DontRequireReceiver);
		}
		yield return new WaitForSeconds(respawnInvulTime);
		GodMode = false;
	}

	public void Revive()
	{
		isDead = false;
		maxVelocityX = startVelocityX;
		SetHP(GetInitialHP());
		UpdateArmorLevel();
	}

	public void UploadLeaderboardScores()
	{
		SocialManager.instance.SubmitScore(0, GameController.instance.GetScore());
		SocialManager.instance.SubmitScore(1, GetBoltsLifetime());
		SocialManager.instance.SubmitScore(2, GetDistanceLifetime());
	}

	public void CheckForFriendChallengeVictory()
	{
		if (friendChallengeEnabled)
		{
			friendChallengeEnabled = false;
			if (GameController.instance.GetScore() > SocialManager.instance.GetFriendScoreToBeat())
			{
				FriendChallengeVictory();
				return;
			}
			prevFriendChallengeFailure = true;
			FriendChallengeFailure();
		}
	}

	public void FriendChallengeVictory()
	{
		UIManager.instance.ShowFriendChallengePopup(true, GetBolts());
		addBoltz(GetBolts());
	}

	public void FriendChallengeFailure()
	{
		int num = (int)((double)GetBolts() * -0.5);
		addBoltz(num);
		UIManager.instance.ShowFriendChallengePopup(false, num);
	}

	public void Restart()
	{
		travelDist = 0f;
		WeaponsManager.instance.HideWeapon(true);
		int resultingRailNum;
		targetPos = getRailNodePosition(Vector3.zero, Vector3.right, 0, out resultingRailNum, true);
		base.rigidbody.position = Vector3.zero;
		base.rigidbody.MoveRotation(Quaternion.identity);
		base.transform.position = targetPos;
		base.transform.rotation = Quaternion.identity;
		base.rigidbody.angularVelocity = Vector3.zero;
		base.rigidbody.velocity = Vector3.zero;
		isJumping = false;
		activeSwingShot = null;
		activeJumpPad = null;
		isDead = false;
		landingTarget = null;
		lastPos = base.rigidbody.position;
		currentRail = 0;
		anim.SetBool(a_CurrentDeathState, false);
		anim.SetBool("Ground", false);
		anim.SetBool("Rail", false);
		if (playIntroAnim && !TileSpawnManager.instance.overwriteStartTile)
		{
			anim.SetBool("Rail", false);
			anim.SetBool("Start", true);
			isPlayingIntro = true;
			introNeftin = EnemyManager.instance.SpawnIntroLeviathon();
			introNeftin.StartIntroAnim();
			introNeftin.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			GameController.instance.gameCamera.StartIntroCamera();
			playIntroAnim = false;
			base.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}
		else
		{
			GameController.instance.gameCamera.ResetCamera();
			isPlayingIntro = false;
			base.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			if (TileSpawnManager.instance.overwriteStartTile)
			{
				GameController.instance.ChangeState(GameController.eGameState.GS_OnGround);
				anim.SetBool("Ground", true);
			}
			else
			{
				anim.SetBool("Rail", true);
			}
		}
		anim.enabled = true;
		RandomBlinkingOn = true;
		CurrentPickupType = PickupTypes.None;
		CurrentHeldPickup = null;
		if (prevFriendChallengeFailure)
		{
			friendChallengeEnabled = true;
			prevFriendChallengeFailure = false;
		}
		laneSwitches.Clear();
		WeaponsManager.instance.OnRunStart();
		GadgetManager.instance.DeactivateAllOnRestart();
		StatsTracker.instance.SetStat(StatsTracker.Stats.weaponsUsed, 0f);
		ClankEyeLidsOff();
		ClankBlinkCounter = 0;
		RatchetEyeLidsOff();
		RatchetBlinkCounter = 0;
	}

	public bool IsDoingIntro()
	{
		return isPlayingIntro;
	}

	public void addBoltz(int n, bool IgnoreMultiplier = false)
	{
		float num = ((!IgnoreMultiplier) ? ((float)(n * GadgetManager.instance.GetBoltMultiplier())) : ((float)n));
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.boltzCollected, num);
		StatsTracker.instance.SetStat(StatsTracker.Stats.noBoltzDistanceBase, travelDist);
		if (GadgetManager.instance.IsMagnetizerActive())
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.magnetizerBoltzCollected, num);
		}
		if (MegaWeaponManager.instance.tornadoOn)
		{
			StatsTracker.instance.UpdateStat(StatsTracker.Stats.tornadoBoltzCollected, num);
		}
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateBoltsAdded((int)num);
		}
	}

	public void addItem(PurchasableItem purchasedItem)
	{
		string[] array = purchasedItem.description.Split();
		Debug.Log(string.Format("Purchasing item {0} giving {1} bolts", purchasedItem.Id, array[0]));
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.boltzCollected, float.Parse(array[0]));
		StatsTracker.instance.SaveStatsAndReset();
		UIManager.instance.GetVendorMenu().UpdateAll();
	}

	public void addRaritanium(int nAmount)
	{
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.raritaniumCollected, nAmount);
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateRaritaniumCollected();
		}
	}

	public void addTerachnoid(int nAmount)
	{
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.terachnoidsCollected, nAmount);
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateTerachnoidsCollected();
		}
		if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected) >= (float)TileSpawnManager.instance.terachnoidsMax)
		{
			if (!WeaponsManager.instance.HaveBoughtWeapon(WeaponsManager.WeaponList.WEP_RynoM))
			{
				WeaponsManager.instance.UpgradeWeapon(4u);
			}
			if (StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected) > (float)TileSpawnManager.instance.terachnoidsMax)
			{
				addBoltz(TerachnoidBoltValue);
			}
		}
	}

	public void addHeroBolt(int nAmount)
	{
		StatsTracker.instance.UpdateStat(StatsTracker.Stats.heroBoltsCollected, nAmount);
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateHeroBoltsCollected();
		}
	}

	private void checkDifficulty()
	{
		maxVelocityX = startVelocityX;
		for (int i = 0; i < difficultyDist.Length; i++)
		{
			if (travelDist > difficultyDist[i])
			{
				maxVelocityX += difficultySpeedInc;
			}
		}
	}

	private void CheckForIntro()
	{
		if (currentBaseState.nameHash == a_Start)
		{
			if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
			{
				RatchetGround();
			}
			else
			{
				RatchetRails();
			}
		}
		UIHUD hUD = UIManager.instance.GetHUD();
		hUD.NotifyIntroStart();
		if (!anim.GetBool("Start"))
		{
			isPlayingIntro = false;
			if (!grindEffect.particleSystem.isPlaying)
			{
				grindEffect.particleSystem.Play();
			}
			hUD.EnableGadgets();
			hUD.NotifyIntroOver();
			introNeftin.IntroOver();
			curVelocityX = startVelocityX;
			base.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}

	public void EndIntro()
	{
		UIHUD hUD = UIManager.instance.GetHUD();
		isPlayingIntro = false;
		if (!grindEffect.particleSystem.isPlaying)
		{
			grindEffect.particleSystem.Play();
		}
		hUD.NotifyIntroOver();
		introNeftin.IntroOver();
		curVelocityX = startVelocityX;
		base.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}

	private void Update()
	{
		if (GameController.instance.inMenu || isPlayingIntro)
		{
			return;
		}
		if (holdWaitTimer > 0f)
		{
			holdWaitTimer -= Time.deltaTime;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
		{
			nextSwipe = SwipeDirection.SD_Up;
		}
		if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && !isOnTunnelRail() && !isOnGrappleRail() && !isOnJumpPad())
		{
			nextSwipe = SwipeDirection.SD_Left;
		}
		if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && !isOnTunnelRail() && !isOnGrappleRail() && !isOnJumpPad())
		{
			nextSwipe = SwipeDirection.SD_Right;
		}
		if (isDead)
		{
			deathDelay += Time.deltaTime;
			return;
		}
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			lastTouchPos = touch.position;
			if (touch.phase == TouchPhase.Began)
			{
				touchBegan(touch);
				holdWaitTimer = onHoldWait;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				touchEnded(touch);
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				touchMoved(touch);
				HoldCheck(touch.position);
			}
			else
			{
				HoldCheck(touch.position);
			}
		}
		if (Input.touchCount == 0)
		{
			if (Input.GetMouseButton(0))
			{
				lastTouchPos = Input.mousePosition;
				if (!buttonDown)
				{
					if (Input.mousePosition.x < (float)(Screen.width / 3))
					{
						touchDelta.x = 0f - swipeSens - 1f;
						touchDelta.y = 0f;
					}
					else if (Input.mousePosition.x > (float)(Screen.width - Screen.width / 3))
					{
						touchDelta.x = swipeSens + 1f;
						touchDelta.y = 0f;
					}
					else
					{
						touchDelta.x = 0f;
						touchDelta.y = swipeSens + 1f;
					}
					holdWaitTimer = onHoldWait;
				}
				else
				{
					HoldCheck(Input.mousePosition);
				}
				buttonDown = true;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				clicked(Input.mousePosition, 1);
				buttonDown = false;
			}
		}
		if (buttonDown || Input.touchCount > 0 || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			touchHoldTime += Time.deltaTime;
			if ((touchHoldTime > 0.175f || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) && (currentBaseState.nameHash == a_GroundJumpCenter || currentBaseState.nameHash == a_RailJumpCenter || airSideJump) && !anim.IsInTransition(0) && holdWaitTimer <= 0f && !isHeliActive && !isDead)
			{
				if (curRatchetJumpCurve.keys[2].time < curJumpTime - jumpTimer)
				{
					isHeliActive = true;
					wasHeliActive = true;
					SFXManager.instance.PlayUniqueSound("cha_Ratchet_Helipack_activate");
					SFXManager.instance.PlayUniqueSound("cha_Ratchet_Helipack_Loop");
					if (!airSideJump)
					{
						isHeliBoost = true;
						heliSetCurve = true;
					}
					else
					{
						isHeliBoost = false;
					}
				}
				else if (curRatchetJumpCurve.keys[2].time < curJumpTime - jumpTimer)
				{
					isHeliActive = true;
					wasHeliActive = true;
					SFXManager.instance.PlayUniqueSound("cha_Ratchet_Helipack_activate");
					SFXManager.instance.PlayUniqueSound("cha_Ratchet_Helipack_Loop");
					isHeliBoost = false;
				}
			}
		}
		else
		{
			RatchetHeliStop();
			touchHoldTime = 0f;
		}
		RatchetHeliUpdate();
		checkDifficulty();
		if (ClankJet.activeSelf)
		{
			StatsTracker.instance.SetStat(StatsTracker.Stats.distanceFlown, travelDist);
		}
		StatsTracker.instance.SetStat(StatsTracker.Stats.distanceTraveled, travelDist);
		StatsTracker.instance.SetStat(StatsTracker.Stats.noCratesDistanceTraveled, travelDist - StatsTracker.instance.GetStat(StatsTracker.Stats.noCratesDistanceBase));
		StatsTracker.instance.SetStat(StatsTracker.Stats.noDamageDistanceTraveled, travelDist - StatsTracker.instance.GetStat(StatsTracker.Stats.noDamageDistanceBase));
		StatsTracker.instance.SetStat(StatsTracker.Stats.noBoltzDistanceTraveled, travelDist - StatsTracker.instance.GetStat(StatsTracker.Stats.noBoltzDistanceBase));
		switch (currentRail)
		{
		case 0:
			StatsTracker.instance.SetStat(StatsTracker.Stats.onMiddleRail, travelDist - railBaseValues[currentRail]);
			break;
		case 1:
			StatsTracker.instance.SetStat(StatsTracker.Stats.onRightRail, travelDist - railBaseValues[currentRail]);
			break;
		case 2:
			StatsTracker.instance.SetStat(StatsTracker.Stats.onLeftRail, travelDist - railBaseValues[currentRail]);
			break;
		}
	}

	public void DoJump(bool isSwitchingRails)
	{
		if (!isJumping)
		{
			if (!GadgetManager.instance.GetJetpackStatus())
			{
				switch (Random.Range(1, 3))
				{
				case 1:
					SFXManager.instance.PlaySound("DoubleJump1");
					break;
				case 2:
					SFXManager.instance.PlaySound("DoubleJump2");
					break;
				case 3:
					SFXManager.instance.PlaySound("DoubleJump3");
					break;
				}
			}
			if (grindEffect.particleSystem.isPlaying)
			{
				grindEffect.particleSystem.Pause();
			}
			if (GameController.instance.gameState == GameController.eGameState.GS_Grinding)
			{
				curLaneSwitchTime = railBehaviors.ratchetLaneSwitchCurve[railBehaviors.ratchetLaneSwitchCurve.length - 1].time;
				curJumpTime = railBehaviors.ratchetJumpCurve[railBehaviors.ratchetJumpCurve.length - 1].time;
				curRatchetJumpCurve = railBehaviors.ratchetJumpCurve;
				curRatchetLaneSwitchCurve = railBehaviors.ratchetLaneSwitchCurve;
				curRatchetSideJumpCurve = railBehaviors.ratchetSideJumpCurve;
				curRatchetHeliBoostCurve = railBehaviors.ratchetHeliBoostCurve;
				curHeliSlowModifier = railBehaviors.heliSlowModifier;
				boostHeight = railBehaviors.boostHeight;
			}
			else
			{
				curLaneSwitchTime = groundBehaviors.ratchetLaneSwitchCurve[groundBehaviors.ratchetLaneSwitchCurve.length - 1].time;
				curJumpTime = groundBehaviors.ratchetJumpCurve[groundBehaviors.ratchetJumpCurve.length - 1].time;
				curRatchetJumpCurve = groundBehaviors.ratchetJumpCurve;
				curRatchetLaneSwitchCurve = groundBehaviors.ratchetLaneSwitchCurve;
				curRatchetSideJumpCurve = groundBehaviors.ratchetSideJumpCurve;
				curRatchetHeliBoostCurve = groundBehaviors.ratchetHeliBoostCurve;
				curHeliSlowModifier = groundBehaviors.heliSlowModifier;
				boostHeight = groundBehaviors.boostHeight;
			}
			if (isSwitchingRails)
			{
				Vector3 velocity = base.rigidbody.velocity;
				float xOffset = base.rigidbody.position.x + curVelocityX * curLaneSwitchTime;
				float num = TileSpawnManager.instance.getRailNodePosition(targetRail, xOffset, TileSpawnManager.instance.railTileList[0]).z - base.rigidbody.position.z;
				velocity.z = num / curLaneSwitchTime;
				base.rigidbody.velocity = velocity;
				jumpTimer = curLaneSwitchTime;
			}
			else
			{
				jumpTimer = curJumpTime;
				StatsTracker.instance.UpdateStat(StatsTracker.Stats.timesJumped);
				if (MegaWeaponManager.instance.groovitronOn)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.groovitronCurrentJumps);
				}
				else if (MegaWeaponManager.instance.tornadoOn)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.tornadoLauncherCurrentJumps);
				}
				else if (MegaWeaponManager.instance.riftInducerOn)
				{
					StatsTracker.instance.UpdateStat(StatsTracker.Stats.riftInducerCurrentJumps);
				}
			}
			isJumping = true;
		}
		else if (airSideJump && isSwitchingRails)
		{
			Vector3 velocity2 = base.rigidbody.velocity;
			float xOffset2 = base.rigidbody.position.x + curVelocityX * curLaneSwitchTime;
			float num2 = TileSpawnManager.instance.getRailNodePosition(targetRail, xOffset2, TileSpawnManager.instance.railTileList[0]).z - base.rigidbody.position.z;
			velocity2.z = num2 / curLaneSwitchTime;
			ZVelocity = velocity2.z;
			if (isHeliActive)
			{
				velocity2.z = ZVelocity * (1f - curHeliSlowModifier);
			}
			base.rigidbody.velocity = velocity2;
			jumpTimer = curLaneSwitchTime;
		}
	}

	private void NextSwipeCheck()
	{
		int num = currentRail;
		bool flag = false;
		switch (nextSwipe)
		{
		case SwipeDirection.SD_Left:
			if (!IsSideJumping())
			{
				nextSwipe = SwipeDirection.SD_None;
				if (currentRail == 0)
				{
					num = 2;
					targetRail = 2;
					RatchetJumpLeft();
				}
				else if (currentRail == 1)
				{
					num = 0;
					targetRail = 0;
					RatchetJumpLeft();
				}
			}
			break;
		case SwipeDirection.SD_Right:
			if (!IsSideJumping())
			{
				nextSwipe = SwipeDirection.SD_None;
				if (currentRail == 2)
				{
					num = 0;
					targetRail = 0;
					RatchetJumpRight();
				}
				else if (currentRail == 0)
				{
					num = 1;
					targetRail = 1;
					RatchetJumpRight();
				}
			}
			break;
		case SwipeDirection.SD_Up:
			if (!isJumping)
			{
				nextSwipe = SwipeDirection.SD_None;
				flag = true;
				RatchetJumpCenter();
			}
			break;
		}
		if (flag)
		{
			DoJump(false);
		}
		else if (currentRail != num)
		{
			setNewRail(num);
		}
	}

	private void SetLandingTarget()
	{
		if (isJumping)
		{
			Vector3 zero = Vector3.zero;
			float num = 0f;
			num = ((!isHeliActive) ? (curVelocityX * jumpTimer) : (curVelocityX * (jumpTimer + jumpTimer * curHeliSlowModifier)));
			num += base.rigidbody.position.x;
			zero = ((!IsSideJumping()) ? TileSpawnManager.instance.getRailNodePosition(currentRail, num, TileSpawnManager.instance.railTileList[0]) : TileSpawnManager.instance.getRailNodePosition(targetRail, num, TileSpawnManager.instance.railTileList[0]));
			if (landingTarget != null)
			{
				landingTarget.transform.position = zero;
			}
			else if (landingTargetParticles != null)
			{
				landingTarget = GameObjectPool.instance.GetNextFree(landingTargetParticles.name, true);
				landingTarget.transform.position = zero;
				landingTarget.particleSystem.Play();
			}
		}
	}

	private void SetBoltEffectPosition()
	{
		if (boltPickupEffect != null)
		{
			boltPickupEffect.transform.position = base.rigidbody.position;
			boltPickupEffect.transform.parent = base.rigidbody.transform;
		}
	}

	public GameObject GetBoltPickupEffect()
	{
		return boltPickupEffect;
	}

	public float PlayBoltPickupEffect(float Size)
	{
		if (boltPickupEffectParticles != null)
		{
			boltPickupEffect = GameObjectPool.instance.GetNextFree(boltPickupEffectParticles.name, true);
			if (Size != 0f)
			{
				boltPickupEffect.particleSystem.startSize = Size;
			}
			if (isJumping)
			{
				return boltPickupEffect.particleSystem.startSize;
			}
			boltPickupEffect.particleSystem.Play(true);
			return boltPickupEffect.particleSystem.startSize;
		}
		return 0f;
	}

	public void PlayHitJumpPadEffect(GameObject jumpPadObject)
	{
		if (onHitJumpPadEffect != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(onHitJumpPadEffect.name, true);
			nextFree.transform.position = base.transform.position + jumpPadEffectOffset;
			nextFree.particleSystem.Play(true);
		}
	}

	public void hitJumpPad(GameObject jumpPadObject)
	{
		activeJumpPad = jumpPadObject.GetComponent<JumpPad>();
		SFXManager.instance.PlayUniqueSound("cha_Ratchet_BouncePad");
		RatchetJumpPadStart();
		PlayHitJumpPadEffect(jumpPadObject);
	}

	private void FixedUpdate()
	{
		if (GameController.instance.inMenu)
		{
			return;
		}
		UpdateFireIdle();
		if (RandomBlinkingOn)
		{
			ClankBlinkCounter++;
			if (ClankBlinkCounter % 130 == 0)
			{
				ClankBlink();
			}
			RatchetBlinkCounter++;
			if (RatchetBlinkCounter % 160 == 0)
			{
				RatchetBlink();
			}
		}
		CheckForRailIdleLeaning();
		NextSwipeCheck();
		SetLandingTarget();
		SetBoltEffectPosition();
		if (anim != null)
		{
			UpdateAnimationState();
		}
		if (isPlayingIntro)
		{
			CheckForIntro();
		}
		debugDrawCount = 0;
		if (GadgetManager.instance.GetJetpackStatus() && isNearTunnelRail())
		{
			GadgetManager.instance.DeactivateJetpack();
			GadgetManager.instance.invincibleLanding = false;
			GadgetManager.instance.jetpackInvincibleTimer = 0f;
		}
		bool flag = false;
		if ((activeSwingShot != null && activeSwingShot.isSwinging) || activeJumpPad != null)
		{
			flag = true;
			wasSwinging = true;
		}
		bool anyPoint = false;
		if (flag || isJumping || jumpToRail || GadgetManager.instance.GetJetpackStatus())
		{
			anyPoint = true;
		}
		Transform transform = base.rigidbody.transform;
		float num = Mathf.Abs(transform.position.x - lastPos.x);
		if (!isDead)
		{
			travelDist += num;
			if (UIManager.instance.GetHUD() != null)
			{
				UIManager.instance.GetHUD().UpdateDistanceTraveled();
			}
		}
		else
		{
			bool flag2 = currentBaseState.tagHash == a_DeathTag;
			base.rigidbody.MovePosition(deathBoxPos);
			if (wasInTransition && !anim.IsInTransition(0))
			{
				Debug.Log("start death anim");
				wasInTransition = false;
				anim.SetBool(a_CurrentIdleState, false);
				anim.SetBool("DeathStart", true);
				anim.SetBool(a_CurrentDeathState, true);
				anim.SetBool("Start", false);
			}
			if (flag2)
			{
				anim.SetBool(a_CurrentDeathState, false);
			}
			else if (!anim.IsInTransition(0))
			{
				anim.SetBool(a_CurrentIdleState, false);
				anim.SetBool("DeathStart", true);
				anim.SetBool(a_CurrentDeathState, true);
				anim.SetBool("Start", false);
			}
			if (((flag2 && anim.IsInTransition(0) && deathDelay > respawnTime) || deathDelay > 5f) && !GameController.instance.inMenu)
			{
				anim.enabled = false;
				GameController.instance.RoundEnded(!quitToMenu);
				quitToMenu = false;
			}
		}
		lastPos = transform.position;
		if (isOnTunnelRail() && currentRail != 0 && !isDead)
		{
			Kill(EDeathDealer.EDeath_Fall);
		}
		futurePos = base.rigidbody.position + base.rigidbody.velocity.normalized * 2f;
		debugDrawPos[0] = futurePos;
		int resultingRailNum;
		targetPos = getRailNodePosition(futurePos, base.rigidbody.velocity.normalized, currentRail, out resultingRailNum, anyPoint);
		currentRailListIndex = railNodeListIndex;
		if (!GadgetManager.instance.GetJetpackStatus())
		{
			cameraYLook = targetPos.y;
		}
		else
		{
			cameraYLook = base.rigidbody.position.y;
		}
		if (targetPos == Vector3.zero && resultingRailNum == -1)
		{
			if (flag || isJumping || jumpToRail)
			{
				targetPos = futurePos;
			}
			else
			{
				Debug.Log("!!>> no rail node found <<!!");
			}
		}
		debugDrawPos[1] = targetPos;
		if (currentRail != resultingRailNum)
		{
		}
		currentRail = resultingRailNum;
		if (flag && activeSwingShot != null)
		{
			targetPos = activeSwingShot.targetPos;
		}
		if (GadgetManager.instance.GetJetpackStatus())
		{
			Vector3 position = base.rigidbody.position;
			position.y = targetPos.y + GadgetManager.instance.GetJetpackAltitude();
			base.rigidbody.position = Vector3.Lerp(base.rigidbody.position, position, 6f * Time.fixedDeltaTime);
			targetPos.y = base.rigidbody.position.y;
		}
		float num2 = curVelocityX;
		if (flag)
		{
			num2 = swingSpeed;
		}
		if (!isDead && (!isJumping || flag))
		{
			if (currentBaseState.nameHash == a_Falling)
			{
				if (!anim.IsInTransition(0))
				{
					RatchetStopFalling();
				}
			}
			else if (currentBaseState.nameHash == a_HeliCenter && !anim.IsInTransition(0))
			{
				RatchetHeliStop();
			}
			if (base.rigidbody.useGravity)
			{
				isJumping = false;
			}
			if (wasSwinging && activeSwingShot == null && activeJumpPad == null)
			{
				if (currentBaseState.nameHash != a_Swingshot && currentBaseState.nameHash != a_JumpPad)
				{
					wasSwinging = false;
				}
				else
				{
					targetPos.x = base.rigidbody.position.x + 4f;
					targetPos.z = base.rigidbody.position.z;
				}
			}
			else if (!flag)
			{
			}
			targetDir = targetPos - base.rigidbody.position;
			Vector3 normalized = targetDir.normalized;
			normalized.x = 1f;
			normalized *= num2;
			base.rigidbody.velocity = normalized;
			base.rigidbody.useGravity = false;
		}
		else if (!isDead)
		{
			targetPos.y = base.rigidbody.position.y;
			targetDir = targetPos - base.rigidbody.position;
			if (jumpDirection != 0f && Mathf.Abs(targetDir.z) < 2f)
			{
				jumpDirection = 0f;
				jumpToRail = false;
			}
			Vector3 normalized2 = targetDir.normalized;
			normalized2.x = 1f;
			Vector3 velocity = normalized2 * num2;
			if (jumpDirection != 0f && base.rigidbody.velocity.z != 0f)
			{
				velocity.z = base.rigidbody.velocity.z;
				if (isHeliActive)
				{
					velocity.z = ZVelocity * (1f - curHeliSlowModifier);
				}
			}
			velocity.y = 0f;
			if (jumpTimer > 0f)
			{
				float y = TileSpawnManager.instance.getRailNodePosition(targetRail, base.rigidbody.position.x, TileSpawnManager.instance.railTileList[0]).y;
				float num3 = (airSideJump ? curRatchetSideJumpCurve.Evaluate(curLaneSwitchTime - jumpTimer) : ((jumpDirection == 0f) ? curRatchetJumpCurve.Evaluate(curJumpTime - jumpTimer) : curRatchetLaneSwitchCurve.Evaluate(curLaneSwitchTime - jumpTimer)));
				if (isHeliActive && isHeliBoost)
				{
					if (heliSetCurve)
					{
						int num4 = curRatchetHeliBoostCurve.length - 1;
						Keyframe key = new Keyframe(0f, num3);
						Keyframe key2 = new Keyframe(curRatchetHeliBoostCurve[1].time, num3 + boostHeight);
						jumpTimer += curRatchetHeliBoostCurve[1].time;
						Keyframe key3 = new Keyframe(jumpTimer, curRatchetHeliBoostCurve.keys[num4].value);
						key.inTangent = curRatchetHeliBoostCurve.keys[0].inTangent;
						key.outTangent = curRatchetHeliBoostCurve.keys[0].outTangent;
						key2.inTangent = curRatchetHeliBoostCurve.keys[1].inTangent;
						key2.outTangent = curRatchetHeliBoostCurve.keys[1].outTangent;
						key3.inTangent = curRatchetHeliBoostCurve.keys[num4].inTangent;
						key3.outTangent = curRatchetHeliBoostCurve.keys[num4].outTangent;
						heliSetCurve = false;
						curJumpTime = jumpTimer;
						curRatchetHeliBoostCurve.MoveKey(0, key);
						curRatchetHeliBoostCurve.MoveKey(1, key2);
						curRatchetHeliBoostCurve.MoveKey(num4, key3);
						curRatchetJumpCurve = curRatchetHeliBoostCurve;
					}
					else
					{
						num3 = curRatchetHeliBoostCurve.Evaluate(curJumpTime - jumpTimer);
					}
				}
				if (isHeliActive)
				{
					jumpTimer += curHeliSlowModifier * Time.fixedDeltaTime;
				}
				if (!GadgetManager.instance.GetJetpackStatus())
				{
					Vector3 position2 = base.rigidbody.position;
					position2.y = y + num3;
					cameraYLook = y;
					base.rigidbody.position = position2;
				}
			}
			base.rigidbody.velocity = velocity;
		}
		if (isJumping)
		{
			if (jumpTimer > 0f)
			{
				jumpTimer -= Time.fixedDeltaTime;
			}
			else
			{
				StopJump();
				jumpDirection = 0f;
				if (grindEffect.particleSystem.isPaused)
				{
					grindEffect.particleSystem.Play();
					if (!GadgetManager.instance.GetJetpackStatus())
					{
						SFXManager.instance.PlaySound("grind_connect");
						SFXManager.instance.PlaySound("cha_Ratchet_Land_VO");
					}
				}
				RatchetHeliStop();
				isJumping = false;
				airSideJump = false;
				ZVelocity = 0f;
			}
		}
		if (!isDead)
		{
			float angle = -90f + 57.29578f * Mathf.Atan2(base.rigidbody.velocity.x, base.rigidbody.velocity.z);
			Quaternion to = Quaternion.AngleAxis(angle, Vector3.up);
			base.rigidbody.rotation = Quaternion.Lerp(base.rigidbody.rotation, to, Time.fixedDeltaTime * 8f);
			float num5 = maxVelocityX;
			float num6 = 4f;
			if (GadgetManager.instance.GetReflectorHealth() > 0)
			{
				num5 = maxVelocityX * GadgetManager.instance.reflectorSpeedBoostMultiplier;
				num6 += 8f;
			}
			else if (GadgetManager.instance.GetJetpackStatus())
			{
				num5 = maxVelocityX * GadgetManager.instance.jetpackSpeedBoostMultiplier;
				num6 += 8f;
			}
			if (isPlayingIntro)
			{
				curVelocityX = 0.01f;
			}
			else if (curVelocityX < num5)
			{
				curVelocityX += Time.deltaTime * num6;
				if (curVelocityX > num5)
				{
					curVelocityX = num5;
				}
			}
			else if (curVelocityX > num5)
			{
				curVelocityX -= Time.deltaTime * 250f;
				if (curVelocityX < num5)
				{
					curVelocityX = maxVelocityX;
				}
			}
		}
		if (isDead && !lastDead)
		{
			base.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		}
		lastDead = isDead;
	}

	private void CheckForRailIdleLeaning()
	{
		if (!anim.GetBool("Rail") || GameController.instance.gameState == GameController.eGameState.GS_OnGround || isJumping)
		{
			return;
		}
		float num = 2.857143f * ((targetDir.z + lastDirZ1 + lastDirZ2) / 3f);
		if (currentBank > 0f - bankEpsilon && currentBank < bankEpsilon && num > 0f - bankEpsilon && num < bankEpsilon)
		{
			currentBank = Mathf.Lerp(currentBank, 0f, Time.fixedDeltaTime * bankLerp);
		}
		else if (currentBank > num)
		{
			currentBank = Mathf.Lerp(currentBank, currentBank - bankDelta, Time.fixedDeltaTime * bankLerp);
			if (currentBank < -1f)
			{
				currentBank = -1f;
			}
		}
		else if (currentBank < num)
		{
			currentBank = Mathf.Lerp(currentBank, currentBank + bankDelta, Time.fixedDeltaTime * bankLerp);
			if (currentBank > 1f)
			{
				currentBank = 1f;
			}
		}
		lastDirZ2 = lastDirZ1;
		lastDirZ1 = targetDir.z;
		anim.SetFloat("Bank", 0f - currentBank);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(base.rigidbody.position + Vector3.up, base.rigidbody.position + Vector3.up + base.rigidbody.velocity);
		Gizmos.DrawLine(base.rigidbody.position + Vector3.up, targetPos + Vector3.up);
		Gizmos.DrawCube(targetPos, Vector3.one);
		float num = 2f;
		int num2 = ((debugDrawCount >= debugDrawPos.Length) ? debugDrawPos.Length : debugDrawCount);
		for (int i = 0; i < num2; i++)
		{
			switch (i)
			{
			case 0:
				Gizmos.color = Color.green;
				num = 1f;
				Gizmos.DrawCube(debugDrawPos[i], Vector3.one * num);
				break;
			case 1:
				Gizmos.color = Color.red;
				num = 2f;
				Gizmos.DrawCube(debugDrawPos[i], Vector3.one * num);
				break;
			default:
				Gizmos.color = debugDrawColor[i];
				num = 1f;
				Gizmos.DrawWireCube(debugDrawPos[i], Vector3.one * num);
				break;
			}
		}
		num = 1f;
		Gizmos.color = Color.magenta;
		for (int j = 0; j < 3; j++)
		{
			Gizmos.DrawCube(debugPickPos[j], Vector3.one * num);
		}
	}

	public void ChangeAnimationState(GameController.eGameState state)
	{
		if (!(anim == null) && !isPlayingIntro)
		{
			if (state != GameController.eGameState.GS_OnGround)
			{
				RatchetRails();
			}
			else if (state == GameController.eGameState.GS_OnGround)
			{
				RatchetGround();
			}
		}
	}

	private void UpdateAnimationState()
	{
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		if (isDead)
		{
			CheckDeadStates();
		}
		else if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			CheckGroundStates();
		}
		else
		{
			CheckRailStates();
		}
	}

	private void CheckDeadStates()
	{
		if (anim.GetBool("DeathStart") && !anim.GetBool("DeathMenu") && !wasInTransition)
		{
			anim.SetBool("DeathStart", false);
		}
	}

	private void CheckRailStates()
	{
		if (currentBaseState.nameHash == a_Falling)
		{
			if (!anim.IsInTransition(0))
			{
				anim.SetBool("Falling", false);
			}
		}
		else if (currentBaseState.nameHash == a_RailHitReact)
		{
			if (!anim.IsInTransition(0))
			{
				anim.SetBool("RailHitReact", false);
				anim.SetBool("Rail", true);
			}
		}
		else if (currentBaseState.nameHash == a_Swingshot)
		{
			if (!anim.IsInTransition(0))
			{
				anim.SetBool("Swingshot", false);
				anim.SetBool("Rail", true);
			}
		}
		else if (currentBaseState.nameHash == a_JumpPad && !anim.IsInTransition(0))
		{
			anim.SetBool("JumpPad", false);
			anim.SetBool("Rail", true);
		}
	}

	private void CheckGroundStates()
	{
		if ((currentBaseState.nameHash == a_GroundFireOH || currentBaseState.nameHash == a_JetFire) && !anim.IsInTransition(0))
		{
			anim.SetBool("GroundFireOH", false);
			anim.SetBool("Ground", true);
		}
		if (currentBaseState.nameHash == a_GroundHitReact && !anim.IsInTransition(0))
		{
			anim.SetBool("GroundHitReactShot", false);
			anim.SetBool("Ground", true);
		}
	}

	public void SetSkin(string skin)
	{
		armorSkinString = skin;
		switch (skin)
		{
		case "HFlux":
			ArmorLevels = HFlux;
			break;
		case "EFlux":
			ArmorLevels = EFlux;
			break;
		case "TFlux":
			ArmorLevels = TFlux;
			break;
		default:
			ArmorLevels = HFlux;
			armorSkinString = "HFlux";
			break;
		}
	}

	private void LoseArmorPiece()
	{
		GameObject nextFree = GameObjectPool.instance.GetNextFree(loseShoulderArmor.name, true);
		nextFree.transform.position = LeftArmor.transform.position;
		nextFree.particleSystem.Play();
	}

	public void SetArmorLevel(int level)
	{
		switch (level)
		{
		case 1:
			if (LeftArmor.activeSelf)
			{
				LoseArmorPiece();
			}
			LeftArmor.SetActive(false);
			RightArmor.SetActive(false);
			HeadArmor.SetActive(false);
			RatchetRenderer.material.mainTexture = ArmorLevels[0];
			break;
		case 2:
			if (LeftArmor.activeSelf)
			{
				LoseArmorPiece();
			}
			LeftArmor.SetActive(false);
			RightArmor.SetActive(false);
			HeadArmor.SetActive(false);
			RatchetRenderer.material.mainTexture = ArmorLevels[0];
			break;
		case 3:
			if (LeftArmor.activeSelf)
			{
				LoseArmorPiece();
			}
			LeftArmor.SetActive(false);
			RightArmor.SetActive(false);
			HeadArmor.SetActive(false);
			RatchetRenderer.material.mainTexture = ArmorLevels[1];
			break;
		case 4:
			if (HeadArmor.activeSelf)
			{
				LoseArmorPiece();
			}
			LeftArmor.SetActive(true);
			RightArmor.SetActive(true);
			HeadArmor.SetActive(false);
			RatchetRenderer.material.mainTexture = ArmorLevels[2];
			break;
		case 5:
			LeftArmor.SetActive(true);
			RightArmor.SetActive(true);
			HeadArmor.SetActive(true);
			RatchetRenderer.material.mainTexture = ArmorLevels[3];
			break;
		}
	}

	public void UpdateArmorLevel()
	{
		SetArmorLevel(HP);
		if (UIManager.instance.GetHUD() != null)
		{
			UIManager.instance.GetHUD().UpdateHP();
		}
	}

	public void SetHP(int newHP)
	{
		if (newHP <= 5)
		{
			HP = newHP;
		}
	}

	public int GetHP()
	{
		return HP;
	}

	public int GetInitialHP()
	{
		int num = 2;
		if (PlayerPrefs.HasKey("ArmorLevel"))
		{
			num += PlayerPrefs.GetInt("ArmorLevel");
		}
		return num;
	}

	public bool IsPlayerDead()
	{
		return isDead;
	}

	public float GetTravelDist()
	{
		return StatsTracker.instance.GetStat(StatsTracker.Stats.distanceTraveled);
	}

	public int GetBolts()
	{
		return (int)StatsTracker.instance.GetStat(StatsTracker.Stats.boltzCollected);
	}

	public int GetBoltsTotal()
	{
		return (int)(StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.boltzCollected) - StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.boltzSpent));
	}

	public int GetBoltsLifetime()
	{
		return (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.boltzCollected);
	}

	public int GetDistanceLifetime()
	{
		return (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.distanceTraveled);
	}

	public int GetRaritanium()
	{
		int num = (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.raritaniumPurchased);
		return (int)(StatsTracker.instance.GetStat(StatsTracker.Stats.raritaniumCollected) + (float)num);
	}

	public int GetRaritaniumTotal()
	{
		return (int)((float)GetRaritanium() - StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.raritaniumDeposited));
	}

	public int GetLifetimeRaritaniumTotal()
	{
		return (int)(StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.raritaniumCollected) + StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.raritaniumPurchased) - StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.raritaniumDeposited));
	}

	public int GetTerachnoids()
	{
		return (int)StatsTracker.instance.GetStat(StatsTracker.Stats.terachnoidsCollected);
	}

	public int GetTerachnoidsTotal()
	{
		return (int)StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.terachnoidsCollected);
	}

	public int GetHeroBolts()
	{
		return (int)StatsTracker.instance.GetStat(StatsTracker.Stats.heroBoltsCollected);
	}

	public int GetHeroBoltsTotal()
	{
		return (int)(StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.heroBoltsCollected) - StatsTracker.instance.GetLifetimeStat(StatsTracker.Stats.heroBoltsUsed));
	}

	public void SetJetpack(bool val)
	{
		ClankJet.SetActive(val);
		anim.SetBool("JetCenter", val);
		anim.SetBool(a_CurrentIdleState, !val);
	}

	private void RatchetHitReact()
	{
		SFXManager.instance.PlaySound("cha_Ratchet_Gethit_VO");
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			anim.SetBool("GroundHitReactShot", true);
			anim.SetBool("Ground", false);
		}
		else
		{
			anim.SetBool("RailHitReact", true);
			anim.SetBool("Rail", false);
		}
	}

	private bool RatchetAirSideJumpCheck()
	{
		if (anim.GetBool("GroundJumpCenter") || anim.GetBool("RailJumpCenter"))
		{
			return true;
		}
		return false;
	}

	private void RatchetJumpLeft()
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			anim.SetBool("GroundJumpLeft", true);
			anim.SetBool(a_CurrentIdleState, false);
			if (RatchetAirSideJumpCheck())
			{
				anim.SetBool("GroundJumpCenter", false);
				airSideJump = true;
				AdjustAirSideJumpHeight();
				jumpTimer = curLaneSwitchTime;
			}
		}
		else
		{
			anim.SetBool("RailJumpLeft", true);
			anim.SetBool(a_CurrentIdleState, false);
			if (RatchetAirSideJumpCheck())
			{
				anim.SetBool("RailJumpCenter", false);
				airSideJump = true;
				AdjustAirSideJumpHeight();
				jumpTimer = curLaneSwitchTime;
			}
		}
	}

	private void RatchetJumpRight()
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			anim.SetBool("GroundJumpRight", true);
			anim.SetBool(a_CurrentIdleState, false);
			if (RatchetAirSideJumpCheck())
			{
				anim.SetBool("GroundJumpCenter", false);
				airSideJump = true;
				AdjustAirSideJumpHeight();
				jumpTimer = curLaneSwitchTime;
			}
		}
		else
		{
			anim.SetBool("RailJumpRight", true);
			anim.SetBool(a_CurrentIdleState, false);
			if (RatchetAirSideJumpCheck())
			{
				anim.SetBool("RailJumpCenter", false);
				airSideJump = true;
				AdjustAirSideJumpHeight();
				jumpTimer = curLaneSwitchTime;
			}
		}
	}

	private void AdjustAirSideJumpHeight()
	{
		Keyframe key = default(Keyframe);
		key.time = 0f;
		key.value = curRatchetJumpCurve.Evaluate(curJumpTime - jumpTimer);
		curRatchetSideJumpCurve.MoveKey(0, key);
	}

	private void RatchetJumpCenter()
	{
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			anim.SetBool("GroundJumpCenter", true);
			anim.SetBool(a_CurrentIdleState, false);
		}
		else
		{
			anim.SetBool("RailJumpCenter", true);
			anim.SetBool(a_CurrentIdleState, false);
		}
	}

	public void RatchetFireAnim()
	{
		anim.SetBool("GroundFireOH", true);
		anim.SetFloat("FireIdle", 1f);
		fireIdleTimer = fireIdleTime;
	}

	private void RatchetHeliUpdate()
	{
		if (isHeliActive && !isDead)
		{
			anim.SetBool("HeliCenter", true);
			HeliTop.renderer.enabled = true;
			HeliTop.SetActive(true);
			HeliTop.transform.Rotate(Vector3.up * Time.deltaTime * 1750f);
		}
	}

	private void RatchetStopFalling()
	{
		anim.SetBool(a_CurrentIdleState, true);
	}

	private void RatchetHeliStop()
	{
		anim.SetBool("HeliCenter", false);
		HeliTop.renderer.enabled = false;
		HeliTop.SetActive(false);
		isHeliActive = false;
		SFXManager.instance.StopSound("cha_Ratchet_Helipack_Loop");
	}

	private void RatchetRails()
	{
		if (isDead)
		{
			return;
		}
		if (!isPlayingIntro)
		{
			if (!grindEffect.particleSystem.isPlaying)
			{
				grindEffect.particleSystem.Play();
			}
		}
		else if (grindEffect.particleSystem.isPlaying)
		{
			grindEffect.particleSystem.Stop();
		}
		hoverEffectLeft.particleSystem.Stop();
		hoverEffectRight.particleSystem.Stop();
		anim.SetBool("Rail", true);
		anim.SetBool("Ground", false);
		a_CurrentIdleState = "Rail";
	}

	private void RatchetGround()
	{
		if (!isDead)
		{
			grindEffect.particleSystem.Stop();
			hoverEffectLeft.particleSystem.Play();
			hoverEffectRight.particleSystem.Play();
			anim.SetBool("Rail", false);
			anim.SetBool("Ground", true);
			a_CurrentIdleState = "Ground";
		}
	}

	private void StopJump()
	{
		anim.SetBool("GroundJumpLeft", false);
		anim.SetBool("GroundJumpRight", false);
		anim.SetBool("GroundJumpCenter", false);
		anim.SetBool("RailJumpLeft", false);
		anim.SetBool("RailJumpRight", false);
		anim.SetBool("RailJumpCenter", false);
		if (GameController.instance.gameState == GameController.eGameState.GS_OnGround)
		{
			anim.SetBool("Ground", true);
		}
		else
		{
			anim.SetBool("Rail", true);
		}
	}

	public void RatchetSwingStart()
	{
		if (grindEffect.particleSystem.isPlaying)
		{
			grindEffect.particleSystem.Pause();
		}
		anim.SetBool("Swingshot", true);
		anim.SetBool(a_CurrentIdleState, false);
	}

	public void RatchetSwingStop()
	{
		if (grindEffect.particleSystem.isPaused)
		{
			grindEffect.particleSystem.Play();
		}
		anim.SetBool("Swingshot", false);
		anim.SetBool(a_CurrentIdleState, true);
	}

	public void RatchetJumpPadStart()
	{
		anim.SetBool("JumpPad", true);
		anim.SetBool(a_CurrentIdleState, false);
	}

	public void RatchetJumpPadStop()
	{
		anim.SetBool("JumpPad", false);
		anim.SetBool(a_CurrentIdleState, true);
		activeJumpPad = null;
	}

	public bool IsSideJumping()
	{
		if (anim.GetBool("RailJumpRight") || anim.GetBool("RailJumpLeft") || anim.GetBool("GroundJumpRight") || anim.GetBool("GroundJumpLeft"))
		{
			return true;
		}
		return false;
	}

	public float RailJumpTime()
	{
		return railBehaviors.ratchetJumpCurve.keys[railBehaviors.ratchetLaneSwitchCurve.keys.Length - 1].time;
	}

	public float RailSwitchTime()
	{
		return railBehaviors.ratchetLaneSwitchCurve.keys[railBehaviors.ratchetLaneSwitchCurve.keys.Length - 1].time;
	}

	public float GetCameraYFollow()
	{
		return cameraYLook;
	}

	private void RatchetEyeLidsOn()
	{
		RatchetLeftEye.renderer.enabled = true;
		RatchetRightEye.renderer.enabled = true;
	}

	private void RatchetEyeLidsOff()
	{
		RatchetLeftEye.renderer.enabled = false;
		RatchetRightEye.renderer.enabled = false;
	}

	public void RatchetBlink(float time = 0.1f)
	{
		RatchetEyeLidsOn();
		StartCoroutine(RatchetFinishBlink(time));
	}

	private IEnumerator RatchetFinishBlink(float time)
	{
		yield return new WaitForSeconds(time);
		RatchetEyeLidsOff();
	}

	private void ClankEyeLidsOn()
	{
		for (int i = 0; i < 4; i++)
		{
			clankEyeLid[i].renderer.enabled = true;
		}
	}

	private void ClankEyeLidsOff()
	{
		for (int i = 0; i < 4; i++)
		{
			clankEyeLid[i].renderer.enabled = false;
		}
	}

	public void ClankBlink(float time = 0.08f)
	{
		ClankEyeLidsOn();
		StartCoroutine(ClankFinishBlink(time));
	}

	private IEnumerator ClankFinishBlink(float time)
	{
		yield return new WaitForSeconds(time);
		ClankEyeLidsOff();
	}

	public void EyeLidDeathAnimation()
	{
		RandomBlinkingOn = false;
		ClankEyeLidsOn();
		RatchetEyeLidsOn();
	}

	public void StartIntro()
	{
		isPlayingIntro = true;
		playIntroAnim = true;
	}

	public void AdjustAim(Vector3 target)
	{
		Vector3 vector = target - rightHand.position;
		vector.x = 0f;
		vector.Normalize();
		if (vector.z > 1f)
		{
			vector.z = 1f;
		}
		else if (vector.z < -1f)
		{
			vector.z = -1f;
		}
		if (vector.y > 1f)
		{
			vector.y = 1f;
		}
		else if (vector.y < -1f)
		{
			vector.y = -1f;
		}
		anim.SetFloat("Aim_X", 0f - vector.z);
		anim.SetFloat("Aim_Y", vector.y);
	}

	private void UpdateFireIdle()
	{
		if (fireIdleTimer > 0f)
		{
			fireIdleTimer -= Time.fixedDeltaTime;
			if (fireIdleTimer <= 0f)
			{
				lerpFireIdle = 1f;
			}
		}
		else if (lerpFireIdle > 0f)
		{
			lerpFireIdle -= lerpFireIdleAmount;
			anim.SetFloat("FireIdle", lerpFireIdle);
		}
	}

	public void ForceNextJump(bool jumpRight)
	{
		if (jumpRight)
		{
			nextSwipe = SwipeDirection.SD_Right;
		}
		else
		{
			nextSwipe = SwipeDirection.SD_Left;
		}
	}

	public void PlayOnHitParticle()
	{
		if (onHitEffect != null)
		{
			GameObject nextFree = GameObjectPool.instance.GetNextFree(onHitEffect.name, true);
			nextFree.transform.position = base.rigidbody.position + offsetEffect;
			nextFree.particleSystem.Play();
		}
	}

	public void FireCheck(Vector3 target)
	{
		if (dontFire || UIManager.instance.GetHUD().CountdownLabel.gameObject.activeSelf || swipeRegistered || wasHeliActive)
		{
			dontFire = false;
		}
		else if (holdWaitTimer > 0f)
		{
			if (WeaponsManager.instance.FireWeapon(target))
			{
				RatchetFireAnim();
			}
		}
		else if (WeaponsManager.instance.EndHoldFire(target))
		{
			RatchetFireAnim();
		}
	}

	public void HoldCheck(Vector3 target)
	{
		if (dontFire || UIManager.instance.GetHUD().CountdownLabel.gameObject.activeSelf || swipeRegistered || wasHeliActive)
		{
			dontFire = false;
		}
		else if (holdWaitTimer <= 0f && !isJumping)
		{
			WeaponsManager.instance.HoldSpecialWeapon(target);
			anim.SetFloat("FireIdle", 1f);
			fireIdleTimer = fireIdleTime;
		}
	}
}
