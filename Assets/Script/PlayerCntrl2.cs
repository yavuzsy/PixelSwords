using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCntrl2 : MonoBehaviourPunCallbacks
{
    private GameObject manager;
    private Animator animator;
    private Rigidbody2D rb;
    public PhysicsMaterial2D mat;
    public ParticleSystem footParticle;
    private float moveSpeed = 5f;
    public float health = 10;
    private bool attackOn = true;
    private float attackRange = 0.8f;
    public Slider healthSlider;
    public float horizontalInput, verticalInput;
    private bool isJuice; 
    public GameObject winp1;
    private Animator cameraAnimator;
    private bool isParticle;
    private AudioSource audioS;
    public AudioClip stepS, damageS, bonusS;
    public DynamicJoystick joystick2;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isJuice = PlayerPrefs.GetInt("JuiceButtonState", 0) == 1;
        manager=GameObject.FindWithTag("manager");
        cameraAnimator = Camera.main.GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        footParticle.Stop();

        // Oyun başladığında "IdleEast" animasyonunu başlat
        animator.SetBool("Idle", true);
        animator.SetFloat("HorizontalDirection", -1f);
        animator.SetFloat("VerticalDirection", 0f);
    }

    private void Update()
    {
        horizontalInput = joystick2.Horizontal;
        verticalInput = joystick2.Vertical;

        // Yürüme kontrolü
        if (horizontalInput != 0 || verticalInput != 0)
        {
            // Animasyon parametrelerini güncelleme
            animator.SetBool("Idle", false);
            animator.SetBool("Walking", true);
            animator.SetFloat("HorizontalDirection", horizontalInput);
            animator.SetFloat("VerticalDirection", verticalInput);

            // Hareket etme
            Vector3 movement = new Vector3(horizontalInput, verticalInput);
            transform.position += Time.deltaTime * moveSpeed * movement;
            
            // Step Sound
            if (isJuice && !audioS.isPlaying) 
            {
                audioS.clip = stepS;
                audioS.Play();
            }

            // Karakterin sadece z rotasyonunda değişiklik yapma
            Vector3 eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0f);

            // Hareketin diğer oyunculara RPC ile gönderilmesi
            //photonView.RPC("MovePlayerRPC", RpcTarget.Others, transform.position);
        }
        else
        {
            // Durma animasyonu ve hareketi durdurma
            animator.SetBool("Idle", true);
            animator.SetBool("Walking", false);
        }

        footParticleCntrl(horizontalInput, verticalInput);

        // Saldırı kontrolü
        if (Input.GetKeyDown(KeyCode.RightControl)&& attackOn)
        {
            animator.SetBool("SwordAttack", true);
        }
        else if (Input.GetKeyUp(KeyCode.RightControl)&& attackOn)
        {
            animator.SetBool("SwordAttack", false);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player1"))
                {
                    Vector2 damageDirection = (collider.transform.position - transform.position).normalized;
                    collider.GetComponent<PlayerCntrl1>().TakeDamage(1, damageDirection);
                }
            }
            StartCoroutine(attackControl());
        }
    }
    /*[PunRPC]
    void MovePlayerRPC(Vector3 newPosition)
    {
        // Diğer oyuncunun konumunu güncelle
        transform.position = newPosition;
    } */
    
    public void OnAttackButtonPressed()
    {
        if (attackOn)
        {
            animator.SetBool("SwordAttack", true);
            // Photon PUN ile RPC çağrısı yaparak saldırıyı senkronize hale getirin
            photonView.RPC("SwordAttackRPC", RpcTarget.All);
        }
    }

    public void OnAttackButtonReleased()
    {
        if (attackOn)
        {
            animator.SetBool("SwordAttack", false);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player2"))
                {
                    Vector2 damageDirection = (collider.transform.position - transform.position).normalized;
                    // Photon PUN ile RPC çağrısı yaparak hasarı diğer oyuncuya uygulayın
                    collider.GetComponent<PhotonView>().RPC("TakeDamageRPC", RpcTarget.All, 1, damageDirection);
                }
            }
            StartCoroutine(attackControl());
        }
    }

    [PunRPC]
    void SwordAttackRPC()
    {
        // Saldırı animasyonunu senkronize hale getirin
        animator.SetBool("SwordAttack", true);
    }

    [PunRPC]
    void TakeDamageRPC(int damage, Vector2 damageDirection)
    {
        // Hasarı diğer oyuncuya uygulayın
        TakeDamage(damage, damageDirection);
    }
    
    void footParticleCntrl(float xAxis, float yAxis)
    {
        if ((xAxis != 0 || yAxis != 0) && footParticle.isStopped && isJuice)
        {
            footParticle.Play();
        }
        else if ((xAxis == 0 && yAxis == 0) && footParticle.isPlaying && !isParticle & isJuice)
        {
            footParticle.Stop();
        }
    }

    [PunRPC]
    public void TakeDamage(float damageAmount, Vector2 directional)
    {
        photonView.RPC("TakeDamageRPC", RpcTarget.All, damageAmount, directional);
    }
    [PunRPC]
    private void TakeDamageRPC(float damageAmount, Vector2 directional)
    {
        healthSlider.value -= damageAmount;

        if (healthSlider.value <= 0)
        {
            StartCoroutine(ChangeColorCoroutine());
            manager.GetComponent<GameManager>().winner(winp1);
            Destroy(gameObject, 0.52f);
        }
        else
        {
            GetComponent<Rigidbody2D>().AddForce(directional * 50f, ForceMode2D.Impulse);
            StartCoroutine(sekme());
            StartCoroutine(ChangeColorCoroutine());
        }
    }

    private IEnumerator ChangeColorCoroutine()
    {
        if (isJuice)
        {
            audioS.PlayOneShot(damageS);
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.05f);
            GetComponent<SpriteRenderer>().color = Color.white;

            // Slider değerini güncelle ve tüm oyunculara RPC ile gönder
            photonView.RPC("UpdateHealthSliderRPC", RpcTarget.All, healthSlider.value);
        }
    }
    [PunRPC]
    private void UpdateHealthSliderRPC(float value)
    {
        healthSlider.value = value;
    }
    
    IEnumerator BonusCollect()
    {
        if (isJuice)
        {
            audioS.PlayOneShot(bonusS);
            GetComponent<SpriteRenderer>().color = Color.green;
            yield return new WaitForSeconds(0.2f);
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        
        // Rastgele bir skill seçme
        int randomSkill = Random.Range(0, 2); // 0 veya 1

        if (randomSkill == 0)
        {
            // Cana +20 ekleme
            health += 3f;
            if (health>10)
            {
                health = 10;
            }
            photonView.RPC("UpdateHealthSliderRPC", RpcTarget.All, healthSlider.value);
            //healthSlider.value = health;
        }
        else
        {
            // Hıza +3f ekleme
            moveSpeed += 3f;
            yield return new WaitForSeconds(3f);
            moveSpeed -= 3f;
        }
    }
    
    IEnumerator attackControl()
    {
        attackOn = false;
        yield return new WaitForSeconds(0.5f);
        attackOn = true;
    }
    
    IEnumerator sekme()
    {
        isParticle = true;
        rb.sharedMaterial = mat;
        if (isJuice)
        {
            footParticle.Play();
            footParticle.startSize = 1.5f;
        }
        
        yield return new WaitForSeconds(1f);
        rb.sharedMaterial = null;
        if (isJuice)
        {
            footParticle.startSize = 0.8f;
            footParticle.Stop();
        }
        
        rb.velocity = Vector2.zero;
        isParticle = false;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJuice)
        {
            if (collision.gameObject.CompareTag("wallup"))
            {
                //cameraAnimator.SetTrigger("ShakeUp");
                cameraAnimator.Play("Up");
            }
            else if (collision.gameObject.CompareTag("walldown"))
            {
                //cameraAnimator.SetTrigger("ShakeDown");
                cameraAnimator.Play("Down");
            }
            else if (collision.gameObject.CompareTag("wallright"))
            {
                //cameraAnimator.SetTrigger("ShakeRight");
                cameraAnimator.Play("Right");
            }
            else if (collision.gameObject.CompareTag("wallleft"))
            {
                //cameraAnimator.SetTrigger("ShakeLeft");
                cameraAnimator.Play("Left");
            }
            else if (collision.gameObject.CompareTag("Bush"))
            {
                collision.gameObject.GetComponent<BushCntrl>().PlayEffect();
            }
        }
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike")&& attackOn)
        {
            StartCoroutine(attackControl());
            Vector2[] RastegeleYön = new[] { Vector2.left, Vector2.right };
            int randomIndex = Random.Range(0, RastegeleYön.Length);
            Vector2 randomYön = RastegeleYön[randomIndex];
            TakeDamage(0.2f, randomYön/10);
        }
        else if (other.CompareTag("Bonus"))
        {
            StartCoroutine(BonusCollect());
            Destroy(other.gameObject);
        }
    }

}