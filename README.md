# Red Flag 🚩
### Simulador de Gestión de Riesgo Bancario
**Universidad de Santiago de Chile — Departamento de Ingeniería Informática**  
Asignatura: Ingeniería de Videojuegos: Fundamentos y Aplicaciones Interactivas  
Equipo: Diego Altamirano · Felipe Cifuentes · Byron Obregón

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
- **Vista Monitor** (tecla E) — zoom al monitor del escritorio con panel de Documentos y Evidencia (KYC, AML, tabs).
- **Vista Notepad** (tecla Q) — zoom al notepad del escritorio con panel de Expediente del caso.
- **Esc** — vuelve a la vista del cliente.
- **Clic en cliente** — abre panel de diálogo con preguntas predefinidas.
- **Clic en campos del expediente** — genera preguntas automáticas al cliente.
- **APROBAR / ESCALAR / RECHAZAR** — decisiones con consecuencias en el puntaje.

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

## Estructura del proyecto

```
Assets/
├── Audio/
├── Data/
│   └── casos.json          # Banco de casos KYC/AML/Compliance
├── Materials/
├── Models/
│   ├── VNB - Office Set/   # Assets de oficina low-poly
│   └── LowPolyPeople/      # Personajes low-poly
├── Prefabs/
├── Scenes/
│   └── MainScene
├── Scripts/
│   ├── UIManager.cs        # Gestión de UI, turnos y puntaje
│   ├── CaseManager.cs      # Carga y gestión de casos desde JSON
│   └── CameraController.cs # Sistema de 3 vistas con delta time
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

- **Jornada de Trabajo (Nivel)**: El jugador ahora enfrenta una jornada de 5 turnos (casos) con límite de tiempo de 60 segundos por caso.
- **Curva de Dificultad**: Los casos aumentan su complejidad progresivamente (10 casos en total), con mayor frecuencia de objetos sospechosos y sobornos hacia el final del nivel.
- **Sistema de Puntuación (Observer)**: Implementación de un sistema de multiplicador por racha conectado al HUD mediante el patrón arquitectónico Observer (`event Action`).
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
