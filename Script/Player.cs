using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /*
     * References
     */
    //Reference to scene camera
    public Camera cam;
    //Reference to rigidbody of player
    public Rigidbody2D rb;
    //Reference to projectile spawn location on player
    public Transform Projectile_Spawner;
    //Reference to projectile template for projectile spawning
    public GameObject projTemplate;
    //Reference the Game Over Screen
    public GameOverScreen gameOverScreen;
    //Reference for player spawning
    public Transform spawnPoint;
    //Reference to PauseMenu
    public PauseMenu pause;

    /*
     * Variables
     */
    //Variable to manage player movement speed
    public float moveSpeed = 5f;
    //Variable to manage player projectile force
    public float projForce = 20f;
    //Variable for player health
    public static int health;
    public int maxHealth = 50;
    //Variable for player current health
    public int currentHealth;
    //Variable for player lives
    public static int lives;
    //Boolean for player death
    public static bool isDead;
    //Boolean for I-Frames
    private bool isInvincible;
    //How long player will be invincible
    [SerializeField]
    private float invincibilityDurationSeconds;
    //Time between player flashes
    [SerializeField]
    private float delayBetweenInvincibilityFlashes;
    //Model to scale up and down for flashes
    [SerializeField]
    private GameObject model;

    /*
     * Other
     */
    //Vectors to manage player aiming and movement
    Vector2 movement;
    Vector2 mousePos;

    // health bar and life counter
    public HealthBar healthBar;
    public LifeCounter lifeCounter;

    public GameObject life1, life2, life3;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.SetMaxHealth(maxHealth);
        health = maxHealth;
        lives = 3;
        life1.gameObject.SetActive(true);
        life2.gameObject.SetActive(true);
        life3.gameObject.SetActive(true);
        Status.playerScore = 0;
        isDead = false;
        pause.Resume();
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        PlayerDeathCheck();
        if (lives > 3)
            lives = 3;
        switch (lives)
        {
            case 3:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(true);
                life3.gameObject.SetActive(true);
                break;
            case 2:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(true);
                life3.gameObject.SetActive(false);
                break;
            case 1:
                life1.gameObject.SetActive(true);
                life2.gameObject.SetActive(false);
                life3.gameObject.SetActive(false);
                break;
            case 0:
                life1.gameObject.SetActive(false);
                life2.gameObject.SetActive(false);
                life3.gameObject.SetActive(false);
                break;
        }
        if (!PauseMenu.GameIsPaused)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        Vector2 aimDirection = mousePos - rb.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;

        rb.rotation = angle;
    }

    // method that will take the damge
    void TakeDamage(int damage)
    {
        
        health -= damage;
        healthBar.SetHealth(health);
        StartCoroutine(BecomeTemporarilyInvincible());
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projTemplate, Projectile_Spawner.position, Projectile_Spawner.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.AddForce(Projectile_Spawner.up * projForce, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag.Equals("Enemy") && !isInvincible)
        {
            TakeDamage(10);
            PlayerDeathCheck();
            //DO SOMETHING TO MOVE ENEMY AWAY FROM PLAYER AFTER CONTACT
        }
    }

    private void ScaleModelTo(Vector3 scale)
    {
        model.transform.localScale = scale;
    }

    private IEnumerator BecomeTemporarilyInvincible()
    {
        Debug.Log("Player turned invincible!");
        isInvincible = true;

        // Flash on and off for roughly invincibilityDurationSeconds seconds
        for (float i = 0; i < invincibilityDurationSeconds; i += delayBetweenInvincibilityFlashes)
        {
            if (model.transform.localScale == Vector3.one)
            {
                ScaleModelTo(Vector3.zero);
            }
            else
            {
                ScaleModelTo(Vector3.one);
            }
            yield return new WaitForSeconds(delayBetweenInvincibilityFlashes);
        }
        ScaleModelTo(Vector3.one);
        Debug.Log("Player is no longer invincible!");
        isInvincible = false;
    }

    void PlayerDeathCheck()
    {
        if (health <= 0)
        {
            isDead = true;
            health += 50;
            healthBar.SetMaxHealth(maxHealth);
            lives -= 1;
        }
        if (lives <= 0)
        {
            gameOverScreen.Setup(Status.getScore());
        }
        else if (isDead)
        {
            isDead = false;
            transform.position = spawnPoint.position;
        }
    }
}
