DELETE FROM StreamPosition;
DELETE FROM DayRegister;
DELETE FROM DayReading;
DELETE FROM MonthRegister;
DELETE FROM MonthReading;
DELETE FROM YearRegister;
DELETE FROM YearReading;

DROP INDEX LiveReadingIX;
DROP INDEX DayReadingIX;
DROP INDEX MonthReadingIX;
DROP INDEX YearReadingIX;

CREATE TABLE Tmp1 ( Timestamp DATETIME NOT NULL, LabelId INTEGER NOT NULL, Count INTEGER NOT NULL );
INSERT INTO Tmp1 (Timestamp, LabelId, Count) SELECT Timestamp, LabelId, Count(*) AS Count FROM LiveReading GROUP BY Timestamp, LabelId;

CREATE TABLE Tmp2 ( Id INTEGER NOT NULL );
INSERT INTO Tmp2 (Id) SELECT Id FROM LiveReading r JOIN Tmp1 t ON r.Timestamp = t.Timestamp AND r.LabelId = t.LabelId WHERE t.Count > 1;

DELETE FROM LiveRegister WHERE ReadingId IN (SELECT Id FROM Tmp2);
DELETE FROM LiveReading WHERE Id IN (SELECT Id FROM Tmp2);

DROP TABLE Tmp1;
DROP TABLE Tmp2;

CREATE UNIQUE INDEX LiveReadingIX ON LiveReading (Timestamp DESC, LabelId);
CREATE UNIQUE INDEX DayReadingIX ON DayReading (Timestamp DESC, LabelId);
CREATE UNIQUE INDEX MonthReadingIX ON MonthReading (Timestamp DESC, LabelId);
CREATE UNIQUE INDEX YearReadingIX ON YearReading (Timestamp DESC, LabelId);
