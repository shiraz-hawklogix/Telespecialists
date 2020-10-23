
CREATE FUNCTION [dbo].[FormatDateTime]
(
	@dt DateTime,
	@bIncludeTime bit = 1
)
RETURNS varchar(50)
AS
BEGIN
	DECLARE @formatedDateTime varchar(50) = '';
	IF @dt is not null
	select  @formatedDateTime = CAST(DatePart(MONTH,@dt) as varchar) + '/' +
		CAST(DatePart(DAY,@dt) as varchar) + '/' +
		CAST(DatePart(YEAR,@dt) as varchar)   + 
	CASE WHEN(@bIncludeTime = 1) THEN  ' ' + CONVERT(VARCHAR(20), @dt, 108) ELSE '' END
    

	RETURN @formatedDateTime

END
