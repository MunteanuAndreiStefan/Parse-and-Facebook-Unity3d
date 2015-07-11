using UnityEngine;
using Action=System.Action;
using System.Collections;
using System.Collections.Generic;

public class MyKindOfThreadDispatcher : IMyKindOfThread {
		private readonly List<Action> actions = new List<Action>();
		
		public void ExecuteOnMainThreadAtFirstUpdate(Action action) {
			lock (actions) {
				actions.Add(action);
			}
		}
		public void Update() {
			Action[] actionsToRun = null;
			lock (actions) {
				actionsToRun = actions.ToArray();
				actions.Clear();
			}
			foreach (Action action in actionsToRun) {
				action();
			}
		}
	}