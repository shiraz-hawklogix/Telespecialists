
CREATE FUNCTION [dbo].[GetPhysiciansInitial]
(
	@cas_key int
)
RETURNS varchar(MAX)
AS
BEGIN
	DECLARE @Result varchar(max);
	 select @Result = STUFF( (select  '/' + AspNetUsers.UserInitial  from case_assign_history as history inner  join AspNetUsers on history.cah_phy_key = AspNetUsers.Id 
	 where cah_cas_key = @cas_key 
	 and ( cah_action =  'Accepted' 	or cah_action = 'Rejected' or cah_is_manuall_assign = 1)
	and cah_phy_key is not null
order by cah_key 
		 
		   FOR XML PATH('')),
                            1, 1, '')    

	RETURN @Result

END
