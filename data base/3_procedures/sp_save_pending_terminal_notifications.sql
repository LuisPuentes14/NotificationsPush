
/****** Object:  StoredProcedure [dbo].[sp_get_pending_notifications_terminal_schedules]    Script Date: 19/06/2024 04:19:36 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Luis Alejandro Puentes Angel
-- Create date: 18-06-2024
-- Description:	Guarda las notificaciones que no se environ 
-- al terminal para que posteriormente cuando se conecte le lleguen
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[sp_save_pending_terminal_notifications]
	(
		@list_notifications_serials_terminals_schedules list_notifications_serials_terminals_schedules READONLY,
		@status BIT OUT,
		@message VARCHAR(MAX) OUT
	)
AS
BEGIN
	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @status = 0;
	SET @message = '';	

	IF  EXISTS(
			SELECT 1 
			FROM @list_notifications_serials_terminals_schedules lst
			LEFT JOIN terminal t ON t.ter_serial = lst.serial_terminal
			WHERE t.ter_serial IS NULL) 
	BEGIN 
		SET @message = 'No existen algunos terminales del listado recibido';
		RETURN;
	END
	
	BEGIN TRANSACTION
	BEGIN TRY

		INSERT INTO notifications_pending (terminal_serial, notification_id, icon, picture, title, description)
		SELECT
			NP.serial_terminal, 
			NP.notification_id,
			N.icon,
			N.picture,
			N.title,
			N.description
		FROM notifications N
		INNER JOIN @list_notifications_serials_terminals_schedules NP ON NP.notification_id = N.notification_id	   	

		COMMIT TRANSACTION;

		SET @status = 1;
		SET @message = 'Proceso realizado con exito.';

	END TRY 
	BEGIN CATCH 
	    SET @status = 0;
		SET @message = ERROR_MESSAGE();
		
		INSERT INTO [dbo].[LogErrorBaseDatos]([bderr_fecha],[bderr_nombre_funcion],[bderr_mensaje_error],[bderr_datos_adicionales])
		 VALUES (GETDATE(), OBJECT_NAME(@@PROCID), ERROR_MESSAGE(), /*Datos Extra, Parámetros del SP*/ 
									'list_notifications_serials_terminals_schedules = ' + (SELECT  STRING_AGG( CONCAT(serial_terminal,'|',notification_id,'|',schedule_id), ',')  FROM @list_notifications_serials_terminals_schedules)
									+', status = ' + CONVERT(VARCHAR(1), @status) + ', message' + @message );
	END CATCH 
    
END
