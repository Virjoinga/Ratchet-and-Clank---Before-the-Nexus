using UnityEngine;

public class MecanimEventEmitter : MonoBehaviour
{
	public Object animatorController;

	public Animator animator;

	public MecanimEventEmitTypes emitType;

	private void Start()
	{
		if (animator == null)
		{
			Debug.LogWarning("Do not find animator component.");
			base.enabled = false;
		}
		else if (animatorController == null)
		{
			Debug.LogWarning("Please assgin animator in editor. Add emitter at runtime is not currently supported.");
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (GameController.instance.isPaused)
		{
			return;
		}
		MecanimEvent[] events = MecanimEventManager.GetEvents(animatorController.GetInstanceID(), animator);
		MecanimEvent[] array = events;
		foreach (MecanimEvent mecanimEvent in array)
		{
			MecanimEvent.SetCurrentContext(mecanimEvent);
			switch (emitType)
			{
			case MecanimEventEmitTypes.Upwards:
				if (mecanimEvent.paramType != 0)
				{
					SendMessageUpwards(mecanimEvent.functionName, mecanimEvent.parameter, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					SendMessageUpwards(mecanimEvent.functionName, SendMessageOptions.DontRequireReceiver);
				}
				break;
			case MecanimEventEmitTypes.Broadcast:
				if (mecanimEvent.paramType != 0)
				{
					BroadcastMessage(mecanimEvent.functionName, mecanimEvent.parameter, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					BroadcastMessage(mecanimEvent.functionName, SendMessageOptions.DontRequireReceiver);
				}
				break;
			default:
				if (mecanimEvent.paramType != 0)
				{
					SendMessage(mecanimEvent.functionName, mecanimEvent.parameter, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					SendMessage(mecanimEvent.functionName, SendMessageOptions.DontRequireReceiver);
				}
				break;
			}
		}
	}
}
