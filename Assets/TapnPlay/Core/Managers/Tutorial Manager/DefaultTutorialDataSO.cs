using UnityEngine;
using System.Collections.Generic;
using TapNPlay.Core.Managers;
using TapNPlay.Core.Utilities;
using DG.Tweening;
using Coffee.UIExtensions;
using AYellowpaper.SerializedCollections;
using LoopType = DG.Tweening.LoopType;

[CreateAssetMenu(fileName = "DefaultTutorialData", menuName = "Tutorials/Tutorial_Data_Default", order = 0)]
public class DefaultTutorialDataSO : TutorialDataSO
{
    public Sequence handSequence;

    // Keyed by step index; objects are disabled on step end and re-enabled on replay
    private Dictionary<int, List<GameObject>> _spawnedWorldObjects = new();

    protected override void ConfigureStepSettings(TutorialStepSettings stepSettings, TutorialReferences references)
    {
        if (stepSettings.step_Type == Tutorial_Step_Type.waitTap)
        {
            references.skipTutorialStepButton.gameObject.SetActive(true);
            references.skipTutorialStepButton.StartSkipTutorialStepCoroutine(stepSettings.time_To_Next_Step);
        }
        else
        {
            references.skipTutorialStepButton.gameObject.SetActive(false);
        }

        references.MaskedBgImage.raycastTarget = !stepSettings.tap_Anywhere;
    }

    protected override void SetupHand(HandSettings handSettings, TutorialReferences references, TutorialTweenSO tweenSettings, HashSet<Vector2> tapPositions)
    {
        switch (handSettings.hand_Dis_Type)
        {
            case Tutorial_Display_Type.None:
                references.handIcon.gameObject.SetActive(false);
                break;

            case Tutorial_Display_Type.Static:
                references.handIcon.gameObject.SetActive(true);
                references.handIcon.anchoredPosition = new Vector2(handSettings.hand_Positions[0].x, handSettings.hand_Positions[0].y);
                break;

            case Tutorial_Display_Type.Center:
                List<Vector2> positionToCenter = new(handSettings.hand_Positions);
                if (!handSettings.new_Positions)
                    positionToCenter.RemoveAll(position => tapPositions.Contains(position));

                references.handIcon.gameObject.SetActive(true);
                references.handIcon.anchoredPosition = Utils.GetWorldUIPosition(Utils.GetCenter(positionToCenter), references.handIcon.parent, Camera.main, Camera.main);
                break;

            case Tutorial_Display_Type.Animated:
                List<Vector2> positionToAnimate = new(handSettings.hand_Positions);
                if (!handSettings.new_Positions)
                    positionToAnimate.RemoveAll(position => tapPositions.Contains(position));

                AnimateHandMoveLoop(positionToAnimate, references.handIcon, tweenSettings);
                references.handIcon.gameObject.SetActive(true);
                break;
        }
    }

    private void AnimateHandMoveLoop(List<Vector2> uiPositions, RectTransform handIcon, TutorialTweenSO tweenSettings)
    {
        handSequence?.Kill();
        handSequence = DOTween.Sequence();
        handIcon.anchoredPosition = uiPositions[0];

        for (int i = 1; i < uiPositions.Count; i++)
        {
            handSequence.Append(
                handIcon.DOAnchorPos(uiPositions[i], tweenSettings.handMoveDuration).SetEase(tweenSettings.handMoveEase)
            );
        }

        handSequence.SetLoops(-1, LoopType.Restart);
    }

    protected override void SetupMask(MaskOBJSettings maskOBJSettings, TutorialReferences references, TutorialTweenSO tweenSettings)
    {
        switch (maskOBJSettings.maskObj_Dis_Type)
        {
            case Tutorial_Display_Type.None:
                references.MaskedObjectsParent.SetActive(false);
                break;

            case Tutorial_Display_Type.Static:
                references.MaskedObjectsParent.SetActive(true);
                if (maskOBJSettings.maskPos.Length > 0)
                {
                    references.MaskedBgImage.color = references.fadeColor;
                    foreach (var maskPos in maskOBJSettings.maskPos)
                    {
                        GameObject maskedObject = Object.Instantiate(maskPos.mask_Obj_Prefab, references.MaskedObjectsParent.transform);
                        references.unmaskRaycastFilter.targetUnmask = maskedObject.GetComponent<Unmask>();
                        maskedObject.transform.SetAsFirstSibling();
                        maskedObject.GetComponent<RectTransform>().anchoredPosition = maskPos.mask_Positions[0];

                        if (maskPos.animate_Mask)
                        {
                            maskedObject.GetComponent<RectTransform>().localScale = Vector3.zero;
                            maskedObject.GetComponent<RectTransform>().DOScale(Vector3.one, tweenSettings.maskTweenDuration)
                                .SetDelay(tweenSettings.maskTweendelay)
                                .SetEase(tweenSettings.maskEase);
                        }
                        else
                        {
                            maskedObject.GetComponent<RectTransform>().localScale = Vector3.one;
                        }
                    }
                }
                else
                {
                    references.MaskedBgImage.color = references.transparentColor;
                }
                break;

            case Tutorial_Display_Type.Center:
                references.MaskedObjectsParent.SetActive(true);
                foreach (var maskPos in maskOBJSettings.maskPos)
                {
                    GameObject maskedObject = Object.Instantiate(maskPos.mask_Obj_Prefab, references.MaskedObjectsParent.transform);
                    references.unmaskRaycastFilter.targetUnmask = maskedObject.GetComponent<Unmask>();
                    maskedObject.transform.SetAsFirstSibling();
                    maskedObject.GetComponent<RectTransform>().anchoredPosition = Utils.GetWorldUIPosition(
                        Utils.GetCenter(new List<Vector2>(maskPos.mask_Positions)),
                        maskedObject.GetComponent<RectTransform>(),
                        Camera.main,
                        Camera.main
                    );

                    if (maskPos.animate_Mask)
                    {
                        maskedObject.GetComponent<RectTransform>().localScale = Vector3.zero;
                        maskedObject.GetComponent<RectTransform>().DOScale(Vector3.one, tweenSettings.maskTweenDuration)
                            .SetDelay(tweenSettings.maskTweendelay)
                            .SetEase(tweenSettings.maskEase);
                    }
                    else
                    {
                        maskedObject.GetComponent<RectTransform>().localScale = Vector3.one;
                    }
                }
                break;
        }
    }

    protected override void SetupTextBox(TextBoxSettings textBoxSettings, TutorialReferences references, TutorialTweenSO tweenSettings, SerializedDictionary<TutorialTextBoxSize, Vector2> textBoxSizeDictionary)
    {
        if (references.textBox != null)
        {
            references.textBox.anchoredPosition = textBoxSettings.screen_Pos;
        }

        switch (textBoxSettings.textBoxSize)
        {
            case TutorialTextBoxSize.Small:
                references.textBoxBG.sizeDelta = textBoxSizeDictionary[TutorialTextBoxSize.Small];
                break;
            case TutorialTextBoxSize.Medium:
                references.textBoxBG.sizeDelta = textBoxSizeDictionary[TutorialTextBoxSize.Medium];
                break;
            case TutorialTextBoxSize.Large:
                references.textBoxBG.sizeDelta = textBoxSizeDictionary[TutorialTextBoxSize.Large];
                break;
        }

        switch (textBoxSettings.text_Dis_Type)
        {
            case Tutorial_Display_Type.None:
                references.textBox.gameObject.SetActive(false);
                break;

            case Tutorial_Display_Type.Static:
            case Tutorial_Display_Type.Center:
                references.textInfo.SetText(textBoxSettings.tutorial_Text);
                references.textBox.gameObject.SetActive(true);
                references.textBox.transform.localScale = Vector3.one;
                break;

            case Tutorial_Display_Type.Animated:
                references.textInfo.SetText(textBoxSettings.tutorial_Text);
                references.textBox.localScale = Vector3.zero;
                references.textBox.gameObject.SetActive(true);
                references.textBox.transform.DOScale(Vector3.one, tweenSettings.textBoxTweenDuration)
                    .SetEase(tweenSettings.textBoxEase);
                break;
        }
    }

    protected override void SetupWorldObjects(int stepIndex, WorldObjectSettings worldObjectSettings)
    {
        if (worldObjectSettings.spawnData == null || worldObjectSettings.spawnData.Length == 0) return;

        _spawnedWorldObjects ??= new();

        if (_spawnedWorldObjects.TryGetValue(stepIndex, out List<GameObject> cached))
        {
            // Validate cache — objects may have been destroyed by a scene reload
            if (cached.Count > 0 && cached[0] != null)
            {
                foreach (var obj in cached)
                    if (obj != null) obj.SetActive(true);
                return;
            }

            // Stale cache — objects were destroyed, re-instantiate
            _spawnedWorldObjects.Remove(stepIndex);
        }

        List<GameObject> spawned = new();
        foreach (var data in worldObjectSettings.spawnData)
        {
            if (data.prefab == null) continue;
            GameObject obj = Object.Instantiate(data.prefab);
            obj.transform.position = data.world_Position;
            obj.transform.rotation = Quaternion.Euler(data.world_Rotation);
            spawned.Add(obj);
        }

        _spawnedWorldObjects[stepIndex] = spawned;
    }

    public override void OnStepEnded(int stepIndex, TutorialReferences references, MonoBehaviour monoBehaviour)
    {
        if (_spawnedWorldObjects == null) return;

        if (_spawnedWorldObjects.TryGetValue(stepIndex, out List<GameObject> objects))
        {
            foreach (var obj in objects)
                if (obj != null) obj.SetActive(false);
        }
    }
}
