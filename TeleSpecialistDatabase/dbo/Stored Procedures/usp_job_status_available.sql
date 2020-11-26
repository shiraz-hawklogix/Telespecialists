-- =============================================
-- Author:		<Atta H.>
-- Create date: <2020-Feb-26>
-- Description:	<Process Physician Status as per schedule | as a replacement of EF>
-- =============================================
CREATE PROCEDURE [dbo].[usp_job_status_available]
	-- Add the parameters for the stored procedure here
	@DateEST	DATETIME
AS
BEGIN
	
	DECLARE  @available INT = 1;
	DECLARE  @twodaysLater DATETIME = DATEADD(DAY, -1, @DateEST)

	-- get schedule physicians
	SELECT uss_user_id, uss_time_from_calc, uss_time_to_calc, uss_key
	INTO #scheduledPhysicians
	FROM dbo.user_schedule (NOLOCK)
	WHERE @dateEst BETWEEN uss_time_from_calc AND uss_time_to_calc

	-- get physicians and partner physicians
	SELECT AspNetUsers.Id, uss_time_from_calc, uss_time_to_calc,uss_key,  FirstName + ' '+LastName AS FullName
	INTO #selectedPhysiciansIDs
	FROM dbo.AspNetUsers
	INNER JOIN dbo.AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
	INNER JOIN dbo.AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId

	INNER JOIN #scheduledPhysicians ON #scheduledPhysicians.uss_user_id = AspNetUsers.Id
	WHERE AspNetRoles.Name IN ('Physician', 'Partner Physician')


	-- physician status log query
	SELECT psl_key, psl_user_key, uss_time_from_calc, psl_start_date, uss_time_to_calc, psl_created_date, psl_created_by, uss_key,
	dbo.DiffSeconds(uss_time_from_calc, psl_start_date) as DIFFSEC, ROW_NUMBER() over (partition by psl_user_key order by psl_key desc ) as RNUM,
	FullName

	INTO #physicianStatusLogs
	FROM dbo.physician_status_log (NOLOCK)
	INNER JOIN #selectedPhysiciansIDs ON #selectedPhysiciansIDs.Id = physician_status_log.psl_user_key
	WHERE CAST(psl_created_date AS DATE) >= CAST(@twodaysLater AS DATE)
	AND psl_created_by = 'reset physician service'

	SELECT *
	INTO #finalResult FROM (
	SELECT * FROM #physicianStatusLogs
	WHERE 
	--psl_created_by = 'reset physician service'
	RNUM = 1 and DIFFSEC >= 60

	UNION

	SELECT 
	psl_key, A.Id psl_user_key, A.uss_time_from_calc, A.uss_time_from_calc AS psl_start_date, A.uss_time_to_calc, @dateEst psl_created_date, 'reset physician service' AS psl_created_by, A.uss_key,
	0 as DIFFSEC, 0 as RNUM, A.FullName
	FROM #selectedPhysiciansIDs A
	LEFT OUTER JOIN #physicianStatusLogs ON psl_user_key = Id
	WHERE psl_user_key IS NULL 
	--ORDER BY FullName
	) AS temp



	UPDATE dbo.AspNetUsers SET
	status_key = 1,
	status_change_date = @dateEst,
	status_change_date_forAll = @dateEst,
	status_change_cas_key = NULL
	WHERE Id IN (SELECT DISTINCT psl_user_key FROM #finalResult)


	INSERT	 INTO dbo.physician_status_log ( 
										   psl_user_key ,
										   psl_phs_key ,
										   psl_status_name ,
										   psl_created_date ,
										   psl_created_by ,
										   psl_start_date ,
										   psl_end_date ,
										   psl_uss_key 
										)
	SELECT 
			 psl_user_key ,       -- psl_user_key - nvarchar(128)
			 1 ,         -- psl_phs_key - int
			 'Available' ,        -- psl_status_name - varchar(50)
			 @dateEst , -- psl_created_date - datetime
			 N'reset physician service' ,       -- psl_created_by - nvarchar(128)
			 uss_time_from_calc , -- psl_start_date - datetime
			 uss_time_to_calc , -- psl_end_date - datetime
			 uss_key          -- psl_uss_key - bigint
	FROM #finalResult


	SELECT DISTINCT psl_user_key AS UserId FROM #finalResult


	drop table #scheduledPhysicians
	drop table #selectedPhysiciansIDs
	drop table #physicianStatusLogs
	DROP TABLE #finalResult
END