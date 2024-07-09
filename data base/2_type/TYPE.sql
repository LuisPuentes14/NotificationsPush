CREATE TYPE list_notifications_serials_terminals_schedules
AS
TABLE ( 
notification_id BIGINT, 
serial_terminal VARCHAR(50),
schedule_id BIGINT
);

CREATE TYPE list_notifications_pending
AS
TABLE ( 
notification_pending_id BIGINT
);






