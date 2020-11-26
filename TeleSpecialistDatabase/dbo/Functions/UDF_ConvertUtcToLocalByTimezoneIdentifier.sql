

/*
=============================================
Author:         Degen, Andreas
Copyright:      All rights reserved
Create date:    2014-04-26
Description:    Converts the given UTC date to the given timezone

Parameters:
    
    @TargetTimezoneIdentifier NVARCHAR(100): The unique identifier of your target timezone (supported timezone identifiers see table "DateTimeUtil.Timezone" column "Identifier")
    @UtcDate DATETIME2: The original UTC datetime value which you want to convert to local datetime

Return value:
    
    @Result DATETIME2: The converted datetime value in local datetime

Remarks:


Samples:

    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', GETUTCDATE())

    -- northern hemisphere (+3600)
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', '2014-03-30 00:55:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', '2014-03-30 01:05:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', '2014-10-26 00:05:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', '2014-10-26 01:05:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('W. Europe Standard Time', '2014-10-26 02:05:00')

    -- southern hemisphere (+3600)
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('Namibia Standard Time', '2014-04-05 23:55:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('Namibia Standard Time', '2014-04-06 00:55:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('Namibia Standard Time', '2014-04-06 01:55:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('Namibia Standard Time', '2014-09-07 00:55:00')
    SELECT [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] ('Namibia Standard Time', '2014-09-07 01:05:00')

Change log:
    
=============================================
*/
CREATE FUNCTION [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]
(
    @TargetTimezoneIdentifier NVARCHAR(100)
    ,@UtcDate DATETIME2
)
RETURNS DATETIME2
AS
BEGIN
	DECLARE @Result DATETIME2 = NULL ;
	IF(@UtcDate IS NULL) RETURN NULL ;


	/*
    SELECT @Result = 
        DATEADD(SECOND, [tz].[BaseUtcOffsetSec] + COALESCE([ar].[DaylightDeltaSec], 0), @UtcDate)
    FROM
        [dbo].[Timezone] AS [tz] WITH (READUNCOMMITTED)
        LEFT JOIN [dbo].[TimezoneAdjustmentRule] AS [ar] WITH (READUNCOMMITTED)
            ON 1 = 1
            AND [ar].[TimezoneId] = [tz].[Id]
            AND CONVERT(DATE,
                    CASE
                        -- southern hemisphere
                        WHEN [ar].[DaylightTransitionStartMonth] > [ar].[DaylightTransitionEndMonth] THEN DATEADD(SECOND, [tz].[BaseUtcOffsetSec] + [ar].[DaylightDeltaSec], @UtcDate)
                        -- northern hemisphere
                        ELSE DATEADD(SECOND, [tz].[BaseUtcOffsetSec], @UtcDate)
                    END
                ) BETWEEN [ar].[DateStart] AND [ar].[DateEnd]
            AND ( 1 = 0
                OR ( 1 = 1
                    -- southern hemisphere
                    AND [ar].[DaylightTransitionStartMonth] > [ar].[DaylightTransitionEndMonth]
                    AND NOT ( 1 = 1
                        AND DATEADD(SECOND, [tz].[BaseUtcOffsetSec] + [ar].[DaylightDeltaSec], @UtcDate) >=
                            CASE
                                WHEN [ar].[DaylightTransitionEndIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionEndWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionEndWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                    END
                            END
                        AND DATEADD(SECOND, [tz].[BaseUtcOffsetSec], @UtcDate) <=
                            CASE
                                WHEN [ar].[DaylightTransitionStartIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                ELSE 
                                    CASE
                                        WHEN [ar].[DaylightTransitionStartWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                        ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionStartWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                    END
                            END
                    )                          
                ) OR
                ( 1 = 1
                    -- northern hemisphere
                    AND [ar].[DaylightTransitionStartMonth] <= [ar].[DaylightTransitionEndMonth]
                    AND DATEADD(SECOND, [tz].[BaseUtcOffsetSec], @UtcDate) >=
                        CASE
                            WHEN [ar].[DaylightTransitionStartIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                            ELSE 
                                CASE
                                    WHEN [ar].[DaylightTransitionStartWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                    ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionStartWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionStartDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionStartTimeOfDay]), 121)
                                END
                        END
                    AND DATEADD(SECOND, [tz].[BaseUtcOffsetSec] + [ar].[DaylightDeltaSec], @UtcDate) <=
                        CASE
                            WHEN [ar].[DaylightTransitionEndIsFixedDateRule] = 1 THEN CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2) + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                            ELSE 
                                CASE
                                    WHEN [ar].[DaylightTransitionEndWeek] = 5 THEN CONVERT(DATETIME2, CONVERT(NVARCHAR, [dbo].[UDF_GetLastWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                    ELSE CONVERT(DATETIME2, CONVERT(NVARCHAR, CONVERT(DATE, DATEADD(DAY, ([ar].[DaylightTransitionEndWeek] - 1) * 7, [dbo].[UDF_GetFirstWeekdayInMonth] ([ar].[DaylightTransitionEndDayOfWeek], CONVERT(DATETIME2, RIGHT('0000' + CONVERT(NVARCHAR, YEAR(@UtcDate)), 4) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndMonth]), 2) + '-' + RIGHT('00' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndDay]), 2), 121)))), 121)  + ' ' + CONVERT(NVARCHAR, [ar].[DaylightTransitionEndTimeOfDay]), 121)
                                END
                        END
                )              
            )
    WHERE 1 = 1
        AND [tz].[Identifier] = @TargetTimezoneIdentifier
    ;
	*/

	/* Modifying this function for optimization 
	  Change Date : Dec 12 2019
	  Adnan K
	 */
 Select @Result	 =  ((@UtcDate AT TIME ZONE 'UTC') At Time Zone @TargetTimezoneIdentifier )

	RETURN @Result;

END