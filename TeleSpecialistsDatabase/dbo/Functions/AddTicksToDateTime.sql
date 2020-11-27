
CREATE FUNCTION [dbo].[AddTicksToDateTime] (@Date datetime2, @Ticks bigint )
  RETURNS datetime2
AS
BEGIN  
    SET @Date = DATEADD( DAY, @Ticks / 864000000000, @Date );
    SET @Date = DATEADD( SECOND, ( @Ticks % 864000000000) / 10000000, @Date );
    RETURN DATEADD( NANOSECOND, ( @Ticks % 10000000 ) * 100, @Date );
END