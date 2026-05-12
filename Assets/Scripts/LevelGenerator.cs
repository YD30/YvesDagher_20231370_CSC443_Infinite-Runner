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

    [Header("Gameplay")]
    [SerializeField] private float minSwitchRoom = 6f;

    [Header("Coins")]
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private int coinPoolSize = 50;

    [SerializeField] private float coinHeight = 0.5f;
    //[SerializeField] private float coinSpacing = 2f;

    [SerializeField] private int minCoinsPerLine = 3;
    [SerializeField] private int maxCoinsPerLine = 8;

    private ObjectPool<Coin> _coinPool;
    private readonly List<Coin> _activeCoins = new();

    private Chunk[] _prefabTemplates;

    private readonly Dictionary<Chunk, ObjectPool<Chunk>> _pools = new();
    private readonly Dictionary<Chunk, Chunk> _instanceToPrefab = new();

    private readonly List<Chunk> _activeChunks = new();
    private readonly List<Chunk> _candidateBuffer = new();

    private float _spawnZ;

    private LaneMask _currentExit = LaneMask.All;

    // Subway Surfers-style flow memory
    private LaneMask _preferredLane = LaneMask.Middle;

    private float _lastExitBuffer = 999f;

    void Awake()
    {
        _prefabTemplates = new Chunk[chunkPrefabs.Length];

        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            Chunk template = chunkPrefabs[i].GetComponent<Chunk>();

            _prefabTemplates[i] = template;

            _pools[template] =
                new ObjectPool<Chunk>(template, transform, chunkPoolSize);
        }

        _coinPool = new ObjectPool<Coin>(coinPrefab, transform, coinPoolSize);
    }

    void Start()
    {
        SpawnFirstSafeChunk();

        while (_spawnZ < spawnAhead)
            SpawnNextChunk();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        // Move chunks backward
        float scroll =
            GameManager.Instance.ScrollSpeed * Time.deltaTime;

        for (int i = 0; i < _activeChunks.Count; i++)
        {
            _activeChunks[i].transform.position +=
                Vector3.back * scroll;
        }

        _spawnZ -= scroll;

        // Spawn ahead
        while (_spawnZ < spawnAhead)
            SpawnNextChunk();

        // Recycle behind
        for (int i = _activeChunks.Count - 1; i >= 0; i--)
        {
            Chunk c = _activeChunks[i];

            if (c.transform.position.z + c.Length * 0.5f < -recycleBehind)
            {
                Recycle(i);
            }
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
        _preferredLane = chunk.RecommendedLane;
        _lastExitBuffer = chunk.ExitBuffer;
        GenerateCoins(chunk);
    }

    private void SpawnNextChunk()
    {
        Chunk prefab =
            PickNextChunk(_currentExit, _lastExitBuffer);

        if (prefab == null)
        {
            Debug.LogError(
                "LevelGenerator: no valid chunk found.");

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
        _preferredLane = chunk.RecommendedLane;
        _lastExitBuffer = chunk.ExitBuffer;
        GenerateCoins(chunk);
    }

    private Chunk PickNextChunk( LaneMask requiredOpen, float lastExitBuffer)
    {
        _candidateBuffer.Clear();

        for (int i = 0; i < _prefabTemplates.Length; i++)
        {
            Chunk t = _prefabTemplates[i];

            // Connectivity check
            if (!requiredOpen.ConnectsTo(t.Entry))
                continue;

            // Reaction-time fairness check
            bool bufferOk =
                (lastExitBuffer + t.EntryBuffer) >= minSwitchRoom;

            if (!bufferOk)
                continue;

            // Weighted flow continuity
            bool followsFlow =
                (t.Entry & _preferredLane) != 0;

            if (followsFlow)
            {
                // add extra weight
                _candidateBuffer.Add(t);
                _candidateBuffer.Add(t);
            }

            // normal weight
            _candidateBuffer.Add(t);
        }

        if (_candidateBuffer.Count == 0)
            return null;

        return _candidateBuffer[
            Random.Range(0, _candidateBuffer.Count)];
    }

    private void GenerateCoins(Chunk chunk)
    {
        List<int> availableLanes = new List<int>();

        if ((chunk.SafeCoinLanes & LaneMask.Left) != 0) availableLanes.Add(-1);
        if ((chunk.SafeCoinLanes & LaneMask.Middle) != 0) availableLanes.Add(0);
        if ((chunk.SafeCoinLanes & LaneMask.Right) != 0) availableLanes.Add(1);

        if (availableLanes.Count == 0)
            return;

        int lane = availableLanes[Random.Range(0, availableLanes.Count)];

        int coinCount =
            Random.Range(minCoinsPerLine, maxCoinsPerLine + 1);

        //  safe spacing inside chunk bounds
        float safePadding = 2.5f;

        float startZ = -chunk.Length * 0.5f + safePadding;
        float endZ = chunk.Length * 0.5f - safePadding;

        float usableLength = endZ - startZ;

        float step = (coinCount > 1)
            ? usableLength / (coinCount - 1)
            : 0f;

        for (int i = 0; i < coinCount; i++)
        {
            Coin coin = _coinPool.Get(chunk.transform);

            float z = startZ + step * i;

            coin.transform.localPosition = new Vector3(
                lane * 2f,
                coinHeight,
                z
            );

            coin.transform.localRotation = Quaternion.identity;
            coin.gameObject.SetActive(true);
        }
    }

    private void Recycle(int index)
    {
        Chunk chunk = _activeChunks[index];

        _pools[_instanceToPrefab[chunk]].Return(chunk);
        _instanceToPrefab.Remove(chunk);
        _activeChunks.RemoveAt(index);
    }
}