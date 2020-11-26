

/*
=============================================
Author:         Degen, Andreas
Copyright:      All rights reserved
Create date:    2014-04-26
Description:    Converts the given date to UTC applying the given timezone

Parameters:
    
    @OriginalTimezoneIdentifier NVARCHAR(100): The unique identifier of your original timezone (supported timezone identifiers see table "DateTimeUtil.Timezone" column "Identifier")
    @LocalDate DATETIME2: The original local datetime value which you want to convert to UTC datetime

Return value:
    
    @Result DATETIME2: The converted datetime value in UTC datetime

Remarks:


Samples:

    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('W. Europe Standard Time', GETDATE())
    
    -- northern hemisphere (+3600)
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('W. Europe Standard Time', '2014-03-30 01:55:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('W. Europe Standard Time', '2014-03-30 03:05:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('W. Europe Standard Time', '2014-10-26 02:05:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('W. Europe Standard Time', '2014-10-26 03:05:00')

    -- southern hemisphere (+3600)
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('Namibia Standard Time', '2014-04-06 01:55:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('Namibia Standard Time', '2014-04-06 02:05:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('Namibia Standard Time', '2014-09-07 01:55:00')
    SELECT [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] ('Namibia Standard Time', '2014-09-07 03:05:00')

Change log:
    
=============================================
*/
CREATE FUNCTION [dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier]
(
    @OriginalTimezoneIdentifier NVARCHAR(100)
    ,@LocalDate DATETIME2
)
RETURNS DATETIME2
AS
BEGIN
	DECLARE
        @Result DATETIME2 = NULL
    ;

	/*
    SELECT @Result = 
        DATEADD(SECOND, -([tz].[BaseUtcOffsetSec] + COALESCE([ar].[DaylightDeltaSec], 0)), @LocalDate)
    FROM
        [dbo].[Timezone] AS [tz] WITH (READUNCOMMITTED)
        LEFT JOIN [dbo].[TimezoneAdjustmentRule] AS [ar] WITH (READUNCOMMITTED)
            ON 1 = 1
            AND [ar].[TimezoneId] = [tz].[Id]
            AND @LocalDate BETWEEN [ar].[DateStart] AND [ar].[DateEnd]
            AND ( 1 = 0
                OR ( 1 = 1
                    -- southern hemisphere
                    AND [ar].[DaylightTransitionStartMonth] > [ar].[DaylightTransitionEndMonth]
                    AND NOT @LocalDate
                        BETWEEN
                            CASE
                                WHEN [ar].[DaylightTransitionEndIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionEndWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionEndWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                    END
                            END
                        AND
                            CASE
                                WHEN [ar].[DaylightTransitionStartIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionStartWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionStartWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                    END
                            END                
                ) OR
                ( 1 = 1
                    -- northern hemisphere
                    AND [ar].[DaylightTransitionStartMonth] <= [ar].[DaylightTransitionEndMonth]
                    AND @LocalDate
                        BETWEEN
                            CASE
                                WHEN [ar].[DaylightTransitionStartIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionStartWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionStartWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                    END
                            END
                        AND
                            CASE
                                WHEN [ar].[DaylightTransitionEndIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionEndWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionEndWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@LocalDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                    END
                            END
                )                          
            )
    WHERE 1 = 1
        AND [tz].[Identifier] = @OriginalTimezoneIdentifier
    ;

	*/

	
	/* Modifying this function for optimization 
	  Change Date : Dec 12 2019
	  Adnan K
	*/

	Select @Result	 =  ((@LocalDate AT TIME ZONE @OriginalTimezoneIdentifier ) At Time Zone 'UTC' )

	RETURN @Result;

END