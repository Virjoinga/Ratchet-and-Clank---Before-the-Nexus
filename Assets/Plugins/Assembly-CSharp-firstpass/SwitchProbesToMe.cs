using UnityEngine;

public class SwitchProbesToMe : MonoBehaviour
{
	public LightProbes myLightProbes;

	public string playerTag = "player";

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == playerTag)
		{
			LightmapSettings.lightProbes = myLightProbes;
			LightCorrection lightCorrection = other.gameObject.GetComponent("LightCorrection") as LightCorrection;
			lightCorrection.levelOrigin = base.transform;
			Debug.Log("SwitchProbesToMe: switched lightprobes group");
		}
	}
}
