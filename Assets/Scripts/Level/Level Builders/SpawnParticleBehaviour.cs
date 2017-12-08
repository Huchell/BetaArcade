using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticleBehaviour : MonoBehaviour {

    public new ParticleSystem particleSystem;

    [Header("Particle System Transform")]
    [SerializeField] private bool keepPosition;
    [SerializeField] private bool keepRotation, keepScale, keepParent;
    [Header("Offsets")]
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 eulerRotationOffset;
    [SerializeField] private Vector3 scaleOffset;

    public void SpawnParticle()
    {
        if (particleSystem)
        {
            ParticleSystem spawn = Instantiate(particleSystem);

            if (keepPosition) spawn.transform.position = transform.position;
            if (keepRotation) spawn.transform.rotation = transform.rotation;
            if (keepScale) spawn.transform.localScale = spawn.transform.localScale;

            spawn.transform.position += positionOffset;
            spawn.transform.eulerAngles += eulerRotationOffset;
            spawn.transform.localScale += scaleOffset;

            if (keepParent) spawn.transform.SetParent(transform.parent, true);
        }
    }
}
