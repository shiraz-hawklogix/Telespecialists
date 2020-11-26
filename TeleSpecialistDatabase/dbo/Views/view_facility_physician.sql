CREATE VIEW [dbo].[view_facility_physician]  
AS  
SELECT  distinct      dbo.facility_physician.fap_key, dbo.facility_physician.fap_fac_key, dbo.facility_physician.fap_user_key, dbo.facility_physician.fap_is_active, dbo.facility.fac_name, dbo.facility.fac_is_active,   
              dbo.facility_physician.fap_is_on_boarded, dbo.facility.fac_go_live  
FROM          dbo.facility_physician 
              INNER JOIN  dbo.facility ON dbo.facility_physician.fap_fac_key = dbo.facility.fac_key
			  INNER JOIN  dbo.facility_contract_service ON dbo.facility_physician.fap_fac_key = dbo.facility_contract_service.fcs_fct_key
WHERE        (dbo.facility_physician.fap_is_active = 1) AND (dbo.facility.fac_is_active = 1) AND (dbo.facility.fac_go_live = 1) AND (dbo.facility_physician.fap_is_on_boarded = 1)
             AND (dbo.facility_contract_service.fcs_srv_key = 43)