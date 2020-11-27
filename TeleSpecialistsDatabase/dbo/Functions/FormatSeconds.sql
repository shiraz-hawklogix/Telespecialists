CREATE FUNCTION [dbo].[FormatSeconds]
(
	@timeInSeconds bigint
)
RETURNS varchar(20)
AS
BEGIN

declare @time varchar(20);
DECLARE @Hours as int;
 set @Hours =  (@timeInSeconds / 3600);

 set @time =   isnull(RIGHT('0' +   CAST(@timeInSeconds / 3600 AS VARCHAR),2) + ':' +
                   RIGHT('0' + CAST((@timeInSeconds / 60) % 60 AS VARCHAR),2) + ':' +
                   RIGHT('0' + CAST(@timeInSeconds % 60 AS VARCHAR),2),'00:00:00')
				   
return @time ;

/*
	DECLARE @Hours as int  = FLOOR(@timeInSeconds /3600);
	DECLARE @min  as int = Floor((@timeInSeconds - (@Hours * 3600)) / 60);
	DECLARE @seconds as float = @timeInSeconds - (@Hours * 3600) - (@min * 60);
 	DECLARE @secondsRnd as float =  round(@seconds * 100,0) / 100

	DECLARE @result as varchar(20);
	SET @result = IIF(@Hours < 10, '0', '') + CONVERT(VARCHAR(100),  @Hours)
    SET @result  = @result + ':' +  IIF(@min < 10, '0', '') + CONVERT(VARCHAR(2), @min); 
 	SET @result = @result + ':' + IIF(@secondsRnd < 10, '0', '') + CONVERT(VARCHAR(2),@secondsRnd);
    
	RETURN @result;
*/
END