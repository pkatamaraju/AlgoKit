CREATE TABLE KiteAPIKeys
(
UserID VARCHAR(10) PRIMARY KEY,
APIKey  VARCHAR(100),
Secret  VARCHAR(100),
AccessToken VARCHAR(100),
RefreshToken VARCHAR(100),
LastLoginTime Datetime,
CreatedDate Datetime default getdate(),
LastUpdatedDate Datetime
)