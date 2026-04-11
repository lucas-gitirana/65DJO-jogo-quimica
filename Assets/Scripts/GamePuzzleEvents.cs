using System;
using UnityEngine;

/// <summary>
/// Eventos globais leves para puzzles e progressão (evita acoplamento direto entre botões, portas e UI).
/// </summary>
public static class GamePuzzleEvents
{
    /// <summary>numBotao dos prefabs: 1 = esquerda, 2 = meio, 3 = direita.</summary>
    public static event Action<int> LabButtonPressed;

    public static void RaiseLabButtonPressed(int buttonIndex) =>
        LabButtonPressed?.Invoke(buttonIndex);

    public static event Action GameVictoryRequested;

    public static void RaiseGameVictory() =>
        GameVictoryRequested?.Invoke();
}
