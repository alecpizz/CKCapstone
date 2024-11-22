using UnityEngine;

/// <summary>
/// Interface for something that needs its turn managed.
/// Both methods must be implemented.
/// </summary>
public interface ITurnListener
{
    /// <summary>
    /// The category of turn this entity will be a part of.
    /// </summary>
    TurnState TurnState { get; }

    /// <summary>
    /// Method that gets called when the entity's turn begins.
    /// This should be used to start a movement animation or
    /// some other logic. 
    /// </summary>
    /// <param name="direction">The user input direction.</param>
    void BeginTurn(Vector3 direction);

    /// <summary>
    /// Method that gets called to end an entity's turn early.
    /// </summary>
    void ForceTurnEnd();
}