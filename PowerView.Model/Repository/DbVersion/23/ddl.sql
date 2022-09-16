DELETE FROM Setting WHERE Name='InstallationId';
INSERT INTO Setting (Name,Value) SELECT 'SMTP_Email', Value FROM Setting WHERE Name = 'SMTP_User' AND NOT EXISTS(SELECT 1 FROM Setting WHERE Name = 'SMTP_Email');

CREATE TABLE Tmp(Value NVARCHAR(50) NOT NULL);

CREATE TABLE Label (Id INTEGER PRIMARY KEY, LabelName NVARCHAR(50) NOT NULL UNIQUE, Timestamp DATETIME NOT NULL);
INSERT INTO Tmp SELECT DISTINCT Label FROM LiveReading;
INSERT INTO Label (LabelName, Timestamp) SELECT Value, datetime('now') FROM Tmp;

DELETE FROM Tmp;

CREATE TABLE Device (Id INTEGER PRIMARY KEY, DeviceName NVARCHAR(50) NOT NULL UNIQUE, Timestamp DATETIME NOT NULL);
INSERT INTO Tmp SELECT DISTINCT DeviceId FROM LiveReading;
INSERT INTO Device (DeviceName, Timestamp) SELECT Value, datetime('now') FROM Tmp;

DROP TABLE Tmp;


DROP INDEX StreamPositionIX;
DROP TABLE StreamPosition;

CREATE TABLE StreamPosition (Id INTEGER PRIMARY KEY, StreamName NVARCHAR(20) NOT NULL, LabelId INTEGER NOT NULL, Position INTEGER NOT NULL);
CREATE UNIQUE INDEX StreamPositionIX ON StreamPosition (StreamName, LabelId);

DROP TABLE DayRegister;
DROP TABLE MonthRegister;
DROP TABLE YearRegister;
DROP INDEX DayReadingIX;
DROP INDEX MonthReadingIX;
DROP INDEX YearReadingIX;
DROP TABLE DayReading;
DROP TABLE MonthReading;
DROP TABLE YearReading;

DROP INDEX LiveReadingIX;

ALTER TABLE LiveRegister RENAME TO LiveRegisterOld;
ALTER TABLE LiveReading RENAME TO LiveReadingOld;

CREATE TABLE LiveReading (Id INTEGER PRIMARY KEY, LabelId INTEGER NOT NULL, DeviceId INTEGER NOT NULL, Timestamp DATETIME NOT NULL, FOREIGN KEY (LabelId) REFERENCES Label(Id), FOREIGN KEY (DeviceId) REFERENCES Device(Id));
CREATE INDEX LiveReadingIX ON LiveReading (Timestamp DESC);
CREATE TABLE LiveRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES LiveReading(Id)) WITHOUT ROWID;

INSERT INTO LiveReading (Id, LabelId, DeviceId, Timestamp) SELECT r.Id, l.Id, d.Id, r.Timestamp FROM LiveReadingOld r JOIN Label l ON r.Label = l.LabelName JOIN Device d ON r.DeviceId = d.DeviceName;
INSERT INTO LiveRegister (ReadingId, ObisCode, Value, Scale, Unit) SELECT ReadingId, ObisCode, Value, Scale, Unit FROM LiveRegisterOld;

DROP TABLE LiveRegisterOld;
DROP TABLE LiveReadingOld;

CREATE TABLE DayReading (Id INTEGER PRIMARY KEY, LabelId INTEGER NOT NULL, DeviceId INTEGER NOT NULL, Timestamp DATETIME NOT NULL, FOREIGN KEY (LabelId) REFERENCES Label(Id), FOREIGN KEY (DeviceId) REFERENCES Device(Id));
CREATE INDEX DayReadingIX ON DayReading (Timestamp DESC);
CREATE TABLE DayRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES DayReading(Id)) WITHOUT ROWID;

CREATE TABLE MonthReading (Id INTEGER PRIMARY KEY, LabelId INTEGER NOT NULL, DeviceId INTEGER NOT NULL, Timestamp DATETIME NOT NULL, FOREIGN KEY (LabelId) REFERENCES Label(Id), FOREIGN KEY (DeviceId) REFERENCES Device(Id));
CREATE INDEX MonthReadingIX ON MonthReading (Timestamp DESC);
CREATE TABLE MonthRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES MonthReading(Id)) WITHOUT ROWID;

CREATE TABLE YearReading (Id INTEGER PRIMARY KEY, LabelId INTEGER NOT NULL, DeviceId INTEGER NOT NULL, Timestamp DATETIME NOT NULL, FOREIGN KEY (LabelId) REFERENCES Label(Id), FOREIGN KEY (DeviceId) REFERENCES Device(Id));
CREATE INDEX YearReadingIX ON YearReading (Timestamp DESC);
CREATE TABLE YearRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES YearReading(Id)) WITHOUT ROWID;