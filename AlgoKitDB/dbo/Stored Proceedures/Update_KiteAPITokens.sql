Create proc Update_KiteAPITokens
(
@UserID VARCHAR(10),
@AccessToken VARCHAR(100),
@RefreshToken VARCHAR(100) 
)
as
begin

	UPDATE KiteAPIKeys SET RefreshToken=@RefreshToken,AccessToken=@AccessToken,LastUpdatedDate=GETDATE()
	WHERE UserID=@UserID
end