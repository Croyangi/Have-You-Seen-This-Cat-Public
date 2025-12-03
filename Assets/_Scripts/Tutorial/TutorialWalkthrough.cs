using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TutorialWalkthrough : MonoBehaviour
{
    [SerializeField] private Difficulty tutorialDifficulty;
    [SerializeField] private int sectionIndex;
    [SerializeField] private int intendedIndex;
    [SerializeField] private string currentSectionName;
    private ManagerPlayer _managerPlayer;
    private PlayerDialogueHelper _playerDialogueHelper;

    [ContextMenu("Start Tutorial")]
    public void StartTutorial()
    {
        ManagerGame.Instance.StartTutorial();
        _managerPlayer = ManagerPlayer.instance;
        _playerDialogueHelper = _managerPlayer.PlayerDialogueHelper;
        StartCoroutine(Introductions());
        firstMetalDoorTerminal.IsProcessing = false;
    }

    private void ProcessSection(string name, int forceSkip = -1)
    {
        sectionIndex++;

        if (forceSkip != -1)
        {
            intendedIndex = forceSkip;
        } else if (intendedIndex < sectionIndex)
        {
            intendedIndex++;
        }
        currentSectionName = name;
    }

    private IEnumerator StandbyDialogue()
    {
        while ((_playerDialogueHelper.IsOngoing || _playerDialogueHelper.IsContinuous) && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
    }
    
    private IEnumerator Introductions()
    {
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Oh, you're done?");
        _playerDialogueHelper.QueueDialogue("Welcome, " + ManagerDataPersistence.Instance.GetGameData().player.name + ", to the <wave>\nM.E.O.W. Training Program!</wave>");
        
        ProcessSection("waitForIntroductions");
        yield return StandbyDialogue();
        
        StartCoroutine(LearnToMove());
    }
    
    private IEnumerator LearnToMove()
    {
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}In this position, you'll be expected to move regularly using [<bounce>WASD</bounce>] and scan your environment with your [<bounce>MOUSE</bounce>].");
        
        ProcessSection("learnToWalk");
        while (_managerPlayer.PlayerMovementHelper.ProcessedInputMovement == Vector3.zero && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        StartCoroutine(LearnTerminals());
    }

    [SerializeField] private AutomaticMetalDoor firstMetalDoor;
    [SerializeField] private GenericTerminal firstMetalDoorTerminal;
    private IEnumerator LearnTerminals()
    {
        yield return new WaitForSeconds(1f);
        firstMetalDoorTerminal.IsProcessing = true;
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}You'll also be expected to get familiar with our company tools.");
        _playerDialogueHelper.QueueDialogue("{size}For example, our patented Smart Terminals\u2122.");
        _playerDialogueHelper.QueueDialogue("{size}Proceed to the terminal and interact with it using [<bounce>LMB</bounce>].");
        ProcessSection("turnOnTerminal");
        while (!firstMetalDoorTerminal.IsOn && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Due to an increase of \"mutated cats\", we've implemented some new security measures.");
        _playerDialogueHelper.QueueDialogue("{size}So, now you'd have to prove your humanity with \"Catchas\u2122\".");
        //_playerDialogueHelper.QueueDialogue("{size}<wave>(\"Human or cat? We can tell that!\")</wave>\n...is how the jingle goes.");
        _playerDialogueHelper.QueueDialogue("{size}Get close so it can scan you.\nUse [<bounce>LMB</bounce>] again.");
        
        ProcessSection("openMetalDoor");
        while (!firstMetalDoor.MetalDoor.IsOpen && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();

        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Nicely done. Proceed to the next area.");
        ProcessSection("waitForExitFirstDoor");
        while (firstMetalDoor.MetalDoor.IsOpen && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        StartCoroutine(LearnToRun());
    }
    
    [SerializeField] private AutomaticMetalDoor flashlightEntranceDoor;
    private IEnumerator LearnToRun()
    {
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}If you want to proceed faster, you can use [<bounce>SHIFT</bounce>].");
        _playerDialogueHelper.QueueDialogue("{size}We need to do some cleanup, but for now you can use [<bounce>CTRL</bounce>] to move through.");
        
        ProcessSection("learnToRun");
        flashlightEntranceDoor.MetalDoor.Open();
        while (!flashlightEntranceDoor.IsLocked && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        StartCoroutine(FlashlightObtain());
    }
    
    [SerializeField] private GameObject flashlight;
    [SerializeField] private AutomaticMetalDoor flashlightExitDoor;
    private IEnumerator FlashlightObtain()
    {
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}This is one of Catalyst's standard issue flashlights.");
        
        ProcessSection("getFlashlight");
        while (flashlight != null && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Use [<bounce>F</bounce>] to turn it on and off.");
        
        flashlightExitDoor.MetalDoor.Open();
        catEntranceDoor.MetalDoor.Open();
        
        StartCoroutine(EnterCatRoom());
    }
    
    [SerializeField] private AutomaticMetalDoor catEntranceDoor;
    [SerializeField] private AutomaticMetalDoor catExitDoor;
    [SerializeField] private Transform[] catPipes;
    [SerializeField] private GameObject defaultCatPrefab;
    
    [SerializeField] private float catPipeDepositForce;
    
    private IEnumerator EnterCatRoom()
    {
        ProcessSection("enterCatRoom");
        while (!catEntranceDoor.IsLocked && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();

        StartCoroutine(FoundFirstCat());
    }
    
    private List<GameObject> _defaultCats = new List<GameObject>();
    private IEnumerator FoundFirstCat()
    {
        yield return new WaitForSeconds(1f);
        GameObject normalCat = DepositCat(defaultCatPrefab, catPipes[1]);
        _defaultCats.Add(normalCat);
        Cat cat = normalCat.GetComponent<Cat>();
        
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}You'll mainly be working with these.");
        _playerDialogueHelper.QueueDialogue("{size}These cat assets, or cassets, are all made by Catalyst.");
        _playerDialogueHelper.QueueDialogue("{size}Now, they're not all too smart, but they respond pretty well to touch.");
        _playerDialogueHelper.QueueDialogue("{size}Interact with it and see if you can get it to follow you.");
        
        ProcessSection("findFirstCat");
        while (!cat.IsFound && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Nicely done.");
        
        StartCoroutine(FoundAllCats());
    }
    private IEnumerator FoundAllCats()
    {
        yield return new WaitForSeconds(3f);
        List<Cat> cats = new List<Cat>();
        foreach (Transform t in catPipes)
        {
            GameObject normalCat = DepositCat(defaultCatPrefab, t);
            _defaultCats.Add(normalCat);
            cats.Add(normalCat.GetComponent<Cat>());
        }

        bool GetAllCatsFound()
        {
            foreach (Cat cat in cats)
            {
                if (!cat.IsFound)
                {
                    return false;
                }
            }
            return true;
        }
        
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}See if you can handle some more.");
        
        ProcessSection("findAllCats");
        while (!GetAllCatsFound() && sectionIndex >= intendedIndex)yield return new WaitForFixedUpdate();
        
        StartCoroutine(LearnRollCall());
    }

    [SerializeField] private Transform defaultCatCleanupPipe;
    private IEnumerator LearnRollCall()
    {
        yield return new WaitForSeconds(3f);
        ManagerPlayer.instance.PlayerCatHelper.ObtainRollCall();
        
        yield return new WaitForSeconds(1f);
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Now again, these cats aren't all too smart and can get a bit clumped sometimes.");
        _playerDialogueHelper.QueueDialogue("{size}If you ever need some space, you can use your whistle with [<bounce>R</bounce>].");
        
        ProcessSection("learnRollCall");
        while (!ManagerCat.instance.IsRollCalling && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        yield return new WaitForSeconds(2f);
        StartCoroutine(TabletObtain());
        
        yield return new WaitForSeconds(2f);
        
        foreach (GameObject cat in _defaultCats)
        {
            CatStateMachine csm = cat.GetComponentInChildren<CatStateMachine>();
            csm.RequestStateChange(csm.CatStatesDictionary[CatStateMachine.CatStates.Copied]);
            cat.GetComponent<AIDestinationSetter>().target = defaultCatCleanupPipe;
            ManagerCat.instance.OnRollCall(false);
        }
    }
    
    [SerializeField] private GameObject tablet;
    [SerializeField] private AutomaticMetalDoor tabletDoor;
    private IEnumerator TabletObtain()
    {
        catExitDoor.MetalDoor.Open();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("Proceed.");
        ProcessSection("enterTabletRoom");
        while (!catExitDoor.IsLocked && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}On the table is the <wave>PAW PAD</wave>.");
        
        ProcessSection("getTablet");
        while (tablet != null && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Turn it on and off with [<bounce>TAB</bounce>].");
        
        ProcessSection("openTablet");
        while (_managerPlayer.PlayerTabletHelper.IsFullyHidden && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Now, you'll see a familiar face.");
        _playerDialogueHelper.QueueDialogue("{size}Our top researchers developed a way to fully X-Ray and generate a 3D model of a cat.");
        _playerDialogueHelper.QueueDialogue("{size}We call this new procedure a\n\"CAT Scan\".");
        _playerDialogueHelper.QueueDialogue("{size}Proceed.");
        while ((_playerDialogueHelper.IsOngoing || _playerDialogueHelper.IsContinuous) && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        tabletDoor.MetalDoor.Open();
        StartCoroutine(EnterMimicCatRoom());
    }
    
    [SerializeField] private AutomaticMetalDoor mimicCatEntranceDoor;
    [SerializeField] private AutomaticMetalDoor mimicCatExitDoor;
    [SerializeField] private Transform[] mimicCatPipes;
    [SerializeField] private GameObject catPrefab;
    private GameObject _mimicCat;
    
    private IEnumerator EnterMimicCatRoom()
    {
        ProcessSection("enterMimicCatRoom");
        while (!mimicCatEntranceDoor.IsLocked && sectionIndex >= intendedIndex)
        {
            yield return new WaitForFixedUpdate();
        }
        
        StartCoroutine(MimicCatRound1());
    }
    
    private IEnumerator MimicCatRound1()
    {
        ManagerGame.Instance.Difficulty = tutorialDifficulty;
        ManagerCatModifier.instance.GeneratePhysicalModifiers();
        
        yield return new WaitForSeconds(1f);
        
        List<Cat> cats = new List<Cat>();
        List<Transform> pipes = new List<Transform>(mimicCatPipes.ToList());
        
        ////
        int randomIndex = Random.Range(0, pipes.Count);
        _mimicCat = DepositCat(catPrefab, mimicCatPipes[randomIndex]);
        cats.Add(_mimicCat.GetComponent<Cat>());
        _mimicCat.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic = true;
        pipes.RemoveAt(randomIndex);
        ////
        
        foreach (Transform t in pipes)
        {
            GameObject normalCat = DepositCat(catPrefab, t);
            cats.Add(normalCat.GetComponent<Cat>());
        }

        bool GetAllCatsFound()
        {
            foreach (Cat cat in cats)
            {
                if (cat == null) continue;
                
                if (!cat.IsFound)
                {
                    return false;
                }
            }
            return true;
        }
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Get those cats!");
        ProcessSection("allCatsFound");
        while (!GetAllCatsFound() && sectionIndex >= intendedIndex)yield return new WaitForFixedUpdate();
        
        // _playerDialogueHelper.Interrupt();
        // _playerDialogueHelper.QueueDialogue("{size}We take pride in our consistency, ");
        // _playerDialogueHelper.QueueDialogue("{size}It would've been easy to tell which are mutated, but recently, they've had new tendencies into <wave>looking like our cats.</wave>");
        // _playerDialogueHelper.QueueDialogue("{size}But, there is a way.");
        // ProcessSection("explainCopyCat");
        // yield return StandbyDialogue();
        
        ManagerTablet.Instance.TabletAppMimicModifiers.RefreshPopulate();
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Here, I downloaded some new data on to your tablet. Take a look.");
        ProcessSection("downloadedData");
        while (_managerPlayer.PlayerTabletHelper.IsFullyHidden && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Operate the tablet with [<bounce>ARROW KEYS</bounce>].");
        ProcessSection("learnMimicTraits");
        yield return StandbyDialogue();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Catalyst takes pride in their products, so it's your job to check for any defects.");
        _playerDialogueHelper.QueueDialogue("{size}If your cats have at <wave>least ONE</wave> of these traits, dispose of them immediately.");
        _playerDialogueHelper.QueueDialogue("{size}Pick up a cat and throw it away in trash chutes.");
        ProcessSection("learnTrashTerminal");
        while (_mimicCat != null && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        StartCoroutine(CatCollectionTerminal(cats));
    }
    private IEnumerator CatCollectionTerminal(List<Cat> cats)
    {
        mimicCatExitDoor.MetalDoor.Open();
        yield return new WaitForSeconds(1f);
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Now that you filtered the batch, deposit them in that terminal.");
        _playerDialogueHelper.QueueDialogue("{size}Flick the lever to turn it on, and the cats will do the rest.");
        ProcessSection("learningCCT");
        
        bool GetAllCatsGone()
        {
            foreach (Cat cat in cats)
            {
                if (cat == null) continue;
                return false;
            }
            return true;
        }
        
        while (!GetAllCatsGone() && sectionIndex >= intendedIndex)
        {
            yield return new WaitForFixedUpdate();
        }
        
        StartCoroutine(MimicCatRound2());
    }
    
    [SerializeField] private Difficulty tutorial1Difficulty;
    private IEnumerator MimicCatRound2()
    {
        ManagerGame.Instance.Difficulty = tutorial1Difficulty;
        ManagerCatModifier.instance.GeneratePhysicalModifiers();
        ManagerTablet.Instance.TabletAppMimicModifiers.RefreshPopulate();
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Great job! Let's try another batch.");
        _playerDialogueHelper.QueueDialogue("{size}But first, I've updated the data on your tablet. Please check.");
        ProcessSection("recheckTablet");
        while (_managerPlayer.PlayerTabletHelper.IsFullyHidden && sectionIndex >= intendedIndex) yield return new WaitForFixedUpdate();
        
        yield return new WaitForSeconds(1f);
        
        List<Cat> cats = new List<Cat>();
        List<Transform> pipes = new List<Transform>(mimicCatPipes.ToList());
        
        ////
        int randomIndex = Random.Range(0, pipes.Count);
        _mimicCat = DepositCat(catPrefab, mimicCatPipes[randomIndex]);
        cats.Add(_mimicCat.GetComponent<Cat>());
        _mimicCat.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic = true;
        pipes.RemoveAt(randomIndex);
        ////
        
        foreach (Transform t in pipes)
        {
            GameObject normalCat = DepositCat(catPrefab, t);
            cats.Add(normalCat.GetComponent<Cat>());
        }

        bool GetAllCatsFound()
        {
            foreach (Cat cat in cats)
            {
                if (cat == null) continue;
                
                if (!cat.IsFound)
                {
                    return false;
                }
            }
            return true;
        }
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Keep in mind, we're not perfect. Some of our cats are going to look a little weird.");
        _playerDialogueHelper.QueueDialogue("{size}But it all comes down to if at <wave>least ONE</wave> of these traits are present.");
        _playerDialogueHelper.QueueDialogue("{size}Again, throw away any faulty product.");
        ProcessSection("mimicCatRound2");
        while ((_mimicCat != null || !GetAllCatsFound()) && sectionIndex >= intendedIndex)
        {
            yield return new WaitForFixedUpdate();
        }
        
        StartCoroutine(CatCollectionTerminalFinale());
    }

    [SerializeField] private TutorialHelper tutorialHelper;
    [SerializeField] private TutorialCatCollectionTerminal tcct;
    private IEnumerator CatCollectionTerminalFinale()
    {
        yield return new WaitForSeconds(1f);
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Keep processing until you reach the quota.");
        _playerDialogueHelper.QueueDialogue("{size}And then I think you're ready!");
        ProcessSection("finalRound");
        while (!tcct.HasReachedQuota && sectionIndex >= intendedIndex)
        {
            List<Cat> cats = new List<Cat>();
            List<Transform> pipes = new List<Transform>(mimicCatPipes.ToList());
        
            ////
            int randomIndex = Random.Range(0, pipes.Count);
            _mimicCat = DepositCat(catPrefab, mimicCatPipes[randomIndex]);
            cats.Add(_mimicCat.GetComponent<Cat>());
            _mimicCat.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic = true;
            pipes.RemoveAt(randomIndex);
            ////
        
            foreach (Transform t in pipes)
            {
                GameObject normalCat = DepositCat(catPrefab, t);
                cats.Add(normalCat.GetComponent<Cat>());
            }

            bool GetAllCatsFound()
            {
                foreach (Cat cat in cats)
                {
                    if (cat == null) continue;
                
                    if (!cat.IsFound)
                    {
                        return false;
                    }
                }
                return true;
            }
            
            while ((_mimicCat != null || !GetAllCatsFound()) && sectionIndex >= intendedIndex && !tcct.HasReachedQuota)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        
        _playerDialogueHelper.Interrupt();
        _playerDialogueHelper.QueueDialogue("{size}Great job! We'll get you started right away.");
        _playerDialogueHelper.QueueDialogue("{size}Ending feed.");
        ProcessSection("endingFeed");
        yield return StandbyDialogue();
        
        tutorialHelper.CompleteTutorial();
    }

    private GameObject DepositCat(GameObject cat, Transform t)
    {
        GameObject catObj = Instantiate(cat, t.position, Quaternion.identity);
        Vector3 direction = ManagerPlayer.instance.PlayerObj.transform.position - catObj.transform.position;
        direction.y = 0f;
        catObj.transform.rotation = Quaternion.LookRotation(direction);
        catObj.GetComponent<Rigidbody>().AddForce(Vector3.down * catPipeDepositForce, ForceMode.Impulse);
        
        return catObj;
    }

}

