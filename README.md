# ValidacionCFDI

Contiene Todos los Archivos Necesarios, Tiene el Excel y los Script de SQL

Validación y Reporte de CFDIs

1. Ingesta de datos:
·	Cargar un archivo Excel que contiene información básica de CFDIs:
·	Columnas: RFC Emisor, RFC Receptor, Folio Fiscal, Fecha de Emisión, Total, Estatus.
·	Validar que el archivo cumple con un layout específico.
2. Validación de CFDIs:
·	Consumir un API de validación de CFDIs para validar el estatus de cada CFDI:Validar si el CFDI es válido, cancelado o no encontrado.
·	Actualizar el estatus en la base de datos y generar un log de errores en caso de que falle la validación.
3. Persistencia de datos:
·	Diseñar y crear un esquema en SQL Server que incluya:
·	Una tabla para almacenar los datos del CFDI.
·	Una tabla para almacenar los logs de validación (e.g., CFDI no encontrado o errores en la consulta). Nota: es deseable usar procedimientos almacenados para realizar esta actividad.
4. Generación de Reportes:
·	Crear un API REST que permita:
·	Consultar todos los CFDIs procesados.
·	Filtrar por RFC Emisor, RFC Receptor, y estatus.
·	Descargar un archivo Excel con los resultados filtrados.
5. Requerimientos Técnicos:
·	C#:
·	Implementar la aplicación en ASP.NET Core.
·	Usar Entity Framework para la interacción con la base de datos.
·	Integrar librerías para la lectura/escritura de Excel (e.g., EPPlus o ClosedXML).
·	SQL Server:
·	Diseñar un modelo relacional normalizado.
·	Utilizar procedimientos almacenados para la inserción y validación masiva si se requiere.
·	APIs:
·	Consumir un API externo para la validación de CFDIs.
·	Crear un API interno para la consulta y generación de reportes.

By: Caamal Dzul Angel Adrian
