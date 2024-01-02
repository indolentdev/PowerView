CREATE TABLE LabelObisLive (LabelId INTEGER NOT NULL, ObisId INTEGER NOT NULl, LatestTimestamp DATETIME NOT NULL, PRIMARY KEY(LabelId, ObisId), FOREIGN KEY(LabelId) REFERENCES Label(Id), FOREIGN KEY (ObisId) REFERENCES Obis(Id)) WITHOUT ROWID;

INSERT INTO LabelObisLive (LabelId, ObisId, LatestTimestamp) SELECT rea.LabelId, reg.ObisId, MAX(rea.Timestamp) FROM LiveReading rea JOIN LiveRegister reg on rea.Id = reg.ReadingId GROUP BY rea.LabelId, reg.ObisId;
