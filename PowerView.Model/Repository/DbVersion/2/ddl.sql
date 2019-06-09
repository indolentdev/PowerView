DROP INDEX LiveRegisterIX;
ALTER TABLE LiveRegister RENAME TO TmpLiveRegister;

CREATE TABLE LiveRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES LiveReading(Id));
CREATE UNIQUE INDEX LiveRegisterIX ON LiveRegister (ReadingId, ObisCode);

INSERT INTO LiveRegister(Id, ObisCode, Value, Scale, Unit, ReadingId) SELECT Id, ObisCode, Value, Scale, Unit, LiveReadingId FROM TmpLiveRegister;
DROP TABLE TmpLiveRegister;

CREATE TABLE DayReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX DayReadingIX ON DayReading (Timestamp DESC);

CREATE TABLE DayRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES DayReading(Id));
CREATE UNIQUE INDEX DayRegisterIX ON DayRegister (ReadingId, ObisCode);

CREATE TABLE StreamPosition (Id INTEGER PRIMARY KEY, StreamName NVARCHAR(20) NOT NULL, Position INTEGER NOT NULL);
CREATE UNIQUE INDEX StreamPositionIX ON StreamPosition (StreamName);