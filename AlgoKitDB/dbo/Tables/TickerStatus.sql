CREATE TABLE TickerStatus
(
ID INT IDENTITY(1,1),
Status Varchar(200),
CreatedDate datetime default getdate()
)