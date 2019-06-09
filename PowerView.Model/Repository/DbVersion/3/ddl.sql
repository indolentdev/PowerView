DELETE FROM DayRegister;
DELETE FROM DayReading;

DROP INDEX StreamPositionIX;
DROP TABLE StreamPosition;

CREATE TABLE StreamPosition (Id INTEGER PRIMARY KEY, StreamName NVARCHAR(20) NOT NULL, Label NVARCHAR(10) NOT NULL, Position INTEGER NOT NULL);
CREATE UNIQUE INDEX StreamPositionIX ON StreamPosition (StreamName, Label);