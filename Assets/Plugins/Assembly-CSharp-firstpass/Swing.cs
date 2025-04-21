using UnityEngine;

public class Swing : MonoBehaviour
{
	public bool isActive;

	public bool isSwinging;

	public bool swingAutoActivate;

	public bool sfxActivated;

	private bool swingAutoActivateReset;

	public float swingAutoActivateRange = 75f;

	public float swingActivateRange = 50f;

	public Vector3 targetPos;

	public int targetRail = -1;

	public float grappleRotateSpeed = 180f;

	private Vector3 spinRot;

	public LineRenderer lineRenderer;

	private CameraFollow cameraFollowCache;

	private GameObject SwingShotFX;

	private GameObject SwingShotFXGreen;

	private GameObject GodrayL;

	private GameObject GodrayR;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.enabled = false;
		cameraFollowCache = Camera.main.GetComponent<CameraFollow>();
		SpawnInit();
	}

	public void SpawnInit()
	{
		isActive = false;
		isSwinging = false;
		sfxActivated = true;
		spinRot = Vector3.zero;
		SwingShotFX = base.gameObject.transform.Find("FX_SwingShot").gameObject;
		SwingShotFXGreen = base.gameObject.transform.Find("FX_SwingShotGreen").gameObject;
		GodrayL = base.gameObject.transform.Find("GodrayL").gameObject;
		GodrayR = base.gameObject.transform.Find("GodrayR").gameObject;
		SwingShotFX.GetComponent<ParticleSystem>().Play();
		SwingShotFXGreen.GetComponent<ParticleSystem>().Stop();
		GodrayL.GetComponent<ParticleSystem>().Stop();
		GodrayR.GetComponent<ParticleSystem>().Stop();
	}

	private void SwingOn()
	{
		Debug.Log(base.name + ".SwingOn()");
		if (cameraFollowCache.grappleCamMode == 0)
		{
			cameraFollowCache.grappleCamMode = 1;
		}
		PlayerController playerController = GameController.instance.playerController;
		if (playerController.activeSwingShot != null)
		{
			playerController.activeSwingShot.SwingOff();
		}
		SFXManager.instance.PlaySound("cha_Ratchet_swingshot_activate");
		playerController.RatchetSwingStart();
		playerController.activeSwingShot = this;
		lineRenderer.enabled = true;
		swingAutoActivateReset = true;
		isSwinging = true;
		isActive = false;
	}

	public void SwingOff()
	{
		Debug.Log(base.name + ".SwingOff()");
		PlayerController playerController = GameController.instance.playerController;
		if (playerController.activeSwingShot != null && playerController.activeSwingShot == this)
		{
			playerController.activeSwingShot = null;
		}
		SFXManager.instance.PlaySound("cha_Ratchet_swingshot_deactivate");
		lineRenderer.enabled = false;
		isSwinging = false;
		SwingShotFX.GetComponent<ParticleSystem>().Stop();
		SwingShotFXGreen.GetComponent<ParticleSystem>().Play();
		GodrayL.GetComponent<ParticleSystem>().Stop();
		GodrayR.GetComponent<ParticleSystem>().Stop();
	}

	public bool CanSwing()
	{
		if (GameController.instance.playerController.IsPlayerDead())
		{
			return false;
		}
		if (GameController.instance.playerController.currentRail != 0)
		{
			return false;
		}
		if (GameController.instance.playerController.transform.position.x > base.transform.position.x)
		{
			return false;
		}
		return true;
	}

	private void RotateGrapple()
	{
		spinRot += Vector3.up * Time.deltaTime * grappleRotateSpeed;
	}

	private void Update()
	{
		if (isSwinging)
		{
			float y = GameController.instance.playerController.transform.position.y;
			float x = base.transform.position.x;
			targetPos.y = y;
			targetPos.x = x;
			targetPos.z = base.transform.position.z;
			if (GameController.instance.playerController.transform.position.x > base.transform.position.x + 16f)
			{
				SwingOff();
			}
			RotateGrapple();
		}
		else if (isActive)
		{
			if ((base.transform.position - GameController.instance.playerController.transform.position).magnitude < swingActivateRange && CanSwing())
			{
				SwingOn();
			}
			if (sfxActivated)
			{
				SFXManager.instance.PlaySound("cha_Ratchet_swingshot_connect");
				sfxActivated = false;
			}
			RotateGrapple();
			SwingShotFX.GetComponent<ParticleSystem>().Stop();
			SwingShotFXGreen.GetComponent<ParticleSystem>().Play();
			GodrayL.GetComponent<ParticleSystem>().Play();
			GodrayR.GetComponent<ParticleSystem>().Play();
		}
		else
		{
			if (swingAutoActivate && GameController.instance.playerController.currentRail == 0)
			{
				Vector3 vector = base.transform.position - GameController.instance.playerController.transform.position;
				if (swingAutoActivateReset)
				{
					if (vector.magnitude > swingAutoActivateRange)
					{
						swingAutoActivateReset = false;
					}
				}
				else if (vector.magnitude < swingAutoActivateRange)
				{
					isActive = true;
				}
			}
			if (lineRenderer.enabled)
			{
				SwingOff();
			}
		}
		base.transform.rotation = Quaternion.Euler(spinRot);
	}

	private void LateUpdate()
	{
		if (lineRenderer.enabled)
		{
			lineRenderer.SetPosition(0, base.transform.position);
			lineRenderer.SetPosition(1, GameController.instance.playerController.leftHand.position);
		}
	}

	private void OnDrawGizmos()
	{
		if (isSwinging)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(targetPos, Vector3.one);
		}
	}
}
