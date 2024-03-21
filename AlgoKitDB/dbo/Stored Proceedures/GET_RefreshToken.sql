create PROC GET_RefreshToken
(@UserID VARCHAR(10))
as
begin

	SELECT RefreshToken from KiteAPIKeys
	where UserID=@UserID
end