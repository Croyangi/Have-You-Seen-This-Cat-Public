using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ManagerDataPersistence : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    private GameData _gameData;
    private List<IDataPersistence> _dataPersistenceObjects = new List<IDataPersistence>();
    private FileDataHandler _dataHandler;

    public static ManagerDataPersistence Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        Instance = this;
    }

    private void Start()
    {
        this._dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this._dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> _dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(_dataPersistenceObjects);
    }

    public void NewGame()
    {
        _gameData = new GameData();
    }

    public void LoadGame()
    {
        this._gameData = _dataHandler.Load();

        if (this._gameData == null)
        {
            Debug.Log("No data was found. Initializing data to default values.");
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(_gameData);
        }
    }
    
    public GameData GetGameData()
    {
        _gameData = _dataHandler.Load();

        if (_gameData == null)
        {
            Debug.Log("No data was found. Initializing data to default values.");
            NewGame();
        }

        return _gameData;
    }

    public void SaveGame()
    {
        //Debug.Log("Saving game.");
        foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            if (dataPersistenceObj != null)
            {
                dataPersistenceObj.SaveData(ref _gameData);
            }
        }
        _dataHandler.Save(_gameData);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
