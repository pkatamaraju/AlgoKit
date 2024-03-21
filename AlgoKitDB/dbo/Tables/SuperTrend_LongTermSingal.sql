
Create table SuperTrend_ShortTermSingal
(
ID INT IDENTITY(1,1),
Signal Varchar(100),
CreatedDate Datetime default getdate()
)