 using System;
 using System.Collections;
 using System.Collections.Generic;
 using Shapes;
 using UnityEngine;
 using UnityEngine.InputSystem;
 using Random = UnityEngine.Random;

 public class OpenDoorTerminal : MonoBehaviour, ITerminalMinigame
{
     private PlayerInput _playerInput = null;
     [SerializeField] private TerminalMinigameBase terminalMinigameBase;

     [Header("Minigame")] 
     [SerializeField] private float confirmStayTime;
     [SerializeField] private int index;

     [SerializeField] private Triangle rightArrow;
     [SerializeField] private Triangle leftArrow;
     [SerializeField] private GameObject arrows;
     
     [SerializeField] private AudioClip modemBeepSFX;
     [SerializeField] private AudioClip computerClickSFX;
     [SerializeField] private AudioClip computerErrorSFX;

     [SerializeField] private Color inactiveColor;
     [SerializeField] private Color activeColor;
     
     [SerializeField] private Line progressBar;

     [SerializeField] private Transform switchesParent;
     [SerializeField] private List<Switch> switches;

     [SerializeField] private int power;

     private float _blockSpacing = 0.0215f;

     private void Awake()
     {
         _playerInput = new PlayerInput();
     }

     private void Start()
     {
         Initialize();
     }

     [Serializable]
     public class Switch
     {
         public GameObject obj;
         public bool isOn;
         public bool isCalibratedGood;
         public Rectangle outline;
         public Rectangle inside;
         public Rectangle block;
     }

     private void Initialize()
     {
         index = 0;

         foreach (Transform child in switchesParent)
         {
             Switch s = new Switch();
             foreach (Transform baby in child)
             {
                 if (baby.gameObject.name == "Outline")
                 {
                     s.outline = baby.gameObject.GetComponent<Rectangle>();
                 } else if (baby.gameObject.name == "Inside")
                 {
                     s.inside = baby.gameObject.GetComponent<Rectangle>();
                 } else if (baby.gameObject.name == "Block")
                 {
                     s.block = baby.gameObject.GetComponent<Rectangle>();
                 }
             }
             
             s.obj = child.gameObject;
             s.isOn = Random.value > 0.5f;
             s.isCalibratedGood = Random.value > 0.5f;
             switches.Add(s);
         }

         DrawUI(index);
         
         if (CheckSolution())
         {
             ManagerSFX.Instance.PlaySFX(modemBeepSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
             _confirmStandby = StartCoroutine(ConfirmStandby());
         }
     }

     private void DrawUI(int selectedIndex)
     {
         foreach (Switch s in switches)
         {
             if (s.isOn)
             {
                 s.block.transform.localPosition = new Vector3(_blockSpacing, 0, 0);
                 s.inside.Color = activeColor;
             }
             else
             {
                 s.block.transform.localPosition = new Vector3(-_blockSpacing, 0, 0);
                 s.inside.Color = inactiveColor;
             }
             
             s.outline.Color = inactiveColor;
         }
         
         switches[selectedIndex].outline.Color = activeColor;
         
         arrows.transform.localPosition = new Vector3(arrows.transform.localPosition.x, switches[selectedIndex].obj.transform.localPosition.y, 0);
     }

     private Vector2 _input;
     private void OnActionPerformed(InputAction.CallbackContext value)
     {
         _input = Vector2.zero;
         _input = value.ReadValue<Vector2>();
        
         if (_input.x > 0) // Right
         {
             if (!switches[index].isOn)
             {
                 PlayInputSFX();
                 switches[index].isOn = true;
                 rightArrow.Color = activeColor;
                 PlayInputSFX();
             }
             else
             {
                 PlayErrorSFX();
             }
             
         } else if (_input.x < 0) // Left
         {
             if (switches[index].isOn)
             {
                 PlayInputSFX();
                 switches[index].isOn = false;
                 leftArrow.Color = activeColor;
                 PlayInputSFX();
             }
             else
             {
                 PlayErrorSFX();
             }
             
         } else if (_input.y > 0) // Up
         {
             index--;
             if (index < 0) { index = switches.Count - 1; }
             PlayInputSFX();
         }
         else if (_input.y < 0) // Down
         {
             index++;
             index %= switches.Count;
             PlayInputSFX();
         }
        
         DrawUI(index);
        
         if (_confirmStandby != null) StopCoroutine(_confirmStandby);
         if (CheckSolution())
         {
             ManagerSFX.Instance.PlaySFX(modemBeepSFX, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
             _confirmStandby = StartCoroutine(ConfirmStandby());
         }
     }

     private bool CheckSolution()
     {
         power = 0;
         int max = switches.Count;
         foreach (Switch s in switches)
         {
             if (s.isOn == s.isCalibratedGood) power++;
         }
         
         return power >= max;
     }

     private void OnActionCanceled(InputAction.CallbackContext value)
     {
         leftArrow.Color = inactiveColor;
         rightArrow.Color = inactiveColor;
     }
     
     [SerializeField] private float animationPulseMultiplier;
     [SerializeField] private float animationLerpSpeed;
     private float _progress;
     private void FixedUpdate()
     {
         float percent = Mathf.Clamp01((float)power / switches.Count); 
         
         float min = -0.1f;
         float max = 0.1f;
         float targetProgress = Mathf.Lerp(min, max, percent);
         
         _progress = Mathf.Lerp(_progress, targetProgress, Time.deltaTime * animationLerpSpeed);

         progressBar.End = new Vector3(0, _progress, 0);
         
         float H, S, V;
         Color.RGBToHSV(activeColor, out H, out S, out V);
         
         float minPulse = 0.2f; // always at least 20% brightness
         float peakPulse = minPulse + 0.5f * (power / (float)switches.Count);
         float pulse = Mathf.Sin(Time.time * animationPulseMultiplier) * peakPulse + peakPulse;
         float animatedV = Mathf.Clamp01(V * (0.5f + 0.5f * pulse));
         progressBar.Color = Color.HSVToRGB(H, S, animatedV);
     }

     private void PlayInputSFX()
     {
         ManagerSFX.Instance.PlaySFX(computerClickSFX, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
     }
     
     private void PlayErrorSFX()
     {
         ManagerSFX.Instance.PlaySFX(computerErrorSFX, transform.position, 0.01f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
     }

     private Coroutine _confirmStandby;
     private IEnumerator ConfirmStandby()
     {
         yield return new WaitForSeconds(confirmStayTime);
         terminalMinigameBase.OnMinigameEnd();
     }

     [ContextMenu("Test")]
     private void SubscribeToInput()
     {
         _playerInput.Enable();
         _playerInput.TerminalMinigames.OpenDoor.performed += OnActionPerformed;
         _playerInput.TerminalMinigames.OpenDoor.canceled += OnActionCanceled;
     }
     
     private void UnsubscribeToInput()
     {
         _playerInput.TerminalMinigames.OpenDoor.performed -= OnActionPerformed;
         _playerInput.TerminalMinigames.OpenDoor.canceled -= OnActionCanceled;
         _playerInput.Disable();
     }
     
     public void OnMinigameStart(TerminalMinigameBase minigameBase)
     {
         terminalMinigameBase = minigameBase;
     }
     
     public void OnMinigameEnd()
     {
         StopAllCoroutines();
         UnsubscribeToInput();
     }

     public void OnMinigameFocus()
     {
         SubscribeToInput();
     }
     
     public void OnMinigameUnfocus()
     {
         UnsubscribeToInput();
     }

     private void OnDisable()
     {
         UnsubscribeToInput();
     }
 }
