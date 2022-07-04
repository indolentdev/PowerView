CREATE TABLE EmailRecipient (Id INTEGER PRIMARY KEY, Name NVARCHAR(255) NOT NULL, EmailAddress NVARCHAR(255) NOT NULL);
CREATE UNIQUE INDEX EmailRecipientIX ON EmailRecipient (EmailAddress);

CREATE TABLE EmailRecipientMeterEventPosition (Id INTEGER PRIMARY KEY, EmailRecipientId INTEGER NOT NULL, MeterEventId INTEGER NOT NULL, FOREIGN KEY (EmailRecipientId) REFERENCES EmailRecipient(Id), FOREIGN KEY (MeterEventId) REFERENCES MeterEvent(Id));

CREATE TABLE EmailMessage (Id INTEGER PRIMARY KEY, FromName NVARCHAR(255) NOT NULL, FromEmailAddress NVARCHAR(255) NOT NULL, ToName NVARCHAR(255) NOT NULL, ToEmailAddress NVARCHAR(255) NOT NULL, Subject NVARCHAR(255) NOT NULL, Body NVARCHAR(8192) NOT NULL);