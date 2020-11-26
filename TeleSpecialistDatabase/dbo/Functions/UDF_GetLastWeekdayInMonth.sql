




/*
=============================================
Author:         Degen, Andreas
Copyright:      All rights reserved
Create date:    2014-04-26
Description:    Returns the last occurence of a given weekday in a month

Parameters:
    
    @DayOfWeek INT: The day of week you search for; see remarks for supported values
    @ReferenceDate DATETIME2: The reference date used as basis for the calculation

Return value:
    
    @Result DATETIME2: The last occurence of a given weekday in a month

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

    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (0, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (1, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (2, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (3, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (4, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (5, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (6, GETDATE())
    SELECT [dbo].[UDF_GetLastWeekdayInMonth] (7, GETDATE())

Change log:
    
=============================================
*/
CREATE FUNCTION [dbo].[UDF_GetLastWeekdayInMonth]
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

    -- support of .NET values
    IF @DayOfWeek = 0 BEGIN
        SET @DayOfWeek = 7;
    END;  

    DECLARE
        @FirstOfWeekday DATETIME2 = DATEADD(DAY, @DayOfWeek - 1, 0)
    ;  

    SELECT @Result =
        DATEADD(
            DAY
            ,DATEDIFF(
                DAY
                ,@FirstOfWeekday
                ,DATEADD(
                    MONTH
                    ,DATEDIFF(
                        MONTH
                        ,0
                        ,@ReferenceDate
                    )
                    ,DATEADD(
                        DAY
                        ,-1
                        ,DATEADD(
                            MONTH
                            ,1
                            ,0
                        )
                    )
                )
            ) / 7 * 7
            ,@FirstOfWeekday
        )
    ;

	RETURN @Result;

END