CREATE TABLE ProfileGraph (Id INTEGER PRIMARY KEY, Period NVARCHAR(20) NOT NULL, Page NVARCHAR(32) NOT NULL, Title NVARCHAR(32) NOT NULL, Interval NVARCHAR(20) NOT NULL);
CREATE UNIQUE INDEX ProfileGraphIX ON ProfileGraph (Period, Page, Title);
CREATE TABLE ProfileGraphSerie (Id INTEGER PRIMARY KEY, Label NVARCHAR(10) NOT NULL, ObisCode INTEGER NOT NULL, ProfileGraphId INTEGER NOT NULL, FOREIGN KEY (ProfileGraphId) REFERENCES ProfileGraph(Id));
CREATE UNIQUE INDEX ProfileGraphSerieIX ON ProfileGraphSerie (ProfileGraphId, Label, ObisCode);