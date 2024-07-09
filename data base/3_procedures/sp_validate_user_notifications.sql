
/****** Object:  StoredProcedure [dbo].[sp_validate_user_notifications]    Script Date: 20/06/2024 02:28:22 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Luis Alejandro Puentes Angel
-- Create date: 18-06-2024
-- Description:	valida el usuario en la api de notificaciones
-- =============================================
CREATE OR ALTER    PROCEDURE [dbo].[sp_validate_user_notifications]
	(
		@in_name_user VARCHAR(255),
		@in_passwor_user VARCHAR(255),
		@in_type_user VARCHAR(50),
		@status BIT OUT,
		@message VARCHAR(MAX) OUT
	)
AS
BEGIN
	
	SET NOCOUNT ON;

	SET @status = 0;
	SET @message = '';

	BEGIN TRY

	    IF @in_type_user = 'TERMINAL'
		BEGIN
			IF NOT EXISTS(SELECT 1 FROM terminal WHERE ter_serial = @in_name_user )
			BEGIN 					
				SET @message = 'Terminal incorrecto';
				RETURN;
			END

			IF NOT EXISTS(SELECT 1 
						  FROM terminal 
						  WHERE CONCAT( ter_serial ,'-', ter_serial) = @in_passwor_user 
						  AND ter_serial = @in_name_user)
			BEGIN 					
				SET @message = 'Contraseña incorrecta.';
				RETURN;
			END

		END
		ELSE IF @in_type_user = 'USER'
		BEGIN 
			IF NOT EXISTS(SELECT 1 FROM sec_users WHERE login = @in_name_user )
			BEGIN 					
				SET @message = 'Usuario incorrecto';
				RETURN;
			END

			IF NOT EXISTS(SELECT 1 
						  FROM sec_users 
						  WHERE login = @in_name_user
						  AND pswd = @in_passwor_user)
			BEGIN 					
				SET @message = 'Contraseña incorrecta.';
				RETURN;
			END
		END 
		ELSE
		BEGIN 
			SET @message = 'Tipo de usuario no valido';
			RETURN;
		END
	
		SET @status = 1;
		SET @message = 'Proceso exitoso';

	END TRY 
	BEGIN CATCH 
	    SET @status = 0;
		SET @message = ERROR_MESSAGE();

		DECLARE  @bderr_datos_adicionales VARCHAR(MAX) =
		(CONCAT(
		'in_name_user = ', @in_name_user,
		', in_passwor_user = ', @in_passwor_user,
		', in_type_user = ', @in_type_user,
		', status = ', @status,
		', message = ', @message
		));
		
		INSERT INTO [dbo].[LogErrorBaseDatos]([bderr_fecha],[bderr_nombre_funcion],[bderr_mensaje_error],[bderr_datos_adicionales])
		 VALUES (GETDATE(), OBJECT_NAME(@@PROCID), ERROR_MESSAGE(), /*Datos Extra, Parámetros del SP*/ @bderr_datos_adicionales )
	END CATCH 
    
END
