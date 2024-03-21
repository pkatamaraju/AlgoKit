
Create table SuperTrend_LongTermSingal
(
ID INT IDENTITY(1,1),
Signal Varchar(100),
CreatedDate Datetime default getdate()
)