create PROCEDURE insert_update_Mute_FB_notification
	@key int,
	@userid nvarchar(128) ,
	@firebaseuid nvarchar(100) ,
	@createOn datetime ,
	@startFrom datetime ,
	@toEnd datetime 
AS
BEGIN
	if(@key = 0)
	begin
	insert into mute_firebase_notification ([mfn_user_key],[mfn_firebase_uid],[mfn_created_on],[mfn_start_from],[mfn_to_end] )
	values(@userid, @firebaseuid, @createOn, @startFrom, @toEnd)
	end
	else
	begin

	if(@startFrom = '1900-01-01 00:00:00.000' and @toEnd = '1900-01-01 00:00:00.000')
	begin 
	update mute_firebase_notification set 
	[mfn_start_from] = null
	,[mfn_to_end] = null
	where [mfn_key] = @key
	end

	else
	begin
	update mute_firebase_notification set 
	[mfn_start_from] = @startFrom
	,[mfn_to_end] = @toEnd
	where [mfn_key] = @key
	end
	
	end
END
