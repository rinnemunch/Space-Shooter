using System.Collections;
using UnityEngine;

public class Rapid : MonoBehaviour
{
    [SerializeField] private GameObject _rapidFirePowerup;
    private bool _stopSpawning = false;

    public void StartSpawning()
    {
        StartCoroutine(SpawnRapidPowerupRoutine());
    }

    IEnumerator SpawnRapidPowerupRoutine()
    {
        yield return new WaitForSeconds(10f); // Delay before first drop

        while (!_stopSpawning)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
            Instantiate(_rapidFirePowerup, posToSpawn, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(20f, 40f)); // Rare drop timing
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}
