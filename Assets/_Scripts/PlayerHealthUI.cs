// PlayerHealthUI.cs (Atualizado para usar Getter)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referências")]
    public Image healthImage;
    public HealthSystem playerHealthSystem;

    [Header("Sprites de Vida (Ordem: 0=Cheio, Último=Vazio)")]
    public List<Sprite> healthSprites;

    private float lastDisplayedHealth = -1f; // Guarda a última vida exibida

    void Start()
    {
        // Validações
        if (healthImage == null) { Debug.LogError("UI: Health Image missing!"); enabled = false; return; }
        if (playerHealthSystem == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if(player != null) playerHealthSystem = player.GetComponent<HealthSystem>();
            if (playerHealthSystem == null) { Debug.LogError("UI: Player HealthSystem missing!"); enabled = false; return; }
        }
        if (healthSprites == null || healthSprites.Count == 0) { Debug.LogError("UI: Health Sprites missing!"); enabled = false; return; }

        UpdateHealthDisplay(); // Atualiza no início
    }

    void Update()
    {
        if (playerHealthSystem != null && playerHealthSystem.CurrentHealth != lastDisplayedHealth)
        {
            UpdateHealthDisplay();
            lastDisplayedHealth = playerHealthSystem.CurrentHealth; // Guarda a vida atual
        }
    }

    void UpdateHealthDisplay()
    {
        float currentHealth = playerHealthSystem.CurrentHealth;
        float maxHealth = playerHealthSystem.maxHealth;

        float healthPercent = (maxHealth > 0) ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        int spriteIndex = CalculateSpriteIndex(healthPercent);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, healthSprites.Count - 1);

        if (healthSprites[spriteIndex] != null) // Verifica se o sprite existe na lista
        {
             healthImage.sprite = healthSprites[spriteIndex];
        } else {
            Debug.LogWarning($"UI: Sprite no índice {spriteIndex} está faltando!");
        }
    }

    int CalculateSpriteIndex(float healthPercent)
    {
        int totalSprites = healthSprites.Count;
        int lastIndex = totalSprites - 1; // Índice do sprite VAZIO (ex: 6 para 7 sprites)

        // Se a vida for zero ou menos, mostra o último sprite 
        if (healthPercent <= 0f)
        {
            return lastIndex; 
        }
        else
        {
            float value = healthPercent * (totalSprites - 1);
            int indexBasedOnSegments = Mathf.FloorToInt(value);

            indexBasedOnSegments = Mathf.Min(indexBasedOnSegments, totalSprites - 2);

            int finalIndex = (lastIndex - 1) - indexBasedOnSegments; 
            return Mathf.Clamp(finalIndex, 0, lastIndex - 1); 
        }
    }
}