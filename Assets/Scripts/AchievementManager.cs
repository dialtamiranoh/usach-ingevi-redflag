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

    private void CargarLogros()
    {
        foreach (ObjetoSospechoso.TipoObjeto tipo in Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            string key = $"Logro_{tipo}";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                logrosDesbloqueados.Add(tipo);
            }
        }
    }

    public void DesbloquearLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        if (logrosDesbloqueados.Contains(tipo)) return;

        logrosDesbloqueados.Add(tipo);
        PlayerPrefs.SetInt($"Logro_{tipo}", 1);
        PlayerPrefs.Save();

        Debug.Log($"[LOGROS] Logro desbloqueado: {tipo}");
        OnLogroDesbloqueado?.Invoke(tipo);
    }

    public bool TieneLogro(ObjetoSospechoso.TipoObjeto tipo)
    {
        return logrosDesbloqueados.Contains(tipo);
    }

    // Método utilitario para reiniciar logros en modo desarrollo
    [ContextMenu("Borrar Logros")]
    public void BorrarTodosLosLogros()
    {
        logrosDesbloqueados.Clear();
        foreach (ObjetoSospechoso.TipoObjeto tipo in Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            PlayerPrefs.DeleteKey($"Logro_{tipo}");
        }
        PlayerPrefs.Save();
        Debug.Log("[LOGROS] Todos los logros han sido borrados.");
    }
}
