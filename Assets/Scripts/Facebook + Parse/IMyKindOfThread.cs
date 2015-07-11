using UnityEngine;
using Action=System.Action;

public interface IMyKindOfThread {
		void ExecuteOnMainThreadAtFirstUpdate(Action action);
	}