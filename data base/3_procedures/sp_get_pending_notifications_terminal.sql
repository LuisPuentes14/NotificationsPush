
/****** Object:  StoredProcedure [dbo].[sp_get_pending_notifications_terminal_schedules]    Script Date: 19/06/2024 05:55:15 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Luis Alejandro Puentes Angel
-- Create date: 18-06-2024
-- Description:	Obtener notificaciones pendientes
-- =============================================
CREATE OR ALTER  PROCEDURE [dbo].sp_get_pending_notifications_terminal
	(
		@in_terminal_serial VARCHAR(50),
		@status BIT OUT,
		@message VARCHAR(MAX) OUT
	)
AS
BEGIN
	
	SET NOCOUNT ON;

	SET @status = 0;
	SET @message = '';	

	IF NOT EXISTS(SELECT 1 FROM Terminal WHERE ter_serial = @in_terminal_serial ) 
	BEGIN 
		SET @message = 'Terminal no existe';
		RETURN;
	END

	BEGIN TRY

	    SELECT 
			notification_pending_id,
			icon,
			picture,
			title,
			description			
		FROM notifications_pending
		WHERE terminal_serial = @in_terminal_serial;
	
		SET @status = 1;
		SET @message = 'Proceso exitoso';

	END TRY 
	BEGIN CATCH 

		SET @status = 0;
		SET @message = ERROR_MESSAGE();
		
		INSERT INTO [dbo].[LogErrorBaseDatos]([bderr_fecha],[bderr_nombre_funcion],[bderr_mensaje_error],[bderr_datos_adicionales])
		 VALUES (GETDATE(), OBJECT_NAME(@@PROCID), ERROR_MESSAGE(), /*Datos Extra, Parámetros del SP*/ 
									'in_terminal_serial = ' + @in_terminal_serial + ', status = ' + CONVERT(VARCHAR(1), @status) + ', message' + @message )
	END CATCH 
    
END
