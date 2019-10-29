using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionAndRotation : MonoBehaviour {

    public GameObject obj;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        this.transform.position = obj.transform.position;
        this.transform.rotation = obj.transform.rotation;
    }
}
