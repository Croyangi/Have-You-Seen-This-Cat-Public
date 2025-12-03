using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class TabletAppMimicModifiers : MonoBehaviour, ITabletApp
{
    [SerializeField] private GameObject cameraBlock;
    
    [SerializeField] private List<CatPhysicalModifier> invalidPhysicalModifiers = new List<CatPhysicalModifier>();
    [SerializeField] private List<TextMeshProUGUI> invalidModifierTexts = new List<TextMeshProUGUI>();
    [SerializeField] private GameObject mimicTraitTextPrefab;
    [SerializeField] private GameObject mimicTraitTextParent;
    [SerializeField] private GameObject highlight; 
    [SerializeField] private int index;
    [SerializeField] private int prevIndex;
    [SerializeField] private int scrollIndexThreshold;
    
    [SerializeField] private Material catMaterial;
    [SerializeField] private Material catOutlineMaterial;
    [SerializeField] private Material wireframeMaterial;
    
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;

    [SerializeField] private CatInspectionModel catInspectionModel;
    [SerializeField] private GameObject cat;
    [SerializeField] private float catRotationSpeed;
    [SerializeField] private float catAutoRotationSpeed;
    [SerializeField] private float catAutoTimer;
    [SerializeField] private float catAutoTimerSet;
    [SerializeField] private GameObject activeRightArrow;
    [SerializeField] private GameObject activeLeftArrow;
    
    [SerializeField] private GameObject scroll;
    [SerializeField] private GameObject scrollBar;
    private float _minScroll;
    private float _maxScroll;
    
    [SerializeField] private AudioClip scrollSfx;
    [SerializeField] private GameObject scrollSfxObject;
    [SerializeField] private AudioClip rotateSfx;
    [SerializeField] private bool isRotating;
    
    [SerializeField] private int furEliseIndex;
    
    [field: SerializeField] public bool IsShowing { get; private set; }
    [SerializeField] private Vector2 input; 
    
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void Start()
    {
        InitializeOriginalMaterials(catInspectionModel);
        RefreshPopulate();
    }

    [ContextMenu("Refresh Populate")]
    public void RefreshPopulate()
    {
        index = 0;
        prevIndex = 0;

        invalidPhysicalModifiers.Clear();
        foreach (TextMeshProUGUI text in invalidModifierTexts)
        {
            if (text != null) Destroy(text.gameObject);
        }
        invalidModifierTexts.Clear();
        
        Populate();
        
        ApplyText();
        ColorText();
        
        if (invalidModifierTexts.Count - 1 <= scrollIndexThreshold)
        {
            scroll.SetActive(false);
        }
        else
        {
            scroll.SetActive(true);
            InitializeScrollBar();
            ApplyScrollBar();
        }
        
        ApplyPhysicalModifier(invalidPhysicalModifiers[index], catInspectionModel, isDefault: true);
        
        // Apply any dependent modifiers
        if (invalidPhysicalModifiers[index].Dependencies != null)
        {
            foreach (CatPhysicalModifier dependency in invalidPhysicalModifiers[index].Dependencies)
            {
                ApplyPhysicalModifier(dependency, catInspectionModel, true);
            }
        }
    }

    public Vector3 GetCameraPos()
    {
        return cameraBlock.transform.localPosition;
    }
    
    public void OnShow()
    {
        IsShowing = true;
        _playerInput.Tablet.Operate.performed += OnOperatePerformed;
        _playerInput.Tablet.Operate.canceled += OnOperateCanceled;
        StartCoroutine(ToggledUpdate());
    }
    
    public void OnHide()
    {
        IsShowing = false;
        catAutoTimer = 0f;
            
        _playerInput.Tablet.Operate.performed -= OnOperatePerformed;
        _playerInput.Tablet.Operate.canceled -= OnOperateCanceled;
    }

    private void OnEnable()
    {
        _playerInput.Enable();
        ManagerCatModifier.OnPopulate += RefreshPopulate;
    }
    
    private void OnDisable()
    {
        _playerInput.Disable();
        _playerInput.Tablet.Operate.performed -= OnOperatePerformed;
        _playerInput.Tablet.Operate.canceled -= OnOperateCanceled;
        ManagerCatModifier.OnPopulate -= RefreshPopulate;
    }

    private IEnumerator ToggledUpdate()
    {
        while (IsShowing)
        {
            if (catAutoTimer > 0)
            {
                catAutoTimer = Mathf.Max(0, catAutoTimer -= Time.deltaTime);
            }
            else
            {
                cat.transform.Rotate(Vector3.up * (catAutoRotationSpeed * Time.deltaTime));
            }
            
            if (input.x != 0)
            {
                if (!isRotating)
                {
                    isRotating = true;
                    StartCoroutine(PlayRotateSFX());
                }
                
                catAutoTimer = catAutoTimerSet;
                cat.transform.Rotate(Vector3.up * (catRotationSpeed * Mathf.Sign(input.x) * Time.deltaTime));

                if (input.x > 0)
                {
                    activeRightArrow.SetActive(true);
                } else if (input.x < 0)
                {
                    activeLeftArrow.SetActive(true);
                }
            }
            else
            {
                isRotating = false;
                activeLeftArrow.SetActive(false);
                activeRightArrow.SetActive(false);
            }

            yield return null;
        }
    }

    private IEnumerator PlayRotateSFX()
    {
        Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
        while (isRotating)
        {
            ManagerSFX.Instance.PlaySFX(rotateSfx, pos,0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: input.x > 0 ? 1.1f : 0.8f, isRandomPitch: false);
            yield return new WaitForSeconds(0.07f);
        }
    }
    
    private void OnOperatePerformed(InputAction.CallbackContext value)
    {
        input = value.ReadValue<Vector2>();
        if (catAutoTimer > 0)
        {
            catAutoTimer = catAutoTimerSet;
        }

        if (invalidModifierTexts.Count <= 0) return;
        prevIndex = index;

        // Handle input
        if (input.y > 0) // Up
        {
            index--;
            if (index < 0) index = invalidModifierTexts.Count - 1;
            PlayScrollSFX();
            furEliseIndex++;
        }
        else if (input.y < 0) // Down
        {
            index++;
            index %= invalidModifierTexts.Count;
            PlayScrollSFX();
            furEliseIndex++;
        }

        furEliseIndex %= 18;

        ApplyText();
        ColorText();
        if (invalidModifierTexts.Count > scrollIndexThreshold) ApplyScrollBar();
        
        ApplyPhysicalModifier(invalidPhysicalModifiers[index], catInspectionModel, isDefault: index == 0);
        // Apply any dependent modifiers
        if (invalidPhysicalModifiers[index].Dependencies != null)
        {
            foreach (CatPhysicalModifier dependency in invalidPhysicalModifiers[index].Dependencies)
            {
                ApplyPhysicalModifier(dependency, catInspectionModel, true);
            }
        }
    }

    private void PlayScrollSFX()
    {
        if (scrollSfxObject != null)
        {
            Destroy(scrollSfxObject);
        }
        float pitch = 1f;

        switch (furEliseIndex)
        {
           // A section
           case 0:  pitch = 1.498f; break; // E5
           case 1:  pitch = 1.414f; break; // D#5
           case 2:  pitch = 1.498f; break; // E5
           case 3:  pitch = 1.414f; break; // D#5
           case 4:  pitch = 1.498f; break; // E5
           case 5:  pitch = 1.122f; break; // B4
           case 6:  pitch = 1.335f; break; // D5
           case 7:  pitch = 1.189f; break; // C5
           case 8:  pitch = 1.000f; break; // A4
           case 9:  pitch = 1.189f; break; // C5
           case 10: pitch = 1.498f; break; // E5
           case 11: pitch = 2.000f; break; // A5
           case 12: pitch = 2.245f; break; // B5
           case 13: pitch = 1.498f; break; // E5
           case 14: pitch = 1.888f; break; // G#5
           case 15: pitch = 2.245f; break; // B5
           case 16: pitch = 2.378f; break; // C6
           case 17: pitch = 1.498f; break; // E5
           case 18: pitch = 1.498f; break; // E5
           default: pitch = 1.0f; break;   // fallback to A4
        }       

        Vector3 pos = ManagerPlayer.instance.PlayerTabletHelper.Tablet.transform.position;
        scrollSfxObject = ManagerSFX.Instance.PlaySFX(scrollSfx, pos, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: pitch, isRandomPitch: false).gameObject;
    }
    
    private void ApplyText()
    {
        int spacing = 120;

        // Handle text visibility for scrolling
        if (index > scrollIndexThreshold)
        {
            // Disable items above the scroll threshold
            for (int i = 0; i < invalidModifierTexts.Count; i++)
            {
                invalidModifierTexts[i].gameObject.SetActive(i >= (index - scrollIndexThreshold));
            }

            // Keep highlight in same place
            highlight.transform.localPosition = new Vector3(
                highlight.transform.localPosition.x,
                15f + scrollIndexThreshold * -spacing,
                highlight.transform.localPosition.z
            );
        }
        else
        {
            // Show all texts if we're within threshold
            foreach (TextMeshProUGUI obj in invalidModifierTexts)
            {
                obj.gameObject.SetActive(true);
            }

            // Move highlight to match index
            highlight.transform.localPosition = new Vector3(
                highlight.transform.localPosition.x,
                index * -spacing,
                highlight.transform.localPosition.z
            );
        }
    }

    private void InitializeScrollBar()
    {
        float scrollAreaHeight = 1.5f;
        float scrollFraction = (float) scrollIndexThreshold / invalidPhysicalModifiers.Count;
        Rectangle scrollBarShape = scrollBar.GetComponent<Rectangle>();
        RectTransform rectTransform = scrollBar.GetComponent<RectTransform>();
        scrollBarShape.Height = scrollAreaHeight * scrollFraction;
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x,
            (scrollAreaHeight - scrollBarShape.Height) / 2f
        );
        
        _minScroll = rectTransform.anchoredPosition.y;
        _maxScroll = -rectTransform.anchoredPosition.y;
    }
    
    private void ApplyScrollBar()
    {
        float percentage = 0f;
        
        if (index > scrollIndexThreshold)
        {
            percentage = (float) (index - scrollIndexThreshold) / (invalidPhysicalModifiers.Count - 1 - scrollIndexThreshold);
        }
        
        float diff = Mathf.Abs(_maxScroll - _minScroll);

        scrollBar.transform.localPosition = new Vector3(
                scrollBar.transform.localPosition.x,
                _minScroll - diff * percentage,
                scrollBar.transform.localPosition.z
            );
    }

    private void ColorText()
    {
        for (int i = 0; i < invalidModifierTexts.Count; i++)
        {
            if (!invalidModifierTexts[i].gameObject.activeSelf) continue;

            if (i == 0)
                invalidModifierTexts[i].color = Color.white; // Always same for first
            else if (i == index)
                invalidModifierTexts[i].color = selectedColor;
            else
                invalidModifierTexts[i].color = unselectedColor;
        }
    }
    
    private void OnOperateCanceled(InputAction.CallbackContext value)
    {
        input = Vector2.zero;
    }

    [ContextMenu("Generate Tablet")]
    public void Populate()
    {
        CatPhysicalModifier defaultCPM = ScriptableObject.CreateInstance<CatPhysicalModifier>();
        defaultCPM.name = "Reference Cat";
        defaultCPM.PhysicalModifier = catInspectionModel.defaultModifiers[0].PhysicalModifier;
        
        invalidPhysicalModifiers.Add(defaultCPM);
        invalidPhysicalModifiers.AddRange(ManagerCatModifier.instance.InvalidPhysicalModifiers);
        
        for (int i = 0; i < invalidPhysicalModifiers.Count; i++)
        {
            CatPhysicalModifier pm = invalidPhysicalModifiers[i];
            GameObject text = Instantiate(mimicTraitTextPrefab, mimicTraitTextParent.transform);
            TextMeshProUGUI tmp = text.GetComponent<TextMeshProUGUI>();

            tmp.text = pm.name; // Using a verbatim string literal
            invalidModifierTexts.Add(tmp);

            if (i == index)
            {
                tmp.color = selectedColor;
            }
            else
            {
                tmp.color = unselectedColor;
            }
        }
    }

    private void InitializeOriginalMaterials(CatInspectionModel cim)
    {
        cim.OriginalMaterials.Clear();
        
        // Get original mats
        for (int i = 0; i < cim.defaultModifiers.Count; i++)
        {
            cim.OriginalMaterials.Add(cim.bodyParts[i].smr.materials);
        }
    }
    
    // Helper function to avoid repeated code
    private void ApplyPhysicalModifier(CatPhysicalModifier pm, CatInspectionModel cim, bool isDependency = false, bool isDefault = false)
    {
        // Reset
        if (!isDependency)
        {
            // Apply wireframes
            for (int i = 0; i < cim.defaultModifiers.Count; i++)
            {
                Material[] temp = isDefault ? cim.OriginalMaterials[i] : new Material[] { wireframeMaterial };
                cim.bodyParts[i].smr.materials = temp;
            }
            
            // Reset meshes
            for (int i = 0; i < cim.defaultModifiers.Count; i++)
            {
                cim.bodyParts[i].smr.sharedMesh = cim.defaultModifiers[i].PhysicalModifier;
                cim.bodyParts[i].catPhysicalModifier = cim.defaultModifiers[i];
            }
        }

        // Apply meshes
        for (int i = 0; i < cim.bodyParts.Count; i++)
        {
            if ((pm.OccupationFlags & (OccupationFlags)(1 << cim.bodyParts[i].id)) != 0)
            {
                cim.bodyParts[i].smr.materials = new []{ cim.OriginalMaterials[i][0], catOutlineMaterial };
                cim.bodyParts[i].smr.sharedMesh = pm.PhysicalModifier;
                cim.bodyParts[i].catPhysicalModifier = pm;
            }
        }
    }
}
