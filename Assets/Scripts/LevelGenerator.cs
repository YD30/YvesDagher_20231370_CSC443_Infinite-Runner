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

    private Chunk[] _prefabTemplates;
    private readonly Dictionary<Chunk, ObjectPool<Chunk>> _pools = new();
    private readonly Dictionary<Chunk, Chunk> _instanceToPrefab = new();
    private readonly List<Chunk> _activeChunks = new();
    private readonly List<Chunk> _candidateBuffer = new();

    private float _spawnZ;
    private LaneMask _currentExit = LaneMask.All; // first chunk has no upstream constraint

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
        while (_spawnZ < spawnAhead) SpawnNextChunk();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        // Treadmill: slide every active chunk backward by speed * dt.
        float scroll = GameManager.Instance.ScrollSpeed * Time.deltaTime;
        for (int i = 0; i < _activeChunks.Count; i++)
            _activeChunks[i].transform.position += Vector3.back * scroll;
        _spawnZ -= scroll;

        // Spawn ahead until we've filled the visible distance.
        while (_spawnZ < spawnAhead) SpawnNextChunk();

        // Recycle anything that has fallen far behind the camera.
        for (int i = _activeChunks.Count - 1; i >= 0; i--)
        {
            Chunk c = _activeChunks[i];
            if (c.transform.position.z + c.Length * 0.5f < -recycleBehind)
                Recycle(i);
        }
    }

    private void SpawnNextChunk()
    {
        Chunk prefab = PickNextChunk(_currentExit);
        if (prefab == null)
        {
            Debug.LogError("LevelGenerator: no chunk in the prefab list connects to the current exit state. " +
                           "Add a chunk whose Entry includes one of the open lanes.");
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
    }

    // Socket matching: keep prefabs whose Entry shares at least one open lane with requiredOpen.
    private Chunk PickNextChunk(LaneMask requiredOpen)
    {
        _candidateBuffer.Clear();
        for (int i = 0; i < _prefabTemplates.Length; i++)
        {
            Chunk t = _prefabTemplates[i];
            if (requiredOpen.ConnectsTo(t.Entry))
                _candidateBuffer.Add(t);
        }
        if (_candidateBuffer.Count == 0) return null;
        return _candidateBuffer[Random.Range(0, _candidateBuffer.Count)];
    }

    private void Recycle(int index)
    {
        Chunk chunk = _activeChunks[index];
        _pools[_instanceToPrefab[chunk]].Return(chunk);
        _instanceToPrefab.Remove(chunk);
        _activeChunks.RemoveAt(index);
    }
}
