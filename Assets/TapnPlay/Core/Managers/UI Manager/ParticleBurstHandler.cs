using System.Collections.Generic;
using AssetKits.ParticleImage;
using MEC;
using UnityEngine;

public class ParticleBurstHandler : MonoBehaviour
{
    [SerializeField] private ParticleImage particleImage;
    [SerializeField] private int particleBurstCount;
    [SerializeField] private float burstInterval;

    public void PlayParticle()
    {
        Timing.RunCoroutine(ParticleBurstRoutine());
    }

    private IEnumerator<float> ParticleBurstRoutine()
    {
        int tempCount = 0;

        while (tempCount < particleBurstCount)
        {
            particleImage.Play();
            tempCount++;
            yield return Timing.WaitForSeconds(burstInterval);
        }
    }
}
