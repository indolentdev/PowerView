ALTER TABLE ProfileGraph ADD COLUMN Rank INTEGER NOT NULL DEFAULT 0;
UPDATE ProfileGraph SET Rank = Id; 
CREATE UNIQUE INDEX ProfileGraphRankIX ON ProfileGraph (Period, Page, Rank);