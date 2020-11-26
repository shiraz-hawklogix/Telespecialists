create FUNCTION CheckDayNumber
(
	@daynumber varchar(100)
)
RETURNS varchar(100)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @result varchar(100)
	DECLARE @length int
	set @length  = len(@daynumber)
	if(@length = 1)
		begin 
		set @result =  '00' + @daynumber;
	end
	
	else if(@length = 2)
			begin 
			set @result = '0' + @daynumber;
	end
	
	else
	begin
	set @result  = @daynumber
	end
	-- Return the result of the function
	--set   @result = len(@daynumber)
	RETURN @result



END
