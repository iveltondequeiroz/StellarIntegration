CREATE TABLE [dbo].[ExchangeUsers] (
    [CustomerID] INT          IDENTITY (1, 1) NOT NULL,
    [Account]    VARCHAR (56) NOT NULL,
    [Balance]    INT          DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomerID] ASC)
);

