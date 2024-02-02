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
