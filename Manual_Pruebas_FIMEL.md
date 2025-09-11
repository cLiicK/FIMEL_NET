# Manual de Pruebas - Sistema FIMEL

## Índice
1. [Acceso al Sistema](#1-acceso-al-sistema)
2. [Configuración de Horarios](#2-configuración-de-horarios)
3. [Gestión de Citas](#3-gestión-de-citas)
4. [Iniciar Cita](#4-iniciar-cita)
5. [Gestión de Pacientes](#5-gestión-de-pacientes)
6. [Nueva Consulta](#6-nueva-consulta)
7. [Historial de Pacientes](#7-historial-de-pacientes)
8. [Configuración de Usuario](#8-configuración-de-usuario)

---

## 1. Acceso al Sistema

### 1.1 Login de Usuario
**Objetivo:** Verificar que el usuario pueda acceder al sistema correctamente.

**Pasos:**
1. Abrir el navegador y acceder a la URL del sistema
2. En la pantalla de login, ingresar:
   - **Usuario:** [Ingresar usuario válido]
   - **Contraseña:** [Ingresar contraseña válida]
3. Hacer clic en "Iniciar Sesión"

**Resultado Esperado:**
- El sistema debe redirigir al usuario a la página principal
- Debe mostrar el menú de navegación con las opciones disponibles
- El nombre del usuario debe aparecer en la parte superior

### 1.2 Recuperar Contraseña
**Objetivo:** Verificar la funcionalidad de recuperación de contraseña.

**Pasos:**
1. En la pantalla de login, hacer clic en "¿Olvidaste tu contraseña?"
2. Ingresar el email registrado en el sistema
3. Hacer clic en "Enviar"

**Resultado Esperado:**
- Debe mostrar un mensaje de confirmación
- Se debe enviar un email con instrucciones para recuperar la contraseña

---

## 2. Configuración de Horarios

### 2.1 Configurar Horario de Atención
**Objetivo:** Configurar los horarios disponibles para atención de pacientes.

**Pasos:**
1. Desde el menú principal, ir a "Mi Horario"
2. Si es usuario administrativo, seleccionar un profesional del combo
3. En la sección "Configurar Horario", seleccionar:
   - **Día de la semana:** Lunes
   - **Hora de inicio:** 09:00
   - **Hora de fin:** 17:00
   - **Comentario:** "Horario regular de atención"
4. Hacer clic en "Guardar"

**Resultado Esperado:**
- Debe aparecer un mensaje de confirmación
- El bloque horario debe aparecer en la lista de horarios configurados
- El calendario debe mostrar el bloque disponible

### 2.2 Eliminar Bloque Horario
**Objetivo:** Verificar que se pueda eliminar un bloque horario.

**Pasos:**
1. En la lista de horarios configurados, hacer clic en el ícono de eliminar (🗑️)
2. Confirmar la eliminación en el popup

**Resultado Esperado:**
- Debe aparecer un mensaje de confirmación
- El bloque debe desaparecer de la lista
- El calendario debe actualizarse

---

## 3. Gestión de Citas

### 3.1 Crear Nueva Cita
**Objetivo:** Crear una nueva cita para un paciente.

**Pasos:**
1. En el calendario, hacer clic en un horario disponible
2. En el modal que aparece, completar:
   - **Hora de término:** 10:00
   - **Nombre y apellido:** "María González"
   - **Correo:** "maria.gonzalez@email.com"
   - **Teléfono:** "+56912345678"
   - **Nota:** "Primera consulta"
3. Hacer clic en "Guardar"

**Resultado Esperado:**
- Debe aparecer un mensaje de confirmación
- La cita debe aparecer en el calendario
- El horario debe marcarse como ocupado

### 3.2 Ver Detalles de Cita
**Objetivo:** Verificar que se puedan ver los detalles de una cita existente.

**Pasos:**
1. En el calendario, hacer clic en una cita existente
2. Revisar que se muestren todos los datos:
   - Hora de inicio y término
   - Nombre del paciente
   - Correo y teléfono
   - Notas

**Resultado Esperado:**
- Debe abrirse un modal con todos los datos de la cita
- Debe mostrar los botones "Iniciar Cita" y "Eliminar Cita"

### 3.3 Eliminar Cita
**Objetivo:** Verificar que se pueda eliminar una cita.

**Pasos:**
1. Hacer clic en una cita existente
2. En el modal de detalles, hacer clic en "Eliminar Cita"
3. Confirmar la eliminación

**Resultado Esperado:**
- Debe aparecer un mensaje de confirmación
- La cita debe desaparecer del calendario
- El horario debe quedar disponible nuevamente

---

## 4. Iniciar Cita

### 4.1 Iniciar Cita con RUT Existente
**Objetivo:** Iniciar una cita para un paciente que ya tiene ficha clínica.

**Pasos:**
1. Hacer clic en una cita existente
2. Hacer clic en "Iniciar Cita"
3. En el modal que aparece:
   - Seleccionar "RUT" en tipo de documento
   - Ingresar un RUT válido (ej: 12.345.678-9)
4. Hacer clic en "Validar"

**Resultado Esperado:**
- Debe aparecer un mensaje "Paciente encontrado"
- Debe preguntar si desea iniciar una nueva consulta
- Al confirmar, debe redirigir a "Nueva Consulta"

### 4.2 Iniciar Cita con Pasaporte
**Objetivo:** Iniciar una cita para un paciente extranjero.

**Pasos:**
1. Hacer clic en una cita existente
2. Hacer clic en "Iniciar Cita"
3. En el modal:
   - Seleccionar "Pasaporte" en tipo de documento
   - Ingresar número de pasaporte (ej: AB123456)
4. Hacer clic en "Validar"

**Resultado Esperado:**
- Debe buscar el paciente por número de pasaporte
- Debe redirigir según si existe o no la ficha clínica

### 4.3 Iniciar Cita - Paciente Nuevo
**Objetivo:** Iniciar una cita para un paciente sin ficha clínica.

**Pasos:**
1. Hacer clic en una cita existente
2. Hacer clic en "Iniciar Cita"
3. Ingresar un RUT que no exista en el sistema
4. Hacer clic en "Validar"

**Resultado Esperado:**
- Debe aparecer mensaje "Paciente no encontrado"
- Debe preguntar si desea crear una nueva ficha clínica
- Al confirmar, debe redirigir a "Ficha Paciente"

---

## 5. Gestión de Pacientes

### 5.1 Crear Nueva Ficha Clínica
**Objetivo:** Crear una ficha clínica para un paciente nuevo.

**Pasos:**
1. Ir a "Pacientes" → "Ficha Paciente"
2. En la sección de búsqueda:
   - Seleccionar tipo de documento (RUT/Pasaporte/DNI)
   - Ingresar número de documento
   - Hacer clic en "Buscar"
3. Si no existe, completar los datos:
   - **Datos Demográficos:**
     - Nombres: "Ana"
     - Apellido paterno: "Rodríguez"
     - Apellido materno: "López"
     - Fecha de nacimiento: "15/03/1985"
     - Sexo biológico: Femenino
     - Nacionalidad: Chile
   - **Antecedentes Generales:**
     - Antecedentes familiares: "Diabetes en familia"
     - Medicamentos: "Ninguno"
   - **Información de Contacto:**
     - Dirección: "Av. Providencia 123"
     - Teléfono: "+56987654321"
     - Email: "ana.rodriguez@email.com"
4. Hacer clic en "Guardar Paciente"

**Resultado Esperado:**
- Debe aparecer mensaje de confirmación
- La ficha debe guardarse correctamente
- Debe mostrar el botón "Actualizar Paciente"

### 5.2 Buscar Paciente Existente
**Objetivo:** Buscar y ver la ficha de un paciente existente.

**Pasos:**
1. Ir a "Pacientes" → "Ficha Paciente"
2. Ingresar RUT de un paciente existente
3. Hacer clic en "Buscar"

**Resultado Esperado:**
- Debe cargar todos los datos del paciente
- Debe mostrar el historial de consultas
- Debe permitir editar los datos

### 5.3 Actualizar Datos de Paciente
**Objetivo:** Modificar información de un paciente existente.

**Pasos:**
1. Buscar un paciente existente
2. Modificar algún campo (ej: teléfono)
3. Hacer clic en "Actualizar Paciente"

**Resultado Esperado:**
- Debe aparecer mensaje de confirmación
- Los cambios deben guardarse correctamente

---

## 6. Nueva Consulta

### 6.1 Crear Nueva Consulta
**Objetivo:** Crear una nueva consulta médica para un paciente.

**Pasos:**
1. Ir a "Consulta" → "Nueva Consulta"
2. Buscar un paciente existente por RUT
3. Una vez cargado el paciente, completar:
   - **Tipo de consulta:** "Control Ginecológico"
   - **Talla:** "165"
   - **Peso:** "60"
   - **Presión arterial:** "120/80"
   - **Motivo consulta:** "Control anual"
   - **Anamnesis:** "Paciente refiere estar bien"
   - **Examen físico:** "Normal"
   - **Diagnóstico:** "Paciente sana"
   - **Indicaciones:** "Continuar con controles anuales"
4. Hacer clic en "Guardar Consulta"

**Resultado Esperado:**
- Debe aparecer mensaje de confirmación
- La consulta debe guardarse
- Debe aparecer en el historial del paciente

### 6.2 Calcular IMC
**Objetivo:** Verificar la funcionalidad de cálculo automático de IMC.

**Pasos:**
1. En nueva consulta, ingresar talla y peso
2. Hacer clic en el ícono de calculadora (🧮) junto a IMC

**Resultado Esperado:**
- Debe calcular automáticamente el IMC
- Debe mostrar el estado nutricional correspondiente

---

## 7. Historial de Pacientes

### 7.1 Ver Historial de Pacientes
**Objetivo:** Ver la lista de todos los pacientes registrados.

**Pasos:**
1. Ir a "Pacientes" → "Historial Pacientes"
2. Revisar la tabla de pacientes
3. Probar los filtros:
   - Tipo de documento
   - RUT
   - Fecha desde/hasta

**Resultado Esperado:**
- Debe mostrar la lista de pacientes
- Los filtros deben funcionar correctamente
- La tabla debe ocupar todo el ancho disponible

### 7.2 Ver Detalle de Paciente
**Objetivo:** Ver el detalle completo de un paciente.

**Pasos:**
1. En el historial, hacer clic en el ícono de ficha (📋) de un paciente
2. Revisar todas las secciones:
   - Datos demográficos
   - Antecedentes generales
   - Antecedentes gineco-obstétricos
   - Historial de consultas

**Resultado Esperado:**
- Debe mostrar todos los datos del paciente
- El historial debe mostrar las consultas anteriores
- Debe permitir ver el detalle de cada consulta

---

## 8. Configuración de Usuario

### 8.1 Cambiar Contraseña
**Objetivo:** Cambiar la contraseña del usuario logueado.

**Pasos:**
1. Ir a "Usuario" → "Configuración"
2. En la sección "Cambiar Contraseña":
   - Ingresar contraseña actual
   - Ingresar nueva contraseña
   - Confirmar nueva contraseña
3. Hacer clic en "Cambiar Contraseña"

**Resultado Esperado:**
- Debe aparecer mensaje de confirmación
- La contraseña debe actualizarse
- Debe permitir login con la nueva contraseña

### 8.2 Configurar Horario de Bloque
**Objetivo:** Configurar la duración de los bloques horarios.

**Pasos:**
1. En configuración, ir a "Configuración de Horario"
2. Seleccionar duración de bloque (ej: 30 minutos)
3. Hacer clic en "Guardar"

**Resultado Esperado:**
- Debe guardar la configuración
- Los nuevos bloques deben usar esta duración

---

## Casos de Prueba Especiales

### CP1: Usuario Administrativo vs Especialista
**Objetivo:** Verificar diferencias entre perfiles.

**Pasos:**
1. Login como usuario administrativo
2. Verificar que aparece combo de selección de profesional
3. Login como especialista
4. Verificar que no aparece el combo

**Resultado Esperado:**
- Administrativo: debe poder seleccionar profesionales
- Especialista: debe ver directamente su agenda

### CP2: Validación de RUT
**Objetivo:** Verificar validaciones de formato RUT.

**Pasos:**
1. En cualquier campo RUT, ingresar formatos incorrectos:
   - Sin puntos ni guión
   - Con formato incorrecto
   - Con dígito verificador incorrecto
2. Verificar mensajes de error

**Resultado Esperado:**
- Debe mostrar mensajes de error apropiados
- Debe formatear automáticamente el RUT válido

### CP3: Responsive Design
**Objetivo:** Verificar que el sitio funcione en diferentes dispositivos.

**Pasos:**
1. Probar en diferentes tamaños de pantalla
2. Verificar que los modales se adapten
3. Verificar que las tablas sean responsive

**Resultado Esperado:**
- Debe adaptarse a diferentes tamaños
- Los elementos deben ser legibles en móviles

---

## Criterios de Aceptación

### Funcionalidad
- ✅ Todas las funcionalidades principales deben funcionar correctamente
- ✅ Los datos deben guardarse y recuperarse correctamente
- ✅ Las validaciones deben funcionar apropiadamente

### Usabilidad
- ✅ La interfaz debe ser intuitiva
- ✅ Los mensajes de error deben ser claros
- ✅ La navegación debe ser fluida

### Rendimiento
- ✅ Las páginas deben cargar en menos de 3 segundos
- ✅ Los modales deben abrirse rápidamente
- ✅ Las búsquedas deben ser responsivas

### Compatibilidad
- ✅ Debe funcionar en Chrome, Firefox, Safari
- ✅ Debe ser responsive en móviles y tablets
- ✅ Debe funcionar con diferentes resoluciones

---

## Notas Importantes

1. **Datos de Prueba:** Usar datos ficticios para las pruebas
2. **Backup:** Hacer backup de la base de datos antes de pruebas extensivas
3. **Navegadores:** Probar en al menos 3 navegadores diferentes
4. **Dispositivos:** Probar en desktop, tablet y móvil
5. **Reporte:** Documentar cualquier error encontrado con:
   - Pasos para reproducir
   - Comportamiento esperado vs actual
   - Captura de pantalla si es posible

---

**Versión del Manual:** 1.0  
**Fecha de Creación:** [Fecha actual]  
**Responsable:** [Nombre del tester]
