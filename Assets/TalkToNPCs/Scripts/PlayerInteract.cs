using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour {


    private void Update() {
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            INPCInteractable interactable = GetInteractableObject();
            if (interactable != null) {
                interactable.Interact(transform);
            }
        }
    }

    public INPCInteractable GetInteractableObject() {
        List<INPCInteractable> interactableList = new List<INPCInteractable>();
        float interactRange = 3f;
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray) {
            if (collider.TryGetComponent(out INPCInteractable interactable)) {
                interactableList.Add(interactable);
            }
        }

        INPCInteractable closestInteractable = null;
        foreach (INPCInteractable interactable in interactableList) {
            if (closestInteractable == null) {
                closestInteractable = interactable;
            } else {
                if (Vector3.Distance(transform.position, interactable.GetTransform().position) < 
                    Vector3.Distance(transform.position, closestInteractable.GetTransform().position)) {
                    // Closer
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }

}