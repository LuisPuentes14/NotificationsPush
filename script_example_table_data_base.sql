CREATE TABLE mi_tabla (
    id SERIAL PRIMARY KEY,
    datos TEXT NOT NULL
);

CREATE OR REPLACE FUNCTION fn_notificar_cambio()
RETURNS TRIGGER AS $$
BEGIN
    -- Enviar notificación con el nombre del canal 'canal_cambios' y el nuevo ID insertado como mensaje
    PERFORM pg_notify('canal_cambios', NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_notificar_despues_insertar
AFTER INSERT ON mi_tabla
FOR EACH ROW EXECUTE FUNCTION fn_notificar_cambio();

INSERT INTO mi_tabla (datos)
VALUES ('ALEJANDRO');



-------------------------------------------------

CREATE OR REPLACE FUNCTION send_notification_push()
    RETURNS TRIGGER AS $$
DECLARE
   notification_to_send_json TEXT;
BEGIN

    notification_to_send_json := (SELECT
                                      JSONB_BUILD_OBJECT(
                                                          'notification_id', NEW.notification_id,
                                                          'notification_send_login',  NEW.notification_send_push_login,
                                                          'notification_message',  NEW.notification_send_push_message,
                                                          'notification_register_date',  NEW.notification_send_push_date,
                                                          'notification_register_by',  NEW.notification_send_push_register_by)
                                   );

    RAISE NOTICE '%', notification_to_send_json;
    -- Enviar notificación con el nombre del canal 'canal_cambios' y el nuevo ID insertado como mensaje
    PERFORM pg_notify('chanel_send_notification_push', CONCAT( NEW.notification_send_push_login,'*~*',NEW.notification_id,'*~*', notification_to_send_json ));

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_send_notification_push
    AFTER INSERT ON polaris_core.notifications_send_push
FOR EACH ROW EXECUTE FUNCTION send_notification_push();

CREATE OR REPLACE PROCEDURE polaris_core.get_notifications(
    IN in_login TEXT,
    OUT status BOOLEAN,
    OUT notification TEXT,
    OUT message TEXT
)
    LANGUAGE 'plpgsql'
AS
$BODY$
    --
-----------------------------------------------------------------------
--Objetivo: obtener las notificaciones que tiene el usuario
-----------------------------------------------------------------------
DECLARE
    code_error         TEXT DEFAULT ''; -- varible para almacenar codigos de errores
    message_error      TEXT DEFAULT ''; -- varible para almacenar mensajes de errores

    id_log             INTEGER; -- varible para log

    in_parameters      TEXT DEFAULT ''; -- variable para almacenar los parametros de entrada y registrarlos en log
    out_parameters     TEXT DEFAULT ''; -- variable para almacenar los parametros de salida y registrarlos en log

    success_code       VARCHAR(20) DEFAULT 'P0000'; --variabal para asignar codigo de proceso exitoso
    error_code_checked VARCHAR(20) DEFAULT 'P0001'; --variable para asignar codigo de error controlado de validaciones parametros

    --variables locales
    -----------------------------
    local_row_count    INTEGER;


BEGIN

    in_parameters := 'in_login:' || polaris_core.any_element_to_string(in_login);

    -- se registra inicio de transactions
    id_log := register_log_entry(polaris_core.any_element_to_string(in_login),
                                 'request_get_notifications',
                                 in_parameters);

    BEGIN
        --valida que el que el tecnico no venga vacio
        IF in_login IS NULL OR in_login = '' THEN
            RAISE EXCEPTION 'Error: el parametro  in_login no pueder ir vacio.' USING ERRCODE = error_code_checked;
        END IF;

        --valida que el usuario exista
        IF NOT EXISTS(SELECT 1 FROM polaris_core.sec_users WHERE login = in_login) THEN
            RAISE EXCEPTION 'Error: el usuario "%" no existe.', in_login USING ERRCODE = error_code_checked;
        END IF;

        --se obtienen las notificaciones del usuario en formato JSON
        notification := (SELECT JSONB_AGG(
                                        JSONB_BUILD_OBJECT(
                                                'notification_id', notification_id,
												'notification_send_login', notification_login,
                                                'notification_message', notification_message,
                                                'notification_register_date', notification_register_date,
                                                'notification_register_by', notification_register_by))
                         FROM polaris_core.notifications
                         WHERE notification_login = in_login);

        --si no se no tiene notificaciones se envia un array vacio
        IF notification IS NULL THEN
            notification := '[]';
        END IF;

        message := 'Proceso exitoso.';
        status := TRUE;

        out_parameters := 'status:' || any_element_to_string(status) || '|'
                              || 'notification:' || any_element_to_string(notification) || '|'
                              || 'message:' || any_element_to_string(message);

        PERFORM polaris_core.register_log_output(
                id_log,
                out_parameters,
                '',
                '',
                '',
                '',
                success_code,
                message);

    EXCEPTION

        WHEN SQLSTATE 'P0001' THEN
            code_error := SQLSTATE;
            message_error := SQLERRM;

            status := FALSE;
            message := SQLERRM;

            out_parameters := 'status:' || any_element_to_string(status) || '|'
                                  || 'notification:' || any_element_to_string(notification) || '|'
                                  || 'message:' || any_element_to_string(message);

            RAISE NOTICE 'Se ha producido una excepción controlada:';
            RAISE NOTICE 'Código de error: %', code_error;
            RAISE NOTICE 'Mensaje de error: %', message_error;

            -- se registra fin de transactions
            PERFORM polaris_core.register_log_output(
                    id_log,
                    out_parameters,
                    '',
                    '',
                    code_error,
                    message_error,
                    '',
                    '');

        WHEN OTHERS THEN
            code_error := SQLSTATE;
            message_error := SQLERRM;

            status := FALSE;
            message := 'Error en base de datos';

            out_parameters := 'status:' || any_element_to_string(status) || '|'
                                  || 'notification:' || any_element_to_string(notification) || '|'
                                  || 'message:' || any_element_to_string(message);

            RAISE NOTICE 'Se ha producido una excepción:';
            RAISE NOTICE 'Código de error: %', code_error;
            RAISE NOTICE 'Mensaje de error: %', message_error;

            -- se registra fin de transactions
            PERFORM polaris_core.register_log_output(
                    id_log,
                    out_parameters,
                    code_error,
                    message_error,
                    '',
                    '',
                    '',
                    '');
    END;
END ;
$BODY$;



CREATE OR REPLACE PROCEDURE polaris_core.validate_authentication_user(
    IN in_user_login TEXT,
    IN in_password TEXT,
    OUT out_status BOOLEAN,
    OUT out_message TEXT,
    OUT out_email TEXT ,
    OUT out_name TEXT
)
    LANGUAGE 'plpgsql'
AS
$BODY$
DECLARE
    code_error         TEXT DEFAULT ''; -- varible para almacenar codigos de errores
    message_error      TEXT DEFAULT ''; -- varible para almacenar mensajes de errores

    id_log             INTEGER; -- varible para log

    in_parameters      TEXT DEFAULT ''; -- variable para almacenar los parametros de entrada y registrarlos en log
    out_parameters     TEXT DEFAULT ''; -- variable para almacenar los parametros de salida y registrarlos en log

    success_code       VARCHAR(20) DEFAULT 'P0000'; --variabal para asignar codigo de proceso exitoso
    error_code_checked VARCHAR(20) DEFAULT 'P0001'; --variable para asignar codigo de error controlado de validaciones parametros

BEGIN

    in_parameters := 'in_user_login:' || polaris_core.any_element_to_string(in_user_login) || '|'
                         || 'in_password:' || any_element_to_string(in_password);

    -- se registra inicio de transactions
    id_log := register_log_entry(polaris_core.any_element_to_string(in_user_login), 'validate_authentication_user',
                                 in_parameters);

    BEGIN

        IF NOT EXISTS(SELECT 1 FROM sec_users WHERE login = in_user_login) THEN
            RAISE EXCEPTION 'Usuario % no encontrado.', in_user_login USING ERRCODE = error_code_checked;
        END IF;

        IF NOT EXISTS(SELECT 1 FROM polaris_core.sec_users WHERE login = in_user_login AND pswd = in_password) THEN
            RAISE EXCEPTION 'Contraseña no valida.' USING ERRCODE = error_code_checked;
        END IF;

        SELECT email, name
        INTO out_email, out_name
        FROM polaris_core.sec_users
        WHERE login = in_user_login AND pswd = in_password;

        out_status = TRUE;
        out_message = 'Autenticacion exitosa.';

        out_parameters := 'out_status:' || any_element_to_string(out_status) || '|'
                              || 'out_message:' || any_element_to_string(out_message);

        PERFORM polaris_core.register_log_output(
                id_log,
                out_parameters,
                '',
                '',
                '',
                '',
                success_code,
                out_message);

    EXCEPTION

        WHEN SQLSTATE 'P0001' THEN
            code_error := SQLSTATE;
            message_error := SQLERRM;

            out_status = FALSE;
            out_message = SQLERRM;

            RAISE NOTICE 'Se ha producido una excepción controlada:';
            RAISE NOTICE 'Código de error: %', code_error;
            RAISE NOTICE 'Mensaje de error: %', message_error;

            out_parameters := 'out_status:' || any_element_to_string(out_status) || '|'
                                  || 'out_message:' || any_element_to_string(out_message);

            -- se registra fin de transactions
            PERFORM polaris_core.register_log_output(
                    id_log,
                    out_parameters,
                    '',
                    '',
                    code_error,
                    message_error,
                    '',
                    '');

        WHEN OTHERS THEN
            code_error := SQLSTATE;
            message_error := SQLERRM;

            out_status = FALSE;
            out_message = 'Error validando usuario.';

            out_parameters := 'out_status:' || any_element_to_string(out_status) || '|'
                                  || 'out_message:' || any_element_to_string(out_message);

            RAISE NOTICE 'Se ha producido una excepción:';
            RAISE NOTICE 'Código de error: %', code_error;
            RAISE NOTICE 'Mensaje de error: %', message_error;

            -- se registra fin de transactions
            PERFORM polaris_core.register_log_output(
                    id_log,
                    out_parameters,
                    code_error,
                    message_error,
                    '',
                    '',
                    '',
                    '');


    END;
END;
$BODY$;






