

Create FUNCTION [dbo].[GetFacilityContractServices]
(
	@fct_key uniqueidentifier
)
RETURNS varchar(MAX)
AS
BEGIN
	DECLARE @Result varchar(max);
	 select @Result = STUFF( (select  ',' + ucl_data.ucd_title  from facility_contract_service as serviceType inner  join ucl_data  on serviceType.fcs_srv_key = ucl_data.ucd_key
	 where fcs_fct_key = @fct_key 
	
order by fcs_key 
		 
		   FOR XML PATH('')),
                            1, 1, '')    

	RETURN @Result

END