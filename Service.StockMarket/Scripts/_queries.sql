DROP DATABASE IF EXISTS BD_StockMarket
CREATE DATABASE BD_StockMarket;
GO

USE BD_StockMarket;
GO

DROP TABLE IF EXISTS BaseCurrencies
CREATE TABLE BaseCurrencies
(
	 ID			INT				NOT NULL	IDENTITY(1, 1)
	,Name		VARCHAR(10)		NOT NULL
	,Active		BIT				NOT NULL	DEFAULT 1
	,Deleted	BIT				NOT NULL	DEFAULT 0
	,Date		DATETIME		DEFAULT		GETDATE()

	CONSTRAINT PK_BaseCurrencyID PRIMARY KEY(ID)
);

INSERT INTO BaseCurrencies (Name) VALUES('USD');
INSERT INTO BaseCurrencies (Name) VALUES('EUR');


DROP TABLE IF EXISTS Currencies
CREATE TABLE Currencies
(
	 ID				INT				NOT NULL	IDENTITY(1, 1)
	,Exchange		VARCHAR(10)		NOT NULL
	,BaseCurrencyID	INT				NOT NULL
	,Active			BIT				NOT NULL	DEFAULT 1
	,Deleted		BIT				NOT NULL	DEFAULT 0
	,Date			DATETIME		DEFAULT		GETDATE()

	 CONSTRAINT PK_CurrencyID PRIMARY KEY(ID)

	,CONSTRAINT FK_Currencies_BaseCurrencyID FOREIGN KEY(BaseCurrencyID)
	 REFERENCES BaseCurrencies(ID)
);

INSERT INTO Currencies (Exchange, BaseCurrencyID) VALUES('US', 1);


SELECT		C.*
FROM		Currencies		C  (NOLOCK)
INNER JOIN	BaseCurrencies	BC (NOLOCK)
	ON BC.ID = C.BaseCurrencyID
WHERE		C.Deleted	= 0
  AND		C.Active	= 1
  AND		BC.Deleted	= 0
  AND		BC.Active	= 1;


DROP TABLE IF EXISTS StockTypes
CREATE TABLE StockTypes
(
	 ID				INT				NOT NULL	IDENTITY(1, 1)
	,Description	VARCHAR(200)	NOT NULL
	,Active			BIT				NOT NULL	DEFAULT 1
	,Deleted		BIT				NOT NULL	DEFAULT 0
	,Date			DATETIME		DEFAULT		GETDATE()

	CONSTRAINT PK_StockTypeID PRIMARY KEY(ID)
);

INSERT INTO StockTypes (Description)
VALUES
 ('Common Stock')
,('ETP')
,('ADR')
,('Unit')
,('Equity WRT')
,('REIT')
,('Ltd Part')
,('Preference')
,('Tracking Stk')
,('PUBLIC')
,('Closed-End Fund');

SELECT * FROM StockTypes WHERE Description = 'Preference' AND Active = 1 AND Deleted = 0;


DROP TABLE IF EXISTS Symbols
CREATE TABLE Symbols
(
	 ID				INT				NOT NULL	IDENTITY(1, 1)
	,Description	VARCHAR(200)	NOT NULL
	,DisplaySymbol	VARCHAR(10)		NOT NULL
	,Figi			VARCHAR(100)
	,Isin			VARCHAR(100)
	,Mic			VARCHAR(50)
	,ShareClassFIGI	VARCHAR(100)
	,Symbol			VARCHAR(50)		NOT NULL
	,Symbol2		VARCHAR(50)
	,StokeTypeID	INT
	,Active			BIT				NOT NULL	DEFAULT 1
	,Deleted		BIT				NOT NULL	DEFAULT 0
	,Date			DATETIME		DEFAULT		GETDATE()

	CONSTRAINT PK_SymbolID PRIMARY KEY(ID)

	CONSTRAINT FK_Symbols_StokeTypeID FOREIGN KEY(StokeTypeID)
	REFERENCES StockTypes(ID)
);

SELECT * FROM Symbols


DROP TABLE IF EXISTS CompaniesProfile
CREATE TABLE CompaniesProfile
(
	 ID						INT			NOT NULL	IDENTITY(1, 1)
	,Ipo					DATETIME
	,Logo					VARCHAR(200)
	,MarketCapitalization	VARCHAR(20)
	,Name					VARCHAR(200)
	,Phone					VARCHAR(200)
	,ShareOutstanding		MONEY
	,Ticker					VARCHAR(50)
	,Weburl					VARCHAR(100)
	,SymbolID				INT
	,CurrencyID				INT
	,Active					BIT			NOT NULL	DEFAULT 1
	,Deleted				BIT			NOT NULL	DEFAULT 0
	,Date					DATETIME	DEFAULT		GETDATE()

	CONSTRAINT PK_CompanyProfileID PRIMARY KEY(ID)

	CONSTRAINT FK_CompaniesProfile_SymbolID FOREIGN KEY(SymbolID)
	REFERENCES Symbols(ID),

	CONSTRAINT FK_CompaniesProfile_CurrencyID FOREIGN KEY(CurrencyID)
	REFERENCES Currencies(ID)
);