CREATE PROC INSERT_TickerStatus
(@Status Varchar(200)
)
AS
BEGIN
	INSERT INTO TickerStatus(Status)
	VALUES(@Status)
END