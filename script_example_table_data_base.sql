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
    RETURNS TRIGGER AS
$$
DECLARE
    notification_to_send_json TEXT;
BEGIN

    notification_to_send_json := (SELECT JSONB_BUILD_OBJECT(
                                                 'notification_id', NEW.notification_id,
                                                 'notification_send_login', NEW.notification_login,
                                                 'notification_message', NEW.notification_message,
                                                 'notification_register_date', NEW.notification_register_date,
                                                 'notification_register_by', NEW.notification_register_by));
    notification_to_send_json :=  notification_to_send_json;

    -- Enviar notificación con el nombre del canal 'canal_cambios' y el nuevo ID insertado como mensaje
    PERFORM pg_notify('chanel_send_notification_push',
                      CONCAT(NEW.notification_login, '*~*', NEW.notification_id, '*~*',
                             notification_to_send_json));

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_send_notification_push
    AFTER INSERT
    ON polaris_core.notifications
    FOR EACH ROW
EXECUTE FUNCTION send_notification_push();



	
---------------------------------------------------------

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

----------------------------------------------------------------------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE polaris_core.delete_notifications(
    IN in_login TEXT,
    IN in_notification_id INTEGER,
    OUT status BOOLEAN,
    OUT message TEXT
)
    LANGUAGE 'plpgsql'
AS
$BODY$
    --
-----------------------------------------------------------------------
--Objetivo: eliminar notificaciones
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

    in_parameters := 'in_login:' || polaris_core.any_element_to_string(in_login)||
                     'in_notification_id:' || polaris_core.any_element_to_string(in_notification_id);

    -- se registra inicio de transactions
    id_log := register_log_entry(polaris_core.any_element_to_string(in_login),
                                 'delete_notifications',
                                 in_parameters);

    BEGIN


        --valida que el usuario exista
        IF NOT EXISTS(SELECT 1 FROM polaris_core.sec_users WHERE login = in_login) THEN
            RAISE EXCEPTION 'Error: el usuario "%" no existe.', in_login USING ERRCODE = error_code_checked;
        END IF;

        IF NOT EXISTS(SELECT 1 FROM polaris_core.notifications WHERE notification_id = in_notification_id) THEN
            RAISE EXCEPTION 'Error: notificacion no encontrada.' USING ERRCODE = error_code_checked;
        END IF ;

        DELETE FROM notifications WHERE notification_id = in_notification_id;

        GET DIAGNOSTICS local_row_count = ROW_COUNT;

        IF local_row_count =  0 THEN
            RAISE EXCEPTION 'Error: eliminando notificacion.' USING ERRCODE = error_code_checked;
        END IF;


        message := 'Proceso exitoso.';
        status := TRUE;

        out_parameters := 'status:' || any_element_to_string(status) || '|'
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

--------------------------------------------------------

-- Crear una función que valide el ucbario al hacer login
CREATE OR REPLACE PROCEDURE polaris_core.generate_incident_expiration_notification()
    LANGUAGE 'plpgsql'
AS
$BODY$
    --
-----------------------------------------------------------------------
--Objetivo: generar notificaciones cuando las incidencias estan apunto de vencer
-----------------------------------------------------------------------
DECLARE
    code_error                        TEXT DEFAULT ''; -- varible para almacenar codigos de errores
    message_error                     TEXT DEFAULT ''; -- varible para almacenar mensajes de errores

    id_log                            INTEGER; -- varible para log

    in_parameters                     TEXT DEFAULT ''; -- variable para almacenar los parametros de entrada y registrarlos en log
    out_parameters                    TEXT DEFAULT ''; -- variable para almacenar los parametros de salida y registrarlos en log

    success_code                      VARCHAR(20) DEFAULT 'P0000'; --variabal para asignar codigo de proceso exitoso
    error_code_checked                VARCHAR(20) DEFAULT 'P0001'; --variable para asignar codigo de error controlado de validaciones parametros

    --variables configuracion
    -----------------------------
    --tiempo en horas para indicar cuando mostrar la notificacion,
    --es decir si se coloca 3 horas mostrara la notificacion cuando la incidencia esta apunto de vencer dos horas antes
    local_hours_generate_notification INTEGER DEFAULT 6;

    --tiempo en minutos para estabalcer cada cuanto se tiene que generar la
    --notificacion despues de averce generado una notificacion de la incidencia
    local_minutes_elapsed             INTEGER DEFAULT 10;


    --variables locales
    -----------------------------
    local_row_count                   INTEGER;
    local_incidence_id                INTEGER;
    local_hour                        INTEGER;
    local_minutes                     INTEGER;
    local_province_id                 INTEGER;
    local_city_id                     INTEGER;
    local_login                       VARCHAR(255);
    local_notification_message        VARCHAR(255);


BEGIN

    -- se registra inicio de transactions
    id_log := register_log_entry('api',
                                 'generate_incident_expiration_notification',
                                 in_parameters);

    BEGIN

        -- guarda la incidencias que van a vencer
        DROP TABLE IF EXISTS temp_incidences_expiration;
        CREATE TEMPORARY TABLE temp_incidences_expiration
        (
            incidence_id                 INTEGER,
            province_id                  INTEGER,
            city_id                      INTEGER,
            incidence_attention_deadline TIMESTAMP,
            hours                        INTEGER,
            minutes                      INTEGER
        );

        INSERT INTO temp_incidences_expiration (incidence_id, province_id, city_id,
                                                incidence_attention_deadline, hours, minutes)
            -- se obtiene las incidencias que van a vencer en la cantidad de horas establecida
        SELECT T.incidence_id,
               T.province_id,
               T.city_id,
               T.incidence_attention_deadline,
               T.hours,
               EXTRACT(MINUTE FROM (t.incidence_attention_deadline - CURRENT_TIMESTAMP +
                                    (T.hours || ' HOURS'):: interval)) AS minutes
        FROM (SELECT i.incidence_id,
                     CASE
                         WHEN w.warehouse_type = 'CAJERO' THEN
                             a.province_id
                         ELSE
                             cb.province_id END province_id,
                     CASE
                         WHEN w.warehouse_type = 'CAJERO' THEN
                             a.city_id
                         ELSE
                             cb.city_id END     city_id,
                     i.incidence_attention_deadline,
                     FLOOR(EXTRACT(EPOCH FROM
                                   (i.incidence_attention_deadline - CURRENT_TIMESTAMP)) /
                           3600) AS             hours
              FROM polaris_core.incidences i
                       INNER JOIN warehouses w ON w.warehouse_id = i.warehouse_id
                       LEFT JOIN commerces_branches cb ON cb.warehouse_id = w.warehouse_id
                       LEFT JOIN atms a ON a.warehouse_id = w.warehouse_id
                       INNER JOIN incidences_statuses ist
                                  ON i.incidence_status_id = ist.incidence_status_id
              WHERE ist.incidence_status_name = 'EN PROCESO DE ATENCION') AS T
        WHERE T.hours <= local_hours_generate_notification;

        -- recorre la incidencias para generar las notificaciones a la personas que
        -- tienen acceseso a la provincia y a la ciudad donde se encuentra la incidencia
        FOR local_incidence_id,
            local_hour,
            local_minutes,
            local_province_id,
            local_city_id IN (SELECT incidence_id, hours, minutes, province_id, city_id
                              FROM (
                                       --obtiene las incidencias que no se han enviado o que han paso el tiempo
                                       --limite especificado para volver a enviar las notificacion
                                       SELECT c.incidence_id,
                                              c.hours,
                                              c.minutes,
                                              c.province_id,
                                              c.city_id,
                                              EXTRACT(MINUTE FROM
                                                      (CURRENT_TIMESTAMP - nie.notification_incidence_expiration_date)) AS minutos_transcurridos
                                       FROM temp_incidences_expiration c
                                                LEFT JOIN notifications_incidences_expiration nie
                                                          ON nie.incidence_id = C.incidence_id) AS T
                              WHERE t.minutos_transcurridos >= local_minutes_elapsed
                                 OR t.minutos_transcurridos IS NULL)
            LOOP


                FOR local_login IN (
                    -- se obtienen los usuarios que tienen permisos a la
                    -- provincia y a la ciudad que esta la incidencia
                    SELECT cp.login
                    FROM polaris_core.cities_permissions cp
                             INNER JOIN cities c ON cp.city_id = c.city_id
                    WHERE c.province_id = local_province_id
                      AND c.city_id = local_city_id)
                    LOOP

                        --se ajusta mensaje si la incidencia esta a punto de vencer o si ya vencio
                        IF local_hour <= 0 THEN
                            local_notification_message := 'El tiempo de atencion de la incidencia con id "' ||
                                                          polaris_core.any_element_to_string(local_incidence_id) ||
                                                          '" vencio hace ' ||
                                                          polaris_core.any_element_to_string(ABS(local_hour)) ||
                                                          CASE WHEN local_hour = -1 THEN ' hora' ELSE ' horas' END ||
                                                          ' Y ' ||
                                                          polaris_core.any_element_to_string(ABS(local_minutes)) ||
                                                          CASE
                                                              WHEN local_hour = -1 THEN ' minuto'
                                                              ELSE ' minutos' END || '.';

                        ELSE
                            local_notification_message := 'El tiempo de atencion de la incidencia con id "' ||
                                                          polaris_core.any_element_to_string(local_incidence_id) ||
                                                          '" vence en ' ||
                                                          polaris_core.any_element_to_string(local_hour) ||
                                                          CASE WHEN local_hour > 1 THEN ' horas' ELSE ' hora' END ||
                                                          ' Y ' || polaris_core.any_element_to_string(local_minutes) ||
                                                          CASE WHEN local_hour > 1 THEN ' minutos' ELSE ' minuto' END ||
                                                          '.';
                        END IF;

                        INSERT INTO notifications (notification_login, notification_message, notification_type,
                                                   notification_register_by)
                        VALUES (local_login, local_notification_message, 'INCIDENCE',
                                'Coordinador incidencias');

                    END LOOP;

                IF EXISTS(SELECT 1
                          FROM notifications_incidences_expiration
                          WHERE incidence_id = local_incidence_id) THEN

                    DELETE FROM notifications_incidences_expiration WHERE incidence_id = local_incidence_id;
                END IF;

                INSERT INTO notifications_incidences_expiration (incidence_id)
                VALUES (local_incidence_id);

            END LOOP;


        PERFORM polaris_core.register_log_output(
                id_log,
                out_parameters,
                '',
                '',
                '',
                '',
                success_code,
                '');

    EXCEPTION

        WHEN SQLSTATE 'P0001' THEN
            code_error := SQLSTATE;
            message_error := SQLERRM;

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










