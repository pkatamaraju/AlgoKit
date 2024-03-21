CREATE PROC INSERT_SignalData-- 'BUY','short'
(@Signal Varchar(100),
@Period Varchar(100))
AS
BEGIN

IF(@Period='SHORT')
	INSERT INTO SuperTrend_ShortTermSingal(Signal,CreatedDate)
	VALUES(@Signal,GETDATE())
ELSE IF(@Period='LONG')
	INSERT INTO SuperTrend_LongTermSingal(Signal,CreatedDate)
	VALUES(@Signal,GETDATE())

END