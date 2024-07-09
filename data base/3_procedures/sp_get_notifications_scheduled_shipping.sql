USE [CAJA_PIURA_POLARIS_CLOUD]
GO
/****** Object:  StoredProcedure [dbo].[sp_get_pending_notifications_terminal_schedules]    Script Date: 25/06/2024 03:37:50 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Luis Alejandro Puentes Angel
-- Create date: 18-06-2024
-- Description:	Obtener notificaciones de en envio programado
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[sp_get_notifications_scheduled_shipping]
	(
		@status BIT OUT,
		@message VARCHAR(MAX) OUT
	)
AS
BEGIN
	
	SET NOCOUNT ON;

	SET @status = 0;
	SET @message = '';

	BEGIN TRY

		DECLARE @current_date DATETIME = GETDATE();

		DECLARE @Time TIME = CAST(@current_date AS TIME);
		-- Convertir la hora a minutos
		DECLARE @Minutes INT = DATEPART(HOUR, @Time) * 60 + DATEPART(MINUTE, @Time);
		-- Truncar los minutos al inicio de la hora actual
		DECLARE @TruncatedMinutes INT = FLOOR(@Minutes / 60.0) * 60;
		-- Convertir los minutos truncados de vuelta a la hora
		DECLARE @TruncatedTime TIME = DATEADD(MINUTE, @TruncatedMinutes, '00:00');


		-- notificaciones que tiene el grupo que esta asociado la terminal
		SELECT 
			N.notification_id,
			NS.notification_schedule_id,
			T.ter_serial,
			N.icon,
			N.picture,
			N.title,
			N.description
		FROM notifications N
			INNER JOIN notifications_schedule NS ON NS.notification_id = N.notification_id 
			INNER JOIN notifications_groups NG ON NG.notification_id = N.notification_id
			INNER JOIN Terminal T ON T.ter_grupo_id =NG.group_id
		WHERE N.enable_frequency_time =1
			AND  CAST(GETDATE() AS DATE) BETWEEN N.start_date AND N.end_date
			AND NS.hour = @TruncatedTime

		UNION ALL

		-- notificaciones que tiene la terminal
		SELECT 
			N.notification_id,
			NS.notification_schedule_id,	
			NT.terminal_serial,
			N.icon,
			N.picture,
			N.title,
			N.description
		FROM notifications N
			INNER JOIN notifications_schedule NS ON NS.notification_id = N.notification_id 
			INNER JOIN notifications_terminals NT ON NT.notification_id = N.notification_id
		WHERE  N.enable_frequency_time =1
			AND  CAST(GETDATE() AS DATE) BETWEEN N.start_date AND N.end_date
			AND NS.hour = @TruncatedTime;
	    
	
		SET @status = 1;
		SET @message = 'Proceso exitoso';

	END TRY 
	BEGIN CATCH 
	    SET @status = 0;
		SET @message = ERROR_MESSAGE();
		
		INSERT INTO [dbo].[LogErrorBaseDatos]([bderr_fecha],[bderr_nombre_funcion],[bderr_mensaje_error],[bderr_datos_adicionales])
		 VALUES (GETDATE(), OBJECT_NAME(@@PROCID), ERROR_MESSAGE(), /*Datos Extra, Parámetros del SP*/ 
									'status = ' + CONVERT(VARCHAR(1), @status) + ', message' + @message )
	END CATCH 
    
END
