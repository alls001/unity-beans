// PlayerController3D.cs (CORRIGIDO: Sprint Parado, Bloqueio de Ações no Ataque)

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float sprintForce = 15f;
    public float sprintDuration = 0.3f;
    public float sprintCooldown = 1.0f;

    [Header("Combate")]
    public Transform attackPoint;
    [Range(0.1f, 3f)] public float attackRange = 1f;
    public float attackDamage = 2.5f;
    public LayerMask enemyLayer;
    public float attackAnimationDuration = 0.5f; 

    // Componentes
    private Rigidbody rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Estado interno
    private Vector2 input;
    private bool canSprint = true;
    private bool isSprinting = false;
    private bool isAttacking = false; 
    private float lastNonZeroHorizontal = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (attackPoint == null) Debug.LogError("PlayerController3D: AttackPoint não atribuído!");
    }

    void Update()
    {
        // Só permite input e ações se NÃO estiver atacando
        if (!isAttacking)
        {
            HandleMovementInput();
            HandleSprintInput();
            HandleAttackInput();
        }
        // O Flip pode acontecer mesmo atacando para virar na direção certa
        HandleFlip();
        UpdateAnimator(); // Atualiza o animator com os estados atuais
    }

    void HandleMovementInput()
    {
        if (!isSprinting) // Só pega input de movimento se não estiver sprintando
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            input = Vector2.zero; // Zera input durante sprint
        }
    }

     void HandleSprintInput()
    {
        // Removemos a checagem 'input.sqrMagnitude > 0.01f'
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && canSprint)
        {
            StartCoroutine(SprintDash());
        }
    }

     void HandleAttackInput()
    {
        // Ação de Ataque (só se não estiver sprintando)
        if (Input.GetButtonDown("Fire1") && !isSprinting)
        {
            StartCoroutine(AttackSequence()); // --- MUDANÇA: Inicia a sequência de ataque
        }
    }


    void HandleFlip()
    {
        // Atualiza a direção apenas se houver input horizontal E não estiver atacando/sprintando
        if (Mathf.Abs(input.x) > 0.01f && !isSprinting && !isAttacking)
        {
            lastNonZeroHorizontal = input.x;
            spriteRenderer.flipX = (input.x < 0);
        }
        else // Mantém a última direção se parado, movendo vertical, sprintando ou atacando
        {
             spriteRenderer.flipX = (lastNonZeroHorizontal < 0);
        }
    }

    IEnumerator SprintDash()
    {
        canSprint = false;
        isSprinting = true;
        animator.SetBool("IsSprinting", true);

        Vector3 dashDirection = new Vector3(input.x, 0f, input.y).normalized;
        if (dashDirection == Vector3.zero) // Usa última direção se parado
        {
             dashDirection = new Vector3(lastNonZeroHorizontal > 0 ? 1 : -1, 0, 0);
        }

        // Para momentaneamente antes de aplicar força (opcional, dá mais "peso")
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        // Aplica a velocidade do dash
        rb.linearVelocity = new Vector3(dashDirection.x * sprintForce, rb.linearVelocity.y, dashDirection.z * sprintForce);


        yield return new WaitForSeconds(sprintDuration);

        isSprinting = false;
        animator.SetBool("IsSprinting", false);

        // Zera velocidade horizontal pós-dash para parada mais brusca (opcional)
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        yield return new WaitForSeconds(sprintCooldown);
        canSprint = true;
    }


    void FixedUpdate()
    {
        // Só aplica movimento normal se NÃO estiver sprintando OU atacando
        if (!isSprinting && !isAttacking)
        {
            MoveCharacter();
        }
        // Se estiver atacando, força a parada horizontal
        else if (isAttacking)
        {
             rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void UpdateAnimator()
    {
        // Se estiver atacando ou sprintando, força Speed=0 para animações Idle/Walk não tocarem
        float effectiveSpeed = (isAttacking || isSprinting) ? 0f : input.sqrMagnitude;

        animator.SetFloat("Horizontal", Mathf.Abs(input.x));
        animator.SetFloat("Vertical", input.y);
        animator.SetFloat("Speed", effectiveSpeed); // Usa a velocidade efetiva
    }

    void MoveCharacter()
    {
        Vector3 targetVelocity = new Vector3(input.x, 0f, input.y).normalized * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true; // Bloqueia outras ações
        animator.SetTrigger("Attack"); // Dispara a animação

        // Espera um pequeno delay para a animação começar
        yield return new WaitForSeconds(0.1f); 

        // Realiza a detecção de dano 
        PerformDamageCheck();

        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false; 
    }

    // Função separada para checar o dano
    void PerformDamageCheck()
    {
        if (attackPoint == null) return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider enemyCollider in hitEnemies)
        {
            HealthSystem enemyHealth = enemyCollider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }


    void OnDrawGizmosSelected()
    {

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}