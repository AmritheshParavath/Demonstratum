﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaterFallBlocker : MonoBehaviour
{
    [SerializeField]
    Transform particleParent;
    ParticleSystem[] particles;
    [SerializeField]
    WaterFallBlocker[] parents;
    [SerializeField]
    Transform model;

    [HideInInspector] public bool active;

    public bool open;
    private bool opening;
    public bool Opening {
        get { return opening; }
    }
    public Vector3 openOffset = new Vector3(0.51f, 0, 0);
    private Vector3 initialPosition;
    public float openTime = 0.3f;
    public AnimationCurve openCurve;
    public float activatePercOffset = 0.5f;

    private void Start() {
        initialPosition = model.position;
        if (!open)
            model.position = initialPosition + openOffset;
        particles = particleParent.GetComponentsInChildren<ParticleSystem>();
        SetParticles(open);
    }

    public void Select() {
        if (!opening)
            StartCoroutine(ToggleActive());
    }

    IEnumerator ToggleActive() {
        opening = true;
        float lastTime = Time.timeSinceLevelLoad;
        float perc = open ? 1 : 0;
        Vector3 startPosition = initialPosition + openOffset;
        Vector3 endPosition = initialPosition;
        do {
            perc += (open ? -1 : 1) * (Time.timeSinceLevelLoad - lastTime) / openTime;
            // print(perc + " " + open + " geq: " + (perc >= activatePercOffset) + " leq: " + (perc <= activatePercOffset));
            if (open && perc <= activatePercOffset && particles[0].isEmitting) {
                active = false;
                SetParticles(false);
            } else if (perc >= activatePercOffset && !particles[0].isEmitting) {
                if (parents.Count() == 0 || parents.Any(p => p.active)) {
                    active = true;
                    SetParticles(true);
                }
            }
            lastTime = Time.timeSinceLevelLoad;
            model.position = Vector3.Lerp(startPosition, endPosition, openCurve.Evaluate(perc));
            yield return null;
        } while(perc <= 1 && perc >= 0);
        model.position = open ? startPosition : endPosition;
        open = !open;
        opening = false;
    }

    void SetParticles(bool on) {
        foreach (ParticleSystem p in particles) {
            if (on)
                p.Play();
            else
                p.Stop();
        }
    }
}
