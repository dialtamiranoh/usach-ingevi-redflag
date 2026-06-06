using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjetosManager : MonoBehaviour
{
    [Header("Prefabs de objetos")]
    public GameObject prefabPendrive;
    public GameObject prefabCelular;
    public GameObject prefabPostIt;
    public GameObject prefabDocumento;
    public GameObject prefabLlave;
    public GameObject prefabTarjeta;
    public GameObject prefabCarpeta;

    [Header("Prefabs soborno")]
    public GameObject prefabRegalo;
    public GameObject prefabSoborno;


    [Header("Spawning")]
    // Puntos sobre el escritorio donde pueden aparecer objetos
    public Transform[] puntosSpawn;
    public float intervaloMinimo = 20f;
    public float intervaloMaximo = 20f;
    public int maxObjetosSimultaneos = 3;
    public float alturaEscritorio = -0.1538271f; // ajusta este valor

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
        // StartCoroutine(CicloSpawnSoborno()); ← comentar esta línea
        SpawnSoborno(); // spawn inicial del soborno
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

    void SpawnSoborno()
    {
        if (puntosSpawn.Length == 0) return;

        // Elegir prefab de soborno aleatorio
        GameObject[] prefabsSoborno = { prefabRegalo, prefabSoborno };
        GameObject prefabElegido = prefabsSoborno[
            Random.Range(0, prefabsSoborno.Length)];

        Transform punto = puntosSpawn[
            Random.Range(0, puntosSpawn.Length)];

        GameObject obj = Instantiate(prefabElegido,
            punto.position, Quaternion.identity);
        ObjetoSospechoso comp = obj.GetComponent<ObjetoSospechoso>();

        if (comp != null)
            objetosActivos.Add(comp);
    }


    IEnumerator CicloSpawnSoborno()
    {
        // Esperar el primer spawn
        yield return new WaitForSeconds(20f);
        SpawnSoborno();
        // El respawn lo maneja ProgramarRespawn — no necesita loop
    }

    void SpawnObjetoAleatorio()
    {
        if (puntosSpawn.Length == 0) return;

        GameObject[] prefabs = {
        prefabPendrive, prefabCelular, prefabPostIt,
        prefabDocumento, prefabLlave, prefabTarjeta, prefabCarpeta
    };

        // Filtrar prefabs null
        var prefabsValidos = System.Array.FindAll(prefabs, p => p != null);

        Debug.Log($"[SPAWN] Prefabs válidos: {prefabsValidos.Length}");

        if (prefabsValidos.Length == 0)
        {
            Debug.LogWarning("[SPAWN] No hay prefabs válidos asignados");
            return;
        }

        GameObject prefabElegido = prefabsValidos[Random.Range(0, prefabsValidos.Length)];
        Debug.Log($"[SPAWN] Spawneando: {prefabElegido.name}");

        Transform punto = null;
        foreach (Transform p in puntosSpawn)
            if (p != null) { punto = p; break; }
        if (punto == null) return;

        Vector3 posSpawn = new Vector3(punto.position.x, alturaEscritorio, punto.position.z);
        GameObject obj = Instantiate(prefabElegido, posSpawn, Quaternion.identity);
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

    public void ProgramarRespawn(ObjetoSospechoso obj, float tiempo)
    {
        StartCoroutine(RespawnCoroutine(obj, tiempo));
    }

    IEnumerator RespawnCoroutine(ObjetoSospechoso obj, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        obj.Respawn();
    }
}