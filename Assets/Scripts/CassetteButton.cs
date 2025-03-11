using UnityEngine;
using UnityEngine.Events;

public class CassetteButton : MonoBehaviour
{
    [field: SerializeField] public bool PlayClosingAnimation { get; private set; } = false;
    public UnityEvent OnHover;
    public UnityEvent OnUnHover;
    public UnityEvent OnClick;
}