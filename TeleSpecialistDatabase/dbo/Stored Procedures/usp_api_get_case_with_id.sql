-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_api_get_case_with_id]
	@id int
AS
BEGIN
	select 
	c.cas_key,
	  c.cas_case_number
	   ,c.[cas_ctp_key]
	   ,c.cas_cst_key
      ,c.[cas_fac_key], 
	   fac.fac_name,
	  ISNULL(  fac.fac_emr,'') as  fac_emr,
	   c.cas_phy_key,
      (users.FirstName + ' ' + users.LastName) as PhysicianName ,
	  c.cas_created_by_name,
ISNULL ( c.cas_patient,'') as cas_patient,
ISNULL( c.cas_billing_dob,'') as  cas_billing_dob,
ISNULL( c.cas_metric_patient_gender,'')as cas_metric_patient_gender,
coalesce(cast(c.cas_identification_type as varchar(255)), '') as  cas_identification_type,
--c.cas_identification_type,
ISNULL( c.cas_identification_number,'') as  cas_identification_number, ISNULL(c.cas_callback,'') as cas_callback,fac.fac_timezone, ISNULL( c.[cas_cart],'') as [cas_cart],ISNULL( c.[cas_response_ts_notification],'')as [cas_response_ts_notification], 
	 ISNULL( c.cas_metric_door_time_est,'') as cas_metric_door_time_est
	from [case] c
	inner join AspNetUsers users on  users.Id = c.cas_phy_key
	inner join facility fac  on fac.fac_key =  c.cas_fac_key
	where c.cas_key = @id

END
