using UnityEngine; 
using System.Collections.Generic; 
using Action=System.Action;

public class PutOnGameObject : MonoBehaviour {
	
	private static NullMyKindOfThread EmptyMyKind = new NullMyKindOfThread();
	private static MyKindOfThreadDispatcher Current;
	public static IMyKindOfThread MyKindOfThread {
		get {
			if (Current != null) {
				return Current as IMyKindOfThread;
			}
			return EmptyMyKind as IMyKindOfThread;
		}
	}
	void Awake() {
		Current = new MyKindOfThreadDispatcher();
	}
	void OnDestroy() {
		Current = null;
	}
	void Update() {
		if (Application.isPlaying) {
			Current.Update();
		}
	}
}