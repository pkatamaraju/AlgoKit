create PROC GET_AccessToken
(@UserID VARCHAR(10))
as
begin

	SELECT AccessToken from KiteAPIKeys
	where UserID=@UserID
end