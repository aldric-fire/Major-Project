using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Renamed to avoid conflict with main project's IInteractable
public interface INPCInteractable {

    void Interact(Transform interactorTransform);
    string GetInteractText();
    Transform GetTransform();

}