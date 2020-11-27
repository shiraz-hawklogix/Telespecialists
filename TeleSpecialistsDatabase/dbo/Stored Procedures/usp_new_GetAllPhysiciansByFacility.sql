







CREATE PROCEDURE [dbo].[usp_new_GetAllPhysiciansByFacility] 
	@FacilityKey				UNIQUEIDENTIFIER,
	@CaseType					INT,
	@isTimeBetween7and12		INT = 0,
	@SoftSaveGuid				UNIQUEIDENTIFIER = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @strCurrentDate				NVARCHAR(100) = CONVERT(VARCHAR, GETUTCDATE(), 127)
	DECLARE @currentDateEST				DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', @strCurrentDate) 
	DECLARE @currentDateOnly			DATE = CAST(@currentDateEST AS DATE)

	DECLARE @intMaxValue				INT = 2147483647
	DECLARE @maxDate					DATETIME = CAST('12/31/9999 23:59:59.997' as datetime)
	DECLARE @PhysicianStatus_Available	INT = 1
	DECLARE @maxCredentialIndex			INT = 100
	DECLARE @defaultStatus				INT  

	DECLARE @CaseType_StatEEG			INT = 13
	DECLARE @CaseType_RoutineEEG		INT = 14
	DECLARE @CaseType_LongTermEEG		INT = 15
	DECLARE @CaseType_StrokeAlert		INT = 9
	
CREATE TABLE #FinalPhysicianTable
(ID INT IDENTITY(1, 1) primary key ,	scheduleExist int NULL,	AspNetUser_Id uniqueidentifier NULL,	AspNetUser_status_key int NULL,	AspNetUser_status_change_date datetime NULL,	AspNetUser_CredentialIndex float NULL,	AspNetUser_FirstName varchar(256) NULL,	AspNetUser_LastName varchar(256) NULL,	AspNetUser_CreatedDate datetime NULL,	AspNetUser_PhoneNumber nvarchar(max) NULL,	AspNetUser_MobilePhone nvarchar(max) NULL,	fap_start_date datetime NULL,	currentSchedule_uss_time_to_calc datetime NULL,	AspNetUserRoles_RoleId uniqueidentifier NULL ,	phs_assignment_priority int NULL,	AspNetUser_physician_status int NULL,	phs_color_code varchar(256) NULL,	phs_name varchar(256) NULL, FinalSorted bit NULL)  

	BEGIN TRY
	 
	-----------------------------
	PRINT 'At facility'

	SELECT * 
	INTO #facility
	FROM dbo.facility WHERE fac_key = @FacilityKey

	-----------------------------

	PRINT 'At licenseQuery'


	SELECT DISTINCT phl_user_key
	INTO #licenseQuery
	FROM dbo.physician_license
	WHERE phl_is_active = 1
	AND @currentDateOnly >= CAST(phl_issued_date AS DATE)
	AND (phl_license_state = null OR phl_license_state = (SELECT fac_stt_key FROM #facility))
	AND (phl_expired_date = null OR @currentDateOnly <= CAST(phl_expired_date AS DATE))
                    
	-----------------------------


	PRINT 'At physicianRole'

	DECLARE @physicianRole UNIQUEIDENTIFIER

	SELECT @physicianRole = Id
	FROM dbo.AspNetRoles WHERE [Name] = 'Physician'

	-----------------------------

	PRINT 'At scheduleQuery'

	SELECT * 
	INTO #scheduleQuery
	FROM user_schedule
	WHERE @currentDateEST >= uss_time_from_calc AND @currentDateEST <= uss_time_to_calc

	-----------------------------
 
	PRINT 'At GetPhysicians'

	SELECT U.Id AS PhyId
	INTO #GetPhysicians
	FROM dbo.AspNetUsers U
	INNER JOIN dbo.AspNetUserRoles UR ON U.Id = UR.UserId
	INNER JOIN dbo.AspNetRoles R ON R.Id = UR.RoleId
	INNER JOIN dbo.physician_status US ON US.phs_key = U.status_key
	WHERE R.Name IN ('Physician')

	-----------------------------

	PRINT 'At physiciansQuery'


	SELECT 
	 
	scheduleExist = CASE WHEN scheduleRecords.uss_key IS NOT NULL THEN 1 ELSE 0 END,
	 
	MP.Id AS AspNetUser_Id,
	MP.status_key AS AspNetUser_status_key,
	MP.status_change_date AS AspNetUser_status_change_date,
	MP.CredentialIndex AS AspNetUser_CredentialIndex,
	MP.FirstName AS AspNetUser_FirstName,
	MP.LastName AS AspNetUser_LastName,
	MP.CreatedDate AS AspNetUser_CreatedDate,
	MP.PhoneNumber AS AspNetUser_PhoneNumber,
	MP.MobilePhone AS AspNetUser_MobilePhone,

	fap_start_date,
	scheduleRecords.uss_time_to_calc AS currentSchedule_uss_time_to_calc,
	R.RoleId AS AspNetUserRoles_RoleId,
	phs_assignment_priority,

	PS.phs_key AS AspNetUser_physician_status,
	PS.phs_color_code ,
	PS.phs_name

	INTO #physiciansQuery

	FROM dbo.facility_physician AS m
	INNER JOIN dbo.AspNetUsers AS  MP ON m.fap_user_key = MP.Id
	INNER JOIN dbo.AspNetUserRoles R ON R.UserId = MP.Id
	LEFT OUTER JOIN dbo.physician_status PS ON PS.phs_key = MP.status_key

	INNER JOIN #licenseQuery ON fap_user_key = phl_user_key
	INNER JOIN #GetPhysicians AS phy ON fap_user_key =  phy.PhyId
	LEFT OUTER JOIN #scheduleQuery AS scheduleRecords ON fap_user_key = uss_user_id

	WHERE 
			fap_fac_key = @FacilityKey
			AND MP.IsActive = 1
			AND MP.IsDeleted = 0
			AND m.fap_is_active = 1
			AND m.fap_is_on_boarded = 1
			 
			AND MP.IsEEG = CASE WHEN @CaseType IN (@CaseType_StatEEG, @CaseType_RoutineEEG, @CaseType_LongTermEEG) THEN 1 ELSE MP.IsEEG END
			AND MP.IsStrokeAlert = CASE WHEN @CaseType  = @CaseType_StrokeAlert THEN 1 ELSE MP.IsStrokeAlert END
		
		-----------------------------
		
		
	PRINT 'At WaitingToAcceptPhysicianId'

	;WITH cte AS (
		SELECT ROW_NUMBER() OVER(PARTITION BY cas_phy_key ORDER BY  cas_physician_assign_date DESC) AS RowNum
		, *
		FROM [dbo].[case] CA
		INNER JOIN #physiciansQuery PQ ON CA.cas_phy_key = PQ.AspNetUser_Id
		WHERE DATEDIFF(minute, CA.cas_physician_assign_date , @currentDateEST) <= 3 --ONE DAY --2880  --TWO DAYS
	)		
	

	SELECT DISTINCT c.cas_phy_key AS BusyPhyId--, c.cas_physician_assign_date
	INTO #WaitingToAcceptPhysicianId
	FROM cte c
	WHERE -- RowNum = 1
	--AND 
	cas_cst_key = 18 AND cas_ctp_key = 9

	--Select anu.Id,anu.FirstName,anu.LastName, wt.cas_physician_assign_date  from AspNetUsers anu
	--INNER JOIN #WaitingToAcceptPhysicianId wt
	--ON 
	--anu.Id = wt.BusyPhyId


		-----------------------------	

	PRINT 'At All Onboarded Physicans'

		SELECT *  
		INTO #AllOnboardedPhysicians
		FROM #physiciansQuery

	-----------------------------	
	PRINT  ' At Deleting Soft Saved Physician, bound for 3 minutes, From physician_case_temp '

	DELETE pct
	FROM [dbo].physician_case_temp as pct
	WHERE DATEDIFF(minute,pct.pct_created_date, @currentDateEST) >= 3 
	
	-----------------------------

	PRINT 'At Get Busy Physicans'


	SELECT * 
	INTO #busyTempPhysicianIds 
	FROM
	( SELECT ROW_NUMBER() 
	  OVER(PARTITION BY pct_phy_key ORDER BY  pct_created_date DESC
	)
	  AS RowNum,
	pct_phy_key, pct_cst_key	
	FROM [dbo].physician_case_temp as PT
		INNER JOIN #physiciansQuery PQ ON PT.pct_phy_key = PQ.AspNetUser_Id
		AND PT.pct_cst_key = 18
		AND PT.pct_key <>  ISNULL(@SoftSaveGuid,'00000000-0000-0000-0000-000000000000')
	
	) as SoftInsertedPhysicians
	WHERE RowNum = 1


	-----------------------------	
	PRINT 'At Delete Busy physicans'

	DELETE PQinitial
	FROM #physiciansQuery PQinitial
	WHERE EXISTS (SELECT *
					FROM #busyTempPhysicianIds 
					WHERE PQinitial.AspNetUser_Id = #busyTempPhysicianIds.pct_phy_key)
		

-- Available

		IF exists (Select COUNT(*) from #physiciansQuery where AspNetUser_physician_status = 1)	-- Available Check
		 
			Select * 
			INTO #Availablephysicians    -- Available Temp Table
			From #physiciansQuery where AspNetUser_physician_status = 1

				IF exists (Select COUNT(*) from #Availablephysicians) 

					Select APtemp.*
					INTO #Available_SCHEDULED_physicians			-- Available Scheduled Temp Table
					from #Availablephysicians APtemp where APtemp.scheduleExist = 1 
			
						IF exists (Select COUNT(*) from #Available_SCHEDULED_physicians)

							Select ASPtemp.*
							INTO #Available_ShiftLastHour_physicians			-- Available ShiftLastHour Temp Table
							from #Available_SCHEDULED_physicians ASPtemp 
							where DATEDIFF(minute, @currentDateEST, ASPtemp.currentSchedule_uss_time_to_calc) <= 60 
								  AND DATEDIFF(MINUTE, @currentDateEST, ASPtemp.currentSchedule_uss_time_to_calc) >= 1

										IF exists (Select COUNT(*) from #Available_ShiftLastHour_physicians)

												Select *
												INTO #Check_NinetyThreePercent_OnLastHourPhysicians  -- Available ShiftLastHour NinetyThreePercent Temp Table
												From #Available_ShiftLastHour_physicians
												Where AspNetUser_CredentialIndex >= 93 
												AND DATEDIFF(MINUTE, AspNetUser_status_change_date, @currentDateEST) >= 60


											IF exists (Select COUNT(*) from #Check_NinetyThreePercent_OnLastHourPhysicians)

												INSERT INTO #FinalPhysicianTable
												Select CNTP_OLHP.*, FinalSorted = 1
												From #Check_NinetyThreePercent_OnLastHourPhysicians CNTP_OLHP
												Order By AspNetUser_status_change_date ASC

									-- Delete [#Check_NinetyThreePercent_OnLastHourPhysicians] From [#Available_ShiftLastHour_physicians] To Avoid Re Addition in Final Table
									DELETE ASLHP
									FROM  #Available_ShiftLastHour_physicians ASLHP
									WHERE EXISTS (SELECT *
													FROM #Check_NinetyThreePercent_OnLastHourPhysicians 
													WHERE ASLHP.AspNetUser_Id = #Check_NinetyThreePercent_OnLastHourPhysicians.AspNetUser_Id)
						
									-- Delete [#Check_NinetyThreePercent_OnLastHourPhysicians] From [Available Physicians] To Avoid Re Addition in Final Table
											DELETE ASP
											FROM  #Available_SCHEDULED_physicians ASP
											WHERE EXISTS (SELECT *
															FROM #Check_NinetyThreePercent_OnLastHourPhysicians 
															WHERE ASP.AspNetUser_Id = #Check_NinetyThreePercent_OnLastHourPhysicians.AspNetUser_Id)

						Drop table #Check_NinetyThreePercent_OnLastHourPhysicians


										Select *
										INTO #FirstPriorityPhysicians
										from #Available_ShiftLastHour_physicians

											IF exists (Select COUNT(*) from #FirstPriorityPhysicians)
												
												INSERT INTO #FinalPhysicianTable
												SELECT FPP.*, FinalSorted = 1
												FROM #FirstPriorityPhysicians FPP
												order by AspNetUser_CredentialIndex ASC , AspNetUser_status_change_date  ASC


											-- Delete [Shift Last Hour Physicians] From [Available Physicians] To Avoid Re Addition in Final Table
											DELETE ASP
											FROM  #Available_SCHEDULED_physicians ASP
											WHERE EXISTS (SELECT *
															FROM #FirstPriorityPhysicians 
															WHERE ASP.AspNetUser_Id = #FirstPriorityPhysicians.AspNetUser_Id)
		
										Drop table #FirstPriorityPhysicians

							Drop table #Available_ShiftLastHour_physicians

					IF exists (Select COUNT(*) from #Available_SCHEDULED_physicians)

							Select *
							INTO #NinetyThreePercentPhysicians
							From #Available_SCHEDULED_physicians 
							Where AspNetUser_CredentialIndex >= 93 
							AND DATEDIFF(MINUTE, AspNetUser_status_change_date, @currentDateEST) >= 60

								IF exists (Select COUNT(*) from #NinetyThreePercentPhysicians)
								
										INSERT INTO #FinalPhysicianTable
										Select NTPP.*, FinalSorted = 1
										From #NinetyThreePercentPhysicians NTPP
										Order By AspNetUser_status_change_date ASC



							-- Delete [Ninety Three Percent Physicians] From [Available Physicians] To Avoid Re-Addition in Final Table
											DELETE ASP
											FROM  #Available_SCHEDULED_physicians ASP
											WHERE EXISTS (SELECT *
															FROM #NinetyThreePercentPhysicians 
															WHERE ASP.AspNetUser_Id = #NinetyThreePercentPhysicians.AspNetUser_Id)
		
							Drop table #NinetyThreePercentPhysicians

					IF exists (Select COUNT(*) from #Available_SCHEDULED_physicians)

							INSERT INTO #FinalPhysicianTable
								Select ASP.* , FinalSorted = 1
								From #Available_SCHEDULED_physicians ASP
								Order By AspNetUser_CredentialIndex ASC , AspNetUser_status_change_date  ASC


					Drop table #Available_SCHEDULED_physicians

			Drop table #Availablephysicians							
			






			-- Rounding 

			IF exists (Select COUNT(*) from #physiciansQuery where AspNetUser_physician_status = 4 AND scheduleExist = 1)	-- Rounding Check
		 
					Select *
					INTO #SCHEDULED_For_Rounding_physicians			-- Scheduled Temp Table
					from #physiciansQuery where scheduleExist = 1 

						IF exists (Select COUNT(*) from #SCHEDULED_For_Rounding_physicians)

							Select * 
							INTO #Roundingphysicians    -- Rounding Temp Table
							From #SCHEDULED_For_Rounding_physicians where AspNetUser_physician_status = 4 AND scheduleExist = 1

								IF exists (Select COUNT(*) from #Roundingphysicians) 

									INSERT INTO #FinalPhysicianTable
									SELECT RP.*, FinalSorted = 1
									FROM #Roundingphysicians RP
									Order By AspNetUser_CredentialIndex ASC
				
							Drop table #Roundingphysicians

					Drop table #SCHEDULED_For_Rounding_physicians


			-- Stroke Alert || TPA || Stat Consult || BREAK

			IF exists (Select COUNT(*) from #physiciansQuery where AspNetUser_physician_status = 3 OR AspNetUser_physician_status = 2  OR AspNetUser_physician_status = 8 OR AspNetUser_physician_status = 16  AND scheduleExist = 1)	-- Stroke Alert || TPA || Stat Consult || BREAK  Check
		 
					Select *
					INTO #SCHEDULED_physicians			-- Scheduled Temp Table
					from #physiciansQuery where scheduleExist = 1 

						IF exists (Select COUNT(*) from #SCHEDULED_physicians)

							Select * 
							INTO #StrokeAlertphysicians    -- StrokeAlert Temp Table
							From #SCHEDULED_physicians where AspNetUser_physician_status = 3 OR AspNetUser_physician_status = 2 OR AspNetUser_physician_status = 8  OR AspNetUser_physician_status = 16 AND scheduleExist = 1


					Drop Table #SCHEDULED_physicians

			--IF  ((Select COUNT(*) from #FinalPhysicianTable) > 0)
				
			--	BEGIN
			--		IF exists (Select COUNT(*) from #StrokeAlertphysicians) 

			--			INSERT INTO #FinalPhysicianTable
			--			SELECT * FROM #StrokeAlertphysicians
			--			Order By AspNetUser_CredentialIndex ASC
			--	END
			
			--ELSE

			--	BEGIN
					IF exists (Select COUNT(*) from #StrokeAlertphysicians) 

						INSERT INTO #FinalPhysicianTable
						SELECT SAP.*, FinalSorted = 1 
						FROM #StrokeAlertphysicians SAP
						Order By AspNetUser_status_change_date ASC
--				END

			Drop table #StrokeAlertphysicians
				

				IF exists (Select COUNT(*) from #FinalPhysicianTable) 
					BEGIN

							IF exists (Select COUNT(*) from #WaitingToAcceptPhysicianId) 

								Select ft.*
								INTO #InsertWith_WAITING_TO_ACCEPT_Status
								FROM #FinalPhysicianTable ft
								INNER JOIN #WaitingToAcceptPhysicianId wt ON 
								ft.AspNetUser_Id = wt.BusyPhyId

											DELETE fpt
											FROM #FinalPhysicianTable fpt
											WHERE EXISTS (	SELECT *
											FROM #InsertWith_WAITING_TO_ACCEPT_Status 
											WHERE fpt.AspNetUser_Id = #InsertWith_WAITING_TO_ACCEPT_Status.AspNetUser_Id )

												IF exists (Select COUNT(*) from #InsertWith_WAITING_TO_ACCEPT_Status) 

													ALTER TABLE #InsertWith_WAITING_TO_ACCEPT_Status
													DROP COLUMN ID;
													UPDATE #InsertWith_WAITING_TO_ACCEPT_Status
													SET phs_color_code = '#000000' ,phs_name = 'Waiting To Accept'
													
													INSERT INTO #FinalPhysicianTable 
													SELECT *
													FROM #InsertWith_WAITING_TO_ACCEPT_Status


					END


				IF exists (Select COUNT(*) from #FinalPhysicianTable) 
					BEGIN

						--DELETE FROM #AllOnboardedPhysicians To Avoid Re-Addition
						DELETE AOP
						FROM #AllOnboardedPhysicians AOP
						WHERE EXISTS (	SELECT *
										FROM #FinalPhysicianTable 
										WHERE AOP.AspNetUser_Id = #FinalPhysicianTable.AspNetUser_Id )

						-- INSERTING ALL ONBOARDED TO FINAL PHYSICIAN TABLE 

						INSERT INTO #FinalPhysicianTable 
						SELECT AOP.*, FinalSorted = 0
						FROM #AllOnboardedPhysicians AOP

					END
				ELSE
					BEGIN

						-- INSERTING ALL ONBOARDED TO FINAL PHYSICIAN TABLE 

						INSERT INTO #FinalPhysicianTable 
						SELECT AOP.*, FinalSorted = 0
						FROM #AllOnboardedPhysicians AOP

					END
		SELECT
		FPT.*,
		FullName = FPT.AspNetUser_FirstName + ' ' + FPT.AspNetUser_LastName,
		ElapsedTime = dbo.FormatSeconds_V2(@currentDateEST , COALESCE(FPT.AspNetUser_status_change_date, FPT.AspNetUser_CreatedDate))
		 
		FROM #FinalPhysicianTable FPT
		Order By ID ASC 

			--Select * from #FinalPhysicianTable Order By ID ASC 
	
	END TRY
	BEGIN CATCH
		PRINT 'At ERROR'
		SELECT  
			ERROR_NUMBER() AS ErrorNumber  
			,ERROR_SEVERITY() AS ErrorSeverity  
			,ERROR_STATE() AS ErrorState  
			,ERROR_PROCEDURE() AS ErrorProcedure  
			,ERROR_LINE() AS ErrorLine  
			,ERROR_MESSAGE() AS ErrorMessage;  

	END CATCH

	--------------------------

	PRINT 'At cleanup'


			DROP  table #facility
			DROP  table #licenseQuery
			DROP  table #scheduleQuery
			DROP  table #GetPhysicians
			DROP  table #physiciansQuery
			DROP  table #busyTempPhysicianIds
			DROP  table #AllOnboardedPhysicians
			Drop  table #WaitingToAcceptPhysicianId
			Drop  table #InsertWith_WAITING_TO_ACCEPT_Status
			-- THIS IS FINAL
			DROP  table #FinalPhysicianTable


END