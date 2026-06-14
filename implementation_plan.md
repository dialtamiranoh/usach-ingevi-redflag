# LAB 4 — Jugabilidad y Diseño de Niveles (RedFlag)

## Contexto del proyecto

**RedFlag** es un **serious game 3D** desarrollado en Unity que gamifica el entrenamiento en **gestión de riesgo bancario** bajo normativa chilena. El jugador es un analista de cumplimiento que revisa expedientes de clientes, interactúa con ellos, detecta objetos sospechosos en su escritorio, y toma decisiones (Aprobar / Escalar / Rechazar).

### Estado actual (LAB 3)

| Componente | Estado | Archivos |
|------------|--------|----------|
| Escenas | 4 escenas: SceneInicio, SceneTutorial, MainScene, SceneResultados | `Assets/Scenes/` |
| UI | UI Toolkit completo (UXML + USS) con top-bar, expediente, documentos, diálogo, feedback | `Assets/UI/` |
| GameManager | Singleton con persistencia, ranking, navegación entre escenas | [GameManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/GameManager.cs) |
| UIManager | Gestión completa de UI, turnos (10), timer (5min), puntaje, decisiones con feedback visual | [UIManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/UIManager.cs) |
| CaseManager | Carga casos desde JSON, validación de decisiones | [CaseManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/CaseManager.cs) |
| ScoreManager | Puntaje individual/equipo, records en PlayerPrefs | [ScoreManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/ScoreManager.cs) |
| ObjetosManager | Spawn cíclico de objetos sospechosos y sobornos | [ObjetosManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/ObjetosManager.cs) |
| ObjetoSospechoso | Drag & drop, categorías (seguridad/soborno), timers, puntaje | [ObjetoSospechoso.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/ObjetoSospechoso.cs) |
| AudioManager | Singleton con música ambiente y SFX | [AudioManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/AudioManager.cs) |
| CameraController | 3 vistas (Cliente, Monitor, Notepad) con transiciones | [CameraController.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/CameraController.cs) |
| Tutorial | 5 pasos con navegación, contexto narrativo del rol | [SceneTutorialManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/SceneTutorialManager.cs) |
| Resultados | Pantalla con puntaje final, récord, ranking top 5, reiniciar/inicio | [SceneResultados.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/SceneResultados.cs) |

### Lo que YA existe parcialmente

- ✅ **Puntaje**: UIManager lleva puntaje interno y lo muestra en top-bar
- ✅ **Turnos**: 10 turnos con timer de 5 min cada uno
- ✅ **Feedback visual**: Flash de pantalla + sello en decisiones (APROBADO/ESCALADO/RECHAZADO)
- ✅ **Pantalla de resultados**: SceneResultados con puntaje, récord, ranking
- ✅ **Tutorial**: SceneTutorial con 5 pasos narrativos
- ✅ **Casos JSON**: 5 casos con diferentes dificultades
- ⚠️ **NO usa patrón Observer**: el puntaje se modifica directamente con `AgregarPuntaje()`
- ⚠️ **Condición de fin**: solo "turnos completados", no hay victoria/derrota clara
- ⚠️ **HUD limitado**: muestra turno, tiempo y puntaje pero sin feedback reactivo dinámico
- ⚠️ **Curva de dificultad**: no hay escalamiento progresivo entre turnos
- ❌ **No hay nivel jugable de 2-5 min**: actualmente son 10 turnos × 5 min = 50 min teóricos

---

## Requisitos del LAB 4 adaptados a este serious game

| Requisito | Adaptación para RedFlag | Peso |
|-----------|------------------------|------|
| **R1** — Nivel jugable | Una **jornada** de 2-5 min con curva de dificultad (casos fáciles → difíciles, más objetos sospechosos) | 25% |
| **R2** — Sistema de jugabilidad | **Sistema de puntuación con multiplicador** (Opción B) conectado al HUD vía **eventos C#** | 20% |
| **R3** — HUD funcional | Top-bar mejorada con ≥2 elementos en tiempo real + ≥1 con **feedback visual reactivo** (animaciones) | 15% |
| **R4** — Condición de fin | **Victoria** (puntaje mínimo al final) y **derrota** (puntaje negativo o tiempo agotado) + pantalla resultado | 15% |
| **R5** — Elemento narrativo | **Pantalla de introducción narrativa** (Opción A) antes de la jornada | 10% |
| **Doc** | Documento MDA actualizado, código comentado, reflexión | 15% |

---

## Open Questions

> [!IMPORTANT]
> **Duración de la jornada (R1)**
> Actualmente hay 10 turnos × 5 min. Para cumplir el requisito de **2-5 minutos**, propongo reducir a **5 turnos con ~60 segundos por caso**, totalizando ~5 min de jornada. ¿Están de acuerdo con esta configuración?

> [!IMPORTANT]
> **Sistema de puntuación (R2)**
> Propongo **Opción B — Puntuación con multiplicador/racha**: Decisiones correctas consecutivas incrementan un multiplicador (×1, ×1.5, ×2). Un error resetea el multiplicador. Esto agrega profundidad al sistema de puntaje existente. ¿Prefieren otro sistema?

> [!IMPORTANT]
> **Condiciones de victoria/derrota (R4)**
> Propongo:
> - **Victoria**: Completar la jornada con puntaje ≥ umbral (ej. 500 pts) → "Jornada exitosa — Buen trabajo, analista"
> - **Derrota por puntaje**: Puntaje cae a 0 o negativo → "Demasiados errores — Jornada terminada"  
> - **Derrota por tiempo**: Timer llega a 0 sin decidir → "Tiempo agotado"
>
> ¿Están de acuerdo con estas condiciones?

> [!IMPORTANT]
> **Curva de dificultad (R1)**
> Propongo ampliar el banco de casos JSON a **10 casos** ordenados por dificultad:
> - **Turnos 1-2**: Casos fáciles (todo en orden → APROBAR)
> - **Turno 3**: Dificultad media (discrepancia sutil → ESCALAR)  
> - **Turnos 4-5**: Casos difíciles (PEP + smurfing → RECHAZAR)
> - Los objetos sospechosos aparecen con mayor frecuencia en turnos posteriores
>
> ¿Quieren que agregue los 5 casos adicionales al JSON?

---

## Proposed Changes

---

### Componente 1: Sistema de Eventos (Patrón Observer)

El LAB 4 exige que el HUD se conecte al sistema de jugabilidad mediante **eventos (patrón Observer)**, no polling en `Update`. Actualmente `UIManager.AgregarPuntaje()` modifica el puntaje directamente y llama `ActualizarUI()` — esto es polling implícito.

#### [MODIFY] [UIManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/UIManager.cs)

Cambios principales:

1. **Agregar eventos C#** para notificar cambios de estado:
```csharp
// Eventos para patrón Observer
public event System.Action<int, int> OnPuntajeChanged;     // (nuevoPuntaje, delta)
public event System.Action<int, int> OnTurnoChanged;       // (turnoActual, turnoTotal)
public event System.Action<float> OnTiempoChanged;         // tiempoRestante
public event System.Action<bool> OnJornadaTerminada;       // true=victoria, false=derrota
public event System.Action<int> OnMultiplicadorChanged;    // nuevoMultiplicador
```

2. **Sistema de multiplicador/racha**:
```csharp
private int rachaCorrectas = 0;
private float multiplicador = 1.0f;

// En TomarDecision():
if (correcto) {
    rachaCorrectas++;
    multiplicador = 1.0f + (rachaCorrectas - 1) * 0.5f; // ×1, ×1.5, ×2, ×2.5...
    multiplicador = Mathf.Min(multiplicador, 3.0f);       // Cap en ×3
    int puntosConMulti = Mathf.RoundToInt(puntos * multiplicador);
    AgregarPuntaje(puntosConMulti);
    OnMultiplicadorChanged?.Invoke(rachaCorrectas);
} else {
    rachaCorrectas = 0;
    multiplicador = 1.0f;
    AgregarPuntaje(penalizacion);
    OnMultiplicadorChanged?.Invoke(0);
}
```

3. **Disparar eventos** en `AgregarPuntaje()`, `SiguienteTurno()`, y `Update()`:
```csharp
public void AgregarPuntaje(int puntos) {
    int anterior = puntaje;
    puntaje += puntos;
    if (puntaje < 0) puntaje = 0;
    OnPuntajeChanged?.Invoke(puntaje, puntos); // ← evento Observer
    ActualizarUI();
}
```

4. **Reducir jornada a ~5 min**: Cambiar `turnoTotal = 5` y `tiempoRestante = 60f` por turno.

5. **Condiciones de fin de partida**:
```csharp
// Derrota por puntaje (en AgregarPuntaje):
if (puntaje <= 0 && turnoActual > 1) {
    TerminarJornada(false, "Demasiados errores de compliance");
}

// Derrota por tiempo (en Update):
if (tiempoRestante <= 0) {
    TerminarJornada(false, "Tiempo agotado — caso no resuelto");
}

// Victoria (en SiguienteTurno cuando turnoActual >= turnoTotal):
if (puntaje >= UMBRAL_VICTORIA)
    TerminarJornada(true, "Jornada exitosa");
else
    TerminarJornada(false, "Puntaje insuficiente");
```

---

### Componente 2: HUD Mejorado con Feedback Visual Reactivo (R3)

#### [MODIFY] [RedFlagUI.uxml](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/UI/RedFlagUI.uxml)

Agregar elementos al `top-bar` existente:

```xml
<!-- Multiplicador de racha -->
<ui:VisualElement name="multiplicador-container" class="multiplicador-container hidden">
    <ui:Label name="label-multiplicador" text="×1" class="label-multiplicador"/>
    <ui:Label name="label-racha" text="RACHA: 0" class="label-racha"/>
</ui:VisualElement>

<!-- Barra de progreso de jornada -->
<ui:VisualElement name="progreso-container" class="progreso-container">
    <ui:VisualElement name="progreso-fill" class="progreso-fill"/>
</ui:VisualElement>
```

#### [MODIFY] [RedFlagUI.uss](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/UI/RedFlagUI.uss)

Agregar estilos para:
- **Animación de puntaje**: clase `.puntaje-subio` (scale + color verde) y `.puntaje-bajo` (shake + color rojo)
- **Multiplicador**: glow dorado con escala progresiva según racha
- **Timer urgente**: clase `.tiempo-urgente` (parpadeo rojo cuando < 15 seg)
- **Barra de progreso**: fill animado con colores por fase de jornada
- Transiciones CSS (`transition: all 0.3s ease`)

#### [MODIFY] [UIManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/UIManager.cs)

Agregar **HUDFeedback** — lógica de feedback visual reactivo suscrita a los eventos:

```csharp
// En Awake(), después de InicializarElementos:
ConfigurarHUDFeedback();

void ConfigurarHUDFeedback() {
    // Suscribirse a eventos propios (Observer)
    OnPuntajeChanged += AnimarCambioPuntaje;
    OnMultiplicadorChanged += AnimarMultiplicador;
    OnTiempoChanged += AnimarTiempoUrgente;
}

void AnimarCambioPuntaje(int nuevoPuntaje, int delta) {
    if (delta > 0) {
        labelPuntaje.AddToClassList("puntaje-subio");
        // Remover después de la animación
        StartCoroutine(RemoverClaseDespues(labelPuntaje, "puntaje-subio", 0.5f));
    } else if (delta < 0) {
        labelPuntaje.AddToClassList("puntaje-bajo");
        StartCoroutine(RemoverClaseDespues(labelPuntaje, "puntaje-bajo", 0.5f));
    }
}

void AnimarMultiplicador(int racha) {
    if (racha >= 2) {
        multiplicadorContainer.RemoveFromClassList("hidden");
        labelMultiplicador.text = $"×{multiplicador:F1}";
        labelRacha.text = $"RACHA: {racha}";
        // Scale bounce
        multiplicadorContainer.AddToClassList("multiplicador-activo");
    } else {
        multiplicadorContainer.AddToClassList("hidden");
    }
}

void AnimarTiempoUrgente(float tiempo) {
    if (tiempo <= 15f)
        labelTiempo.AddToClassList("tiempo-urgente");
    else
        labelTiempo.RemoveFromClassList("tiempo-urgente");
}
```

**Elementos del HUD en tiempo real** (cumple ≥2):
1. **Puntaje** — se actualiza vía `OnPuntajeChanged`
2. **Tiempo restante** — se actualiza cada frame (ya existe) + feedback urgente
3. **Turno actual** — se actualiza vía `OnTurnoChanged`
4. **Multiplicador/racha** — se actualiza vía `OnMultiplicadorChanged`

**Feedback visual reactivo** (cumple ≥1):
1. ✨ Puntaje **pulsa verde** al subir, **tiembla rojo** al bajar
2. ⏱️ Timer **parpadea rojo** cuando quedan < 15 segundos
3. 🔥 Multiplicador hace **bounce + glow dorado** al activarse

---

### Componente 3: Condición de Fin de Partida (R4)

#### [MODIFY] [UIManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/UIManager.cs)

Agregar método `TerminarJornada()` y panel de resultado in-game:

```csharp
void TerminarJornada(bool victoria, string mensaje) {
    juegoActivo = false;
    
    // Mostrar panel de resultado
    var root = uiDocument.rootVisualElement;
    var panelResultado = root.Q<VisualElement>("panel-resultado-jornada");
    
    // Configurar contenido
    var titulo = panelResultado.Q<Label>("resultado-titulo");
    titulo.text = victoria ? "✓ JORNADA EXITOSA" : "✗ JORNADA FALLIDA";
    titulo.style.color = victoria 
        ? new Color(0.2f, 0.9f, 0.4f) 
        : new Color(0.9f, 0.3f, 0.3f);
    
    panelResultado.Q<Label>("resultado-mensaje").text = mensaje;
    panelResultado.Q<Label>("resultado-puntaje").text = $"{puntaje} pts";
    panelResultado.Q<Label>("resultado-racha-max").text = $"Racha máxima: {rachaMaxima}";
    
    // Botones
    panelResultado.Q<Button>("btn-reintentar").clicked += () => GameManager.Instance?.Reiniciar();
    panelResultado.Q<Button>("btn-menu").clicked += () => GameManager.Instance?.IrAInicio();
    
    // Mostrar con animación fade-in
    panelResultado.RemoveFromClassList("hidden");
    panelResultado.AddToClassList("fade-in");
    
    // Guardar puntaje
    GameManager.Instance?.SetPuntajeFinal(puntaje);
    
    OnJornadaTerminada?.Invoke(victoria);
}
```

#### [MODIFY] [RedFlagUI.uxml](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/UI/RedFlagUI.uxml)

Agregar panel de resultado in-game (antes del cierre del root):

```xml
<!-- Panel de resultado de jornada (victoria/derrota) -->
<ui:VisualElement name="panel-resultado-jornada" class="panel-resultado hidden">
    <ui:VisualElement class="resultado-modal">
        <ui:Label name="resultado-titulo" text="JORNADA EXITOSA" class="resultado-titulo"/>
        <ui:Label name="resultado-mensaje" text="" class="resultado-mensaje"/>
        <ui:VisualElement class="divider"/>
        <ui:Label name="resultado-puntaje" text="0 pts" class="resultado-puntaje"/>
        <ui:Label name="resultado-racha-max" text="Racha máxima: 0" class="resultado-racha"/>
        <ui:VisualElement class="resultado-botones">
            <ui:Button name="btn-reintentar" text="↻ REINTENTAR" class="btn btn-reintentar"/>
            <ui:Button name="btn-menu" text="◄ MENÚ PRINCIPAL" class="btn btn-menu"/>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:VisualElement>
```

---

### Componente 4: Curva de Dificultad (R1)

#### [MODIFY] [casos.json](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Data/casos.json)

Ampliar de 5 a **10 casos** ordenados por dificultad progresiva:
- **Casos 1-2**: Fáciles (todo en orden → APROBAR, señales obvias → RECHAZAR)
- **Casos 3-4**: Medios (PEP, discrepancia sutil → ESCALAR)
- **Casos 5+**: Difíciles (múltiples señales contradictorias, smurfing + PEP)

#### [MODIFY] [ObjetosManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/ObjetosManager.cs)

Ajustar spawn de objetos según progresión:
```csharp
// Ciclo más rápido en turnos avanzados
float intervaloSpawn = Mathf.Max(10f, 25f - (turnoActual * 3f)); // 25s → 22s → 19s → 16s → 13s
```

**Guía visual implícita**: Los campos con discrepancias ya se resaltan con `field-alerta`, las señales automáticas guían la atención del jugador. Esto se refuerza con el **brillo de objetos sospechosos** (`BrilloSospechoso.cs`) que usan iluminación pulsante como guía visual implícita natural del juego.

---

### Componente 5: Elemento Narrativo (R5)

#### [MODIFY] [SceneTutorialManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/SceneTutorialManager.cs)

Mejorar el tutorial existente para incluir un **contexto narrativo más inmersivo** (Opción A — Pantalla de introducción):

```csharp
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
```

---

### Componente 6: README Actualizado

#### [MODIFY] [README.md](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/README.md)

Actualizar con:
- Descripción del estado LAB 4
- Instrucciones para completar el nivel (jornada)
- Condiciones de victoria/derrota
- Enlace al video de demostración (placeholder)

---

## Resumen de archivos

| Acción | Archivo | Cambio principal |
|--------|---------|-----------------|
| MODIFY | [UIManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/UIManager.cs) | Eventos Observer, multiplicador, condiciones fin, feedback reactivo |
| MODIFY | [RedFlagUI.uxml](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/UI/RedFlagUI.uxml) | Panel multiplicador, barra progreso, panel resultado |
| MODIFY | [RedFlagUI.uss](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/UI/RedFlagUI.uss) | Animaciones CSS para feedback reactivo |
| MODIFY | [SceneTutorialManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/SceneTutorialManager.cs) | Narrativa inmersiva mejorada |
| MODIFY | [casos.json](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Data/casos.json) | Ampliar a 10 casos con curva de dificultad |
| MODIFY | [ObjetosManager.cs](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/Assets/Scripts/ObjetosManager.cs) | Spawn progresivo según turno |
| MODIFY | [README.md](file:///c:/Users/Felipe/Desktop/INGEVI/usach-ingevi-redflag-feat-gameplay-lab3/README.md) | Documentación LAB 4 actualizada |

---

## Verification Plan

### Compilación
- Verificar que todos los scripts compilan sin errores en Unity
- Verificar que no hay warnings de null reference en consola

### Pruebas en Play Mode
1. **Jornada completa**: Jugar los 5 turnos y verificar duración de 2-5 min
2. **Curva de dificultad**: Verificar que los casos se vuelven más difíciles
3. **Multiplicador**: Acertar 3 seguidas y verificar que el multiplicador sube a ×2
4. **Feedback puntaje**: Verificar animación verde al sumar, roja al restar
5. **Timer urgente**: Esperar a < 15 seg y verificar parpadeo rojo
6. **Victoria**: Completar jornada con buen puntaje → pantalla victoria
7. **Derrota por puntaje**: Errar repetidamente hasta 0 → pantalla derrota
8. **Derrota por tiempo**: Dejar pasar el tiempo → pantalla derrota
9. **Reintentar**: Verificar que el botón recarga la escena correctamente
10. **Narrativa**: Verificar tutorial mejorado con contexto inmersivo

### Arquitectura Observer
- Verificar que el HUD **no consulta estado en Update** sino que **reacciona a eventos**
- Revisar que los eventos `OnPuntajeChanged`, `OnMultiplicadorChanged` se disparan correctamente

---

> [!NOTE]
> **Nota sobre serious games**: El LAB 4 permite adaptar los sistemas al contexto del proyecto. El "sistema de vida" se adapta como **sistema de puntuación con multiplicador** (Opción B), y la "condición de derrota" es el **puntaje cayendo a 0** o el **tiempo agotándose**. Estas adaptaciones deben justificarse en el documento de diseño MDA.
