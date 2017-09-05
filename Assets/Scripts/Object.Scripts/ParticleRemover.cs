using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRemover : MonoBehaviour {
    public float removeTime;
    private float timeElapsed;
	void Update () {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > removeTime)
        {
            Destroy(this.gameObject);
        }
    }
}
