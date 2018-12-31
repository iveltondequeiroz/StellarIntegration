CREATE TABLE [dbo].[StellarTransactions] (
    [UserId]      INT          NOT NULL,
    [Destination] VARCHAR (56) NOT NULL,
    [XLMAmount]   INT          NOT NULL,
    [State]       VARCHAR (8)  NOT NULL
);
