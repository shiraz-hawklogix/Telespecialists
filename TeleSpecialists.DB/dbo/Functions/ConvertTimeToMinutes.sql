-- =============================================
-- Author:		<Atta H.>
-- Create date: <2020-Feb-27>
-- Description:	<Convert sql timespan to minutes>
-- =============================================
CREATE FUNCTION [dbo].[ConvertTimeToMinutes]
(
	-- Add the parameters for the function here
	@time  DATETIME
)
RETURNS decimal(10, 2)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ResultVar decimal(10, 2) 

	-- Add the T-SQL statements to compute the return value here
	SELECT @ResultVar = ISNULL(DATEPART(MINUTE, @time) + 60*DATEPART(Hour, @time) + (cast(DATEPART(Second,@time) as float) / 60) , 0)

	-- Return the result of the function
	RETURN @ResultVar 

END