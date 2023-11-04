-- copies from current database to an attached database named dst (of same schema)

attach '/var/lib/PowerView/PowerView2.sqlite3' as dst;

select 'Databases:';
.database

begin transaction;

select '';
select 'Building basic data map...';

create table LabelMap (SrcLabelId integer primary key, LabelName nvarchar(50) not null unique, Timestamp datetime not null, DstLabelId integer null) without rowid;
insert into LabelMap select Id, LabelName, Timestamp, null from Label;
update LabelMap set DstLabelId = (select Id from dst.Label where LabelName = LabelMap.LabelName) where exists (select Id from dst.Label where LabelName = LabelMap.LabelName);

create table DeviceMap (SrcDeviceId integer primary key, DeviceName nvarchar(50) not null unique, Timestamp datetime not null, DstDeviceId integer null) without rowid;
insert into DeviceMap select Id, DeviceName, Timestamp, null from Device;
update DeviceMap set DstDeviceId = (select Id from dst.Device where DeviceName = DeviceMap.DeviceName) where exists (select Id from dst.Device where DeviceName = DeviceMap.DeviceName);

create table ObisMap (SrcObisId integer primary key, ObisCode integer not null unique, DstObisId integer null) without rowid;
insert into ObisMap select Id, ObisCode, null from Obis;
update ObisMap set DstObisId = (select Id from dst.Obis where ObisCode = ObisMap.ObisCode) where exists (select Id from dst.Obis where ObisCode = ObisMap.ObisCode);

select '';
select 'Copying basic data...';

insert into dst.Label select null, LabelName, Timestamp from LabelMap where DstLabelId is null;
update LabelMap set DstLabelId = (select Id from dst.Label where LabelName = LabelMap.LabelName) where exists (select Id from dst.Label where LabelName = LabelMap.LabelName) and DstLabelId is null;

insert into dst.Device select null, DeviceName, Timestamp from DeviceMap where DstDeviceId is null;
update DeviceMap set DstDeviceId = (select Id from dst.Device where DeviceName = DeviceMap.DeviceName) where exists (select Id from dst.Device where DeviceName = DeviceMap.DeviceName) and DstDeviceId is null;

insert into dst.Obis select null, ObisCode from ObisMap where DstObisId is null;
update ObisMap set DstObisId = (select Id from dst.Obis where ObisCode = ObisMap.ObisCode) where exists (select Id from dst.Obis where ObisCode = ObisMap.ObisCode) and DstObisId is null;


select '';
select 'Building live data map...';

create table LiveReadingMap (SrcLiveReadingId integer primary key, SrcLabelId integer not null, SrcDeviceId integer not null, Timestamp datetime not null, DstLiveReadingId integer null, DstLabelId integer null, DstDeviceId integer null) without rowid;
create unique index LiveReadingMap1IX ON LiveReadingMap (Timestamp DESC, SrcLabelId);
create unique index LiveReadingMap2IX ON LiveReadingMap (Timestamp DESC, DstLabelId);
insert into LiveReadingMap select lr.Id, lr.LabelId, lr.DeviceId, lr.Timestamp, null, null, null from LiveReading lr join Label l on lr.LabelId=l.Id where l.LabelName='TODO';
update LiveReadingMap set DstLabelId = (select DstLabelId from LabelMap where SrcLabelid = LiveReadingMap.SrcLabelId) where exists (select DstLabelId from LabelMap where SrcLabelid = LiveReadingMap.SrcLabelId) and DstLabelId is null;
update LiveReadingMap set DstDeviceId = (select DstDeviceId from DeviceMap where SrcDeviceId = LiveReadingMap.SrcDeviceId) where exists (select DstDeviceId from DeviceMap where SrcDeviceId = LiveReadingMap.SrcDeviceId) and DstDeviceId is null;

select '';
select 'Copying live data...';
insert into dst.LiveReading select null, DstLabelId, DstDeviceId, Timestamp from LiveReadingMap;
update LiveReadingMap set DstLiveReadingId = (select Id from dst.LiveReading where Timestamp = LiveReadingMap.Timestamp and LabelId = LiveReadingMap.DstLabelId) where exists (select Id from dst.LiveReading where Timestamp = LiveReadingMap.Timestamp and LabelId = LiveReadingMap.DstLabelId) and DstLiveReadingId is null;

insert into dst.LiveRegister select lrm.DstLiveReadingId, om.DstObisId, lr.Value, lr.Scale, lr.Unit from LiveReadingMap lrm join LiveRegister lr on lrm.SrcLiveReadingId=lr.ReadingId join ObisMap om on lr.ObisId = om.SrcObisId;
insert into dst.LiveRegisterTag select lrm.DstLiveReadingId, om.DstObisId, lrt.Tags from LiveReadingMap lrm join LiveRegisterTag lrt on lrm.SrcLiveReadingId=lrt.ReadingId join ObisMap om on lrt.ObisId = om.SrcObisId;

select '';
select 'Copy complete.';

select '';
select 'Dropping live data map...';

drop table LiveReadingMap;


select '';
select 'Dropping basic data map...';

drop table ObisMap;
drop table DeviceMap;
drop table LabelMap;

select '';
select 'Committing transaction...';

commit transaction;

select 'Done.';

