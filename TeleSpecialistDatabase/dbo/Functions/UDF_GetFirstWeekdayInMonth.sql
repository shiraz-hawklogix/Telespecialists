



/*
=============================================
Author:         Degen, Andreas
Copyright:      All rights reserved
Create date:    2014-04-26
Description:    Returns the first occurence of a given weekday in a month

Parameters:
    
    @DayOfWeek INT: The day of week you search for; see remarks for supported values
    @ReferenceDate DATETIME2: The reference date used as basis for the calculation

Return value:
    
    @Result DATETIME2: The first occurence of a given weekday in a month

Remarks:

    Supported values for parameter @DayOfWeek
        1 = Monday
        2 = Tuesday
        3 = Wednesday
        4 = Thursday
        5 = Friday
        6 = Saturday
        0 or 7 = Sunday

Samples:

    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (0, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (1, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (2, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (3, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (4, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (5, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (6, GETDATE())
    SELECT [dbo].[UDF_GetFirstWeekdayInMonth] (7, GETDATE())

Change log:
    
=============================================
*/
CREATE FUNCTION [dbo].[UDF_GetFirstWeekdayInMonth]
(
    @DayOfWeek INT
    ,@ReferenceDate DATETIME2
)
RETURNS DATE
AS
BEGIN
	DECLARE
        @Result DATE = NULL
    ;  

    SELECT @Result =
        DATEADD(
            DAY
            ,7
            ,[dbo].[UDF_GetLastWeekdayInMonth] (@DayOfWeek, DATEADD(MONTH, -1, @ReferenceDate))
        )
    ;

	RETURN @Result;

END