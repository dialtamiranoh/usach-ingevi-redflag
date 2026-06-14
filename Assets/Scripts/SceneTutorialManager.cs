using UnityEngine;
using UnityEngine.UIElements;

public class SceneTutorialManager : MonoBehaviour
{
    public UIDocument uiDocument;

    private int pasoActual = 0;

    private readonly string[] titulos = {
        "Departamento de Cumplimiento — Banco Central del Sur",
        "Tu primer día como analista",
        "Analiza cada caso",
        "Cuidado con los sobornos",
        "Tu jornada comienza ahora"
    };

    private readonly string[] descripciones = {
        "Santiago, Chile — Lunes 7:45 AM\n\nEl Departamento de Cumplimiento del Banco Central del Sur ha detectado un aumento del 40% en operaciones sospechosas este trimestre. La UAF exige resultados. Tu predecesor fue despedido por aprobar un caso de lavado de activos.",
        "Tu escritorio tiene todo lo que necesitas:\n\n• ESC → Vista del cliente\n• Q → Monitor con documentos KYC/AML\n• E → Notepad con el expediente\n\nCada cliente espera tu decisión.",
        "Revisa documentos, interroga al cliente, detecta discrepancias. Cada decisión correcta suma puntos. Las rachas multiplican tu puntaje.\n\n¡Pero cuidado! Un error grave puede costarte la jornada.",
        "Durante la jornada pueden aparecer objetos sospechosos en tu escritorio — pendrives, celulares, sobres con efectivo. Arrastra los peligrosos al cajón. Los sobornos: NO LOS TOQUES.",
        "Tienes 5 casos que resolver en esta jornada. La dificultad aumenta progresivamente.\n\nMantén tu puntaje sobre 0 y demuestra que eres digno del cargo.\n\n¿Estás listo, analista?"
    };

    private Label labelTitulo;
    private Label labelDescripcion;
    private Label labelPaso;
    private Button btnSiguiente;
    private Button btnSkip;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        labelTitulo = root.Q<Label>("label-titulo");
        labelDescripcion = root.Q<Label>("label-descripcion");
        labelPaso = root.Q<Label>("label-paso");
        btnSiguiente = root.Q<Button>("btn-siguiente");
        btnSkip = root.Q<Button>("btn-skip");

        btnSiguiente.clicked += OnSiguiente;
        btnSkip.clicked += OnSkip;

        ActualizarPaso();
    }

    void ActualizarPaso()
    {
        labelTitulo.text = titulos[pasoActual];
        labelDescripcion.text = descripciones[pasoActual];
        labelPaso.text = $"{pasoActual + 1} / {titulos.Length}";

        bool esUltimo = pasoActual == titulos.Length - 1;
        btnSiguiente.text = esUltimo ? "COMENZAR JORNADA" : "Siguiente →";
    }

    void OnSiguiente()
    {
        if (pasoActual < titulos.Length - 1)
        {
            pasoActual++;
            ActualizarPaso();
        }
        else
        {
            GameManager.Instance?.IrAlJuego();
        }
    }

    void OnSkip()
    {
        GameManager.Instance?.IrAlJuego();
    }
}