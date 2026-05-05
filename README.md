# Red Flag 🚩
### Simulador de Gestión de Riesgo Bancario
**Universidad de Santiago de Chile — Departamento de Ingeniería Informática**  
Asignatura: Ingeniería de Videojuegos: Fundamentos y Aplicaciones Interactivas  
Equipo: Diego Altamirano · Felipe Cifuentes · Byron Obregón

---

## Descripción

Red Flag es un serious game 3D desarrollado en Unity que gamifica el entrenamiento en gestión de riesgo bancario bajo el marco regulatorio chileno. El jugador asume el rol de un analista junior que debe revisar expedientes, interactuar con clientes y tomar decisiones de compliance (Aprobar / Escalar / Rechazar) basadas en normativa real (Ley 19.913, Ley 20.393, Circular UAF N°62).

---

## Requisitos previos

Antes de clonar el repositorio asegúrate de tener instalado:

- [Unity Hub](https://unity.com/download)
- **Unity 6.4** (6000.4.1f1) con los módulos **Web** y **Windows**
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con el workload **Game development with Unity**
- [Git](https://git-scm.com/)

---

## Clonar el repositorio

Abre una terminal y ejecuta:

```bash
git clone https://github.com/dialtamiranoh/usach-ingevi-redflag.git
```

---

## Importar en Unity Hub

1. Abre **Unity Hub**
2. Haz clic en **Add → Add project from disk**
3. Navega a la carpeta `usach-ingevi-redflag/` que clonaste
4. Selecciónala y haz clic en **Open**
5. Unity Hub detectará automáticamente la versión 6.4 — haz clic en **Open with Unity 6.4**

> ⚠️ La primera apertura puede tardar varios minutos mientras Unity regenera la carpeta `Library/`.

---

## Configurar Visual Studio

1. En Unity ve a **Edit → Preferences → External Tools**
2. En **External Script Editor** selecciona **Visual Studio 2022**
3. Haz clic en **Regenerate project files**

---

## Estructura del proyecto

```
Assets/
├── Audio/          # Efectos de sonido y música
├── Data/
│   └── casos.json  # Banco de casos KYC/AML/Compliance
├── Materials/      # Materiales y shaders
├── Models/
│   ├── VNB - Office Set/   # Assets de oficina low-poly
│   └── LowPolyPeople/      # Personajes low-poly
├── Prefabs/        # Prefabs reutilizables
├── Scenes/
│   └── MainScene   # Escena principal del juego
├── Scripts/
│   ├── UIManager.cs    # Gestión de la UI y lógica de turnos
│   └── CaseManager.cs  # Carga y gestión de casos desde JSON
├── Settings/       # Configuración de render pipeline (URP)
└── UI/
    ├── RedFlagUI.uxml  # Estructura de la interfaz
    └── RedFlagUI.uss   # Estilos de la interfaz
```

---

## Abrir la escena principal

1. En el panel **Project** navega a `Assets/Scenes/`
2. Doble clic en **MainScene**

---

## Ejecutar el juego en el editor

1. Asegúrate de tener **MainScene** abierta
2. Presiona el botón **▶ Play** en la barra superior
3. Para detener presiona **▶ Play** nuevamente

---

## Exportar a WebGL

1. Ve a **File → Build Profiles**
2. Selecciona **Web** en la lista de plataformas
3. Verifica que **MainScene** esté en la Scene List
4. Haz clic en **Build And Run**
5. Selecciona una carpeta de destino (ej. `Builds/WebGL/`)

---

## Agregar nuevos casos

Los casos del juego se definen en `Assets/Data/casos.json`. Para agregar un nuevo caso copia la siguiente estructura y completa los campos:

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
    "actividad": "Respuesta del cliente sobre su actividad.",
    "origen_fondos": "Respuesta del cliente sobre origen de fondos.",
    "esPEP": "Respuesta del cliente sobre PEP.",
    "cuentasExtranjero": "Respuesta del cliente sobre cuentas extranjeras."
  },
  "discrepancias": [],
  "decisionCorrecta": "APROBAR",
  "normativaAplicable": "Circular UAF N°62",
  "explicacion": "Explicación de por qué esta es la decisión correcta."
}
```

**Valores válidos:**
- `tipo`: `"KYC"`, `"AML"`, `"Compliance"`
- `prioridad`: `"ALTA"`, `"MEDIA"`, `"BAJA"`
- `decisionCorrecta`: `"APROBAR"`, `"ESCALAR"`, `"RECHAZAR"`

---

## Flujo de desarrollo Git

```bash
# Antes de empezar a trabajar — traer últimos cambios
git pull

# Después de hacer cambios
git add .
git commit -m "descripción del cambio"
git push
```

---

## Próximos pasos (pendientes)

- [ ] Feedback de decisión — mostrar si la decisión fue correcta con normativa aplicable
- [ ] Sistema de scoring real — penalizar decisiones incorrectas
- [ ] Ampliar banco de casos JSON
- [ ] Pantalla de inicio y pantalla de fin de jornada

---

## Normativa de referencia

- Ley N°19.913 — Crea la Unidad de Análisis Financiero (UAF)
- Ley N°20.393 — Responsabilidad penal de personas jurídicas
- Ley N°21.521 — Ley Fintech
- Circular UAF N°62 (2025) — Procedimientos AML actualizados
