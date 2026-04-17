using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackGeneration : MonoBehaviour
{
    public LevelChunkData[] levelChunkData;
    public LevelChunkData firstChunk;
    private LevelChunkData previousChunk;

    public Vector3 spawnOrigin;
    private Vector3 spawnPosition;


    public int chunksToSpawn = 20;

    private Transform currentEndPoint;

    void onEnable()
    {
        OnTriggerExit.OnChunkExited += PickAndSpawnChunk;
    }

    private void OnDisable()
    {
        OnTriggerExit.OnChunkExited -= PickAndSpawnChunk;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PickAndSpawnChunk();
        }
    }

    void Start()
    {
        previousChunk = firstChunk;

        for (int i = 0; i < chunksToSpawn; i++)
        {
            PickAndSpawnChunk();
        }
    }

    LevelChunkData PickNextChunk()
    {
        List<LevelChunkData> allowedChunkList = new List<LevelChunkData>();
        LevelChunkData nextChunk = null;

        LevelChunkData.Direction nextRequiredDirection = levelChunkData.Direction.North;

        switch (previousChunk.exitDirection)
        {
            case LevelChunkData.Direction.North:
                nextRequiredDirection = LevelChunkData.Direction.South;
                spawnPosition = spawnPosition + new Vector3(0f, 0, previousChunk.chunkSize.y);
                break;

            case LevelChunkData.Direction.East:
                nextRequiredDirection= LevelChunkData.Direction.West;
                spawnPosition = spawnPosition + new Vector3(previousChunk.chunkSize.x, 0, 0);
                break;
            case LevelChunkData.Direction.South:
                nextRequiredDirection = (LevelChunkData.Direction.North);
                spawnPosition = spawnPosition + new Vector3(0, 0, -previousChunk.chunkSize.y);
                break;
            case LevelChunkData.Direction.West:
                nextRequiredDirection=(LevelChunkData.Direction.East);
                spawnPosition = spawnPosition + new Vector3(-previousChunk.chunkSize.x, 0, 0);
                break;
            default:
                break;
        }
        for (int i = 0; i < levelChunkData.Length; i++)
        {
            if (levelChunkData[i].entryDirection == nextRequiredDirection)
            {
                allowedChunkList.Add(levelChunkData[i]);
            }
        }

        nextChunk = allowedChunkList[Random.Range(0, allowedChunkList.Count)];

        return nextChunk;

    }

    void PickAndSpawnChunk()
    {
        LevelChunkData chunkToSpawn = PickNextChunk();

        GameObject objectFromChunk = chunkToSpawn.levelChunks[Random.Range(0, chunkToSpawn.levelChunks.Length)];
        previousChunk = chunkToSpawn;
        Instantiate(objectFromChunk, spawnPosition + spawnOrigin, Quaternion.identity);
    }

    void UpdateSpawnOrigin(Vector3 originDelta)
    {
        spawnOrigin = spawnOrigin + originDelta;
    }

    void GenerateTrack()
    {
        // Start at generator position
        currentEndPoint = this.transform;

        for (int i = 0; i < trackLength; i++)
        {
            SpawnNextPiece();
        }
    }

    void SpawnNextPiece()
    {
        // Pick random prefab
        GameObject prefab = trackPieces[Random.Range(0, trackPieces.Length)];

        // Instantiate
        GameObject newPiece = Instantiate(prefab);

        TrackPiece piece = newPiece.GetComponent<TrackPiece>();

        // Align start of new piece to current end
        newPiece.transform.position = currentEndPoint.position;
        newPiece.transform.rotation = currentEndPoint.rotation;

        // Offset so startPoint matches perfectly
        Vector3 offset = piece.startPoint.position - newPiece.transform.position;
        newPiece.transform.position -= offset;

        // Update endpoint for next piece
        currentEndPoint = piece.endPoint;
    }
}