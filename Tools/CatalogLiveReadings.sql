.headers on
.mode column
with grp as (
select LabelId, DeviceId, Min(Timestamp) as MinTimestamp, Max(Timestamp) as MaxTimestamp, Count(*) as Cnt
from LiveReading
group by LabelId, DeviceId
)
select l.LabelName, d.DeviceName, datetime(g.MinTimestamp, 'unixepoch') as MinTimestamp, datetime(g.MaxTimestamp, 'unixepoch') as MaxTimestamp, g.Cnt as ReadingCount
from grp g
join Label l on g.LabelId = l.Id
join Device d on g.DeviceId = d.Id
order by LabelName, MinTimestamp, Devicename
;
