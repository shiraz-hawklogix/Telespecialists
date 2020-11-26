-- =============================================
-- Author:		<Atta H.>
-- Create date: <2020-Feb-27>
-- Description:	<Convert sql timespan to minutes>
-- =============================================
CREATE FUNCTION [dbo].[ConvertTimeToSeconds]
(
	-- Add the parameters for the function here
	@time  DATETIME
)
RETURNS BIGINT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ResultVar BIGINT

	-- Add the T-SQL statements to compute the return value here
	-- SELECT @ResultVar = ISNULL(DATEPART(MINUTE, @time) + 60*DATEPART(Hour, @time) + (cast(DATEPART(Second,@time) as float) / 60) , 0)
	SELECT @ResultVar = ISNULL((DATEPART(Hour, @time) * 60 * 60 ) + (60 * DATEPART(Minute, @time)) + DATEPART(Second, @time) , 0)

	-- Return the result of the function
	RETURN @ResultVar 

END