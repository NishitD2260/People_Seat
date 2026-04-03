using System.Collections;
using System.Collections.Generic;
using TapNPlay.Core.Data;
using UnityEngine;

public enum ControlMode
{
    Tap,
    TapAndHold,
    DragAndDrop
}


namespace TapNPlay.Core.Data
{

    [CreateAssetMenu(fileName = "InputSettings", menuName = "TapnPlay/Create/Input Settings")]
    public class InputSettings : ScriptableObject
    {
        [Header("Control Settings")]
        public ControlMode controlMode;
        public LayerMask interactableLayer;
        public float holdThreshold = 0.5f;         // Time for Tap & Hold

        [Header("Drag Settings")]
        public Vector3 dragOffset = Vector3.zero;           // Visual lift while dragging
        public float dragSmoothingSpeed = 10f;
    }
}
