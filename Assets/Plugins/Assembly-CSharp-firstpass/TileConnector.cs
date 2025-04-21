using UnityEngine;

public class TileConnector : MonoBehaviour
{
	protected GameObject left;

	protected GameObject right;

	protected GameObject front;

	protected GameObject rear;

	private void Start()
	{
		UpdateSockets();
	}

	public void UpdateSockets()
	{
		Transform transform = base.transform.Find("leftSocket");
		if ((bool)transform)
		{
			left = transform.gameObject;
		}
		transform = base.transform.Find("rightSocket");
		if ((bool)transform)
		{
			right = transform.gameObject;
		}
		transform = base.transform.Find("frontSocket");
		if ((bool)transform)
		{
			front = transform.gameObject;
		}
		transform = base.transform.Find("rearSocket");
		if ((bool)transform)
		{
			rear = transform.gameObject;
		}
	}

	public void ConnectToLeft(GameObject target, bool restoreHierarchy = false)
	{
		if (!target)
		{
			Debug.LogError("Invalid target");
		}
		if ((bool)left)
		{
			Transform parent = target.transform.parent;
			target.transform.parent = left.transform;
			target.transform.localPosition = Vector3.zero;
			if (restoreHierarchy)
			{
				target.transform.parent = parent;
			}
		}
		else
		{
			Debug.LogError("There is no left socket!");
		}
	}

	public void ConnectToRight(GameObject target, bool restoreHierarchy = false)
	{
		if (!target)
		{
			Debug.LogError("Invalid target");
		}
		if ((bool)right)
		{
			Transform parent = target.transform.parent;
			target.transform.parent = right.transform;
			target.transform.localPosition = Vector3.zero;
			if (restoreHierarchy)
			{
				target.transform.parent = parent;
			}
		}
		else
		{
			Debug.LogError("There is no right socket!");
		}
	}

	public void ConnectToFront(GameObject target, bool restoreHierarchy = false)
	{
		if (!target)
		{
			Debug.LogError("Invalid target");
		}
		if ((bool)front)
		{
			Transform parent = target.transform.parent;
			target.transform.parent = front.transform;
			target.transform.localPosition = Vector3.zero;
			if (restoreHierarchy)
			{
				target.transform.parent = parent;
			}
		}
		else
		{
			Debug.LogError("There is no left socket!");
		}
	}

	public void ConnectToFront(TileConnector targetConnector, bool restoreHierarchy = false)
	{
		if (!targetConnector)
		{
			Debug.LogError("Invalid connector");
		}
		Transform parent = targetConnector.gameObject.transform.parent;
		ConnectToFront(targetConnector.gameObject);
		targetConnector.gameObject.transform.localPosition = -targetConnector.rear.transform.localPosition;
		if (restoreHierarchy)
		{
			targetConnector.gameObject.transform.parent = parent;
		}
	}

	private void OnDrawGizmos()
	{
		if ((bool)left)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(left.transform.position, 0.05f);
		}
		if ((bool)right)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(right.transform.position, 0.05f);
		}
		if ((bool)front)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(front.transform.position, 0.05f);
		}
		if ((bool)rear)
		{
			Gizmos.color = Color.red + Color.blue;
			Gizmos.DrawSphere(rear.transform.position, 0.05f);
		}
	}
}
