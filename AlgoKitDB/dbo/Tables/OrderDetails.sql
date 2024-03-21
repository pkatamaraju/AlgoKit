CREATE TABLE OrderDetails
(KiteOrderID VARCHAR(100),
Quantity int,
Price decimal,
Product VARCHAR(100),
TradingSymbol VARCHAR(100),
TradingSymbolID INT,
TransactionType VARCHAR(100),
OrderType  VARCHAR(100),
Variety VARCHAR(100),
CreatedDate datetime,
KiteOrderStatus VARCHAR(100),
status VARCHAR(100),
TriggerPrice decimal,
constraint PK_OrderDetails_KiteOrderID PRIMARY KEY (KiteOrderID)
)