using UnityEngine;
using TapNPlay.Core.Data;
using UnityEngine.SceneManagement;

namespace TapNPlay.Core.Managers
{
    public class InputManager : MonoBehaviour
    {
        public InputSettings SettingsSO;
        private Camera mainCamera;
        [SerializeField] private IInteractable currentInteractable;
        private Transform interactableTransform;
        private Collider hitCollider;   // ✅ store which collider was hit
        private Vector3 dragTargetPosition;

        private float touchStartTime;
        private bool isHolding;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {

#if UNITY_EDITOR
            HandleMouseInput();

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCurrentScene();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Time.timeScale = 2f;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Time.timeScale = 1f;
            }

#else
            HandleTouchInput();
#endif

            SmoothDragUpdate();
        }

        public void ResetCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Open4x4Scene()
        {
            SceneManager.LoadScene(0);
        }

        public void Open5x5Scene()
        {
            SceneManager.LoadScene(1);
        }

        // ─────────────────────────────────────────────────────────────
        // TOUCH
        // ─────────────────────────────────────────────────────────────
        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            if (Utils.IsPointerOverUI()) return;

            Touch touch = Input.GetTouch(0);
            Vector3 worldPosition = GetWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    isHolding = false;

                    currentInteractable = RaycastToInteractable(touch.position, out interactableTransform, out hitCollider);

                    if (SettingsSO.controlMode == ControlMode.Tap)
                    {
                        HandleTap(currentInteractable, hitCollider);
                    }
                    else if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                    {
                        currentInteractable?.OnBeginDrag();
                        dragTargetPosition = worldPosition + SettingsSO.dragOffset;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (SettingsSO.controlMode == ControlMode.TapAndHold && !isHolding)
                    {
                        if (Time.time - touchStartTime >= SettingsSO.holdThreshold)
                        {
                            isHolding = true;
                            currentInteractable?.OnTap();
                        }
                    }

                    if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                    {
                        dragTargetPosition = worldPosition + SettingsSO.dragOffset;
                        currentInteractable?.OnDrag(dragTargetPosition);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                    {
                        currentInteractable?.OnEndDrag();
                    }

                    ResetInteraction();
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // MOUSE
        // ─────────────────────────────────────────────────────────────
        private void HandleMouseInput()
        {
            if (Utils.IsPointerOverUI()) return;

            if (Input.GetMouseButtonDown(0))
            {
                touchStartTime = Time.time;
                isHolding = false;

                currentInteractable = RaycastToInteractable(Input.mousePosition, out interactableTransform, out hitCollider);

                if (SettingsSO.controlMode == ControlMode.Tap)
                {
                    HandleTap(currentInteractable, hitCollider);
                }
                else if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                {
                    currentInteractable?.OnBeginDrag();
                    dragTargetPosition = GetWorldPoint(Input.mousePosition) + SettingsSO.dragOffset;
                }
                else
                {
                    currentInteractable?.OnHoldStart();
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (SettingsSO.controlMode == ControlMode.TapAndHold)
                {
                    if (Time.time - touchStartTime >= SettingsSO.holdThreshold)
                    {
                        isHolding = true;
                        currentInteractable?.OnHolding();
                    }
                }

                if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                {
                    dragTargetPosition = GetWorldPoint(Input.mousePosition) + SettingsSO.dragOffset;
                    currentInteractable?.OnDrag(dragTargetPosition);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (SettingsSO.controlMode == ControlMode.DragAndDrop)
                {
                    currentInteractable?.OnEndDrag();
                }
                else if (SettingsSO.controlMode == ControlMode.TapAndHold)
                {
                    currentInteractable?.OnHoldEnd();
                }

                ResetInteraction();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // DRAG SMOOTHING
        // ─────────────────────────────────────────────────────────────
        private void SmoothDragUpdate()
        {
            if (SettingsSO.controlMode == ControlMode.DragAndDrop && interactableTransform != null)
            {
                interactableTransform.position = Vector3.Lerp(
                    interactableTransform.position,
                    dragTargetPosition,
                    Time.deltaTime * SettingsSO.dragSmoothingSpeed
                );
            }
        }

        private void ResetInteraction()
        {
            currentInteractable = null;
            interactableTransform = null;
            hitCollider = null;
            isHolding = false;
        }

        // ─────────────────────────────────────────────────────────────
        // TAP HANDLER (Handles normal vs dual containers)
        // ─────────────────────────────────────────────────────────────
        private void HandleTap(IInteractable interactable, Collider tappedCollider)
        {
            if (interactable == null) return;
            interactable.OnTap();
        }

        // ─────────────────────────────────────────────────────────────
        // RAYCAST HELPERS
        // ─────────────────────────────────────────────────────────────
        private IInteractable RaycastToInteractable(Vector2 screenPosition, out Transform foundTransform, out Collider foundCollider)
        {
            foundTransform = null;
            foundCollider = null;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, SettingsSO.interactableLayer);

            if (hits.Length > 0)
            {
                // get closest hit
                var hit = hits[0];
                float closest = hit.distance;

                for (int i = 1; i < hits.Length; i++)
                {
                    if (hits[i].distance < closest)
                    {
                        hit = hits[i];
                        closest = hit.distance;
                    }
                }

                foundTransform = hit.transform;
                foundCollider = hit.collider;

                return hit.collider.GetComponent<IInteractable>();
            }

            return null;
        }

        private Vector3 GetWorldPoint(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }
    }
}
