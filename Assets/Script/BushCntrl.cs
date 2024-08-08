using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushCntrl : MonoBehaviour
{
    private ParticleSystem bushParticle;
    private bool isJuice;
    void Start()
    {
        bushParticle = GetComponentInChildren<ParticleSystem>();
        bushParticle.Stop();
    }

    public void PlayEffect()
    {
        bushParticle.Play();
    }
}
