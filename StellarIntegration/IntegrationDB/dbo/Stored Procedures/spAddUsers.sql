CREATE PROCEDURE [dbo].[spAddUsers]
	@Account varchar(56),
	@Balance int = 0
AS
	INSERT INTO dbo.ExchangeUsers values(@Account, @Balance)

RETURN 0
