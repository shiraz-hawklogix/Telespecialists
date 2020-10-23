-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Nov-26>
-- Description:	<Optimize Physician Search for Case Create|Edit>
-- =============================================
-- Exec [dbo].[usp_GetAllPhysiciansByFacility] 'C9288656-15C6-4AAB-BDA7-43AEB316E37D', 9, 0
CREATE PROCEDURE [dbo].[usp_GetAllPhysiciansByFacility] 
	-- Add the parameters for the stored procedure here
	@FacilityKey				UNIQUEIDENTIFIER,
	@CaseType					INT,
	@isTimeBetween7and12		INT = 0,
	@SoftSaveGuid				UNIQUEIDENTIFIER = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	----- CONSTANTS ---


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

	BEGIN TRY


	PRINT 'At defaultStatus'

	SELECT TOP 1 @defaultStatus = phs_assignment_priority
	FROM dbo.physician_status WHERE phs_is_active = 1 AND phs_is_default = 1


	-----------------------------

	--int nullPhysicianStatusOrder = defaultStatus != null ? defaultStatus.phs_assignment_priority.HasValue ? defaultStatus.phs_assignment_priority.Value : @intMaxValue : @intMaxValue;

	PRINT 'At nullPhysicianStatusOrder'

	DECLARE @nullPhysicianStatusOrder INT 
	SELECT  @nullPhysicianStatusOrder = ISNULL(@defaultStatus, @intMaxValue)


	-----------------------------
	PRINT 'At facility'

	--var facility = _facilityService.GetDetails(fac_key.Value);
	SELECT * 
	INTO #facility
	FROM dbo.facility WHERE fac_key = @FacilityKey

	-----------------------------
	/*
	var licenseQuery = (_unitOfWork.PhysicianLicenseRepository.Query()
									.Where(m => m.phl_is_active)
									.Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
									.Where(m => m.phl_license_state == null /*|| facility.fac_stt_key == null*/ || m.phl_license_state == facility.fac_stt_key)
									.Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
						)
						.Select(m => m.phl_user_key)
						.Distinct();
	*/

	PRINT 'At licenseQuery'


	SELECT DISTINCT phl_user_key
	INTO #licenseQuery
	FROM dbo.physician_license
	WHERE phl_is_active = 1
	AND @currentDateOnly >= CAST(phl_issued_date AS DATE)
	AND (phl_license_state = null OR phl_license_state = (SELECT fac_stt_key FROM #facility))
	AND (phl_expired_date = null OR @currentDateOnly <= CAST(phl_expired_date AS DATE))
                    
	-----------------------------

	--VAR physicianRole = _adminService.GetRoleByName(UserRoles.Physician.ToDescription());

	PRINT 'At physicianRole'

	DECLARE @physicianRole UNIQUEIDENTIFIER

	SELECT @physicianRole = Id
	FROM dbo.AspNetRoles WHERE [Name] = 'Physician'

	-----------------------------
	--VAR partnerPhysician = _adminService.GetRoleByName(UserRoles.PartnerPhysician.ToDescription());

	PRINT 'At partnerPhysician'

	DECLARE @partnerPhysician UNIQUEIDENTIFIER

	SELECT @partnerPhysician = Id 
	FROM dbo.AspNetRoles WHERE [Name] = 'Partner Physician'

	-----------------------------

	--VAR shceduleQuery = _unitOfWork.ScheduleRepository.Query().Where(x => (currentDate >= x.uss_time_from_calc && currentDate <= x.uss_time_to_calc));
	PRINT 'At scheduleQuery'

	SELECT * 
	INTO #scheduleQuery
	FROM user_schedule
	WHERE @currentDateEST >= uss_time_from_calc AND @currentDateEST <= uss_time_to_calc

	-----------------------------
	/*
	var physiciansQuery = from m in _unitOfWork.FacilityPhysicianRepository.Query()
							join n in licenseQuery on m.fap_user_key equals n
							join phy in GetPhysicians() on m.fap_user_key equals phy.Id
							join sch in shceduleQuery on m.fap_user_key equals sch.uss_user_id into scheduleEntity
							from scheduleRecords in scheduleEntity.DefaultIfEmpty()
							where
							m.fap_fac_key == fac_key
							&& m.AspNetUser.IsActive
							&& m.AspNetUser.IsDeleted == false
							&& m.fap_is_active
							&& m.fap_is_on_boarded
							select new
							{
								user = m,
								scheduleExist = scheduleRecords != null ? true : false,
								currentSchedule = scheduleRecords
							};
	*/


	--SELECT U.*, US.*
	PRINT 'At GetPhysicians'

	SELECT U.Id AS PhyId
	INTO #GetPhysicians
	FROM dbo.AspNetUsers U
	INNER JOIN dbo.AspNetUserRoles UR ON U.Id = UR.UserId
	INNER JOIN dbo.AspNetRoles R ON R.Id = UR.RoleId
	INNER JOIN dbo.physician_status US ON US.phs_key = U.status_key
	WHERE R.Name IN ('Physician', 'Partner Physician')

	-----------------------------

	PRINT 'At physiciansQuery'


	SELECT 
	--m.*, 
	--MP.*,
	scheduleExist = CASE WHEN scheduleRecords.uss_key IS NOT NULL THEN 1 ELSE 0 END,
	--scheduleRecords.*,
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
			--AND 
			--(
			--	(@casType IN (@CaseType_StatEEG, @CaseType_RoutineEEG, @CaseType_LongTermEEG) AND MP.IsEEG = 1)
			--	OR
			--	(@casType NOT IN (@CaseType_StatEEG, @CaseType_RoutineEEG, @CaseType_LongTermEEG) AND MP.IsEEG = MP.IsEEG)
			--)
			AND MP.IsEEG = CASE WHEN @CaseType IN (@CaseType_StatEEG, @CaseType_RoutineEEG, @CaseType_LongTermEEG) THEN 1 ELSE MP.IsEEG END
			AND MP.IsStrokeAlert = CASE WHEN @CaseType  = @CaseType_StrokeAlert THEN 1 ELSE MP.IsStrokeAlert END
			
			
	-----------------------------
	/*
	var busyPhysicianIds = getBusyPhysicians(physiciansQuery.Select(m => m.user.AspNetUser.Id).Distinct().ToList());
	double maxCredentialIndex = 100;
	if (physiciansQuery.Count() > 0)
		maxCredentialIndex = physiciansQuery.Max(m => m.user.AspNetUser.CredentialIndex);
	*/

	if exists (select count(*) from #physiciansQuery)
	begin
		select @maxCredentialIndex = max (AspNetUser_CredentialIndex) from #physiciansQuery
	end

	PRINT 'At busyPhysicianIds'

	;WITH cte AS (
		SELECT ROW_NUMBER() OVER(PARTITION BY cas_phy_key ORDER BY  cas_physician_assign_date DESC) AS RowNum
		, *
		FROM [dbo].[case] CA
		INNER JOIN #physiciansQuery PQ ON CA.cas_phy_key = PQ.AspNetUser_Id
		--WHERE [cas_phy_key] IN (@p0, @p1, @p2, @p3)
	)		
	



	SELECT cas_phy_key AS BusyPhyId
	INTO #busyPhysicianIds
	FROM cte
	WHERE RowNum = 1
	AND cas_cst_key = 18

	
	SELECT * 
	INTO #busyTempPhysicianIds 
	FROM
	(
	SELECT ROW_NUMBER() OVER(PARTITION BY pct_phy_key ORDER BY  pct_created_date DESC) AS RowNum, pct_phy_key, pct_cst_key		
	FROM [dbo].physician_case_temp as PT
		INNER JOIN #physiciansQuery PQ ON PT.pct_phy_key = PQ.AspNetUser_Id
		AND PT.pct_cst_key = 18
		AND PT.pct_key <>  ISNULL(@SoftSaveGuid,'00000000-0000-0000-0000-000000000000')
	
	) as SoftInsertedPhysicians
	WHERE RowNum = 1
	
	-----------------------------

	/*
	var physicians = physiciansQuery.Select(m => new
	{
		m.user.AspNetUser,
		m.scheduleExist,
		IsBusy = busyPhysicianIds.Contains(m.user.AspNetUser.Id) ? 1 : 0,
		IsAvailableForMoreThan90M = m.user.AspNetUser.status_change_date == null ? 2 : DbFunctions.DiffMinutes(m.user.AspNetUser.status_change_date, currentDate) >= 90
																						&& m.user.AspNetUser.status_key == (int)PhysicianStatus.Available
																						&& m.user.AspNetUser.CredentialIndex >= maxCredentialIndex
																						&& isTimeBetween7and12
																							? 1 : 2,
		IsLessThan60MLeft = !m.scheduleExist ? 2 : DbFunctions.DiffMinutes(currentDate, m.currentSchedule.uss_time_to_calc) <= 60 && DbFunctions.DiffMinutes(currentDate, m.currentSchedule.uss_time_to_calc) >= 1
													&& m.user.AspNetUser.status_key == (int)PhysicianStatus.Available ? 1 : 2,
		RoleOrder = (m.user.AspNetUser.AspNetUserRoles.FirstOrDefault() != null ? m.user.AspNetUser.AspNetUserRoles.FirstOrDefault().RoleId : "") == physicianRole.Id ? 1 : 2
	}).Distinct();
	*/

	PRINT 'At physicians - 2'

	SELECT  
	--m.user.AspNetUser,
		scheduleExist,
		IsBusy = CASE  WHEN BTP.pct_phy_key IS NOT NULL THEN 1  
						WHEN BTP.pct_phy_key IS NULL AND BP.BusyPhyId IS NOT NULL THEN 1 					  
					    ELSE 0  END ,
		IsAvailableForMoreThan90M = CASE 
										WHEN PQ.fap_start_date IS NULL THEN 2 
										WHEN 
												DATEDIFF(MINUTE, AspNetUser_status_change_date, @currentDateEST) >= 90
												AND AspNetUser_status_key = @PhysicianStatus_Available
												AND AspNetUser_CredentialIndex >= @maxCredentialIndex
												AND @isTimeBetween7and12 = 1
										THEN  1 
										ELSE  2
									END,
		IsLessThan60MLeft = CASE 
									WHEN scheduleExist = 0 THEN  2 
									WHEN 
											DATEDIFF(minute, @currentDateEST, currentSchedule_uss_time_to_calc) <= 60 
											AND DATEDIFF(MINUTE, @currentDateEST, currentSchedule_uss_time_to_calc) >= 1
											AND AspNetUser_status_key = @PhysicianStatus_Available 
									THEN 1 
									ELSE 2
							END,
		RoleOrder = CASE WHEN AspNetUserRoles_RoleId = @physicianRole THEN 1 ELSE 2 END ,
		phs_assignment_priority,
	
		PQ.AspNetUser_CredentialIndex,
		PQ.AspNetUser_status_change_date,
		PQ.AspNetUser_FirstName,
		PQ.AspNetUser_LastName,
		PQ.AspNetUser_CreatedDate,
		PQ.AspNetUser_Id,
		PQ.AspNetUser_PhoneNumber,
		PQ.AspNetUser_MobilePhone,

		PQ.AspNetUser_physician_status,
		PQ.phs_color_code,
		PQ.phs_name


	INTO #physiciansQuery2
	FROM #physiciansQuery PQ
	LEFT OUTER JOIN #busyPhysicianIds BP ON PQ.AspNetUser_Id = BP.BusyPhyId
	LEFT OUTER JOIN #busyTempPhysicianIds BTP ON PQ.AspNetUser_Id = BTP.pct_phy_key


	--select * from #physiciansQuery2
	-----------------------------
	/*
	physicians = physicians
					.OrderBy(m => m.IsBusy)
					.ThenBy(m => m.IsLessThan60MLeft)
					.ThenBy(m => m.IsAvailableForMoreThan90M)
					.ThenBy(m => (m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_assignment_priority : nullPhysicianStatusOrder))
					.ThenBy(m => m.AspNetUser.CredentialIndex)
					.ThenBy(m => m.RoleOrder)
					.ThenBy(m => m.AspNetUser.status_change_date == null ? maxDate : m.AspNetUser.status_change_date);
	*/

	PRINT 'At physicians - 3'

	/*
	SELECT * 
	INTO #physiciansQuery3
	FROM #physiciansQuery2
	ORDER BY 
			IsBusy, 
			IsLessThan60MLeft, 
			IsAvailableForMoreThan90M, 
			COALESCE(phs_assignment_priority, @nullPhysicianStatusOrder),
			AspNetUser_CredentialIndex,
			RoleOrder,
			COALESCE(AspNetUser_status_change_date, @maxDate)
	*/

	-----------------------------
	/*

	if (CaseType.StatEEG.ToInt() == casType || CaseType.RoutineEEG.ToInt() == casType || CaseType.LongTermEEG.ToInt() == casType)
		physicians = physicians.Where(c => c.AspNetUser.IsEEG);

	-----------------------------

	return physicians.Select(m => new PhysicianStatusViewModel
	{
		isScheduled = m.scheduleExist,
		Name = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
		Id = m.AspNetUser.Id,
		CreatedDate = DBHelper.FormatDateTime(m.AspNetUser.CreatedDate, true),
		StatusChangeDate = m.AspNetUser.status_change_date.HasValue ? DBHelper.FormatDateTime(m.AspNetUser.status_change_date.Value, true) : "",
		CredentialIndex = m.AspNetUser.CredentialIndex,
		MobilePhone = m.AspNetUser.MobilePhone,
		PhoneNumber = m.AspNetUser.PhoneNumber,
		IsAvailableStatus = m.AspNetUser.physician_status != null ? true : false,
		StatusColorCode = m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_color_code : "",
		StatusName = m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_name : "",
	});
	*/


	PRINT 'At physicians - final'

	SELECT
		IsBusy,
		isScheduled = CAST(scheduleExist AS BIT),
		Name = AspNetUser_FirstName + ' ' + AspNetUser_LastName,
		Id = AspNetUser_Id,
		CreatedDate = dbo.FormatDateTime(AspNetUser_CreatedDate, 1),
		StatusChangeDate = dbo.FormatDateTime(AspNetUser_status_change_date, 1),
		CredentialIndex = AspNetUser_CredentialIndex, --FORMAT(, 'N'),
		MobilePhone = AspNetUser_MobilePhone,
		PhoneNumber = AspNetUser_PhoneNumber,
		IsAvailableStatus = CAST(CASE WHEN AspNetUser_physician_status IS NOT NULL THEN 1 ELSE 0 END AS BIT),
		StatusColorCode = ISNULL(phs_color_code, ''),
		StatusName = ISNULL(phs_name, ''),
		ElapsedTime = dbo.FormatSeconds_V2(@currentDateEST , COALESCE(AspNetUser_status_change_date, AspNetUser_CreatedDate))
	FROM #physiciansQuery2
	--where scheduleExist = 1
	ORDER BY 
			IsBusy, 
			IsLessThan60MLeft, 
			IsAvailableForMoreThan90M, 
			COALESCE(phs_assignment_priority, @nullPhysicianStatusOrder),
			AspNetUser_CredentialIndex,
			RoleOrder,
			COALESCE(AspNetUser_status_change_date, @maxDate)


	--select @defaultStatus as defaultStatus
	--select @nullPhysicianStatusOrder as nullPhysicianStatusOrder


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

	PRINT 'At cleanup'

	BEGIN TRY

			DROP  table #facility
			DROP  table #licenseQuery
			DROP  table #scheduleQuery
			DROP  table #GetPhysicians
			DROP  table #physiciansQuery
			DROP  table #busyPhysicianIds
			DROP  table #busyTempPhysicianIds
		
			DROP  table #physiciansQuery2
			--DROP  table #physiciansQuery3

	END TRY 
	BEGIN CATCH
	END CATCH



END
