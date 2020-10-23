-- =============================================
-- Author:		<Adnan K.>
-- Create date: <2020-April-21>
-- Description:	<Calculate date different as minutes ()int value>
-- =============================================
Create FUNCTION [dbo].[DiffMinutes]
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
			SET @Result = DATEDIFF(MINUTE, @date2, @date1)
		END 
	ELSE
		BEGIN 
			SET @Result = DATEDIFF(MINUTE, @date1, @date2)
		END 

	RETURN @Result

END