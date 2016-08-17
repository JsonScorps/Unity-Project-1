using UnityEngine;
using System.Collections.Generic;

public class JobSpirteController : MonoBehaviour {


	FurnitureSpriteController fsc;
	Dictionary<Job, GameObject> jobGameObjectMap;

	// Use this for initialization
	void Start () {
		jobGameObjectMap = new Dictionary<Job, GameObject>();

		fsc = GameObject.FindObjectOfType<FurnitureSpriteController> ();

		WorldController.Instance.world.jobQueue.RegisterJobCreationCallback (OnJobCreated);
	}

	//job created
	void OnJobCreated (Job job) {


		if(jobGameObjectMap.ContainsKey(job) ) {
			Debug.LogError ("OnJobCreated -- Job already exists");
			return;
		}

		GameObject job_go = new GameObject();
				 
		// add object data/go pair to Dictionary
		jobGameObjectMap.Add( job, job_go);

		job_go.name = "JOB_"+ job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
		job_go.transform.SetParent (this.transform, true);
		job_go.transform.position = new Vector3( job.tile.X, job.tile.Y, 0);

		// add sprite renderer
		SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
		sr.sprite = fsc.GetSpriteForFurniture (job.jobObjectType);
		//transparency
		sr.color = new Color( 2f, 2f, 2f, 0.25f );
		sr.sortingLayerName = "Jobs";

		job.RegisterJobCompleteCallback (OnJobEnded);
		job.RegisterJobCancelCallback (OnJobEnded);
	}

	//job canceled or completed
	void OnJobEnded (Job job) {

		GameObject job_go = jobGameObjectMap [job];

		job.UnregisterJobCancelCallback (OnJobEnded);
		job.UnregisterJobCompleteCallback (OnJobEnded);

		Destroy (job_go);

	}
}
