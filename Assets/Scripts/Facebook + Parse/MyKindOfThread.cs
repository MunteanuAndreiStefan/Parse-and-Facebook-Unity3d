using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class MyKindOfThread : MonoBehaviour
{
	public static int NumberOfThreads;
	public static int MaximumNumberOfThreads = 4;
	private int _count;
	private static MyKindOfThread ThisCurrent;
	public static bool Running;
	List<Action> CurrentActionsList = new List<Action>();
	private List<DelayedQueue> DelayedList = new  List<DelayedQueue>();
	List<DelayedQueue> CurrentDelayedList = new List<DelayedQueue>();
	private List<Action> Actions = new List<Action>();
	public static MyKindOfThread Current
	{
		get
		{
			FirstRun();
			return ThisCurrent;
		}
	}
	public struct DelayedQueue{
		public float time;
		public Action action;
	}
	void Awake()
	{
		ThisCurrent = this;
		Running = true;
	}
	static void FirstRun()
	{
		if (!Running)
		{
		
			if(!Application.isPlaying)
				return;
			Running = true;
			var g = new GameObject("MyKindOfThread");
			ThisCurrent = g.AddComponent<MyKindOfThread>();
		}
			
	}
	public static void ExecuteOnMainThreadAtFirstUpdate(Action action)
	{
		ExecuteOnMainThreadAtFirstUpdate( action, 0f);
	}
	public static void ExecuteOnMainThreadAtFirstUpdate(Action action, float time)
	{
		if(time != 0)
		{
			lock(Current.DelayedList)
			{
				Current.DelayedList.Add(new DelayedQueue { time = Time.time + time, action = action});
			}
		}
		else
		{
			lock (Current.Actions)
			{
				Current.Actions.Add(action);
			}
		}
	}
	void OnDisable()
	{
		if (ThisCurrent == this)
		{
			
			ThisCurrent = null;
		}
	}
	void Update()
	{
		lock (Actions)
		{
			CurrentActionsList.Clear();
			CurrentActionsList.AddRange(Actions);
			Actions.Clear();
		}
		foreach(var a in CurrentActionsList)
		{
			a();
		}
		lock(DelayedList)
		{
			CurrentDelayedList.Clear();
			CurrentDelayedList.AddRange(DelayedList.Where(d=>d.time <= Time.time));
			foreach(var item in CurrentDelayedList)
			DelayedList.Remove(item);
		}
		foreach(var i in CurrentDelayedList)
		{
			i.action();
		}
	}
}