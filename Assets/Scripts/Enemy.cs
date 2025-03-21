using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4.0f;
    [SerializeField]
    private GameObject _laserPrefab;
    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    private float _fireRate = 3.0f;
    private float _canFire = -1f;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();

        if (_player == null)
        {
            Debug.LogError("The Player is NULL.");
        }

        _anim = GetComponent<Animator>();

        if (_anim == null)
        {
            Debug.LogError("The Animator is NULL.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();

        if (Time.time > _canFire && gameObject.activeInHierarchy)
        {
            _fireRate = Random.Range(3f, 7f);
            _canFire = Time.time + _fireRate;
            FireLaser();
        }
    }

    void CalculateMovement()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < -5f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 7, 0);
        }
    }

    void FireLaser()
    {
        Vector3 laserPosition = transform.position + new Vector3(0, -1.05f, 0);
        GameObject enemyLaser = Instantiate(_laserPrefab, laserPosition, Quaternion.identity, transform);

        // Grab all Laser components in this prefab (including children).
        Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
        foreach (Laser laser in lasers)
        {
            laser.AssignEnemyLaser(); // Makes them move down
        }
    }




    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();

            if (player != null)
            {
                player.Damage();
            }

            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            GetComponent<Collider2D>().enabled = false; // Disable the collider
            _audioSource.Play();
            Destroy(this.gameObject, 2.8f);
        }

        if (other.tag == "Laser" && other.tag != "EnemyLaser")
        {
            Destroy(other.gameObject);

            if (_player != null)
            {
                _player.AddScore(10);
            }

            _anim.SetTrigger("OnEnemyDeath");
            _speed = 0;
            GetComponent<Collider2D>().enabled = false; // Disable the collider
            _audioSource.Play();
            Destroy(this.gameObject, 2.8f);
        }
    }
}
