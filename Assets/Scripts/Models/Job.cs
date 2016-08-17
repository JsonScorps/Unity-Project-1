using UnityEngine;
using System.Collections.Generic;
using System;

public class Job {
	// everything to do with queued jobs

	public Tile tile { get; protected set; }
	float jobTime;
	public string jobObjectType { 
		get; protected set;
	}

	Action<Job> cbJobComplete;
	Action<Job> cbJobCancel;

	public Job(Tile tile,string jobObjectType, Action<Job> cbJobComplete, float jobTime = 0.1f) {
		this.tile = tile;
		this.jobObjectType = jobObjectType;
		this.cbJobComplete += cbJobComplete;
		this.jobTime = jobTime;
	}

	public void RegisterJobCompleteCallback(Action<Job> cb) {
		cbJobComplete += cb;
	}

	public void UnregisterJobCompleteCallback(Action<Job> cb) {
		cbJobComplete -= cb;
	}

	public void RegisterJobCancelCallback(Action<Job> cb) {
		cbJobCancel += cb;
	}

	public void UnregisterJobCancelCallback(Action<Job> cb) {
		cbJobCancel -= cb;
	}

	public void DoWork (float workTime) {
		jobTime -= workTime;

		if(jobTime <= 0) {
			if(cbJobComplete != null)
				cbJobComplete(this);
		}
	}

	public void CancelJob() {
		if(cbJobCancel != null)
			cbJobCancel(this);
	}
}
