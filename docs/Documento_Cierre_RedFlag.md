# Documento de Cierre — Red Flag
### FBD Studio · LAB 6 CIERRE · Ingeniería de Videojuegos (USACH)
Diego Altamirano · Felipe Cifuentes · Byron Obregón — Julio 2026

> Este documento complementa al GDD con la **dimensión aplicada** (Bloque A) y las **pruebas y ajustes** (Bloque B). Máx. 4 páginas.

---

## PARTE 1 — Dimensión aplicada (Bloque A)

**Clasificación del proyecto.** Red Flag es un **serious game** con un fuerte componente de **gamificación de un proceso**. Su objetivo primario no es lúdico sino formativo: entrenar la gestión de riesgo bancario (KYC, AML, Compliance) bajo la normativa chilena vigente. El entretenimiento —la tensión estilo *Papers, Please*— es el vehículo que sostiene la atención, pero cada mecánica traduce un procedimiento real (verificación documental, interrogatorio, detección de red flags, decisión de compliance). Por eso no clasifica como videojuego de fines lúdicos: el juego existe para que se aprenda a decidir bien, no para entretener por sí mismo.

**Necesidad o valor identificado.** ¿Para quién es útil? Para instituciones financieras chilenas y sus analistas de cumplimiento —tanto colaboradores nuevos como equipos ya operativos— que hoy reciben una capacitación obligatoria, teórica y repetitiva. ¿Por qué vale la pena más allá del entretenimiento? Porque el lavado de activos es un problema real y costoso en Chile (según la UAF, 323 sentencias condenatorias y decomisos por US$58,1 millones entre 2007 y 2023) y no existe una herramienta que gamifique este entrenamiento bajo el marco regulatorio local; Red Flag convierte la norma en práctica activa de decisión.

**Objetivo de impacto.** Que quien use el proyecto sea capaz de **reconocer una discrepancia documental o transaccional (red flag) y decidir la acción de compliance correcta —aprobar, escalar o rechazar— justificándola con la normativa aplicable**, después de jugar.

**(Opcional) Análisis TAD resumido.** *Competencia:* el multiplicador por racha y el puntaje creciente dan retroalimentación clara de dominio. *Autonomía:* el jugador elige el orden de revisión (documentos, interrogatorio, objetos) y cómo priorizar bajo el reloj. *Relación:* la ficción del banco, el cliente-NPC y el Gerente que interpela sitúan al jugador dentro de un equipo y una jerarquía profesional.

---

## PARTE 2 — Sesión de prueba y ajustes (Bloque B)

### Documentación de la sesión de playtesting

- **Perfil del participante:** Fernanda Vidal, 31 años; experiencia con videojuegos **nula**; sí conoce el dominio (banca, KYC/AML, compliance). Sesión del 21-07-2026, ~15 minutos, sin intervención del equipo.
- **¿Qué fue lo más confuso?** "Las indicaciones de control, el control de la cámara y el proceso de mover objetos del escritorio."
- **¿Qué fue lo que más disfrutaste?** "La sensación de logro y aprendizaje que se obtiene del juego, junto con la música."
- **¿Qué cambiarías si pudieras?** "El tiempo para resolver el caso cuando el gerente interrumpe, y que el gerente no haga tantas preguntas."
- **Observación 1 del equipo:** El uso de la cámara resulta engorroso; durante toda la sesión la jugadora se enfocó en resolver los casos revisando la documentación, sin percatarse de los objetos que aparecían en el escritorio.
- **Observación 2 del equipo:** Las interrupciones del Gerente afectan el ritmo de juego y generan molestia (perdió su primera jornada con 0 puntos; tras una jornada fallida el Gerente aún lanzaba una última pregunta, y las opciones KYC se salían del cuadro de texto).

### Ajuste N.º 1 — Onboarding de controles

- **Qué observaste:** La participante no distinguió Q de E (tutorial, README y código describían mapeos contradictorios, y la tecla Esc estaba documentada pero no implementada), no utilizó la cámara sin explicación y tuvo dificultades para entender cómo mover los objetos del escritorio, perdiendo su primera jornada con 0 puntos.
- **Qué cambiaste:** Se unificó toda la documentación al mapeo real (**Q=Monitor, E=Notepad**, ambos toggle), se **implementó Esc** para volver a la vista del cliente (`CameraController.cs`), se rediseñó la lámina 2 del tutorial con los controles completos (cámara con clic derecho, arrastre de objetos) en `SceneTutorialManager.cs`, y se añadió una **leyenda de controles siempre visible** en el HUD (`controles-hint` en `RedFlagUI.uxml`/`.uss`).
- **Por qué mejora la experiencia:** Reduce la carga de aprendizaje inicial y permite que una persona sin experiencia comprenda las mecánicas esenciales antes de que el tiempo y el puntaje generen penalizaciones.

### Ajuste N.º 2 — Ritmo e interrupciones del Gerente NPC

- **Qué observaste:** Las preguntas del Gerente se percibieron demasiado frecuentes (la escena fijaba visitas cada 15 s en los tres niveles), consumían el tiempo del caso en curso, llegaban incluso tras una jornada ya fallida, y las opciones de respuesta KYC se salían del cuadro de texto.
- **Qué cambiaste:** Se **pausa el timer del caso** mientras dura la pregunta (`UIManager.PausarTiempo()` invocado desde la FSM), se espaciaron las visitas por nivel (15/15/15 → **45/30/20 s** en `MainScene.unity`), se bloquearon las interrupciones y penalizaciones tras el fin de la jornada (guardas en `GerenteNPC_FSM.cs` y `AgregarPuntaje`), y las opciones pasaron a una columna con ajuste de texto garantizado (clase `.gerente-opcion`).
- **Por qué mejora la experiencia:** Mantiene el desafío de las interrupciones sin convertirlas en una penalización injusta, y asegura que las opciones puedan leerse y responderse correctamente.

---

## PARTE 3 — Reflexión final

**Tres cosas que aprendimos este semestre.**
1. Traducir normativa compleja a mecánicas jugables obliga a priorizar: el valor formativo está en la decisión, no en el volumen de texto.
2. Desacoplar lógica y presentación con el patrón Observer nos permitió incorporar sistemas nuevos (el NPC del LAB 5) sin reescribir el HUD.
3. Una IA "simple" (FSM + NavMesh) es sobre todo un ejercicio de manejar casos borde: rutas rotas, empujes fuera de la malla y dobles respuestas consumieron la mayor parte del esfuerzo.

**Una cosa que haríamos diferente.** Definir de una sola vez —en una fuente de verdad— parámetros como el conteo de turnos y el layout, para evitar inconsistencias entre el diseño, el HUD y el flujo del juego; y hacer playtesting externo desde etapas tempranas, no solo en el cierre.

**Una cosa de la que estamos orgullosos.** Haber construido, sin experiencia previa en Unity, un serious game 3D coherente de principio a fin —con mecánica principal, nivel con HUD y condición de fin, y un agente autónomo integrado a los sistemas anteriores— sobre un tema real y socialmente relevante como el cumplimiento bancario chileno.

---

**Video de respaldo de la demo:** https://drive.google.com/file/d/1OXWIXangi2KwchSyDzezeywh4P7cwcFG/view?usp=sharing
