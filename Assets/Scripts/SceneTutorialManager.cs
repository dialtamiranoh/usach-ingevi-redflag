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

    // Logros
    private Button btnLogros;
    private VisualElement panelLogros;
    private Button btnCerrarLogros;
    private VisualElement grillaLogros;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        labelTitulo = root.Q<Label>("label-titulo");
        labelDescripcion = root.Q<Label>("label-descripcion");
        labelPaso = root.Q<Label>("label-paso");
        btnSiguiente = root.Q<Button>("btn-siguiente");
        btnSkip = root.Q<Button>("btn-skip");

        btnLogros = root.Q<Button>("btn-logros");
        panelLogros = root.Q<VisualElement>("panel-logros");
        btnCerrarLogros = root.Q<Button>("btn-cerrar-logros");
        grillaLogros = root.Q<VisualElement>("grilla-logros");

        btnSiguiente.clicked += OnSiguiente;
        btnSkip.clicked += OnSkip;
        
        if (btnLogros != null) btnLogros.clicked += AbrirPanelLogros;
        if (btnCerrarLogros != null) btnCerrarLogros.clicked += CerrarPanelLogros;

        ActualizarPaso();
    }

    void AbrirPanelLogros()
    {
        if (panelLogros == null || grillaLogros == null) return;
        panelLogros.style.display = DisplayStyle.Flex;
        ActualizarGrillaLogros();
    }

    void CerrarPanelLogros()
    {
        if (panelLogros != null) panelLogros.style.display = DisplayStyle.None;
    }

    void ActualizarGrillaLogros()
    {
        grillaLogros.Clear();

        foreach (ObjetoSospechoso.TipoObjeto tipo in System.Enum.GetValues(typeof(ObjetoSospechoso.TipoObjeto)))
        {
            var itemContainer = new VisualElement();
            itemContainer.style.width = 100;
            itemContainer.style.height = 100;
            itemContainer.style.marginTop = 10; itemContainer.style.marginBottom = 10; itemContainer.style.marginLeft = 10; itemContainer.style.marginRight = 10;
            itemContainer.style.backgroundColor = new Color(0.1f, 0.12f, 0.15f);
            itemContainer.style.borderTopLeftRadius = 8; itemContainer.style.borderTopRightRadius = 8; itemContainer.style.borderBottomLeftRadius = 8; itemContainer.style.borderBottomRightRadius = 8;
            itemContainer.style.borderTopWidth = 2;
            itemContainer.style.borderRightWidth = 2;
            itemContainer.style.borderBottomWidth = 2;
            itemContainer.style.borderLeftWidth = 2;
            itemContainer.style.alignItems = Align.Center;
            itemContainer.style.justifyContent = Justify.Center;

            bool desbloqueado = PlayerPrefs.GetInt($"Logro_{tipo}", 0) == 1;

            if (desbloqueado)
            {
                itemContainer.style.borderTopColor = new Color(1f, 0.82f, 0.31f); // Gold
                itemContainer.style.borderRightColor = new Color(1f, 0.82f, 0.31f);
                itemContainer.style.borderBottomColor = new Color(1f, 0.82f, 0.31f);
                itemContainer.style.borderLeftColor = new Color(1f, 0.82f, 0.31f);
                itemContainer.style.opacity = 1f;
            }
            else
            {
                itemContainer.style.borderTopColor = new Color(0.15f, 0.2f, 0.25f);
                itemContainer.style.borderRightColor = new Color(0.15f, 0.2f, 0.25f);
                itemContainer.style.borderBottomColor = new Color(0.15f, 0.2f, 0.25f);
                itemContainer.style.borderLeftColor = new Color(0.15f, 0.2f, 0.25f);
                itemContainer.style.opacity = 0.4f;
            }

            var icono = new Label(ObtenerIconoPorTipo(tipo));
            icono.style.fontSize = 36;
            icono.style.marginBottom = 5;

            var texto = new Label(desbloqueado ? tipo.ToString() : "???");
            texto.style.fontSize = 10;
            texto.style.unityTextAlign = TextAnchor.MiddleCenter;
            texto.style.whiteSpace = WhiteSpace.Normal;
            texto.style.color = Color.white;

            itemContainer.Add(icono);
            itemContainer.Add(texto);
            grillaLogros.Add(itemContainer);
        }
    }

    string ObtenerIconoPorTipo(ObjetoSospechoso.TipoObjeto tipo)
    {
        return tipo switch
        {
            ObjetoSospechoso.TipoObjeto.Pendrive => "💾",
            ObjetoSospechoso.TipoObjeto.Celular => "📱",
            ObjetoSospechoso.TipoObjeto.PostIt => "📝",
            ObjetoSospechoso.TipoObjeto.Llaves => "🔑",
            ObjetoSospechoso.TipoObjeto.Credencial => "🪪",
            ObjetoSospechoso.TipoObjeto.Carpeta => "📁",
            ObjetoSospechoso.TipoObjeto.Documento => "📄",
            ObjetoSospechoso.TipoObjeto.Tarjeta => "💳",
            ObjetoSospechoso.TipoObjeto.SobreEfectivo => "✉",
            ObjetoSospechoso.TipoObjeto.CajaRegalo => "🎁",
            _ => "❓"
        };
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
