using UnityEngine;

public class LightCorrection : MonoBehaviour
{
	public Transform levelOrigin;

	protected GameObject correctedProxy;

	private void Start()
	{
		if (!base.renderer.useLightProbes)
		{
			Debug.LogError("LightCorrection: Start called for an object, which doesn't use lightprobes!");
			base.enabled = false;
			return;
		}
		levelOrigin = base.transform.root;
		correctedProxy = new GameObject();
		correctedProxy.name = base.name + "_LightProbeAnchor";
		base.renderer.lightProbeAnchor = correctedProxy.transform;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0f, 0f);
		if (correctedProxy != null)
		{
			Gizmos.DrawCube(correctedProxy.transform.position, Vector3.one);
		}
	}

	private void Update()
	{
		correctedProxy.transform.position = levelOrigin.InverseTransformPoint(base.transform.position);
	}
}
