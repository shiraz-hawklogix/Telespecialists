-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[usp_Update_caseForBlast] 
	@cas_key int,
	@response_phy_key nvarchar(128),
	@associated_key nvarchar(128),
	@cas_phy_key nvarchar(128),
	@ctp_key int,
	@modified_by nvarchar(128),
	@modified_on datetime,
	@cst_key int,
	@status_assign_date datetime,
	@response_time_phy datetime,
	@phy_assign_date datetime,
	@phy_initial varchar(50)
AS
BEGIN
	update [case] set
	cas_response_phy_key = @response_phy_key,
                        cas_associate_id = @associated_key,
                        cas_phy_key = @cas_phy_key,
                        cas_ctp_key = @ctp_key,
                        cas_modified_by = @modified_by,
                        cas_modified_date = @modified_on,
                        cas_cst_key = @cst_key,
                        cas_status_assign_date = @status_assign_date,
                        cas_response_time_physician = @response_time_phy,
                        cas_physician_assign_date = @phy_assign_date,
                        cas_history_physician_initial = @phy_initial,
						cas_billing_physician_blast = 1
						where cas_key = @cas_key
END