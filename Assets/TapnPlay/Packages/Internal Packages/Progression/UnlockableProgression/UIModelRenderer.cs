using DG.Tweening;
using UnityEngine;

namespace TapNPlay.Core.Progression
{
    public class UIModelRenderer : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Transform container;
        [SerializeField] private Transform model;

        [Header("Tween Settings")]
        [SerializeField] private Ease rotationEase;
        [SerializeField] private float rotationDuration;
        [SerializeField] private float rotationAngle;

        #region UNITY_METHODS

        private void OnEnable()
        {
            EventController.StartListening(GameEvent.EVENT_UNLOCKABLE_UPDATED, SetupMesh);
        }

        private void OnDisable()
        {
            EventController.StopListening(GameEvent.EVENT_UNLOCKABLE_UPDATED, SetupMesh);
        }

        #endregion

        #region  PRIVATE

        private void SetupMesh(object args)
        {
            Unlockable unlockable = (Unlockable)args;
            if (unlockable != null)
            {
                if (unlockable.DisplayType != DisplayType.RENDER_IMAGE)
                    return;

                Mesh mesh = unlockable.Mesh;
                if (mesh != null)
                {
                    meshFilter.mesh = mesh;
                }
                container.gameObject.SetActive(true);
                model.DORotate(new Vector3(0f, rotationAngle, 0f),
                        rotationDuration, RotateMode.FastBeyond360).SetEase(rotationEase).SetLoops(-1, LoopType.Restart);
            }
        }

        #endregion
    }
}
