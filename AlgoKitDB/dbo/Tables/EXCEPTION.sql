
CREATE TABLE EXCEPTION
(ID INT IDENTITY(1,1),
ExceptionMessage VARCHAR(MAX),
CreatedDate datetime default getdate()
)