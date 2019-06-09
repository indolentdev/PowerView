CREATE TABLE YearReading (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, SerialNumber INTEGER NOT NULL, Timestamp DATETIME NOT NULL);
CREATE INDEX YearReadingIX ON YearReading (Timestamp DESC);
CREATE TABLE YearRegister (Id INTEGER PRIMARY KEY, ObisCode INTEGER NOT NULL, Value INTEGER NOT NULL, Scale INTEGER NOT NULL, Unit INTEGER NOT NULL, ReadingId INTEGER NOT NULL, FOREIGN KEY (ReadingId) REFERENCES YearReading(Id));
CREATE UNIQUE INDEX YearRegisterIX ON YearRegister (ReadingId, ObisCode);