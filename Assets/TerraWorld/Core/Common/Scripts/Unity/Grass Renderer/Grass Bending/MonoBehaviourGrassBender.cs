﻿using System;
using UnityEngine;

namespace TerraUnity.Runtime
{
    public abstract class MonoBehaviourGrassBender : MonoBehaviour, IGrassBender, IEquatable<MonoBehaviourGrassBender>
    {
        public Vector3 Position => transform.position;
        public float BendRadius { get => bendRadius; set => bendRadius = value; }
        public int Priority { get => priority; set => priority = value; }
        public float BendIntensity { get => bendIntensity; set => bendIntensity = value; }

        [Tooltip("Radius of the grass bending sphere"), Range(0.1f, 50f)]
        [SerializeField] private float bendRadius = 1f;
        [Tooltip("Intensity of the grass bending"), Range(0.001f, 2f)]
        [SerializeField] private float bendIntensity = 1f;
        [Tooltip("When concurrent bend sources limit is exceeded, benders with lower priority values will be served first")]
        [SerializeField] private int priority = 0;

        public bool Equals (MonoBehaviourGrassBender other)
        {
            if (other is null) return false;
            return other.GetInstanceID() == GetInstanceID();
        }
    }
}

