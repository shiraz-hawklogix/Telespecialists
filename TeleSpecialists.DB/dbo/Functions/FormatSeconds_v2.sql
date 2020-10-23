-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Nov-12>
-- Description:	<Calculate difference between two dates as time>
-- =============================================
CREATE FUNCTION [dbo].[FormatSeconds_v2]
(
	-- Add the parameters for the function here
	@date1 DATETIME2,
	@date2 DATETIME2
)
RETURNS varchar(20)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ResultVar VARCHAR(20) = ''


	IF(@date1 IS NULL OR @date2 IS NULL)
		BEGIN	
			SET @ResultVar = ''

		END
	
		DECLARE @YearDiff INT = 0
	IF(@date1 > @date2)
	SET @YearDiff  = DATEPART(Year, @date1) - DATEPART(YEAR, @date2)
	ELSE
	SET @YearDiff  =  DATEPART(YEAR, @date2) - DATEPART(Year, @date1)

	IF (@YearDiff > 1)	
		RETURN @ResultVar


			IF(@date1 > @date2)
				BEGIN	
					SET @ResultVar = [dbo].[FormatSeconds](DATEDIFF(second, @date2, @date1)) 
				END
			ELSE 
				BEGIN	
					SET @ResultVar = [dbo].[FormatSeconds](DATEDIFF(second, @date1, @date2)) 
				END
		
		

	-- Return the result of the function
	RETURN @ResultVar

END
