// HealthSystem.cs (Otimizado, com Getter Público)

using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("Configuração")]
    public float maxHealth = 10f;
    public float disappearDelayAfterDeath = 2f;

    // Propriedade pública para LER a vida atual (só leitura externa)
    public float CurrentHealth { get; private set; }

    // Estado interno
    private bool isDead = false;

    // Referências (pegas automaticamente)
    private Animator animator;
    private Rigidbody rb;
    private Collider charCollider;

    void Awake()
    {
        CurrentHealth = maxHealth; // Inicializa a vida atual

        // Pega componentes
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        charCollider = GetComponent<Collider>();

        // Validações
        if (animator == null) Debug.LogWarning("HealthSystem: Animator não encontrado em " + gameObject.name);
        if (rb == null) Debug.LogWarning("HealthSystem: Rigidbody não encontrado em " + gameObject.name);
        if (charCollider == null) Debug.LogWarning("HealthSystem: Collider principal não encontrado em " + gameObject.name);
    }

    // Função para receber dano
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        CurrentHealth -= damageAmount; // Atualiza a propriedade
        Debug.Log(gameObject.name + " tomou " + damageAmount + " de dano. Vida: " + CurrentHealth + "/" + maxHealth);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if (!isDead) // Garante que Die() só seja chamado uma vez
            {
                 StartCoroutine(HandleDeath());
            }
        }
        else
        {
            if (animator != null) animator.SetTrigger("Damage");
        }
    }


    private IEnumerator HandleDeath()
    {
        isDead = true;
        Debug.Log(gameObject.name + " morreu.");

        // Pega o SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white; // Assume cor original branca
        if (spriteRenderer != null) {
            originalColor = spriteRenderer.color; // Guarda a cor original
        }

        // 1. Tenta disparar a animação de morte
        if (animator != null)
        {
             bool hasDieParameter = false;
             foreach (AnimatorControllerParameter param in animator.parameters) {
                 if (param.name == "Die") { hasDieParameter = true; break; }
             }
             if(hasDieParameter) animator.SetTrigger("Die");
             else Debug.LogWarning("Parâmetro 'Die' não encontrado no Animator de " + gameObject.name);
        }

        // 2. Desativa scripts de controle
        PlayerController3D playerController = GetComponent<PlayerController3D>();
        if (playerController != null) playerController.enabled = false;
        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null) enemyController.enabled = false;

        // 3. PARA o movimento, mas NÃO desativa colisão/física AINDA
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Para o movimento
            // NÃO TORNAMOS CINEMÁTICO AINDA
        }
        // NÃO DESATIVAMOS O COLISOR AINDA

        // --- EFEITO DE PISCAR (Mudando Cor/Alpha) ---
        if (spriteRenderer != null)
        {
            Debug.Log("Iniciando efeito de piscar (cor)...");
            float blinkInterval = 0.1f;
            float endTime = Time.time + disappearDelayAfterDeath;
            bool isVisible = true; // Controla o estado do piscar

            while (Time.time < endTime)
            {
                // Alterna entre cor original e transparente (ou outra cor)
                //spriteRenderer.color = isVisible ? originalColor : Color.clear; // Pisca ficando transparente
                spriteRenderer.color = isVisible ? originalColor : Color.red; // Pisca ficando vermelho

                isVisible = !isVisible; // Inverte para o próximo ciclo
                yield return new WaitForSeconds(blinkInterval);
            }

            // Garante que a cor volte ao normal (ou fique numa cor "morta")
             spriteRenderer.color = originalColor; // Volta à cor normal no final do piscar
             // Ou, se quiser deixar cinza, por exemplo:
             // spriteRenderer.color = Color.grey;

            Debug.Log("Fim do efeito de piscar.");
        }
        else
        {
            // Se não tem SpriteRenderer, apenas espera o tempo
            yield return new WaitForSeconds(disappearDelayAfterDeath);
        }

        // --- DESATIVAÇÃO FINAL (APÓS PISCAR) ---
        Debug.Log("Desativando física e colisão de " + gameObject.name);
        // Agora sim, desativa física e colisão
        if (rb != null) rb.isKinematic = true;
        if (charCollider != null) charCollider.enabled = false;

        // Desativa o GameObject
        Debug.Log("Desativando " + gameObject.name);
        gameObject.SetActive(false);
    }

    // Função pública para checar se está morto
    public bool IsDead()
    {
        return isDead;
    }
}