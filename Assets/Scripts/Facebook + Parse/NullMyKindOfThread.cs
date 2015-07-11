using UnityEngine;
using Action=System.Action;

public class NullMyKindOfThread : IMyKindOfThread {
		public void ExecuteOnMainThreadAtFirstUpdate(Action action) {}
	}