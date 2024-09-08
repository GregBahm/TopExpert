using UnityEngine;

public abstract class VisualController : MonoBehaviour
{
    public abstract void DrawState(CardUiState state, float progression);
}
