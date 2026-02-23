using UnityEngine;

/// <summary>
/// Interface for any object the player can interact with by pressing E.
/// Implement on MonoBehaviours attached to objects with colliders.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses E while looking at this object.
    /// </summary>
    /// <param name="player">The player GameObject that initiated the interaction.</param>
    void Interact(GameObject player);

    /// <summary>
    /// Returns the prompt text shown on the HUD (e.g., "Press E to Inspect Computer").
    /// </summary>
    string GetPromptText();

    /// <summary>
    /// Whether this object can currently be interacted with.
    /// Return false if the challenge is already completed, locked, etc.
    /// </summary>
    bool CanInteract();
}
