-- =============================================
-- Author:		<Amir J,,Name>
-- Create date: <2020-Feb-25 >
-- Description:	<Process unscheduled physician status as per schedule | as per requirement>
-- =============================================

-- exec GetUnSchedulePhysicians '2020-02-25 16:33:35.000'

CREATE PROCEDURE [dbo].[usp_job_status_unavailable]
	 
	@DateEST	DATETIME
AS
BEGIN
	 
	DECLARE @updatedPhysicianList	TABLE (psl_user_key nvarchar(128))
	DECLARE @scheduledPhysicians	TABLE (uss_user_id nvarchar(128))
	DECLARE @physicianStatus		INT = 5
	DECLARE @DateEstWithoutTime		DATE = CAST(@DateEST AS DATE)

	insert into @updatedPhysicianList
	select  psl_user_key from user_schedule   (NOLOCK)
	INNER join physician_status_log (NOLOCK)  ON 	psl_uss_key= uss_key
	where CAST( psl_created_date as date) = @DateEstWithoutTime
	and  psl_created_by ='unscheduled physicians service'
	and  psl_created_date is not null

	insert into @scheduledPhysicians
	select distinct uss_user_id from user_schedule (NOLOCK)
	WHERE ( @DateEST >= uss_time_from_calc
	and @DateEST <= DATEADD(MINUTE,30,uss_time_to_calc)) and ( uss_time_from_calc is not null
	and uss_time_to_calc is not null)
	
	 
	select asp_user.Id  AS PhysicianID
	INTO #selectedIDs
	FROM AspNetUsers (NOLOCK) asp_user inner join
	AspNetUserRoles (NOLOCK) asp_roles on  asp_user.Id=asp_roles.UserId
	inner join physician_status  (NOLOCK) on asp_user.status_key=  phs_key
	where asp_user.Id not in 
	(select uss_user_id  from @scheduledPhysicians) and 
	asp_user.Id not in
	(select psl_user_key from @updatedPhysicianList) and 
	(asp_user.status_key !=@physicianStatus) and
	asp_roles.RoleId in (select Id from AspNetRoles (NOLOCK) asp_role where asp_role.Name in ('Physician','Partner Physician')  )


	-- update physician status
	UPDATE dbo.AspNetUsers SET 
	status_key = 5, -- NotAvailable
	status_change_date = @DateEST, -- EST DATE
	status_change_cas_key = NULL,
	status_change_date_forAll = @DateEST -- EST DATE
	WHERE Id IN (SELECT PhysicianID FROM #selectedIDs)

	-- create status log entry

	INSERT INTO dbo.physician_status_log ( psl_user_key ,
	                                       psl_phs_key ,
	                                       psl_status_name ,
	                                       psl_created_date ,
	                                       psl_created_by ,
	                                       psl_start_date
	                                    )
	SELECT 
			PhysicianID,						-- psl_user_key
			5,									-- psl_phs_key
			'Not Available',					-- psl_status_name
			@DateEST,							-- psl_created_date
			'unscheduled physicians service',	-- psl_created_by
			@DateEST							-- psl_start_date

	FROM #selectedIDs

	-- return physicians IDs
	SELECT PhysicianID FROM #selectedIDs
	
	-- drop temp table
	DROP TABLE #selectedIDs


END