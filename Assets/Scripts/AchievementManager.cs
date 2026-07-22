using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public event Action<ObjetoSospechoso.TipoObjeto> OnLogroDesbloqueado;

    private HashSet<ObjetoSospechoso.TipoObjeto> logrosDesbloqueados = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CargarLogros();
    }

    /// <summary>
    /// Clave PlayerPrefs del logro para el usuario activo (GameManager.nombreJugador).
    /// Los logros son por usuario: "Logro_{nombre}_{tipo}". Sin nombre (p. ej. al
    /// ejecutar MainScene directa en el editor) se usa la clave legada "Logro_{tipo}".
    /// </summary>
    public static string ClaveLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        string n = GameManager.NormalizarNombre(GameManager.Instance != null ? GameManager.Instance.nombreJugador : null);
        return string.IsNullOrEmpty(n) ? $"Logro_{tipo}" : $"Logro_{n}_{tipo}";
    }

    private void CargarLogros()
    {
        foreach (ObjetoSospechoso.TipoObjeto tipo in Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            if (PlayerPrefs.GetInt(ClaveLogro(tipo), 0) == 1)
            {
                logrosDesbloqueados.Add(tipo);
            }
        }
    }

    /// <summary>Recarga los logros desde PlayerPrefs (p. ej. al cambiar de usuario).</summary>
    public void RecargarLogros()
    {
        logrosDesbloqueados.Clear();
        CargarLogros();
    }

    public void DesbloquearLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        if (logrosDesbloqueados.Contains(tipo)) return;

        logrosDesbloqueados.Add(tipo);
        PlayerPrefs.SetInt(ClaveLogro(tipo), 1);
        PlayerPrefs.Save();

        Debug.Log($"[LOGROS] Logro desbloqueado: {tipo}");
        OnLogroDesbloqueado?.Invoke(tipo);
    }

    public bool TieneLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        return logrosDesbloqueados.Contains(tipo);
    }

    // Método utilitario para reiniciar los logros del usuario activo en modo desarrollo
    [ContextMenu("Borrar Logros")]
    public void BorrarTodosLosLogros()
    {
        logrosDesbloqueados.Clear();
        foreach (ObjetoSospechoso.TipoObjeto tipo in Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            PlayerPrefs.DeleteKey(ClaveLogro(tipo));
        }
        PlayerPrefs.Save();
        Debug.Log("[LOGROS] Todos los logros del usuario activo han sido borrados.");
    }
}
