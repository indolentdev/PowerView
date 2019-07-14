CREATE TABLE Setting (Id INTEGER PRIMARY KEY, Name NVARCHAR(32) NOT NULL, Value NVARCHAR(255) NOT NULL);
CREATE UNIQUE INDEX SettingIX ON Setting (Name);