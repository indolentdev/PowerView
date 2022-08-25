DELETE FROM Setting WHERE Name='InstallationId';
INSERT INTO Setting (Name,Value) SELECT 'SMTP_Email', Value FROM Setting WHERE Name = 'SMTP_User' AND NOT EXISTS(SELECT 1 FROM Setting WHERE Name = 'SMTP_Email');
UPDATE Version SET [Timestamp] = datetime([Timestamp], 'unixepoch') WHERE datetime([Timestamp], 'unixepoch') IS NOT NULL;
UPDATE LiveReading SET [Timestamp] = datetime([Timestamp], 'unixepoch');
UPDATE DayReading SET [Timestamp] = datetime([Timestamp], 'unixepoch');
UPDATE MonthReading SET [Timestamp] = datetime([Timestamp], 'unixepoch');
UPDATE YearReading SET [Timestamp] = datetime([Timestamp], 'unixepoch');
UPDATE MeterEvent SET [DetectTimestamp] = datetime([DetectTimestamp], 'unixepoch');