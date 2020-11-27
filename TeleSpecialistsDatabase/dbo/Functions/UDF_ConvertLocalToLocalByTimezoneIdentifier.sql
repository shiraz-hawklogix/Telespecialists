
/*
=============================================
Author:         Degen, Andreas
Copyright:      All rights reserved
Create date:    2014-04-26
Description:    Converts the given date to UTC applying the given timezone

Parameters:
    
    @OriginalTimezoneIdentifier NVARCHAR(100): The unique identifier of your original timezone (supported timezone identifiers see table "DateTimeUtil.Timezone" column "Identifier")
    @TargetTimezoneIdentifier NVARCHAR(100): The unique identifier of your target timezone (supported timezone identifiers see table "DateTimeUtil.Timezone" column "Identifier")
    @LocalDate DATETIME2: The datetime value in your original timezone which you want to convert to the corresponding datetime value in your target timezone

Return value:
    
    @Result DATETIME2: The converted datetime value in your target timezone

Remarks:


Samples:

    SELECT [DateTimeUtil].[UDF_ConvertLocalToLocalByTimezoneIdentifier] ('W. Europe Standard Time', 'Middle East Standard Time', GETDATE())
    SELECT [DateTimeUtil].[UDF_ConvertLocalToLocalByTimezoneIdentifier] ('W. Europe Standard Time', 'Middle East Standard Time', '2014-03-30 01:55:00')
    SELECT [DateTimeUtil].[UDF_ConvertLocalToLocalByTimezoneIdentifier] ('W. Europe Standard Time', 'Middle East Standard Time', '2014-03-30 03:05:00')
    SELECT [DateTimeUtil].[UDF_ConvertLocalToLocalByTimezoneIdentifier] ('W. Europe Standard Time', 'Middle East Standard Time', '2014-10-26 02:05:00')
    SELECT [DateTimeUtil].[UDF_ConvertLocalToLocalByTimezoneIdentifier] ('W. Europe Standard Time', 'Middle East Standard Time', '2014-10-26 03:05:00')

Change log:
    
=============================================
*/
CREATE FUNCTION [dbo].[UDF_ConvertLocalToLocalByTimezoneIdentifier]
(
    @OriginalTimezoneIdentifier NVARCHAR(100)
    ,@TargetTimezoneIdentifier NVARCHAR(100)
    ,@LocalDate DATETIME2
)
RETURNS DATETIME2
AS
BEGIN
	DECLARE
        @Result DATETIME2 = NULL
    ;

    SELECT @Result = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier] (
        @TargetTimezoneIdentifier
        ,[dbo].[UDF_ConvertLocalToUtcByTimezoneIdentifier] (
            @OriginalTimezoneIdentifier
            ,@LocalDate
        )
    );

	RETURN @Result;

END