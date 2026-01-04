ALTER TABLE matches
ADD COLUMN IF NOT EXISTS host_team ENUM('red', 'blue') NOT NULL DEFAULT 'red'
AFTER joiner_name;

UPDATE matches SET host_team = 'red' WHERE host_team IS NULL;
