-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Nov-13>
-- Description:	<Calculate date different as seconds ()int value>
-- =============================================
CREATE FUNCTION [dbo].[DiffSeconds]
(
	-- Add the parameters for the function here
	@date1	DATETIME2,
	@date2	DATETIME2
)
RETURNS INT 	
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Result INT = 0

	IF(@date1 IS NULL OR @date2 IS NULL)
	BEGIN	
		RETURN @Result
	END 

	DECLARE @YearDiff INT = 0
	IF(@date1 > @date2)
	SET @YearDiff  = DATEPART(Year, @date1) - DATEPART(YEAR, @date2)
	ELSE
	SET @YearDiff  =  DATEPART(YEAR, @date2) - DATEPART(Year, @date1)

	IF (@YearDiff > 1)	
		RETURN @Result


	IF(@date1 > @date2)
		BEGIN
			SET @Result = DATEDIFF(SECOND, @date2, @date1)
		END 
	ELSE
		BEGIN 
			SET @Result = DATEDIFF(SECOND, @date1, @date2)
		END 

	RETURN @Result

END