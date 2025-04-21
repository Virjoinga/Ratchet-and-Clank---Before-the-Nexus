using UnityEngine;

public class RailComponent : MonoBehaviour
{
	public Vector3[] rail1Indices;

	public Vector3[] rail2Indices;

	public Vector3[] rail3Indices;

	public Vector3[] rail1TransformedIndices;

	public Vector3[] rail2TransformedIndices;

	public Vector3[] rail3TransformedIndices;

	public Vector3 frontSocket;

	public Vector3 rearSocket;

	public RailComponent nextRail;

	private void Awake()
	{
		rail1TransformedIndices = new Vector3[rail1Indices.Length];
		rail2TransformedIndices = new Vector3[rail2Indices.Length];
		rail3TransformedIndices = new Vector3[rail3Indices.Length];
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (rail1Indices.Length > 0)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(rail1Indices[0]), 0.5f);
			for (int i = 1; i <= rail1Indices.Length - 1; i++)
			{
				Gizmos.DrawLine(base.transform.TransformPoint(rail1Indices[i - 1]), base.transform.TransformPoint(rail1Indices[i]));
			}
		}
		Gizmos.color = Color.green;
		if (rail2Indices.Length > 0)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(rail2Indices[0]), 0.5f);
			for (int j = 1; j <= rail2Indices.Length - 1; j++)
			{
				Gizmos.DrawLine(base.transform.TransformPoint(rail2Indices[j - 1]), base.transform.TransformPoint(rail2Indices[j]));
			}
		}
		Gizmos.color = Color.blue;
		if (rail3Indices.Length > 0)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(rail3Indices[0]), 0.5f);
			for (int k = 1; k <= rail3Indices.Length - 1; k++)
			{
				Gizmos.DrawLine(base.transform.TransformPoint(rail3Indices[k - 1]), base.transform.TransformPoint(rail3Indices[k]));
			}
		}
		Gizmos.color = Color.black;
		Gizmos.DrawCube(base.transform.TransformPoint(rearSocket), Vector3.one);
		Gizmos.color = Color.grey;
		Gizmos.DrawCube(base.transform.TransformPoint(frontSocket), Vector3.one);
	}

	public void ConnectRail(RailComponent target)
	{
		if (!target)
		{
			Debug.LogError("Invalid target");
		}
		target.transform.position = base.transform.TransformPoint(frontSocket) - (target.transform.TransformPoint(target.rearSocket) - target.transform.position);
		nextRail = target;
	}

	public void TransformAllPoints()
	{
		for (int i = 0; i < rail1Indices.Length; i++)
		{
			rail1TransformedIndices[i] = base.transform.TransformPoint(rail1Indices[i]);
		}
		for (int j = 0; j < rail2Indices.Length; j++)
		{
			rail2TransformedIndices[j] = base.transform.TransformPoint(rail2Indices[j]);
		}
		for (int k = 0; k < rail3Indices.Length; k++)
		{
			rail3TransformedIndices[k] = base.transform.TransformPoint(rail3Indices[k]);
		}
	}
}
