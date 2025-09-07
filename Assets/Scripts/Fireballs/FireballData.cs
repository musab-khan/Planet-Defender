using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Fireball")]
public class FireballData : ScriptableObject
{
    [Header("Events")]
    public ObjectSpawnEventChannelSO particleSpawn;

    [Header("Fireball Settings")]
    public int scoreValue = 10;
    public int earthDamage = 10;
    public float fallSpeed = 5f;
    public int minEncapsulation = 3;
    public bool canBeEncapsulated = true;
    

    [Header("Snake Detection Settings")]
    public float detectionRadius = 3f;
    public float detectionInterval = 0.5f;
    public LayerMask snakeLayerMask = -1; // Which layers to check for snake parts

}
