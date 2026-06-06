using UnityEngine;
using UnityEngine.UIElements;

public class SceneResultadosManager : MonoBehaviour
{
    public UIDocument uiDocument;

    void Awake()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[Resultados] UIDocument no asignado");
            return;
        }

        var root = uiDocument.rootVisualElement;

        int puntaje = GameManager.Instance?.puntajeFinal ?? 0;
        int record = GameManager.Instance?.recordPersonal ?? 0;
        string nombre = GameManager.Instance?.nombreJugador ?? "Analista";
        bool esNuevoRecord = puntaje >= record && puntaje > 0;

        // Asignar con null check
        var lblNombre = root.Q<Label>("label-nombre");
        var lblPuntaje = root.Q<Label>("label-puntaje");
        var lblRecord = root.Q<Label>("label-record");
        var lblNuevo = root.Q<Label>("label-nuevo-record");
        var btnReinic = root.Q<Button>("btn-reiniciar");
        var btnInicio = root.Q<Button>("btn-inicio");

        if (lblNombre != null) lblNombre.text = nombre;
        if (lblPuntaje != null) lblPuntaje.text = $"{puntaje} pts";
        if (lblRecord != null) lblRecord.text = $"Récord personal: {record} pts";
        if (lblNuevo != null && esNuevoRecord)
            lblNuevo.RemoveFromClassList("hidden");

        CargarRanking(root);

        if (btnReinic != null) btnReinic.clicked += () => GameManager.Instance?.Reiniciar();
        if (btnInicio != null) btnInicio.clicked += () => GameManager.Instance?.IrAInicio();

        var btnSalir = root.Q<Button>("btn-salir");
        if (btnSalir != null)
        {
            btnSalir.clicked += () =>
            {
                #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
                #else
                                Application.Quit();
                #endif
            };
        }
    }

    void CargarRanking(VisualElement root)
    {
        var contenedor = root.Q<VisualElement>("ranking-lista");
        if (contenedor == null) return;

        var ranking = GameManager.Instance?.ObtenerRanking();
        if (ranking == null || ranking.entradas.Count == 0)
        {
            contenedor.Add(new Label("Sin registros aún")
            {
                style = { color = new UnityEngine.Color(1, 1, 1, 0.4f) }
            });
            return;
        }

        for (int i = 0; i < ranking.entradas.Count; i++)
        {
            var entrada = ranking.entradas[i];
            var fila = new VisualElement();
            fila.style.flexDirection = FlexDirection.Row;
            fila.style.justifyContent = Justify.SpaceBetween;
            fila.style.paddingTop = 8;
            fila.style.paddingBottom = 8;
            fila.style.borderBottomWidth = 1;
            fila.style.borderBottomColor = new UnityEngine.Color(1, 1, 1, 0.1f);

            var labelPos = new Label($"#{i + 1}  {entrada.nombre}");
            labelPos.style.color = i == 0
                ? new UnityEngine.Color(1, 0.8f, 0.2f, 1f)  // oro para el primero
                : new UnityEngine.Color(1, 1, 1, 0.7f);

            var labelPts = new Label($"{entrada.puntaje} pts");
            labelPts.style.color = new UnityEngine.Color(1, 1, 1, 0.5f);

            fila.Add(labelPos);
            fila.Add(labelPts);
            contenedor.Add(fila);
        }
    }
}