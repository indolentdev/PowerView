DROP INDEX LiveRegisterIX;
ALTER TABLE LiveRegister RENAME TO LiveRegisterOld;
CREATE TABLE LiveRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES LiveReading(Id)) WITHOUT ROWID;
INSERT INTO LiveRegister (ReadingId, ObisCode, Value, Scale, Unit) SELECT ReadingId, ObisCode, Value, Scale, Unit FROM LiveRegisterOld;
DROP TABLE LiveRegisterOld;
DROP INDEX DayRegisterIX;
ALTER TABLE DayRegister RENAME TO DayRegisterOld;
CREATE TABLE DayRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES DayReading(Id)) WITHOUT ROWID;
INSERT INTO DayRegister (ReadingId, ObisCode, Value, Scale, Unit) SELECT ReadingId, ObisCode, Value, Scale, Unit FROM DayRegisterOld;
DROP TABLE DayRegisterOld;
DROP INDEX MonthRegisterIX;
ALTER TABLE MonthRegister RENAME TO MonthRegisterOld;
CREATE TABLE MonthRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES MonthReading(Id)) WITHOUT ROWID;
INSERT INTO MonthRegister (ReadingId, ObisCode, Value, Scale, Unit) SELECT ReadingId, ObisCode, Value, Scale, Unit FROM MonthRegisterOld;
DROP TABLE MonthRegisterOld;
DROP INDEX YearRegisterIX;
ALTER TABLE YearRegister RENAME TO YearRegisterOld;
CREATE TABLE YearRegister (ReadingId INTEGER NOT NULL, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, PRIMARY KEY(ReadingId,ObisCode), FOREIGN KEY (ReadingId) REFERENCES YearReading(Id)) WITHOUT ROWID;
INSERT INTO YearRegister (ReadingId, ObisCode, Value, Scale, Unit) SELECT ReadingId, ObisCode, Value, Scale, Unit FROM YearRegisterOld;
DROP TABLE YearRegisterOld;