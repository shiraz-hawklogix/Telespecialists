CREATE FUNCTION [dbo].[GetPhysiciansInitialsCount]
(
	@physIntials varchar(max)
)

RETURNS varchar(max)
AS
BEGIN
	DECLARE @Result varchar(max);

	DECLARE @Count int;
	DECLARE @pos INT
	DECLARE @len INT
	DECLARE @value varchar(max)
	DECLARE @PreviousValue varchar(max)
	set @Count = 0
	set	@Result =  ''

	IF(@physIntials is not null)
		Begin
			set @physIntials = @physIntials+'/'
			set @pos = 0
			set @len = 0
	
			WHILE CHARINDEX('/', @physIntials, @pos+1)>0
			BEGIN
				set @len = CHARINDEX('/', @physIntials, @pos+1) - @pos
				set @value = SUBSTRING(@physIntials, @pos, @len)
				 --PRINT @pos
				 if( @value <> '' )
					begin
					 if(@pos = 0 or @PreviousValue <> @value )
						Begin
							set @Count = @Count + 1
						END 
					end
				set @pos = CHARINDEX('/', @physIntials, @pos+@len) +1
				set @PreviousValue = @value    
			END
			-- Return the result of the function
			if(@Count > 0)
				begin
					set	@Result = STR(@Count)
				end
			else
				begin
					set	@Result =  ''
				end
	End
	Return 	@Result
END