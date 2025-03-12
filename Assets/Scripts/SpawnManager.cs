using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _enemyContainer; 
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRountine());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnRountine()
    {
        while (true)
        {
            Vector3 posToSpawn = new Vector4(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
        }
    }
}
