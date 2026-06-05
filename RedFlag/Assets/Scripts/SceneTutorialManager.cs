using UnityEngine;
using UnityEngine.UIElements;

public class SceneTutorialManager : MonoBehaviour
{
    public UIDocument uiDocument;

    private int pasoActual = 0;

    private readonly string[] titulos = {
        "Bienvenido a Red Flag",
        "Tu escritorio",
        "Analiza cada caso",
        "Objetos sospechosos",
        "Toma tu decisión"
    };

    private readonly string[] descripciones = {
        "Eres un analista de cumplimiento bancario. Tu trabajo es revisar clientes, detectar irregularidades y proteger la integridad del sistema financiero.",
        "Tienes tres vistas:\n\n• ESC → Vista cliente\n• Q → Monitor KYC/AML\n• E → Notepad con el expediente",
        "Cada caso presenta un cliente con historial, transacciones y antecedentes. Revisa los datos y decide: APROBAR, ESCALAR o RECHAZAR.",
        "Durante la jornada pueden aparecer objetos sospechosos en tu escritorio — pendrives, celulares, documentos. Arrástralos al cajón antes de que desaparezcan.",
        "Tienes 5 minutos por jornada. Cada decisión correcta suma puntos. Los errores penalizan. ¿Estás listo?"
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