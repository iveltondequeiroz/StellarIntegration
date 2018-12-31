CREATE TABLE [dbo].[StellarDeposits] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [Amount]     INT      DEFAULT ((0)) NOT NULL,
    [Date]       DATETIME NOT NULL,
    [CustomerId] INT      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

