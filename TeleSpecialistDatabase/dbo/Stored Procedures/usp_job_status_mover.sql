-- =============================================
-- Author:		<Adnan K.>
-- Create date: <2020-Feb-28>
-- Description:	<As a replacement of status mover service>
-- =============================================

CREATE PROCEDURE [dbo].[usp_job_status_mover]
	-- Add the parameters for the stored procedure here
	@DateEst DATETIME
AS
BEGIN
	
	-- DECLARE  @DateEst DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', getutcdate())


	DECLARE  @available INT = 1;
	DECLARE  @twodaysLater DATETIME = DATEADD(DAY, -1, @dateEst)

	-- get schedule physicians
	SELECT 
			uss_user_id, 
			uss_time_from_calc, 
			uss_time_to_calc, 
			uss_key
	INTO #scheduledPhysicians
	FROM dbo.user_schedule (NOLOCK)
	WHERE @dateEst BETWEEN uss_time_from_calc AND uss_time_to_calc

	-- get physicians and partner physicians
	SELECT 
			AspNetUsers.Id AS UserId, 
			uss_time_from_calc, 
			uss_time_to_calc,
			uss_key,
			phs_threshhold_time, 
			status_change_date_forAll, 
			phs_move_threshhold_time, 
			phs_enable_snooze,
			phs_move_status_key, 
			status_key, 
			phs_max_snooze_count,
			FirstName + ' '+ LastName AS FullName,
			phs_key AS CurrentStatus

	INTO #selectedPhysiciansIDs

	FROM dbo.AspNetUsers(NOLOCK)
	INNER JOIN dbo.AspNetUserRoles(NOLOCK) ON AspNetUserRoles.UserId = AspNetUsers.Id
	INNER JOIN dbo.AspNetRoles(NOLOCK) ON AspNetRoles.Id = AspNetUserRoles.RoleId

	INNER JOIN dbo.physician_status(NOLOCK) ON physician_status.phs_key = AspNetUsers.status_key
	INNER JOIN #scheduledPhysicians ON #scheduledPhysicians.uss_user_id = AspNetUsers.Id
	WHERE AspNetRoles.Name IN ('Physician', 'Partner Physician')

	AND IsActive = 1
	AND phs_move_status_key IS NOT NULL
	AND status_key <> phs_move_status_key

	

	SELECT 
			UserId, 
			SnoozeSeconds = SUM([dbo].[ConvertTimeToSeconds](pss_snooze_time)), 
			SnoozCount = COUNT(pss_key)
	INTO #snoozeTime
	FROM #selectedPhysiciansIDs
	LEFT OUTER JOIN (Select  pss_key, pss_snooze_time, pss_user_key,pss_phs_key
							 FROM dbo.physician_status_snooze(NOLOCK)  
							 INNER JOIN physician_status (NoLOck) on pss_phs_key  = physician_status.phs_key
						WHERE
						 pss_processed_date IS NULL
						 AND 	physician_status.phs_enable_snooze = 1					 
					) AS TempPhysicianSnooze  ON TempPhysicianSnooze.pss_user_key = #selectedPhysiciansIDs.UserId AND pss_phs_key = status_key
	
	GROUP BY UserId
			
	
	;WITH 
	cte1 AS(
		SELECT * FROM #selectedPhysiciansIDs
		WHERE 
				phs_move_threshhold_time IS NOT NULL  
			--	AND phs_enable_snooze = 1
	),
	cte2 AS (
		-- snooze minutes
		SELECT 
				cte1.UserId, 
				FullName, 
				status_change_date_forAll,
				pss_is_latest_snooze = ISNULL(pss_is_latest_snooze, 0), 				
				SnoozCount = ISNULL(SnoozCount, 0),
				SnoozeSeconds = ISNULL(SnoozeSeconds, 0),
				ElapsedTime = DATEDIFF(SECOND, DATEADD(SECOND, ISNULL(SnoozeSeconds, 0), status_change_date_forAll), @DateEst),
				phs_move_threshhold_time, 
				phs_max_snooze_count, 
				status_key, 
				phs_move_status_key, 
				CurrentStatus
		FROM cte1
		LEFT OUTER JOIN #snoozeTime ON #snoozeTime.UserId = cte1.UserId
		LEFT OUTER JOIN (select pss_user_key, pss_is_latest_snooze from dbo.physician_status_snooze where pss_processed_date is null) as temp_physician_status_snooze ON cte1.UserId = temp_physician_status_snooze.pss_user_key
		
	)

	,cte3 AS(
		-- elapsed time
		SELECT 
				cte2.*,
				Differ =   ABS(ElapsedTime - [dbo].[ConvertTimeToSeconds](phs_move_threshhold_time)) 								
							  
		FROM cte2
	)

	SELECT * ,
	isSendSnoozeNotification = CASE 
								WHEN (Differ BETWEEN 90 AND 120) AND (
								ISNULL((SELECT COUNT(*) FROM #snoozeTime WHERE pss_is_latest_snooze = 1), 0) <= 0) THEN 1 
								ELSE 0 END,
	hasExceededThreshhold = CASE 
								WHEN [dbo].[ConvertTimeToSeconds](phs_move_threshhold_time) <= ElapsedTime THEN 1 
								ELSE 0 END ,

	hasMovingFromStrokeToTPA = Case 
							   WHEN CurrentStatus  = 3 AND  phs_move_status_key  = 2 THEN 1  -- = Stroke Alert and Move to Status = TPA
								ELSE 0 END 
	-- Added By Nabeel
	--hasMovingFromRoundingPrepToRounding = Case 
	--						   WHEN CurrentStatus  = 19 AND  phs_move_status_key  = 4 THEN 1  -- = Rounding Prep and Move to Status = Rounding
	--							ELSE 0 END 
	INTO #condition1
	FROM cte3




	
	-- isUpdateStatusDate = true
	; WITH cte5 AS (
		SELECT UserId, status_key,  phs_move_status_key
		FROM #condition1
		WHERE
				-- isSendSnoozeNotification = 0  
			--	 SnoozCount <= phs_max_snooze_count
				 hasExceededThreshhold = 1
				AND hasMovingFromStrokeToTPA = 0
				--OR hasMovingFromRoundingPrepToRounding = 1 -- Added By Nabeel
	)

	UPDATE U SET
			status_change_date = @DateEst, 
			status_change_cas_key = NULL,
			status_key = cte5.phs_move_status_key,
			status_change_date_forAll = @DateEst
	FROM dbo.AspNetUsers U
	INNER JOIN cte5 ON cte5.UserId = U.Id
	WHERE U.Id = cte5.UserId

	-- update physician status

				UPDATE PSS SET
		pss_processed_date = @DateEst,
		pss_is_latest_snooze = 0,
		pss_modified_by = 'physician status move service',
		pss_modified_by_name = 'physician status move service',
		pss_modified_date = @DateEst
	FROM dbo.physician_status_snooze PSS
	INNER JOIN #condition1 ON #condition1.UserId = PSS.pss_user_key

	WHERE
			hasExceededThreshhold = 1
				AND hasMovingFromStrokeToTPA = 0
				--AND hasMovingFromRoundingPrepToRounding = 1 -- Added By Nabeel

	INSERT INTO physician_status_log 
	(
		psl_user_key, 
		psl_created_by, 
		psl_created_date, 
		psl_phs_key, 
		psl_status_name,
		psl_fac_key -- Added By Nabeel
	)

	SELECT 
		UserId, 
		'move to status service',
		@DateEst,
		#condition1.phs_move_status_key,
		phs_name,
		(Select TOP 1 psl_fac_key from physician_status_log where psl_user_key = UserId AND psl_phs_key = 19  order by psl_key desc)
	from #condition1
	INNER JOIN dbo.physician_status (NOLOCK) ON physician_status.phs_key = #condition1.phs_move_status_key
	WHERE
			--	isSendSnoozeNotification = 0 
			--	 SnoozCount <= #condition1.phs_max_snooze_count
				 hasExceededThreshhold = 1
				 
				--AND hasMovingFromStrokeToTPA = 0


	-- snooze Poupup userIds  will rewrite that
	SELECT DISTINCT UserId, status_key from #condition1
	WHERE
			isSendSnoozeNotification = 1 
			AND SnoozCount < phs_max_snooze_count
			

	-- moved userIds
	SELECT DISTINCT UserId FROM #condition1 
	WHERE
			1 = 1
			
				AND hasExceededThreshhold = 1
				AND hasMovingFromStrokeToTPA = 0 
				AND SnoozCount <= #condition1.phs_max_snooze_count




	DROP TABLE #scheduledPhysicians
	DROP TABLE #selectedPhysiciansIDs
	DROP TABLE #snoozeTime
	DROP TABLE #condition1

	
END