DELETE FROM Setting WHERE Name='InstallationId';
INSERT INTO Setting (Name,Value) SELECT 'SMTP_Email', Value FROM Setting WHERE Name = 'SMTP_User' AND NOT EXISTS(SELECT 1 FROM Setting WHERE Name = 'SMTP_Email');
UPDATE Version SET [Timestamp] = datetime([Timestamp], 'unixepoch', 'utc') WHERE datetime([Timestamp], 'unixepoch', 'utc') IS NOT NULL;
UPDATE LiveReading SET [Timestamp] = datetime([Timestamp], 'unixepoch', 'utc');
UPDATE DayReading SET [Timestamp] = datetime([Timestamp], 'unixepoch', 'utc');
UPDATE MonthReading SET [Timestamp] = datetime([Timestamp], 'unixepoch', 'utc');
UPDATE YearReading SET [Timestamp] = datetime([Timestamp], 'unixepoch', 'utc');
UPDATE MeterEvent SET [DetectTimestamp] = datetime([DetectTimestamp], 'unixepoch', 'utc');