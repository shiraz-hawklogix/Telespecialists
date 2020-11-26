CREATE procedure sp_insert_case_number_in_operation_outlier_log
@cas_case_number varchar(100),  
@cas_case_type varchar(50),
@cas_case_color varchar(50),  
@cas_created_Date datetime = null,
@cas_modified_Date datetime = null,
@cas_case_fac_name varchar(max),
@cas_case_assign_phy_initial varchar(200)

AS
BEGIN

	INSERT INTO OperationOutlierNotificationLog VALUES (@cas_case_number,@cas_case_type,@cas_case_color,@cas_created_Date,@cas_modified_Date,@cas_case_fac_name,@cas_case_assign_phy_initial)

END



