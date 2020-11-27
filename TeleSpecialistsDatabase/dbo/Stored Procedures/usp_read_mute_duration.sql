-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_read_mute_duration
	@userid nvarchar(128)
AS
BEGIN
select [mfn_key],mfn_user_key, mfn_created_on, mfn_firebase_uid,mfn_start_from, mfn_to_end  from [mute_firebase_notification] where [mfn_user_key] = @userid
END
