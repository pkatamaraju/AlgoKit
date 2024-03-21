CREATE PROC GET_SignalSummaryToTrade
AS
BEGIN

DECLARE @LongSingal Varchar(100),@ShortSingal Varchar(100)

select top(1) @LongSingal=Signal from SuperTrend_LongTermSingal
Order by CreatedDate desc

select top(1) @ShortSingal=signal from SuperTrend_ShortTermSingal
Order by CreatedDate desc

If(@LongSingal='BUY' AND @ShortSingal='BUY' )
SELECT 'BUY' --BUY CE POSITIONS

ELSE IF(@LongSingal='BUY' AND @ShortSingal='SELL')
SELECT 'EXIT_CE'

ELSE IF(@LongSingal='SELL' AND @ShortSingal='SELL' )
SELECT 'SELL' --BUY PE POSITIONS

ELSE IF(@LongSingal='SELL' AND @ShortSingal='BUY')
SELECT 'EXIT_PE'

ELSE
	SELECT 'NoTrade'

END