using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TapNPlay.Core.Managers;
using AYellowpaper.SerializedCollections;

public abstract class TutorialDataSO : ScriptableObject
{
    public TutorialSettings tutorial_Settings;
    public List<TutorialStep> Steps;

    public virtual IEnumerator ExecuteStep(int stepIndex, TutorialReferences references, TutorialTweenSO tweenSettings, HashSet<Vector2> tapPositions, SerializedDictionary<TutorialTextBoxSize, Vector2> textBoxSizeDictionary, MonoBehaviour monoBehaviour)
    {
        if (stepIndex >= Steps.Count) yield break;

        TutorialStep step = Steps[stepIndex];

        OnStepStarted(stepIndex, monoBehaviour);
        yield return null;

        ConfigureStepSettings(step.tutorial_Step_Settings, references);
        SetupTextBox(step.text_Box_Settings, references, tweenSettings, textBoxSizeDictionary);
        SetupHand(step.hand_Settings, references, tweenSettings, tapPositions);
        SetupMask(step.mask_OBJ_Settings, references, tweenSettings);
        SetupWorldObjects(stepIndex, step.world_Object_Settings);
    }

    /// <summary>
    /// Called when a step is started. Override to initialize step-specific logic.
    /// </summary>
    public virtual void OnStepStarted(int stepIndex, MonoBehaviour monoBehaviour) { }

    /// <summary>
    /// Called every frame while the tutorial step is active. Override for continuous checks.
    /// </summary>
    public virtual void OnStepUpdate(int stepIndex) { }

    /// <summary>
    /// Called when a step ends. Override to clean up step-specific resources.
    /// </summary>
    public virtual void OnStepEnded(int stepIndex, TutorialReferences references, MonoBehaviour monoBehaviour) { }

    protected virtual void SetupHand(HandSettings handSettings, TutorialReferences references, TutorialTweenSO tweenSettings, HashSet<Vector2> tapPositions) { }

    protected virtual void SetupMask(MaskOBJSettings maskSettings, TutorialReferences references, TutorialTweenSO tweenSettings) { }

    protected virtual void SetupTextBox(TextBoxSettings textSettings, TutorialReferences references, TutorialTweenSO tweenSettings, SerializedDictionary<TutorialTextBoxSize, Vector2> textBoxSizeDictionary) { }

    protected virtual void ConfigureStepSettings(TutorialStepSettings stepSettings, TutorialReferences references) { }

    /// <summary>
    /// Spawns or re-enables cached world-space GameObjects for this step.
    /// Objects are disabled (not destroyed) when the step ends so they can be reused on replay.
    /// </summary>
    protected virtual void SetupWorldObjects(int stepIndex, WorldObjectSettings worldObjectSettings) { }

    public virtual bool TutorialConditionsCheck(int stepIndex) { return true; }
}

[Serializable]
public class TutorialStep
{
    public TutorialStepSettings tutorial_Step_Settings;
    public GifStepSettings gif_Step_Settings;
    public TextBoxSettings text_Box_Settings;
    public MaskOBJSettings mask_OBJ_Settings;
    public HandSettings hand_Settings;
    public WorldObjectSettings world_Object_Settings;
}

[Serializable]
public struct GifStepSettings
{
    public bool show_GIF;
    public int gif_Clip_Index;
}

[Serializable]
public struct TutorialSettings
{
    public int level_To_Display;
}

[Serializable]
public struct TutorialStepSettings
{
    public bool tap_Anywhere;
    public Tutorial_Step_Type step_Type;
    public float time_To_Next_Step;
}

[Serializable]
public struct TextBoxSettings
{
    public Tutorial_Display_Type text_Dis_Type;
    public Vector2 screen_Pos;
    public string tutorial_Text;
    public TutorialTextBoxSize textBoxSize;
}

[Serializable]
public struct HandSettings
{
    public Tutorial_Display_Type hand_Dis_Type;
    public bool new_Positions;
    public Vector2[] hand_Positions;
}

[Serializable]
public struct MaskOBJSettings
{
    public Tutorial_Display_Type maskObj_Dis_Type;
    public MaskPosContainer[] maskPos;

    [Serializable]
    public struct MaskPosContainer
    {
        public bool animate_Mask;
        public Vector2[] mask_Positions;
        public GameObject mask_Obj_Prefab;
    }
}

[Serializable]
public struct WorldObjectSettings
{
    public WorldObjectSpawnData[] spawnData;

    [Serializable]
    public struct WorldObjectSpawnData
    {
        public GameObject prefab;
        public Vector3 world_Position;
        public Vector3 world_Rotation;
    }
}

public enum Tutorial_Display_Type
{
    None,
    Static,
    Animated,
    Center
}

public enum Tutorial_Step_Type
{
    waitTap
}
