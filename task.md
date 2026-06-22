# LAB 4 — Tasks

## Componente 1: Sistema de Eventos (Observer) + Multiplicador
- [x] Agregar eventos C# a UIManager.cs (OnPuntajeChanged, OnTurnoChanged, etc.)
- [x] Implementar sistema de multiplicador/racha
- [x] Disparar eventos en AgregarPuntaje(), SiguienteTurno()
- [x] Reducir jornada a 5 turnos × 60 seg

## Componente 2: HUD con Feedback Visual Reactivo
- [x] Agregar elementos UI en RedFlagUI.uxml (multiplicador, progreso, panel resultado)
- [x] Agregar estilos/animaciones en RedFlagUI.uss
- [x] Implementar feedback reactivo en UIManager (animaciones de puntaje, timer, multiplicador)

## Componente 3: Condición de Fin de Partida
- [x] Implementar TerminarJornada() con victoria/derrota
- [x] Panel de resultado con reintentar/menú
- [x] Derrota por puntaje 0, derrota por tiempo, victoria por puntaje suficiente

## Componente 4: Curva de Dificultad
- [x] Ampliar casos.json a 10 casos con dificultad progresiva
- [x] Ajustar ObjetosManager.cs para spawn progresivo

## Componente 5: Elemento Narrativo
- [x] Mejorar SceneTutorialManager.cs con narrativa inmersiva

## Componente 6: README
- [x] Actualizar README.md con documentación LAB 4

## Verificación
- [ ] Compilación sin errores
- [ ] Revisar consistencia de eventos Observer
