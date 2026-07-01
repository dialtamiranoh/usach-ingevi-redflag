using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de vida del Gerente de Sucursal.
/// HP = 3: cada respuesta correcta del jugador aplica TakeDamage(1).
/// Al llegar a 0, el gerente "es retirado" (secuencia Die).
/// La muerte actualiza el contador HUD via UIManager.
/// </summary>
public class GerenteNPC_Health : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHP = 3;
    private int hpActual;

    public bool EstaMuerto { get; private set; } = false;

    // Evento observable: la muerte del gerente notifica al HUD
    public static event System.Action OnGerenteMuerto;

    void Start()
    {
        hpActual = maxHP;
    }

    /// <summary>
    /// Recibe daño. Llamado por GerenteNPC_FSM.OnRespuestaCorrecta().
    /// </summary>
    public void TakeDamage(int dmg)
    {
        if (EstaMuerto) return;

        hpActual -= dmg;
        hpActual = Mathf.Max(hpActual, 0);

        Debug.Log($"[GerenteHealth] HP restante: {hpActual}/{maxHP}");

        if (hpActual <= 0)
            StartCoroutine(SecuenciaMuerte());
    }

    /// <summary>
    /// Secuencia de muerte: animación → desactivación → notificación HUD.
    /// </summary>
    private IEnumerator SecuenciaMuerte()
    {
        EstaMuerto = true;

        Animator anim = GetComponent<Animator>();
        anim?.SetTrigger("Die");

        // Esperar que termine la animación de muerte (ajustar según clip)
        yield return new WaitForSeconds(1.5f);

        // Notificar al HUD antes de desactivar
        OnGerenteMuerto?.Invoke();

        // Desactivar el NPC
        gameObject.SetActive(false);

        Debug.Log("[GerenteHealth] Gerente de Sucursal retirado.");
    }

    /// <summary>
    /// Devuelve HP normalizado (0-1) para barras de vida opcionales.
    /// </summary>
    public float HPNormalizado() => (float)hpActual / maxHP;
}
