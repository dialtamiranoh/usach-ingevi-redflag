using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjetosManager : MonoBehaviour
{
    [Header("Prefabs de objetos")]
    public GameObject prefabPendrive;
    public GameObject prefabCelular;
    public GameObject prefabPostIt;

    [Header("Spawning")]
    // Puntos sobre el escritorio donde pueden aparecer objetos
    public Transform[] puntosSpawn;
    public float intervaloMinimo = 45f;
    public float intervaloMaximo = 90f;
    public int maxObjetosSimultaneos = 2;

    [Header("Puntaje")]
    public int puntosReportar = 150;
    public int puntosGuardarSinReportar = 50;
    public int penalizacionIgnorar = 200;

    private List<ObjetoSospechoso> objetosActivos = new();
    private ScoreManager scoreManager;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        StartCoroutine(CicloSpawn());
    }

    IEnumerator CicloSpawn()
    {
        while (true)
        {
            // Esperar intervalo aleatorio entre spawns
            float espera = Random.Range(intervaloMinimo, intervaloMaximo);
            yield return new WaitForSeconds(espera);

            if (objetosActivos.Count < maxObjetosSimultaneos)
                SpawnObjetoAleatorio();
        }
    }

    void SpawnObjetoAleatorio()
    {
        if (puntosSpawn.Length == 0) return;

        // Elegir prefab aleatorio
        GameObject[] prefabs = {
            prefabPendrive, prefabCelular, prefabPostIt
        };
        GameObject prefabElegido =
            prefabs[Random.Range(0, prefabs.Length)];

        // Elegir punto de spawn aleatorio
        Transform punto =
            puntosSpawn[Random.Range(0, puntosSpawn.Length)];

        GameObject obj = Instantiate(prefabElegido, punto.position,
            Quaternion.identity);
        ObjetoSospechoso comp = obj.GetComponent<ObjetoSospechoso>();

        if (comp != null)
            objetosActivos.Add(comp);
    }

    // Llamado por ObjetoSospechoso al ser guardado en zona segura
    public void OnObjetoGuardado(ObjetoSospechoso obj)
    {
        objetosActivos.Remove(obj);

        // TODO: implementar boton de reporte en UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        uiManager?.MostrarBotonReporte(obj);
    }

    // Llamado por ObjetoSospechoso al no ser recogido a tiempo
    public void OnObjetoIgnorado(ObjetoSospechoso obj)
    {
        objetosActivos.Remove(obj);

        scoreManager?.AgregarPuntajeEquipo(-penalizacionIgnorar);

        Debug.Log($"[LOG] Objeto ignorado: {obj.ObtenerNombreTipo()} " +
                  $"— penalización -{penalizacionIgnorar}");
    }

    // Llamado desde UI cuando el jugador presiona "Reportar"
    public void ReportarObjeto(ObjetoSospechoso obj)
    {
        scoreManager?.AgregarPuntajeIndividual(puntosReportar);
        scoreManager?.AgregarPuntajeEquipo(100);

        Debug.Log($"[LOG] Objeto reportado: {obj.ObtenerNombreTipo()} " +
                  $"+{puntosReportar} puntos individuales");
    }

    // Llamado si guarda sin reportar (timeout del botón)
    public void GuardarSinReportar(ObjetoSospechoso obj)
    {
        scoreManager?.AgregarPuntajeIndividual(puntosGuardarSinReportar);

        Debug.Log($"[LOG] Objeto guardado sin reporte: " +
                  $"{obj.ObtenerNombreTipo()} " +
                  $"+{puntosGuardarSinReportar} puntos");
    }
}