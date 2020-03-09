using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {

    public bool autoAddForce = false;
    public Rigidbody rigidbody = null;
    public float forsePower = 10f;

    public float destroyTime = 0f;

	void Start () {
        Destroy(gameObject, destroyTime);
	}

    void FixedUpdate() {
        if (autoAddForce)
        {
            rigidbody = this.gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(Vector3.up * forsePower, ForceMode.Impulse);
            autoAddForce = false;
        }
    }
}
