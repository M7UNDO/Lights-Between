using UnityEngine;
using TMPro;

public class InspectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FPSController playerController;
    [SerializeField] private PlayerInputHandler input;

    [Header("Inspection Settings")]
    [SerializeField] private Transform inspectPoint;
    [SerializeField] private float mouseRotationSpeed = 120f;
    [SerializeField] private float controllerRotationSpeed = 180f;

    [Header("UI")]
    [SerializeField] private GameObject inspectionUI;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Instruction Text")]
    [SerializeField] private string mouseInstruction = "Move mouse to rotate • Press Interact/Esc to close";
    [SerializeField] private string controllerInstruction = "Move right stick to rotate • Press Interact/B to close";

    private GameObject inspectedObject;
    private Rigidbody inspectedRigidbody;
    private Collider[] inspectedColliders;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private bool originalRigidbodyKinematic;
    private bool isInspecting;

    [Header("Inspection Layer")]
    [SerializeField] private string inspectionLayerName = "Inspection";

    private int originalLayer;
    private int inspectionLayer;

    private void Start()
    {
        if (inspectionUI != null)
            inspectionUI.SetActive(false);

        inspectionLayer = LayerMask.NameToLayer(inspectionLayerName);
    }

    private void Update()
    {
        if (input == null || !isInspecting) return;

        RotateInspectedObject();

        if (input.cancel || input.interact)
        {
            EndInspection();
            input.cancel = false;
            input.interact = false;
        }
    }

    public void BeginInspection(NarrativeInspectableItem item)
    {
        if (item == null || inspectPoint == null) return;

        isInspecting = true;
        inspectedObject = item.gameObject;

        SaveOriginalTransform();
        PrepareObjectForInspection(item);
        UpdateInspectionText(item);

        if (playerController != null)
            playerController.SetInspectionMode(true);

        if (inspectionUI != null)
            inspectionUI.SetActive(true);
    }

    private void SaveOriginalTransform()
    {
        originalLayer = inspectedObject.layer;

        if (inspectionLayer != -1)
        {
            SetLayerRecursively(inspectedObject, inspectionLayer);
        }

        originalParent = inspectedObject.transform.parent;
        originalPosition = inspectedObject.transform.position;
        originalRotation = inspectedObject.transform.rotation;
        originalScale = inspectedObject.transform.localScale;

        inspectedRigidbody = inspectedObject.GetComponent<Rigidbody>();

        if (inspectedRigidbody != null)
        {
            originalRigidbodyKinematic = inspectedRigidbody.isKinematic;
            inspectedRigidbody.isKinematic = true;
        }

        inspectedColliders = inspectedObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in inspectedColliders)
        {
            col.enabled = false;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void PrepareObjectForInspection(NarrativeInspectableItem item)
    {
        inspectedObject.transform.SetParent(inspectPoint);

        inspectedObject.transform.localPosition = item.InspectionPositionOffset;
        inspectedObject.transform.localRotation = Quaternion.Euler(item.InspectionRotationOffset);
        inspectedObject.transform.localScale = item.InspectionScale;
    }

    private void UpdateInspectionText(NarrativeInspectableItem item)
    {
        if (itemNameText != null)
            itemNameText.text = item.ItemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = item.ItemDescription;

        if (instructionText != null)
            instructionText.text = input.IsUsingMouse ? mouseInstruction : controllerInstruction;
    }

    private void RotateInspectedObject()
    {
        if (inspectedObject == null || playerCamera == null) return;

        Vector2 rotateInput = input.look;

        if (rotateInput.sqrMagnitude < 0.01f) return;

        float rotationSpeed = input.IsUsingMouse ? mouseRotationSpeed : controllerRotationSpeed;

        float xRotation = rotateInput.y * rotationSpeed * Time.unscaledDeltaTime;
        float yRotation = -rotateInput.x * rotationSpeed * Time.unscaledDeltaTime;

        inspectedObject.transform.Rotate(playerCamera.transform.up, yRotation, Space.World);
        inspectedObject.transform.Rotate(playerCamera.transform.right, xRotation, Space.World);
    }

    private void EndInspection()
    {
        isInspecting = false;

        if (inspectedObject != null)
        {
            inspectedObject.transform.SetParent(originalParent);
            inspectedObject.transform.position = originalPosition;
            inspectedObject.transform.rotation = originalRotation;
            inspectedObject.transform.localScale = originalScale;

            SetLayerRecursively(inspectedObject, originalLayer);
        }

        if (inspectedRigidbody != null)
        {
            inspectedRigidbody.isKinematic = originalRigidbodyKinematic;
        }

        if (inspectedColliders != null)
        {
            foreach (Collider col in inspectedColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }

        if (playerController != null)
            playerController.SetInspectionMode(false);

        if (inspectionUI != null)
            inspectionUI.SetActive(false);

        inspectedObject = null;
        inspectedRigidbody = null;
        inspectedColliders = null;

        if (input != null)
            input.ClearLookInput();
    }
}