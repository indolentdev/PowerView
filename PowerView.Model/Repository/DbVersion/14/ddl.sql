DELETE FROM StreamPosition;

DROP INDEX DayRegisterIX;
DROP INDEX DayReadingIX;
DROP TABLE DayRegister;
DROP TABLE DayReading;
CREATE TABLE DayReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, SerialNumber NVARCHAR(10) NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX DayReadingIX ON DayReading (Timestamp DESC);
CREATE TABLE DayRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES DayReading(Id));
CREATE UNIQUE INDEX DayRegisterIX ON DayRegister (ReadingId, ObisCode);

DROP INDEX MonthRegisterIX;
DROP INDEX MonthReadingIX;
DROP TABLE MonthRegister;
DROP TABLE MonthReading;
CREATE TABLE MonthReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, SerialNumber NVARCHAR(10) NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX MonthReadingIX ON MonthReading (Timestamp DESC);
CREATE TABLE MonthRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES MonthReading(Id));
CREATE UNIQUE INDEX MonthRegisterIX ON MonthRegister (ReadingId, ObisCode);

DROP INDEX YearRegisterIX;
DROP INDEX YearReadingIX;
DROP TABLE YearRegister;
DROP TABLE YearReading;
CREATE TABLE YearReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, SerialNumber NVARCHAR(10) NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX YearReadingIX ON YearReading (Timestamp DESC);
CREATE TABLE YearRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES YearReading(Id));
CREATE UNIQUE INDEX YearRegisterIX ON YearRegister (ReadingId, ObisCode);

DROP INDEX LiveRegisterIX;
DROP INDEX LiveReadingIX;
ALTER TABLE LiveRegister RENAME TO OldLiveRegister;
ALTER TABLE LiveReading RENAME TO OldLiveReading;

CREATE TABLE LiveReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, SerialNumber NVARCHAR(10) NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX LiveReadingIX ON LiveReading (Timestamp DESC);
CREATE TABLE LiveRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES LiveReading(Id));
CREATE UNIQUE INDEX LiveRegisterIX ON LiveRegister (ReadingId, ObisCode);

INSERT INTO LiveReading (Id, Label, SerialNumber, Timestamp) SELECT Id, Label, CAST(SerialNumber AS TEXT), Timestamp FROM OldLiveReading;
INSERT INTO LiveRegister (Id, ObisCode, Value, Scale, Unit, ReadingId) SELECT Id, ObisCode, Value, Scale, Unit, ReadingId FROM OldLiveRegister;

DROP TABLE OldLiveRegister;
DROP TABLE OldLiveReading;

VACUUM;