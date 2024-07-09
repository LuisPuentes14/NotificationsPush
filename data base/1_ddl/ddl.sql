

CREATE TABLE [notifications] (
  [notification_id] BIGINT PRIMARY KEY IDENTITY (1,1),
  [icon] VARCHAR(MAX),
  [picture] VARCHAR(MAX),
  [title] VARCHAR(MAX),
  [description] VARCHAR(MAX),  
  [start_date] date,
  [end_date] date,
  [enable_frequency_time] BIT DEFAULT 0
)
GO


CREATE TABLE [notifications_schedule] (
  [notification_schedule_id] BIGINT PRIMARY KEY IDENTITY(1,1),
  [notification_id] BIGINT ,
  [hour] TIME
)
GO



CREATE TABLE scheduled_hours(
hour TIME
);
GO


INSERT INTO scheduled_hours 
VALUES 
('00:00'),
('01:00'),
('02:00'),
('03:00'),
('04:00'),
('05:00'),
('06:00'),
('07:00'),
('08:00'),
('09:00'),
('10:00'),
('11:00'),
('12:00'),
('13:00'),
('14:00'),
('15:00'),
('16:00'),
('17:00'),
('18:00'),
('19:00'),
('20:00'),
('21:00'),
('22:00'),
('23:00');



CREATE TABLE [notifications_groups] (
  notification_group_id BIGINT PRIMARY KEY IDENTITY(1,1),
  group_id BIGINT,
  [notification_id] BIGINT
)
GO

CREATE TABLE [notifications_terminals] (
  notification_terminal_id BIGINT PRIMARY KEY IDENTITY(1,1),
  terminal_serial VARCHAR(50),
  [notification_id] BIGINT
)
GO


CREATE TABLE [notifications_terminals_schedules_sent] (
  notification_terminal_schedule_sent_id BIGINT PRIMARY KEY IDENTITY(1,1),
  [terminal_serial] VARCHAR(50),
  [notification_id] BIGINT, 
  notification_schedule_id BIGINT  
)
GO

CREATE TABLE [notifications_pending] (
  [notification_pending_id] BIGINT PRIMARY KEY IDENTITY(1,1),
  [notification_id] BIGINT,
  [terminal_serial] VARCHAR(50),
  [notification_schedule_id] BIGINT, 
  [icon] VARCHAR(MAX),
  [picture] VARCHAR(MAX),
  [title] VARCHAR(MAX),
  [description] VARCHAR(MAX)
)
GO



ALTER TABLE [notifications_terminals_schedules_sent] 
ADD CONSTRAINT fk_terminal_serial
FOREIGN KEY ([terminal_serial]) REFERENCES [terminal] (ter_serial)
GO

ALTER TABLE [notifications_terminals_schedules_sent]
ADD CONSTRAINT fk_notifications_notification_id
FOREIGN KEY ([notification_id]) REFERENCES [notifications] ([notification_id])
GO

ALTER TABLE [notifications_terminals]
ADD CONSTRAINT fk_terminal_serial_notifications_terminals_terminal_serial
FOREIGN KEY  ([terminal_serial]) REFERENCES [terminal] (ter_serial)
GO

ALTER TABLE [notifications_groups] 
ADD CONSTRAINT fk_notifications_notification_id_notifications_group_notification_id
FOREIGN KEY ([notification_id]) REFERENCES [notifications] ([notification_id])
GO


ALTER TABLE [notifications_groups]
ADD CONSTRAINT fk_grupo_gru_id_notifications_group_grupo_id
FOREIGN KEY ([group_id]) REFERENCES [grupo] (gru_id)
GO

ALTER TABLE [notifications_terminals] 
ADD CONSTRAINT fk_notifications_notification_id_notifications_terminals_notification_id
FOREIGN KEY ([notification_id]) REFERENCES [notifications] ([notification_id])
GO

ALTER TABLE [notifications_schedule] 
ADD  CONSTRAINT fk_notifications_notification_id_otifications_schedule_notification_id
FOREIGN KEY ([notification_id]) REFERENCES [notifications] ([notification_id])
GO


--ALTER TABLE [notifications_terminal_schedules] 
--ADD  CONSTRAINT fk_notifications_schedule_notification_schedule_id_notifications_terminal_schedules_notification_schedule_id
--FOREIGN KEY ([notification_schedule_id]) REFERENCES [notifications_schedule] ([notification_schedule_id])
----GO
--ALTER TABLE notifications_terminal_schedules 
--DROP CONSTRAINT fk_notifications_schedule_notification_schedule_id_notifications_terminal_schedules_notification_schedule_id
