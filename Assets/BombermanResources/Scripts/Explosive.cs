using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour {

    public ParticleSystem explosive_Particle = null;
    public AudioSource explosive_audioSource = null;

    void Start() {
        reactivate();
    }

    public void reactivate() {
        explosive_audioSource.Play();
        explosive_Particle.Play();

        Destroy(this.gameObject, 5f);
    }
}
