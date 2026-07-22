# Red Flag 🚩
### Simulador de Gestión de Riesgo Bancario
**Universidad de Santiago de Chile — Departamento de Ingeniería Informática**  
Asignatura: Ingeniería de Videojuegos: Fundamentos y Aplicaciones Interactivas  
Equipo: **FBD Studio** — Diego Altamirano · Felipe Cifuentes · Byron Obregón

---

## Descripción

Red Flag es un serious game 3D desarrollado en Unity que gamifica el entrenamiento en gestión de riesgo bancario bajo el marco regulatorio chileno. El jugador asume el rol de un analista junior que debe revisar expedientes, interactuar con clientes y tomar decisiones de compliance (Aprobar / Escalar / Rechazar) basadas en normativa real (Ley 19.913, Ley 20.393, Circular UAF N°62).

---

## Ramas

| Rama | Descripción |
|---|---|
| `main` | Versión estable del proyecto |
| `dev_deltatime` | Sistema de cámaras con 3 vistas y movimiento con delta time |

---

## Mecánicas implementadas

- **Vista Cliente** — vista por defecto del analista frente al cliente 3D. Click derecho para mirar alrededor libremente con límites naturales.
- **Vista Monitor** (tecla Q, toggle) — zoom al monitor del escritorio con panel de Documentos y Evidencia (KYC, AML, tabs).
- **Vista Notepad** (tecla E, toggle) — zoom al notepad del escritorio con panel de Expediente del caso.
- **Esc** (o la misma tecla de la vista) — vuelve a la vista del cliente.
- **W** (Nivel 2) — alterna entre tu escritorio KYC y el del compañero analista AML para coordinar la decisión.
- **Clic en cliente** — abre panel de diálogo con preguntas predefinidas.
- **Clic en campos del expediente** — genera preguntas automáticas al cliente.
- **APROBAR / ESCALAR / RECHAZAR** — decisiones con consecuencias en el puntaje.

---

## Controles

| Tecla / Acción | Función |
|---|---|
| **Q** | Vista Monitor (documentos KYC/AML) — misma tecla para volver |
| **E** | Vista Notepad (expediente del caso) — misma tecla para volver |
| **Esc** | Volver a la vista del cliente |
| **W** | Cambiar entre tu escritorio y el de tu compañero analista (Nivel 2) |
| **Clic derecho (mantener)** | Mirar alrededor libremente en la vista del cliente |
| **Clic izquierdo en el cliente** | Abrir panel de diálogo con preguntas |
| **Clic en campos del expediente** | Generar preguntas automáticas al cliente |
| **Arrastrar (drag & drop)** | Mover objetos sospechosos del escritorio al cajón |
| **APROBAR / ESCALAR / RECHAZAR** | Botones de decisión del caso |

---

## Requisitos previos

- [Unity Hub](https://unity.com/download)
- **Unity 6.4** (6000.4.1f1) con módulos **Web** y **Windows**
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con workload **Game development with Unity**
- [Git](https://git-scm.com/)

---

## Clonar el repositorio

```bash
git clone https://github.com/dialtamiranoh/usach-ingevi-redflag.git
```

Para clonar una rama específica:

```bash
git clone -b dev_deltatime https://github.com/dialtamiranoh/usach-ingevi-redflag.git
```

---

## Importar en Unity Hub

1. Abre **Unity Hub**
2. Haz clic en **Add → Add project from disk**
3. Navega a la carpeta `usach-ingevi-redflag/`
4. Selecciónala y haz clic en **Open with Unity 6.4**

> ⚠️ La primera apertura puede tardar varios minutos mientras Unity regenera la carpeta `Library/`.

---

## Configurar Visual Studio

1. **Edit → Preferences → External Tools**
2. **External Script Editor → Visual Studio 2022**
3. Clic en **Regenerate project files**

---

## Abrir la escena principal

En el panel **Project** navega a `Assets/Scenes/` y doble clic en **MainScene**.

---

## Cómo jugar (resumen rápido)

1. Abre el proyecto en **Unity 6.4** y carga `Assets/Scenes/SceneInicio.unity` (o `MainScene` para saltar directo a la jornada).
2. Pulsa **Play**. Avanza por la pantalla de inicio y el tutorial.
3. Resuelve los **5 casos** de la jornada: revisa documentos (Q), consulta el expediente (E), interroga al cliente y detecta objetos sospechosos.
4. Decide **Aprobar / Escalar / Rechazar** en cada caso. Mantén rachas para multiplicar el puntaje.
5. Responde al **Gerente NPC** cuando aparezca. Termina la jornada con más de 500 pts para ganar.

---

## Estructura del proyecto

```
Assets/
├── Audio/
├── Data/
│   ├── casos.json          # Banco de casos KYC/AML/Compliance
│   └── npcPreguntas.json   # Preguntas de normativa del Gerente NPC
├── Materials/
├── Models/
│   ├── VNB - Office Set/   # Assets de oficina low-poly
│   └── LowPolyPeople/      # Personajes low-poly
├── Prefabs/
├── Scenes/
│   ├── SceneInicio
│   ├── SceneTutorial
│   ├── MainScene
│   └── SceneResultados
├── Scripts/
│   ├── UIManager.cs        # Gestión de UI, turnos, puntaje y eventos (Observer)
│   ├── CaseManager.cs      # Carga y gestión de casos desde JSON
│   ├── CameraController.cs # Sistema de 3 vistas con delta time
│   ├── GerenteNPC_FSM.cs   # FSM del NPC autónomo (Patrol/Approach/Interact)
│   └── ...
├── Settings/
└── UI/
    ├── RedFlagUI.uxml
    └── RedFlagUI.uss
```

---

## Agregar nuevos casos

En `Assets/Data/casos.json` agrega casos con esta estructura:

```json
{
  "id": "#00147",
  "tipo": "KYC",
  "prioridad": "ALTA",
  "cliente": {
    "nombre": "Nombre Apellido",
    "rut": "12.345.678-9",
    "nacionalidad": "Chilena",
    "actividad": "Actividad económica",
    "esPEP": false
  },
  "documentos": {
    "cedulaVigente": true,
    "fotoCoincide": true,
    "rutValido": true,
    "domicilioVerificado": true,
    "actividadConcuerda": true
  },
  "respuestasCliente": {
    "actividad": "Respuesta sobre actividad.",
    "origen_fondos": "Respuesta sobre origen de fondos.",
    "esPEP": "Respuesta sobre PEP.",
    "cuentasExtranjero": "Respuesta sobre cuentas extranjeras."
  },
  "discrepancias": [],
  "decisionCorrecta": "APROBAR",
  "normativaAplicable": "Circular UAF N°62",
  "explicacion": "Explicación de la decisión correcta."
}
```

**Valores válidos:**
- `tipo`: `"KYC"`, `"AML"`, `"Compliance"`
- `prioridad`: `"ALTA"`, `"MEDIA"`, `"BAJA"`
- `decisionCorrecta`: `"APROBAR"`, `"ESCALAR"`, `"RECHAZAR"`

---

## Flujo de desarrollo Git

```bash
# Antes de trabajar — traer últimos cambios
git pull

# Crear rama para nueva funcionalidad
git checkout -b nombre-de-rama

# Después de hacer cambios
git add .
git commit -m "descripción del cambio"
git push -u origin nombre-de-rama
```

---

## Novedades LAB 4 — Jugabilidad y Diseño de Niveles

- **Jornada de Trabajo (Nivel)**: El jugador enfrenta una jornada de 5 turnos (casos) con límite de tiempo de 60 segundos por caso.
- **Curva de Dificultad**: Los casos aumentan su complejidad progresivamente, con mayor frecuencia de objetos sospechosos y sobornos hacia el final del nivel.
- **Sistema de Puntuación (Observer)**: Sistema de multiplicador por racha conectado al HUD mediante el patrón arquitectónico Observer (`event Action`).
- **HUD Reactivo**: La interfaz visual responde dinámicamente:
  - Multiplicador activo (Glow + Scale)
  - Animaciones verde/rojo al ganar o perder puntos
  - Timer parpadeante cuando quedan menos de 15 segundos
  - Barra de progreso de la jornada
- **Condiciones de Victoria/Derrota**:
  - *Victoria*: Finalizar la jornada con puntaje superior a 500 puntos.
  - *Derrota*: Perder todo el puntaje (0 pts) o quedarse sin tiempo durante un caso.
- **Narrativa Inmersiva**: Pantalla de introducción (Tutorial) con contexto narrativo sobre el rol del analista y la importancia del departamento de cumplimiento.

---

## Normativa de referencia

- Ley N°19.913 — Crea la Unidad de Análisis Financiero (UAF)
- Ley N°20.393 — Responsabilidad penal de personas jurídicas
- Ley N°21.521 — Ley Fintech
- Circular UAF N°62 (2025) — Procedimientos AML actualizados

---

## Gerente de Sucursal — NPC (LAB 5)

### Comportamiento
El **Gerente de Sucursal** es un NPC autónomo que merodea la oficina e interrumpe al analista durante la jornada. Combina rol de **jefe corrupto** (presiona con preguntas) y **colega espía** (si lo ignoras, penaliza revisando el expediente).

### Estados FSM
| Estado | Descripción |
|---|---|
| Patrol | Camina por la oficina entre waypoints aleatorios |
| Approach | Detecta al jugador activo y se dirige al escritorio |
| Interact | Lanza pregunta de normativa con timer de 15 segundos |

### Parámetros configurables desde Inspector
| Parámetro | Descripción | Default |
|---|---|---|
| rangoInteraccion | Distancia al escritorio para activar Interact | 1.5m |
| intervaloNivel1 | Segundos entre visitas en Nivel 1 | 40s |
| intervaloNivel2 | Segundos entre visitas en Nivel 2 | 25s |
| intervaloNivel3 | Segundos entre visitas en Nivel 3 | 15s |
| tiempoRespuesta | Segundos para responder la pregunta | 15s |
| penalizacionPuntos | Puntos restados al fallar | 100 |

### Cómo interactuar
- Cuando el Gerente aparezca en pantalla, responde la pregunta de normativa antes de que se agote el timer. El popup muestra un mensaje de **Correcto/Incorrecto** al responder, y el timer del caso se pausa durante la pregunta.
- **Respuesta correcta**: el gerente pierde 1 HP (3 golpes = retirado).
- **Respuesta incorrecta o timeout**: -100 pts y racha reseteada. El gerente "espía" el expediente.
- Al acumular 3 respuestas correctas al mismo gerente: **Toast HUD** "Gerente retirado".

---

## LAB 6 CIERRE — Entrega final

- **Documento de cierre** (Bloques A y B) y **GDD**: ver la carpeta [`/docs`](docs/).
- **Playtesting (Bloque B)**: se realizó una sesión con un usuario externo; los dos ajustes derivados están documentados en el Documento de cierre e integrados en `main`.
- **Video de respaldo de la demo** (máx. 2 min): _[PENDIENTE — pegar enlace aquí antes de la entrega]_
