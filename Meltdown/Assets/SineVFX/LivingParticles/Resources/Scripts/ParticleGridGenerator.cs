﻿using UnityEngine;

public class ParticleGridGenerator : MonoBehaviour {

    public float particleSize = 1f;
    public Color particleColor = Color.white;
    public bool randomColorAlpha = true; // For MetallicSmoothness random offset
    public float xDistance = 0.25f;
    public float yDistance = 0.25f;
    public float zDistance = 0.25f;
    public int xSize = 10;
    public int ySize = 10;
    public int zSize = 10;
    public float OffsetEven = 0.125f;
    public bool updateEveryFrame = false;

    private float even;
    private Vector3[] positions;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

	void Start () {
        ps = GetComponent<ParticleSystem>();
        UpdateGrid();
    }

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        GenerateGrid();
        GenerateParticles();
    }

    // Generating array of positions
    private void GenerateGrid()
    {
        positions = new Vector3[xSize * ySize * zSize];
        for (int z = 0, i = 0; z < zSize; z++)
        {
            even = 0f;
            if (z % 2 == 0)
            {
                even = OffsetEven;
            }
            for (int y = 0; y < ySize; y++)
            {                
                for (int x = 0; x < xSize; x++, i++)
                {                    
                    positions[i] = new Vector3(x * xDistance + even, y * yDistance, z * zDistance);
                }
            }
        }        
    }

    // Generating particles with grid based positions
    private void GenerateParticles()
    {
        particles = new ParticleSystem.Particle[xSize * ySize * zSize];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].position = positions[i];
            if (randomColorAlpha == true)
            particleColor.a = Random.Range(0f, 1f);
            particles[i].startColor = particleColor;
            particles[i].startSize = particleSize;
        }
        ps.SetParticles(particles, particles.Length);
    }

    private void FixedUpdate()
    {
        if (updateEveryFrame == true)
        {
            UpdateGrid();
        }
    }
}
