using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3.5f;
    private float _speedMultiplier = 2;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _tripleShotPrefab;
    [SerializeField]
    private float _fireRate = 0.5f;
    private float _canFire = -1f;
    [SerializeField]
    private int _lives = 3;
    private SpawnManager _spawnManager;

    [SerializeField]
    private int _score;

    private UIManager _uiManager;

    [SerializeField]
    private AudioClip _laserSoundClip;
    private AudioSource _audioSource;

    private bool _isTripleShotActive = false;
    private bool _isSpeedBoostActive = false;
    private bool _isShieldActive = false;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private GameObject _rightEngine, _leftEngine;

    private int _shieldStrength = 3;
    private SpriteRenderer _shieldRenderer;

    // Thruster System
    [SerializeField] private float _thrusterSpeed = 5.0f;
    [SerializeField] private float _maxThrusterCharge = 1.0f;
    private float _currentThrusterCharge;
    private bool _isCoolingDown;
    [SerializeField] private Slider _thrusterSlider;
    [SerializeField] private Image _thrusterFillImage;

    // Ammo System
    [SerializeField] private int _maxAmmo = 15;
    private int _currentAmmo;

    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();
        _shieldRenderer = _shieldVisualizer.GetComponent<SpriteRenderer>();

        if (_spawnManager == null) Debug.LogError("The Spawn Manager is NULL.");
        if (_uiManager == null) Debug.LogError("The UI Manager is NULL.");
        if (_audioSource == null) Debug.LogError("The Audio Source on the player is NULL.");
        if (_shieldRenderer == null) Debug.LogError("Shield Visualizer is missing a SpriteRenderer!");
        else _audioSource.clip = _laserSoundClip;

        _currentThrusterCharge = _maxThrusterCharge;
        _thrusterSlider.value = _currentThrusterCharge;

        // Initialize Ammo
        _currentAmmo = _maxAmmo;
        _uiManager.UpdateAmmo(_currentAmmo);
    }

    void Update()
    {
        CalculateMovement();
        UpdateThrusterUI();

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            FireLaser();
        }
    }

    void CalculateMovement()
    {

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
        float currentSpeed = _speed;

        if (Input.GetKey(KeyCode.RightShift) && _currentThrusterCharge > 0 && !_isCoolingDown)
        {
            currentSpeed *= _thrusterSpeed;
            _currentThrusterCharge -= Time.deltaTime * 0.5f;
            _thrusterFillImage.color = Color.green;
        }
        else if (!_isCoolingDown)
        {
            _thrusterFillImage.color = Color.white;
        }

        if (_currentThrusterCharge <= 0)
        {
            _currentThrusterCharge = 0;
            if (!_isCoolingDown)
            {
                StartCoroutine(ThrusterCooldown());
            }
        }

        transform.Translate(direction * currentSpeed * Time.deltaTime);

        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 0), 0);

        if (transform.position.x > 11.3f)
            transform.position = new Vector3(-11f, transform.position.y, 0);
        else if (transform.position.x < -11.3f)
            transform.position = new Vector3(11f, transform.position.y, 0);

        Debug.Log("Horizontal: " + horizontalInput + " | Vertical: " + verticalInput);

    }

    IEnumerator ThrusterCooldown()
    {
        _isCoolingDown = true;

        yield return new WaitForSeconds(2.0f);

        while (_currentThrusterCharge < _maxThrusterCharge)
        {
            _currentThrusterCharge += Time.deltaTime * 0.25f;
            UpdateThrusterUI();
            yield return null;
        }

        _currentThrusterCharge = _maxThrusterCharge;
        _thrusterFillImage.color = Color.green;
        yield return new WaitForSeconds(1f);
        _thrusterFillImage.color = Color.white;

        _isCoolingDown = false;
    }

    void UpdateThrusterUI()
    {
        _thrusterSlider.value = _currentThrusterCharge / _maxThrusterCharge;
    }

    void FireLaser()
    {
        if (_currentAmmo <= 0)
        {
            // No ammo left — block firing
            return;
        }

        _canFire = Time.time + _fireRate;
        _currentAmmo--;
        _uiManager.UpdateAmmo(_currentAmmo);

        if (_isTripleShotActive)
        {
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
        }

        _audioSource.Play();
    }

    public void Damage()
    {
        if (_isShieldActive)
        {
            _shieldStrength--;

            switch (_shieldStrength)
            {
                case 2:
                    _shieldRenderer.color = Color.yellow;
                    break;
                case 1:
                    _shieldRenderer.color = Color.red;
                    break;
                case 0:
                    _isShieldActive = false;
                    _shieldVisualizer.SetActive(false);
                    break;
            }

            return;
        }

        _lives--;

        if (_lives == 2)
            _leftEngine.SetActive(true);
        else if (_lives == 1)
            _rightEngine.SetActive(true);

        _uiManager.UpdatetLives(_lives);

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();
            Destroy(this.gameObject);
        }
    }

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    public void SpeedBoostActive()
    {
        _isSpeedBoostActive = true;
        _speed *= _speedMultiplier;
        StartCoroutine(SpeedBoostPowerDownRoutine());
    }

    IEnumerator SpeedBoostPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpeedBoostActive = false;
        _speed /= _speedMultiplier;
    }

    public void ShieldsActive()
    {
        _isShieldActive = true;
        _shieldStrength = 3;
        _shieldVisualizer.SetActive(true);
        _shieldRenderer.color = Color.blue;
    }

    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    public void RefillAmmo()
    {
        _currentAmmo = _maxAmmo;
        _uiManager.UpdateAmmo(_currentAmmo);
        Debug.Log("Ammo refilled!");
    }

    public void HealPlayer()
    {
        if (_lives < 3)
        {
            _lives++;
            _uiManager.UpdatetLives(_lives);

            if (_lives == 2)
            {
                _rightEngine.SetActive(false);
            }
            else if (_lives == 3)
            {
                _leftEngine.SetActive(false);
                _rightEngine.SetActive(false);
            }

            Debug.Log("Player healed by 1!");
        }
    }

}
