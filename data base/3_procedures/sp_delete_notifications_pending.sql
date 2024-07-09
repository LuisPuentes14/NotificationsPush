
/****** Object:  StoredProcedure [dbo].[sp_get_pending_notifications_terminal_schedules]    Script Date: 19/06/2024 04:19:36 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Luis Alejandro Puentes Angel
-- Create date: 18-06-2024
-- Description:	eliminar las notificaciones pendientes
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[sp_delete_notifications_pending]
	(
		@list_notifications_pending list_notifications_pending READONLY,
		@status BIT OUT,
		@message VARCHAR(MAX) OUT
	)
AS
BEGIN
	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @status = 0;
	SET @message = '';
	
	BEGIN TRANSACTION
	BEGIN TRY

	    DELETE FROM notifications_pending
		WHERE notification_pending_id IN (SELECT
											notification_pending_id 
										  FROM @list_notifications_pending)

		COMMIT TRANSACTION;

		SET @status = 1;
		SET @message = 'Proceso realizado con exito.';

	END TRY 
	BEGIN CATCH 
	    SET @status = 0;
		SET @message = ERROR_MESSAGE();
		
		INSERT INTO [dbo].[LogErrorBaseDatos]([bderr_fecha],[bderr_nombre_funcion],[bderr_mensaje_error],[bderr_datos_adicionales])
		 VALUES (GETDATE(), OBJECT_NAME(@@PROCID), ERROR_MESSAGE(), /*Datos Extra, Parámetros del SP*/ 
									'list_notifications_pending = ' + ( SELECT  STRING_AGG(notification_pending_id , ',')  FROM @list_notifications_pending)
									+', status = ' + CONVERT(VARCHAR(1), @status) + ', message' + @message );
	END CATCH 
    
END
