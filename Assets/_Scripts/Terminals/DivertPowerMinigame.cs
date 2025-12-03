using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DivertPowerMinigame : MonoBehaviour, ITerminalMinigame
{
    private PlayerInput _playerInput = null;
    [SerializeField] private TerminalMinigameBase terminalMinigameBase;

    [Header("Minigame")] 
    private float _spacing = 0.0775f;
    
    [SerializeField] private int entranceIndex;
    [SerializeField] private int exitIndex;
    [SerializeField] private GameObject entrance;
    [SerializeField] private GameObject exit;
    
    [SerializeField] private float confirmStayTime;
    
    [SerializeField] private AudioClip modemBeep;
    [SerializeField] private AudioClip computerClick;
    [SerializeField] private AudioClip tabletClick;

    [SerializeField] private GameObject arrows;
    [SerializeField] private Triangle upArrow;
    [SerializeField] private Triangle downArrow;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeColor;

    [Serializable]
    public class Tile
    {
        public Vector2 entrance;
        public Vector2 exit;
        public GameObject obj;
        public Rectangle outline;
        
        public Tile(Vector2 entrance, Vector2 exit, GameObject obj, Rectangle outline)
        {
            this.entrance = entrance;
            this.exit = exit;
            this.obj = obj;
            this.outline = outline;
        }
    }

    [SerializeField] private Tile horizontalTile;
    [SerializeField] private Tile upTile;
    [SerializeField] private Tile downTile;
    [SerializeField] private Tile rightUpTile;
    [SerializeField] private Tile rightDownTile;
    [SerializeField] private Tile upRightTile;
    [SerializeField] private Tile downRightTile;
    private Tile[] _presetTiles;
    
    
    private Tile[][] tiles = new Tile[3][];
    [SerializeField] private int columnIndex;

    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        Initialize();
    }

    private void Initialize()
    {
        _presetTiles = new Tile[] { horizontalTile, upTile, downTile, rightUpTile, rightDownTile, upRightTile, downRightTile };
        
        tiles[0] = new Tile[3];
        tiles[1] = new Tile[3];
        tiles[2] = new Tile[3];
        
        entranceIndex = Random.Range(0, 3);
        exitIndex = Random.Range(0, 3);
        entrance.transform.localPosition = new Vector3(entrance.transform.localPosition.x, _spacing - (_spacing * entranceIndex), 0);
        exit.transform.localPosition = new Vector3(exit.transform.localPosition.x, _spacing - (_spacing * exitIndex), 0);
        
        GenerateSolution();
        Populate();
        Format();
        
        RandomizeSolution();
        if (CheckSolution())
        {
            ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            _confirmStandby = StartCoroutine(ConfirmStandby());
        }
        
        StartCoroutine(AnimateArrows());
    }

    [ContextMenu("Randomize Solution")]
    private void RandomizeSolution()
    {
        // Randomize
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < Random.Range(0, 2); j++)
            {
                ShiftColumnTiles(i, Random.Range(0, 1) == 0);
            }
        }
        DrawUI();
    }

    private void DrawUI()
    {
        arrows.transform.localPosition = new Vector3(-_spacing + (_spacing * columnIndex), 0, 0);

        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                tiles[i][j].outline.Color = columnIndex == j ? activeColor : inactiveColor;
            }
        }
    }
    
    private int _arrowsIndex;
    [SerializeField] private float arrowAnimationOffsetTime;
    [SerializeField] private float arrowAnimationPulseMultiplier;
    [SerializeField] private List<Triangle> triangles;
    
    private IEnumerator AnimateArrows()
    {
        while (true)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];
                float H, S, V;
                Color.RGBToHSV(Color.white, out H, out S, out V);
                float pulse = (Mathf.Sin((Time.time + arrowAnimationOffsetTime * i) * arrowAnimationPulseMultiplier) * 0.5f) + 0.5f; // value from 0 to 1
                float animatedV = Mathf.Clamp01(V * (0.4f + 0.6f * pulse));
                triangle.Color = Color.HSVToRGB(H, S, animatedV);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    [SerializeField] private Transform tileHolder;
    private void Populate()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                SolutionTile match = _solutionTiles.Find(st => st.X == i && st.Y == j);
                Tile sourceTile = match != null ? match.Tile : _presetTiles[Random.Range(0, _presetTiles.Length)];
                GameObject tileObj = Instantiate(sourceTile.obj, tileHolder);
                tiles[i][j] = new Tile(sourceTile.entrance, sourceTile.exit, tileObj, tileObj.GetComponent<Rectangle>());
            }
        }
    }

    private void Format()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (tiles[i][j] != null)
                {
                    tiles[i][j].obj.transform.localPosition = new Vector3(-_spacing + (j * _spacing), _spacing - (i * _spacing), 0);
                }
            }
        }
    }

    private class SolutionTile
    {
        public int X;
        public int Y;
        public Tile Tile;

        public SolutionTile(int x, int y, Tile tile)
        {
            X = x;
            Y = y;
            Tile = tile;
        }
    }
    
    private List<SolutionTile> _solutionTiles = new List<SolutionTile>();

    private void GenerateSolution()
    {
        int x = entranceIndex;
        int y = 0;
        Vector2 direction = new Vector2(0, 1);
        
        _solutionTiles.Clear();
        string log = "Starting Log:\n";
        int loops = 0;

        while (y < 3 || x != exitIndex)
        {
            loops++;
            if (loops > 50)
            {
                Debug.Log("Loop exceeded.");
                break;
            }
            
            List<Tile> availableTiles = new List<Tile>();

            foreach (Tile tile in _presetTiles)
            {
                if (x == exitIndex && y >= 2)
                {
                    if (tile.exit.y <= 0) continue; // Tile needs to travel right
                } else if (y >= 2)
                {
                    if (tile.exit.y > 0) continue; // Tile cannot travel right
                    if (x > exitIndex && tile.exit.x > 0) continue; // Tile can only travel "up" exit
                    if (x < exitIndex && tile.exit.x < 0) continue; // Tile can only travel "down" exit
                }
                
                if (x <= 0 && tile.exit.x < 0) continue;
                if (x >= 2 && tile.exit.x > 0) continue;

                if ((int) direction.x == (int) -tile.entrance.x && (int) direction.y == (int) -tile.entrance.y)
                {
                    availableTiles.Add(tile);
                }
            }

            if (availableTiles.Count == 0)
            {
                Debug.Log("No available tiles.");
                break;
            }
            
            log += "Going dir: " + direction + " need a: " + -direction.x + ", " + -direction.y + " at " + x + ", " + y + "\n";

            string tiles = "[";
            foreach (Tile tile in availableTiles)
            {
                tiles += tile.obj.name + ", ";
            }
            log += tiles + "]\n";
            
            int random = Random.Range(0, availableTiles.Count);
            Tile randomTile = availableTiles[random];
            
            log += randomTile.obj.name + "entrance: " + randomTile.entrance + " at " + x + ", " + y + "\n\n";
            _solutionTiles.Add(new SolutionTile(x, y, randomTile));
            direction = randomTile.exit;
            
            // Apply
            x += (int) randomTile.exit.x;
            y += (int) randomTile.exit.y;
            log += "Now at " + x + ", " + y + " Dir: " + direction + "\n";
        }

        //Debug.Log(log);
    }
    
    private void OnActionPerformed(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        
        if (input.x > 0 && input.y == 0) // Right
        {
            columnIndex++;
            columnIndex %= tiles.Length;
        } else if (input.x < 0 && input.y == 0) // Left
        {
            columnIndex--;
            if (columnIndex < 0) { columnIndex = tiles.Length - 1; }
        } else if (input.y > 0) // Up
        {
            ShiftColumnTiles(columnIndex, true);
            upArrow.Color = activeColor;
        }
        else if (input.y < 0) // Down
        {
            ShiftColumnTiles(columnIndex, false);
            downArrow.Color = activeColor;
        }

        ManagerSFX.Instance.PlaySFX(computerClick, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        DrawUI();
        
        if (_confirmStandby != null) StopCoroutine(_confirmStandby);
        if (CheckSolution())
        {
            ManagerSFX.Instance.PlaySFX(modemBeep, transform.position, 0.05f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
            _confirmStandby = StartCoroutine(ConfirmStandby());
        }
    }
    
    private Coroutine _confirmStandby;
    private IEnumerator ConfirmStandby()
    {
        yield return new WaitForSeconds(confirmStayTime);
        StartCoroutine(AnimateOutlines());
    }

    private IEnumerator AnimateOutlines()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                tiles[i][j].outline.Color = inactiveColor;
            }
        }

        int pitch = 0;
        foreach (Tile tile in solutionTilesInOrder)
        {
            tile.outline.Color = Color.white;
            ManagerSFX.Instance.PlaySFX(tabletClick, transform.position, 0.3f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.7f + (0.1f * pitch), isRandomPitch: false);
            yield return new WaitForSeconds(0.075f);
            pitch++;
        }
        
        yield return new WaitForSeconds(0.5f);
        terminalMinigameBase.OnMinigameEnd();
    }

    private void ShiftColumnTiles(int index, bool isShiftUp)
    {
        int rowCount = tiles.Length; // number of rows (x dimension)
        Tile[] columnTiles = new Tile[rowCount];

        // extract column
        for (int x = 0; x < rowCount; x++)
        {
            columnTiles[x] = tiles[x][index];
        }

        if (isShiftUp)
        {
            // save top
            Tile temp = columnTiles[0];
            for (int x = 0; x < rowCount - 1; x++)
            {
                columnTiles[x] = columnTiles[x + 1];
            }
            columnTiles[rowCount - 1] = temp;
        }
        else
        {
            // save bottom
            Tile temp = columnTiles[rowCount - 1];
            for (int x = rowCount - 1; x > 0; x--)
            {
                columnTiles[x] = columnTiles[x - 1];
            }
            columnTiles[0] = temp;
        }

        // put column back
        for (int x = 0; x < rowCount; x++)
        {
            tiles[x][index] = columnTiles[x];
        }

        Format();
    }

    List<Tile> solutionTilesInOrder = new List<Tile>();
    private bool CheckSolution()
    {
        int x = entranceIndex;
        int y = 0;
        int loops = 0;
        Vector2 direction = new Vector2(0, 1);
        solutionTilesInOrder.Clear();
        
        while (true)
        {
            Tile tile = tiles[x][y];
            solutionTilesInOrder.Add(tile);
            
            if ((int) direction.x == (int) -tile.entrance.x && (int) direction.y == (int) -tile.entrance.y)
            {
                direction = tile.exit;
                
                x += (int) tile.exit.x;
                y += (int) tile.exit.y;

                if (x == exitIndex && y >= 3) return true; 
                if (x < 0 || x >= tiles.Length || y < 0 || y >= tiles.Length) return false;
            }
            else
            {
                return false;
            }
            
            loops++;
            if (loops > 50)
            {
                Debug.Log("Loop exceeded.");
                return false;
            }
        }
    }

    private void OnActionCanceled(InputAction.CallbackContext value)
    {
        upArrow.Color = inactiveColor;
        downArrow.Color = inactiveColor;
    }

    [ContextMenu("SubscribeToInput")]
    private void SubscribeToInput()
    {
        _playerInput.Enable();
        _playerInput.TerminalMinigames.DivertPower.performed += OnActionPerformed;
        _playerInput.TerminalMinigames.DivertPower.canceled += OnActionCanceled;
    }
    
    private void UnsubscribeToInput()
    {
        _playerInput.TerminalMinigames.DivertPower.performed -= OnActionPerformed;
        _playerInput.TerminalMinigames.DivertPower.canceled -= OnActionCanceled;
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
    
    private void OnDestroy()
    {
        UnsubscribeToInput();
    }
}
