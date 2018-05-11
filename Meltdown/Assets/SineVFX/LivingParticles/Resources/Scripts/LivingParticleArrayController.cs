﻿using UnityEngine;

public class LivingParticleArrayController : MonoBehaviour {

    public Transform[] affectors;

    private Vector4[] positions;
    private ParticleSystemRenderer psr;

	void Start () {
        psr = GetComponent<ParticleSystemRenderer>();
	}
	
    // Sending an array of positions to particle shader
	void Update () {
        positions = new Vector4[affectors.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = affectors[i].position;
        }
        psr.material.SetVectorArray("_Affectors", positions);
    }
}
