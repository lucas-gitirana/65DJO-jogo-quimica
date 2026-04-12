#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Gera Animator Controller + clip de vitória (trigger <b>Victory</b>) compatível com <see cref="GameVictoryPresenter"/>.
/// <para><b>Menu:</b> VR Lab / Create Game Victory Animator (Screen UI)</para>
/// <para><b>Hierarquia sugerida:</b> Canvas (Screen Space) → filho <c>VictoryPanel</c> com RectTransform + CanvasGroup + Animator (controller gerado). Começar com painel desativado ou alpha 0.</para>
/// </summary>
public static class GameVictoryAnimatorFactory
{
    private const string MenuPath = "VR Lab/Create Game Victory Animator (Screen UI)";
    private const string Folder = "Assets/GameVictory";
    private const string ClipPath = Folder + "/GameVictoryCelebration.anim";
    private const string ControllerPath = Folder + "/GameVictoryUI.controller";

    [MenuItem(MenuPath)]
    private static void CreateVictoryAnimatorAssets()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets", "GameVictory");

        if (AssetDatabase.LoadAssetAtPath<Object>(ClipPath) != null)
            AssetDatabase.DeleteAsset(ClipPath);
        if (AssetDatabase.LoadAssetAtPath<Object>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);
        AssetDatabase.Refresh();

        var clip = new AnimationClip { name = "GameVictoryCelebration" };

        // Painel raiz: CanvasGroup (fade-in) + leve “pop” de escala (RectTransform).
        var alpha = AnimationCurve.EaseInOut(0f, 0f, 1.25f, 1f);
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve(string.Empty, typeof(CanvasGroup), "m_Alpha"),
            alpha);

        var scale = AnimationCurve.EaseInOut(0f, 0.65f, 0.9f, 1f);
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve(string.Empty, typeof(RectTransform), "m_LocalScale.x"),
            scale);
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve(string.Empty, typeof(RectTransform), "m_LocalScale.y"),
            scale);
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve(string.Empty, typeof(RectTransform), "m_LocalScale.z"),
            scale);

        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        AssetDatabase.CreateAsset(clip, ClipPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Victory", AnimatorControllerParameterType.Trigger);

        var root = controller.layers[0].stateMachine;
        var toRemove = new List<AnimatorState>();
        foreach (var ch in root.states)
            toRemove.Add(ch.state);
        foreach (var s in toRemove)
            root.RemoveState(s);

        var idle = root.AddState("Idle");
        root.defaultState = idle;

        var victory = root.AddState("Victory");
        victory.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);

        var transition = idle.AddTransition(victory);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.offset = 0f;
        transition.AddCondition(AnimatorConditionMode.If, 0, "Victory");

        idle.writeDefaultValues = true;
        victory.writeDefaultValues = true;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = controller;
        Debug.Log(
            "[GameVictory] Criado: " + ControllerPath + " e " + ClipPath + ". " +
            "Atribui o Controller ao Animator do painel de vitória e liga esse Animator no GameVictoryPresenter.");
    }
}
#endif
