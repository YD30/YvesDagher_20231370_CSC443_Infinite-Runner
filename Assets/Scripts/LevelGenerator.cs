using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Chunks")]
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private int chunkPoolSize = 3;

    [Header("Streaming")]
    [SerializeField] private float spawnAhead = 80f;
    [SerializeField] private float recycleBehind = 20f;
    [SerializeField] private float minSwitchRoom = 6f;

    private Chunk[] _prefabTemplates;
    private readonly Dictionary<Chunk, ObjectPool<Chunk>> _pools = new();
    private readonly Dictionary<Chunk, Chunk> _instanceToPrefab = new();
    private readonly List<Chunk> _activeChunks = new();
    private readonly List<Chunk> _candidateBuffer = new();

    private float _spawnZ;
    private LaneMask _currentExit = LaneMask.All;
    private float _lastExitBuffer = 999f;

    void Awake()
    {
        _prefabTemplates = new Chunk[chunkPrefabs.Length];
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            Chunk template = chunkPrefabs[i].GetComponent<Chunk>();
            _prefabTemplates[i] = template;
            _pools[template] = new ObjectPool<Chunk>(template, transform, chunkPoolSize);
        }
    }

    void Start()
    {
        SpawnFirstSafeChunk();
        while (_spawnZ < spawnAhead) SpawnNextChunk();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        float scroll = GameManager.Instance.ScrollSpeed * Time.deltaTime;
        for (int i = 0; i < _activeChunks.Count; i++)
            _activeChunks[i].transform.position += Vector3.back * scroll;
        _spawnZ -= scroll;

        while (_spawnZ < spawnAhead) SpawnNextChunk();

        for (int i = _activeChunks.Count - 1; i >= 0; i--)
        {
            Chunk c = _activeChunks[i];
            if (c.transform.position.z + c.Length * 0.5f < -recycleBehind)
                Recycle(i);
        }
    }

    private void SpawnFirstSafeChunk()
    {
        Chunk prefab = _prefabTemplates[0];
        Chunk chunk = _pools[prefab].Get(transform);

        chunk.transform.SetPositionAndRotation(
            new Vector3(0f, 0f, _spawnZ + chunk.Length * 0.5f),
            Quaternion.identity);

        _activeChunks.Add(chunk);
        _instanceToPrefab[chunk] = prefab;

        _spawnZ += chunk.Length;
        _currentExit = chunk.Exit;
        _lastExitBuffer = chunk.ExitBuffer;
    }

    private void SpawnNextChunk()
    {
        Chunk prefab = PickNextChunk(_currentExit, _lastExitBuffer);
        if (prefab == null)
        {
            Debug.LogError("LevelGenerator: no chunk connects to the current exit state.");
            return;
        }

        Chunk chunk = _pools[prefab].Get(transform);
        chunk.transform.SetPositionAndRotation(
            new Vector3(0f, 0f, _spawnZ + chunk.Length * 0.5f),
            Quaternion.identity);

        _activeChunks.Add(chunk);
        _instanceToPrefab[chunk] = prefab;
        _spawnZ += chunk.Length;
        _currentExit = chunk.Exit;
        _lastExitBuffer = chunk.ExitBuffer;
    }

    private Chunk PickNextChunk(LaneMask requiredOpen, float lastExitBuffer)
    {
        _candidateBuffer.Clear();
        Chunk bestFallback = null;
        float bestBuffer = -1f;

        for (int i = 0; i < _prefabTemplates.Length; i++)
        {
            Chunk t = _prefabTemplates[i];
            if (!requiredOpen.ConnectsTo(t.Entry)) continue;

            bool bufferOk = (lastExitBuffer + t.EntryBuffer) >= minSwitchRoom;
            if (bufferOk)
            {
                _candidateBuffer.Add(t);
            }
            else
            {
                // track the best fallback in case no candidate passes buffer check
                if (t.EntryBuffer > bestBuffer)
                {
                    bestBuffer = t.EntryBuffer;
                    bestFallback = t;
                }
            }
        }

        if (_candidateBuffer.Count > 0)
            return _candidateBuffer[Random.Range(0, _candidateBuffer.Count)];

        // no chunk had enough buffer — pick the one with the most entry breathing room
        if (bestFallback != null)
        {
            Debug.LogWarning("LevelGenerator: no chunk met minSwitchRoom, using best available fallback.");
            return bestFallback;
        }

        Debug.LogError("LevelGenerator: no chunk connects to the current exit state at all.");
        return null;
    }

    private void Recycle(int index)
    {
        Chunk chunk = _activeChunks[index];
        _pools[_instanceToPrefab[chunk]].Return(chunk);
        _instanceToPrefab.Remove(chunk);
        _activeChunks.RemoveAt(index);
    }
}