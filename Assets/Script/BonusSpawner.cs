using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BonusSpawner : MonoBehaviour
{
    public GameObject bonusPrefab;
    public float spawnInterval = 20f;
    public Vector2 spawnAreaMin = new Vector2(-8f, -3.5f);
    public Vector2 spawnAreaMax = new Vector2(5f, 3.5f);

    private void Start()
    {
        if (PlayerPrefs.GetInt("JuiceButtonState")==0)
        {
            bonusPrefab.GetComponent<Animator>().enabled = false;
        }
        InvokeRepeating("SpawnBonus", 5f, spawnInterval);
    }

    [PunRPC]
    private void SpawnBonus()
    {
        if (PlayerPrefs.GetInt("notPlayable")==0)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            PhotonNetwork.Instantiate("bonusPrefab", spawnPosition, Quaternion.identity,0,null);
        }
        
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector2(randomX, randomY);
    }
}
