-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetUnSchedulePhysicians]
	 
	@date datetime,
	@physicianStatus int=5
AS
BEGIN
	 
	SET NOCOUNT ON;

	DECLARE @updatedPhysicianList TABLE (psl_user_key nvarchar(128))
	DECLARE @scheduledPhysicians TABLE (uss_user_id nvarchar(128))

	insert into @updatedPhysicianList
     select psl.psl_user_key from user_schedule uc inner join physician_status_log psl on
	 psl.psl_uss_key=uc.uss_key
	 where CAST(psl.psl_created_date as date)=cast(@date as date)
	 and LOWER(psl.psl_created_by)='unscheduled physicians service'
	 and psl.psl_created_date is not null

	 insert into @scheduledPhysicians
	 select distinct uss_user_id from user_schedule where ( @date >= uss_time_from_calc
	 and @date <= DATEADD(MINUTE,30,uss_time_to_calc)) and ( uss_time_from_calc is not null
	 and uss_time_to_calc is not null)
	 
	 select asp_user.Id from AspNetUsers asp_user inner join
	 AspNetUserRoles asp_roles on  asp_user.Id=asp_roles.UserId
	  inner join physician_status phy_status on asp_user.status_key=phy_status.phs_key
	 where asp_user.Id not in 
	 (select uss_user_id  from @scheduledPhysicians) and 
	 asp_user.Id not in
	  (select psl_user_key from @updatedPhysicianList) and 
	 (asp_user.status_key !=@physicianStatus) and
	  asp_roles.RoleId in (select Id from AspNetRoles asp_role where asp_role.Name in ('Physician','Partner Physician')  )


	
END