DELETE FROM Setting WHERE Name='InstallationId';
INSERT INTO Setting (Name,Value) SELECT 'SMTP_Email', Value FROM Setting WHERE Name = 'SMTP_User' AND NOT EXISTS(SELECT 1 FROM Setting WHERE Name = 'SMTP_Email');