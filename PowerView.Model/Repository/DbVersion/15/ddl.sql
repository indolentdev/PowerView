CREATE TABLE DisconnectRule (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, ObisCode INTEGER NOT NULL, EvaluationLabel NVARCHAR(10) NOT NULL, EvaluationObisCode INTEGER NOT NULL, DurationSeconds INTEGER NOT NULL, DisconnectToConnectValue INTEGER NOT NULL, ConnectToDisconnectValue INTEGER NOT NULL, Unit INTEGER NOT NULL);
CREATE UNIQUE INDEX DisconnectRuleIX ON DisconnectRule (Label, ObisCode);