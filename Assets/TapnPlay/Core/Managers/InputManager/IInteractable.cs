using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnTap();
    void OnHoldStart();
    void OnHolding();
    void OnHoldEnd();              // Tap or Tap-Hold action
    void OnBeginDrag();         // Start of drag
    void OnDrag(Vector3 pos);   // Ongoing drag
    void OnEndDrag();           // Drop / release
}

