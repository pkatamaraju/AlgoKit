Create Proc INSERT_EXCEPTION
(@ExceptionMessage VARCHAR(MAX)
)
AS
BEGIN
	INSERT INTO EXCEPTION (ExceptionMessage)
	VALUES(@ExceptionMessage)
END