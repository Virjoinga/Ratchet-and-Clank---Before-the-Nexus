using System;
using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public enum CameraSettings
	{
		Rails = 0,
		Ground = 1,
		Tunnel = 2,
		Boss = 3,
		Death = 4
	}

	[Serializable]
	public class CameraSetup
	{
		public CameraSettings Name;

		public Transform target;

		public Vector3 camOffset;

		public Vector3 lookOffset;

		public Vector3 grappleCamOffset = new Vector3(-5f, 5f, -10f);

		public Vector3 grappleLookOffset = new Vector3(5f, 5f, 0f);

		public float grappleLerpSpeed = 2f;

		public Vector3 jumppadCamOffset = new Vector3(-5f, 5f, -10f);

		public Vector3 jumppadLookOffset = new Vector3(5f, 5f, 0f);

		public float jumppadLerpSpeed = 2f;

		public float camLerpSpeed;

		private float _camSpeed;

		public float fogDist;

		public Color fogColor;

		public bool freezeCameraWhenDead;

		public int grappleCamMode;

		public int jumppadCamMode;

		private Vector3 _oldLook = Vector3.zero;

		private Vector3 _introLook = Vector3.zero;

		public float camSpeed
		{
			get
			{
				return _camSpeed;
			}
			set
			{
				_camSpeed = value;
			}
		}

		public Vector3 oldLook
		{
			get
			{
				return _oldLook;
			}
			set
			{
				_oldLook = value;
			}
		}

		public Vector3 introLook
		{
			get
			{
				return _introLook;
			}
			set
			{
				_introLook = value;
			}
		}
	}

	private Transform target;

	private Vector3 camOffset;

	private Vector3 lookOffset;

	private Vector3 jumppadCamOffset = new Vector3(-5f, 5f, -10f);

	private Vector3 jumppadLookOffset = new Vector3(5f, 5f, 0f);

	private float jumppadLerpSpeed = 2f;

	private bool jumppadCam;

	private Vector3 grappleCamOffset = new Vector3(-5f, 5f, -10f);

	private Vector3 grappleLookOffset = new Vector3(5f, 5f, 0f);

	private float grappleLerpSpeed = 2f;

	private float camLerpSpeed;

	private float camSpeed;

	private float fogDist;

	private Color fogColor;

	private bool freezeCameraWhenDead;

	private int _grappleCamMode;

	private Vector3 oldLook = Vector3.zero;

	private Vector3 introLook = Vector3.zero;

	private int lastCamMode;

	private bool firstCamUse = true;

	private CameraSettings curCamSet;

	public CameraSetup[] cameraSettings;

	public int grappleCamMode
	{
		get
		{
			return _grappleCamMode;
		}
		set
		{
			_grappleCamMode = value;
		}
	}

	private void Start()
	{
		for (int i = 0; i < 5; i++)
		{
			cameraSettings[i].target = GameController.instance.ratchet.transform;
		}
		SwitchCameraSettings(CameraSettings.Rails);
		RenderSettings.fogEndDistance = fogDist;
		RenderSettings.fogColor = fogColor;
		if (base.GetComponent<Animation>() != null)
		{
			base.GetComponent<Animation>().Stop("Cam_Start");
		}
	}

	private void Update()
	{
		if (!(TileSpawnManager.instance.railTileList[0] != null))
		{
			return;
		}
		PlayerController playerController = GameController.instance.playerController;
		string text = TileSpawnManager.instance.railTileList[0].name;
		if (text.Equals("TIL_PGO02") || text.Equals("TIL_PJO02"))
		{
			if ((playerController.activeSwingShot != null && playerController.activeSwingShot.isSwinging) || playerController.activeJumpPad != null)
			{
				if (playerController.activeSwingShot != null)
				{
					playerController.RatchetSwingStart();
				}
				else
				{
					jumppadCam = true;
				}
				grappleCamMode = 1;
				lastCamMode = 1;
			}
		}
		else if (text.Equals("TIL_PGO03") || text.Equals("TIL_PJO03"))
		{
			if (!(playerController.transform.position.x > TileSpawnManager.instance.railTileList[0].transform.position.x))
			{
				return;
			}
			if (lastCamMode == 1)
			{
				if (playerController.activeJumpPad != null)
				{
					playerController.RatchetJumpPadStop();
				}
				else
				{
					playerController.RatchetSwingStop();
				}
			}
			jumppadCam = false;
			grappleCamMode = 2;
			lastCamMode = 2;
		}
		else if (!text.Contains("TIL_PG") && !text.Contains("TIL_PJ"))
		{
			grappleCamMode = 0;
			lastCamMode = 0;
		}
	}

	public void StartIntroCamera()
	{
		if (base.GetComponent<Animation>() != null)
		{
			if (firstCamUse)
			{
				firstCamUse = false;
				base.GetComponent<Animation>().Play("Cam_Start");
			}
			else
			{
				StartCoroutine("startSoon");
			}
		}
	}

	private IEnumerator startSoon()
	{
		yield return new WaitForSeconds(0.2f);
		base.GetComponent<Animation>().Play("Cam_Start");
	}

	public void ResetCamera()
	{
		Vector3 worldPosition = GameController.instance.ratchet.GetComponent<Rigidbody>().position + lookOffset;
		Vector3 position = GameController.instance.ratchet.GetComponent<Rigidbody>().position + camOffset;
		position.y = GameController.instance.ratchet.GetComponent<Rigidbody>().position.y + camOffset.y;
		position.x = GameController.instance.ratchet.GetComponent<Rigidbody>().position.x + camOffset.x;
		base.transform.position = position;
		base.transform.LookAt(worldPosition);
		oldLook = worldPosition;
	}

	private void LateUpdate()
	{
		Vector3 vector = target.position + lookOffset;
		Vector3 vector2 = target.position + camOffset;
		if (base.GetComponent<Animation>().IsPlaying("Cam_Start"))
		{
			introLook = base.transform.forward;
			return;
		}
		Vector3 position;
		if (grappleCamMode == 1)
		{
			float num = grappleLerpSpeed;
			if (jumppadCam)
			{
				vector2 = GameController.instance.playerController.RatchetRoot.position + jumppadCamOffset;
				vector = GameController.instance.playerController.RatchetHips.position + jumppadLookOffset;
				num = jumppadLerpSpeed;
			}
			else
			{
				vector2 = target.position + grappleCamOffset;
				vector = target.position + grappleLookOffset;
			}
			position = Vector3.Lerp(base.transform.position, vector2, Time.deltaTime * num);
			position.x = vector2.x;
		}
		else if (grappleCamMode == 2)
		{
			vector2 = target.position + camOffset;
			position = Vector3.Lerp(base.transform.position, vector2, Time.deltaTime * camLerpSpeed);
			if (Vector3.Distance(base.transform.position, vector2) < 1f)
			{
				grappleCamMode = 0;
			}
		}
		else if (GameController.instance.playerController.activeSwingShot == null)
		{
			vector.y = GameController.instance.playerController.GetCameraYFollow() + lookOffset.y;
			if (freezeCameraWhenDead && GameController.instance.playerController.IsPlayerDead())
			{
				vector2.y = target.position.y + camOffset.y;
				vector2.x = target.position.x + camOffset.x;
				vector.y = target.position.y + lookOffset.y;
			}
			else
			{
				vector2.y = GameController.instance.playerController.GetCameraYFollow() + camOffset.y;
				vector2.x = target.position.x + camOffset.x;
			}
			if (introLook != Vector3.zero)
			{
				float num2 = (vector.y - base.transform.position.y) / introLook.y;
				oldLook = base.transform.position + introLook * num2;
				camSpeed = camLerpSpeed * 0.1f;
				introLook = Vector3.zero;
			}
			position = Vector3.Lerp(base.transform.position, vector2, Time.deltaTime * camSpeed);
			Vector3 vector3 = Vector3.Lerp(oldLook, vector, Time.deltaTime * camSpeed);
			camSpeed = camLerpSpeed;
			vector = vector3;
			position.z = vector2.z;
		}
		else
		{
			position = vector2;
		}
		base.transform.position = position;
		GameController.instance.playerController.IsPlayerDead();
		base.transform.LookAt(vector);
		oldLook = vector;
	}

	public void SwitchCameraSettings(CameraSettings cset)
	{
		target = cameraSettings[(int)cset].target;
		camOffset = cameraSettings[(int)cset].camOffset;
		lookOffset = cameraSettings[(int)cset].lookOffset;
		grappleCamOffset = cameraSettings[(int)cset].grappleCamOffset;
		grappleLookOffset = cameraSettings[(int)cset].grappleLookOffset;
		grappleLerpSpeed = cameraSettings[(int)cset].grappleLerpSpeed;
		jumppadCamOffset = cameraSettings[(int)cset].jumppadCamOffset;
		jumppadLookOffset = cameraSettings[(int)cset].jumppadLookOffset;
		jumppadLerpSpeed = cameraSettings[(int)cset].jumppadLerpSpeed;
		camLerpSpeed = cameraSettings[(int)cset].camLerpSpeed;
		fogDist = cameraSettings[(int)cset].fogDist;
		fogColor = cameraSettings[(int)cset].fogColor;
		freezeCameraWhenDead = cameraSettings[(int)cset].freezeCameraWhenDead;
		grappleCamMode = cameraSettings[(int)cset].grappleCamMode;
	}
}
