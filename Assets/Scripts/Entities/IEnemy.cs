/******************************************************************
*    Author: Trinity Hutson
*    Contributors: 
*    Date Created: 04/29/2025
*    Description: Interface for enemies that have shared behaviors
*******************************************************************/

using UnityEngine;

public interface IEnemy
{
    public bool IsSon { get; }

    public void AttackTarget(Transform target);

    
}
